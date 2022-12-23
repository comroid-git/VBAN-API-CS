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
        private protected UdpClient _client;

        protected byte[] Buf = new byte[0];
        protected int BufI;

        protected VBANStreamBase(
            bool canRead,
            bool canWrite,
            IPEndPoint ip,
            UdpClient client)
        {
            CanRead = canRead;
            CanWrite = canWrite;

            Length = 0;
            
            IpEndPoint = ip;
            _client = client;
        }

        public IPEndPoint IpEndPoint { get; }
        public IPAddress IpAddress => IpEndPoint.Address;
        public int Port => IpEndPoint.Port;
        public bool Closed { get; private protected set; }
        public override bool CanRead { get; }
        public override bool CanSeek => false;
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

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
        public VBANInputStreamBase(VBAN.Protocol<T> expectedProtocol, IPAddress ipAddress, int port)
            : base(true, false, new IPEndPoint(ipAddress, port), new UdpClient(new IPEndPoint(ipAddress, port)))
        {
            ExpectedProtocol = expectedProtocol;
        }

        public VBAN.Protocol<T> ExpectedProtocol { get; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T readData()
        {
            var packet = readPacket();
            VBAN.AnyProtocol rcvProt;

            if (!(rcvProt = packet.Head.Protocol).Equals(ExpectedProtocol))
                throw new InvalidOperationException("Expected Protocol mismatches received protocol " +
                                                    $"[exp={ExpectedProtocol};rcv={rcvProt}]");

            return ExpectedProtocol.CreateDataObject(packet.Bytes);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public VBANPacket<T>.Decoded readPacket()
        {
            var bytes = new byte[VBANPacket<T>.MaxSize];

            int nRead = Read(bytes, 0, bytes.Length);

            return VBANPacket<T>.Decode(bytes);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Closed) throw new InvalidOperationException("Stream is closed");

            var endPoint = IpEndPoint;

            var bytes = new byte[count];
            var bi = 0;

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
        public VBANOutputStream(
            IFactory<VBANPacket<T>> packetFactory,
            IPAddress ipAddress,
            int port = VBAN.DefaultPort
        ) : base(false, true, new IPEndPoint(ipAddress, port), new UdpClient(AddressFamily.InterNetwork))
        {
            PacketFactory = packetFactory;
            Buf = new ByteArray();
        }

        public IFactory<VBANPacket<T>> PacketFactory { get; }

        private protected new ByteArray Buf { get; set; }

        public VBANOutputStream<T> SendData(T data)
        {
            Write(Util.CreateByteArray(data));
            Flush();
            return this;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var actual = new byte[count];
            Array.Copy(buffer, offset, actual, 0, count);

            var wInd = 0;

            while (wInd < actual.Length)
            {
                byte b = buffer[wInd++];

                if ((char)b == '\n') Flush();
                else Buf.Append(b);

                if (wInd != 0 && wInd % VBANPacket<T>.MaxSizeWithoutHead == 0)
                    Flush();
            }
        }

        public override void Flush()
        {
            if (Closed) throw new InvalidOperationException("Stream is closed");

            if (Buf.Length > VBANPacket<T>.MaxSize)
                throw new InvalidOperationException(
                    $"Buffer is too large, must be smaller than {VBANPacket<T>.MaxSize}");

            var packet = PacketFactory.Create();
            packet.AttachData(Buf);

            byte[] x = packet;
            _client.Send(x, x.Length, IpEndPoint);

            Buf = new ByteArray();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Reading is not permitted in OutputStream!");
        }
    }
}