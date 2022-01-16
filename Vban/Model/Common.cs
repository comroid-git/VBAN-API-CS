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

    [Obsolete]
    public class UnfinishedByteArray : ByteArray
    {
    }
}