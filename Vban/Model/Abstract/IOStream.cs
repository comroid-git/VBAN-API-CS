using System;
using System.IO;

namespace Vban.Model.Abstract
{
    // ReSharper disable once InconsistentNaming
    public abstract class IOStream : Stream, IByteArray
    {
        protected byte[] _buf;

        protected IOStream(bool canRead, bool canWrite, bool canSeek, bool canTimeout, int? readTimeout,
                           int? writeTimeout)
        {
            CanRead      = canRead;
            CanWrite     = canWrite;
            CanSeek      = canSeek;
            CanTimeout   = canTimeout;
            ReadTimeout  = readTimeout  ?? -1;
            WriteTimeout = writeTimeout ?? -1;

            _buf     = new byte[0];
            Position = 0;
            Closed   = false;
        }

        public abstract void Write(byte b);

        public abstract override void Flush();

        public override void Close()
        {
            Closed = true;
        }

        public override int ReadByte()
        {
            return _buf[Position];
        }

        public override void WriteByte(byte value)
        {
            Write(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int r = 0;
            for (int i = (int) Position; i < _buf.Length && i < Position + count; i++)
            {
                Position++;
                buffer[offset + i] = (byte) ReadByte();
                r++;
            }

            Position -= r;
            return r;
        }

        public override void SetLength(long value)
        {
            throw new IOException("Method \"SetLength\" not supported!");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = (int) (Position + offset); i < _buf.Length && i < Position + count; i++) WriteByte(_buf[i]);
        }

        public void Write(byte[] buffer)
        {
            foreach (byte b in buffer) WriteByte(b);
        }

        public void Write(char[] buffer)
        {
            foreach (char c in buffer) WriteByte((byte) c);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        #region Properties

        public override        bool CanRead      { get; }
        public override        bool CanSeek      { get; }
        public override        bool CanWrite     { get; }
        public override        bool CanTimeout   { get; }
        public override        long Length       => _buf.Length;
        public sealed override long Position     { get; set; }
        public sealed override int  ReadTimeout  { get; set; }
        public sealed override int  WriteTimeout { get; set; }

        public byte[] Bytes => _buf;

        public bool Closed { get; protected set; }

        #endregion
    }
}