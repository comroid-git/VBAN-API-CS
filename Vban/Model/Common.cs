using System;

namespace Vban.Model
{
    // ReSharper disable once InconsistentNaming
    public interface AnyDataRateValue
    {
    }

    public interface IDataRateValue<T> : AnyDataRateValue, IIntEnum, IBindable<T>
    {
    }

    // ReSharper disable once InconsistentNaming
    public interface AnyFormatValue
    {
    }

    public interface IFormatValue<T> : AnyFormatValue, IIntEnum, IBindable<T>
    {
    }

    public class InvalidPacketAttributeException : Exception
    {
        public InvalidPacketAttributeException(string message) : base(message)
        {
        }
    }

    public class UnfinishedByteArray : IByteArray
    {
        private readonly bool _fixedSize;

        public UnfinishedByteArray(int initSize, bool fixedSize = false)
        {
            BufferArray = new byte[initSize];
            _fixedSize = fixedSize;
        }

        public int Length { get; private set; }

        public int BufferSize => BufferArray.Length;

        public byte[] BufferArray { get; private set; }

        public byte[] Bytes => this;

        public static implicit operator byte[](UnfinishedByteArray arr)
        {
            return arr.Finish();
        }

        public void Append(params byte[] bytes)
        {
            if (bytes == null)
                throw new NullReferenceException("bytearray is null");

            int newSize = Length + bytes.Length;

            if (newSize > BufferArray.Length)
            {
                if (_fixedSize)
                    throw new IndexOutOfRangeException(
                        "Cannot append more elements, array is at fixed size");

                // new buffer
                var newBuf = new byte[BufferArray.Length * 2 + bytes.Length];
                Array.Copy(BufferArray, newBuf, BufferArray.Length);
                BufferArray = newBuf;
            }

            foreach (byte aByte in bytes)
                BufferArray[Length++] = aByte;
        }

        public byte[] Finish()
        {
            var finished = new byte[Length];
            Array.Copy(BufferArray, finished, finished.Length);
            return finished;
        }
    }
}