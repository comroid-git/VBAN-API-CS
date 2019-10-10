using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Vban.Model;

// ReSharper disable InconsistentNaming

namespace Vban
{
    public abstract class VBANStreamBase : Stream
    {
        public IPEndPoint IpEndPoint { get; private protected set; }
        public IPAddress IpAddress => IpEndPoint.Address;
        public int Port => IpEndPoint.Port;
        public bool Closed { get; private protected set; }
        public override bool CanRead { get; }
        public override bool CanSeek => false;
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        protected byte[] Buf = new byte[0];
        protected int BufI;

        private protected UdpClient _client;

        protected VBANStreamBase(bool canRead, bool canWrite)
        {
            CanRead = canRead;
            CanWrite = canWrite;

            Length = 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException("Seeking not supported");
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Changing length not supported");
        }

        public override void Close()
        {
            // ReSharper disable once UseIsOperator.2
            if (typeof(VBANOutputStream<>).IsInstanceOfType(this))
                Flush();

            _client.Close();

            Closed = true;
        }
    }

    public class VBANInputStreamBase<T> : VBANStreamBase
    {
        public VBAN.Protocol<T> ExpectedProtocol { get; }

        public VBANInputStreamBase(
            VBAN.Protocol<T> expectedProtocol,
            IPAddress ipAddress,
            int port = VBAN.DefaultPort)
            : base(true, false)
        {
            ExpectedProtocol = expectedProtocol;

            _client = new UdpClient(IpEndPoint = new IPEndPoint(ipAddress, port));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T readData()
        {
            VBANPacket<T>.Decoded packet = readPacket();
            VBAN.AnyProtocol rcvProt;

            if (!(rcvProt = packet.Head.Protocol).Equals(ExpectedProtocol))
                throw new InvalidOperationException("Expected Protocol mismatches received protocol " +
                                                    $"[exp={ExpectedProtocol};rcv={rcvProt}]");

            return ExpectedProtocol.CreateDataObject(packet.Bytes);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public VBANPacket<T>.Decoded readPacket()
        {
            byte[] bytes = new byte[VBANPacket<T>.MaxSize];

            int nRead = Read(bytes, 0, bytes.Length);

            return VBANPacket<T>.Decode(bytes);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Closed) throw new InvalidOperationException("Stream is closed");

            IPEndPoint endPoint = IpEndPoint;

            byte[] bytes = new byte[count];
            int bi = 0;

            while (bi < count)
            {
                if (BufI == 0 || BufI >= Buf.Length)
                {
                    Buf = _client.Receive(ref endPoint);
                    BufI = 0;
                }

                Array.Copy(Buf, BufI, bytes, bi, bi += Math.Min(Buf.Length, bytes.Length));
                BufI += bi;
            }

            Array.Copy(bytes, 0, buffer, offset, count);

            return bi;
        }

        public override void Flush()
        {
            throw new InvalidOperationException("Flushing is not permitted in InputStream!");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Writing is not permitted in InputStream!");
        }
    }

    public class VBANOutputStream<T> : VBANStreamBase
    {
        public IFactory<VBANPacket<T>> PacketFactory { get; }

        private protected new UnfinishedByteArray Buf { get; set; }

        public VBANOutputStream(
            IFactory<VBANPacket<T>> packetFactory,
            IPAddress address,
            int port = VBAN.DefaultPort
            ) : base(false, true)
        {
            PacketFactory = packetFactory;
            Buf = new UnfinishedByteArray(VBANPacket<T>.MaxSize, true);

            _client = new UdpClient(IpEndPoint = new IPEndPoint(address, port));
        }

        public VBANOutputStream<T> sendData(T data)
        {
            Write(Util.CreateByteArray(data));
            Flush();
            return this;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] actual = new byte[count];
            Array.Copy(buffer, offset, actual, 0, count);

            int wInd = 0;

            while (wInd < actual.Length)
            {
                byte b = buffer[wInd++];

                if ((char) b == '\n') Flush();
                else Buf.Append(b);

                if (wInd != 0 && wInd % VBANPacket<T>.MaxSizeWithoutHead == 0)
                    Flush();
            }
        }

        public override void Flush()
        {
            if (Closed) throw new InvalidOperationException("Stream is closed");

            if (Buf.Length > VBANPacket<T>.MaxSize)
                throw new InvalidOperationException($"Buffer is too large, must be smaller than {VBANPacket<T>.MaxSize}");

            VBANPacket<T> packet = PacketFactory.Create();
            packet.Data = Buf.Bytes;

            byte[] x;
            _client.Send(x = packet.Bytes, x.Length, IpEndPoint);

            Buf = new UnfinishedByteArray(VBANPacket<T>.MaxSize, true);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Reading is not permitted in OutputStream!");
        }
    }
}