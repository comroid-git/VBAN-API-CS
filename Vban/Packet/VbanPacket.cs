using System;
using Vban.Model;

namespace Vban.Packet
{
    public class VbanPacket : IByteArray
    {
        public static readonly int MaximumSize = 1436;

        private readonly IByteArray _head;
        private readonly bool       _overridden;

        internal VbanPacket(VbanPacketHead head, bool? overridden)
        {
            _head       = head;
            _overridden = overridden ?? false;
            Data        = new byte[0];
        }

        public VbanPacket(VbanPacketHead head, byte[] data) : this(head, true)
        {
            Data = data;
        }

        public byte[] Data { get; private set; }

        public byte[] Bytes => Util.AppendByteArray(_head.Bytes, Data);

        public void AddData(byte[] data)
        {
            if (_overridden) throw new InvalidOperationException("Cannot add data to predefined Packet!");
            Data = Util.AppendByteArray(Data, data);
        }

        public static Factory DefaultTextFactory()
        {
            return new Factory(VbanPacketHead.DefaultTextFactory(), false);
        }

        public class Factory : IFactory<VbanPacket>
        {
            private readonly byte[]                   _bytes;
            private readonly IFactory<VbanPacketHead> _headFactory;
            private readonly bool                     _overridden;

            internal Factory(IFactory<VbanPacketHead> headFactory, bool overridden)
            {
                _overridden  = overridden;
                _headFactory = headFactory;
            }

            public Factory(IFactory<VbanPacketHead> headFactory, byte[] bytes) : this(headFactory, true)
            {
                _bytes = bytes;
            }

            public VbanPacket Create()
            {
                Counter++;
                return _overridden
                        ? new VbanPacket(_headFactory.Create(), _bytes)
                        : new VbanPacket(_headFactory.Create(), false);
            }

            public int Counter { get; private set; }

            public VbanPacket Create(byte[] withData)
            {
                return new VbanPacket(_headFactory.Create(), withData);
            }

            public class Builder : IBuilder<Factory>
            {
                public IFactory<VbanPacketHead> HeadFactory { get; set; }

                public byte[] ConstantData { get; set; }

                public Factory Build()
                {
                    return ConstantData == null
                            ? new Factory(HandleNullFactory(), false)
                            : new Factory(HandleNullFactory(), ConstantData);
                }

                private IFactory<VbanPacketHead> HandleNullFactory()
                {
                    return HeadFactory ?? throw new NullReferenceException("No Factory for VbanPacketHead defined!");
                }
            }
        }
    }
}