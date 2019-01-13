using System.IO;
using System.Net;
using System.Net.Sockets;
using Vban.Model.Abstract;
using static Vban.Packet.VbanPacket;
using static Vban.Util;

namespace Vban
{
    // ReSharper disable once InconsistentNaming
    public static class VBAN
    {
        public static int DefaultPort = 6980;

        public static VBANStream<string> OpenTextStream(int? port)
        {
            return OpenTextStream(IPAddress.Loopback, port);
        }

        public static VBANStream<string> OpenTextStream(IPAddress ipAddress, int? port)
        {
            return new VBANStream<string>(
                    DefaultTextFactory(),
                    ipAddress,
                    port);
        }
    }

    // ReSharper disable once InconsistentNaming
    public class VBANStream<T> : IOStream
    {
        internal VBANStream(Factory packetFactory, IPAddress ipAddress, int? port)
                : base(false, true, false, false, null, null)
        {
            Closed = false;

            _packetFactory = packetFactory;
            IpEndPoint     = new IPEndPoint(ipAddress, port ?? VBAN.DefaultPort);

            _client = new UdpClient();
        }

        public void SendData(T data)
        {
            Write(GetBytes(data));
        }

        public override void Write(byte value)
        {
            if (_buf.Length + 1 > MaximumSize)
                throw new IOException("Byte array is too large, must be smaller than " + MaximumSize);
            _buf = AppendByteArray(_buf, value);
            if ((char) value == '\n') Flush();
        }

        public override void Flush()
        {
            if (Closed) throw new IOException("Stream is closed");
            if (_buf.Length > MaximumSize)
                throw new IOException("Byte array is too large, must be smaller than " + MaximumSize);
            byte[] bytes = _packetFactory.Create(_buf).Bytes;
            _client.Send(bytes, bytes.Length, IpEndPoint);
            _buf = new byte[0];
        }

        #region Fields

        private readonly Factory   _packetFactory;
        private readonly UdpClient _client;

        #endregion

        #region Properties

        public IPEndPoint IpEndPoint { get; }
        public IPAddress  IpAddress  => IpEndPoint.Address;
        public int        Port       => IpEndPoint.Port;

        #endregion
    }
}