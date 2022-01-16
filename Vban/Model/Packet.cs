using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable 693

namespace Vban.Model
{
    // ReSharper disable once InconsistentNaming
    public class VBANPacket<T> : IByteArray
    {
        public const int MaxSize = 1436;
        public static readonly int MaxSizeWithoutHead = MaxSize - VBANPacketHead<T>.Size;

        protected readonly UnfinishedByteArray UnfinishedByteArray;
        private bool _hasData;

        internal VBANPacket(VBANPacketHead<T> head)
        {
            Head = head;
            UnfinishedByteArray = new UnfinishedByteArray(MaxSize, true);
        }

        public VBANPacket(VBANPacketHead<T> head, byte[] data)
        {
            Head = head;
            UnfinishedByteArray = new UnfinishedByteArray(MaxSize);

            UnfinishedByteArray.Append(head.Bytes);
            AttachData(data);
        }

        public VBANPacketHead<T> Head { get; }

        public byte[] Data
        {
            set
            {
                if (_hasData)
                    throw new InvalidOperationException("Packet already has data attached");
                AttachData(value);
            }
        }

        public byte[] Bytes => this;

        public static implicit operator byte[](VBANPacket<T> packet)
        {
            return packet.UnfinishedByteArray.Bytes;
        }

        private void AttachData(byte[] data)
        {
            if (data.Length > MaxSize)
                throw new InvalidOperationException(
                    "Data is too large to be sent, must be smaller than " + MaxSize);

            UnfinishedByteArray.Append(data);

            _hasData = true;
        }

        public static Decoded Decode(byte[] bytes)
        {
            return new Decoded(bytes);
        }

        public class Decoded : VBANPacket<object>
        {
            public Decoded(byte[] data) : base(
                VBANPacketHead<T>.Decode(Util.SubArray(data, 0, VBANPacketHead<T>.Size)),
                Util.SubArray(data, VBANPacketHead<T>.Size + 1, VBANPacket<T>.MaxSize)
            )
            {
            }

            public new VBANPacketHead<T>.Decoded Head => (VBANPacketHead<T>.Decoded)base.Head;
        }

        public class Factory<T, TS> : IFactory<VBANPacket<T>> where T : TS
        {
            private readonly VBANPacketHead<T>.Factory<T, TS> _headFactory;

            private Factory(VBANPacketHead<T>.Factory<T, TS> headFactory)
            {
                _headFactory = headFactory;
            }

            public int Counter => _headFactory.Counter;

            public VBANPacket<T> Create()
            {
                return new VBANPacket<T>(_headFactory.Create());
            }

            [Obsolete]
            public static Builder<T, TS> CreateBuilder(VBAN.Protocol<T> protocol)
            {
                return new Builder<T, TS>(protocol);
            }

            public class Builder<T, TS> : IBuilder<Factory<T, TS>> where T : TS
            {
                public Builder(VBAN.Protocol<T> protocol)
                {
                    Protocol = protocol;
                }

                public VBAN.Protocol<T> Protocol { get; }
                public VBANPacketHead<T>.Factory<T, TS> HeadFactory { get; set; }

                public Factory<T, TS> Build()
                {
                    if (HeadFactory == null)
                        throw new InvalidOperationException("No head factory defined");
                    
                    return new Factory<T, TS>(HeadFactory);
                }
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    public class VBANPacketHead<T> : IByteArray
    {
        public static readonly int Size = 28;

        private readonly UnfinishedByteArray _unfinishedByteArray;

        internal VBANPacketHead(byte[] bytes)
        {
            _unfinishedByteArray = new UnfinishedByteArray(Size);
            _unfinishedByteArray.Append(bytes);
        }

        public VBANPacketHead(
            int protocol,
            int sampleRateIndex,
            int samples,
            int channel,
            int format,
            int codec,
            string streamName,
            int frameCounter
        )
        {
            Util.CheckRange(samples, 0, 255);
            Util.CheckRange(channel, 0, 255);

            _unfinishedByteArray = new UnfinishedByteArray(Size, true);

            _unfinishedByteArray.Append(Encoding.ASCII.GetBytes("VBAN"));
            _unfinishedByteArray.Append((byte)(protocol | sampleRateIndex));
            _unfinishedByteArray.Append((byte)samples, (byte)channel);
            _unfinishedByteArray.Append((byte)(format | codec));
            _unfinishedByteArray.Append(Util.TrimArray(Encoding.ASCII.GetBytes(streamName), 16));
            _unfinishedByteArray.Append(BitConverter.GetBytes(frameCounter));
        }

        public byte[] Bytes => this;

        public static implicit operator byte[](VBANPacketHead<T> packet)
        {
            return packet._unfinishedByteArray.Bytes;
        }

        public static VBANPacketHead<object> Decode(byte[] bytes)
        {
            return new Decoded(bytes);
        }

        public class Decoded : VBANPacketHead<object>
        {
            [SuppressMessage("ReSharper", "RedundantCaseLabel")]
            internal Decoded(byte[] bytes) : base(bytes)
            {
                if (bytes.Length > Size)
                    throw new InvalidOperationException(
                        "Bytearray is too large, must be exactly " + Size + " bytes long!");

                if (bytes[0] != 'V' || bytes[1] != 'B' || bytes[2] != 'A' || bytes[3] != 'N')
                    throw new InvalidPacketAttributeException(
                        "Invalid packet head: First bytes must be 'VBAN' [rcv='"
                        + Encoding.ASCII.GetString(Util.SubArray(bytes, 0, 4)) + "']");

                int protocolInt = bytes[4] & 0b11111000;
                Protocol = VBAN.Protocol<T>.ByValue(protocolInt);

                // throw exception if protocol is SERVICE
                if (Protocol.IsService)
                    throw new InvalidOperationException("Service Subprotocol is not supported!");

                int dataRateInt = bytes[4] & 0b00000111;
                switch (Protocol.Value)
                {
                    case 0x00: // AUDIO
                        DataRateValue = VBAN.SampleRate.ByValue(dataRateInt);
                        break;
                    case 0x20: // SERIAL
                    case 0x40: // TEXT
                        DataRateValue = VBAN.BitsPerSecond.ByValue(dataRateInt);
                        break;
                    case 0x60: // SERVICE
                    default:
                        // to avoid compiler warning, set to null.
                        // service protocol is not supported
                        DataRateValue = null;
                        break;
                }

                // +1 to avoid indexed counting
                Samples = bytes[5] + 1;
                Channel = bytes[6] + 1;

                int formatInt = bytes[7] & 0b00011111;
                switch (Protocol.Value)
                {
                    case 0x00: // AUDIO
                        Format = VBAN.AudioFormat.ByValue(formatInt);
                        break;
                    case 0x20: // SERIAL
                        Format = VBAN.Format.ByValue(formatInt);
                        break;
                    case 0x40: // TEXT
                        Format = VBAN.CommandFormat.ByValue(formatInt);
                        break;
                    case 0x60: // SERVICE
                    default:
                        // to avoid compiler warning, set to null.
                        // service protocol is not supported
                        Format = null;
                        break;
                }

                // reserved bit
                int unused = bytes[7] & 0b11101111;

                int codecInt = bytes[7] & 0b11110000;

                switch (codecInt)
                {
                    case VBAN.Codec.PCM:
                    case VBAN.Codec.VBCA:
                    case VBAN.Codec.VBCV:
                    case VBAN.Codec.USER:
                        //noinspection MagicConstant
                        Codec = codecInt;
                        break;
                    default:
                        throw new InvalidPacketAttributeException(
                            "Invalid Codec selector: " + Convert.ToString(codecInt, 16));
                }

                var nameBytes = new byte[16];
                Array.Copy(bytes, 8, nameBytes, 0, 16);
                StreamName = Util.BytesToString(nameBytes, Encoding.ASCII);

                var frameBytes = new byte[4];
                Array.Copy(bytes, 24, frameBytes, 0, 4);
                Frame = BitConverter.ToInt32(frameBytes);
            }

            public VBAN.AnyProtocol Protocol { get; }
            public AnyDataRateValue DataRateValue { get; }
            public int Samples { get; }
            public int Channel { get; }
            public AnyFormatValue Format { get; }
            public int Codec { get; }
            public string StreamName { get; }
            public int Frame { get; }
        }

        public class Factory<T, TS> : IFactory<VBANPacketHead<T>> where T : TS
        {
            private Factory(VBAN.Protocol<T> protocol,
                IDataRateValue<TS> sampleRate,
                int samples,
                int channel,
                IFormatValue<TS> format,
                int codec,
                string streamName)
            {
                Protocol = protocol.Value;
                SampleRate = sampleRate.Value;
                Samples = samples;
                Channel = channel;
                Format = format.Value;
                Codec = codec;
                StreamName = streamName;

                Counter = 0;
            }

            public int Protocol { get; }
            public int SampleRate { get; }
            public int Samples { get; }
            public int Channel { get; }
            public int Format { get; }
            public int Codec { get; }
            public string StreamName { get; }
            public int Counter { get; private set; }

            public VBANPacketHead<T> Create()
            {
                return new VBANPacketHead<T>(Protocol, SampleRate, Samples, Channel, Format, Codec,
                    StreamName, Counter++);
            }

            [Obsolete]
            public static Builder<T, TS> CreateBuilder(VBAN.Protocol<T> protocol)
            {
                return new Builder<T, TS>(protocol);
            }

            [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
            [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
            public class Builder<T, TS> : IBuilder<Factory<T, TS>> where T : TS
            {
                public Builder(VBAN.Protocol<T> protocol)
                {
                    Protocol = protocol;

                    switch (Protocol.Value)
                    {
                        case 0x00:
                            SampleRate = VBAN.SampleRate.Hz48000 as IDataRateValue<TS>;
                            Samples = 255;
                            Channel = 2;
                            Format = VBAN.AudioFormat.Int16 as IFormatValue<TS>;
                            StreamName = "Stream1";
                            return;
                        case 0x20:
                            StreamName = "MIDI1";
                            break;
                        case 0x40:
                            SampleRate = VBAN.BitsPerSecond.Bps256000 as IDataRateValue<TS>;
                            Samples = 0;
                            Channel = 0;
                            Format = VBAN.Format.Byte8 as IFormatValue<TS>;
                            // if because we are in a shared branch
                            if (StreamName == null) StreamName = "Command1";
                            return;
                        case 0x60:
                            // SERVICE protocol is not supported
                            break;
                        default:
                            throw new InvalidOperationException("Unknown Protocol: " + protocol);
                    }
                }

                public VBAN.Protocol<T> Protocol { get; }
                public IDataRateValue<TS> SampleRate { get; set; }
                public int Samples { get; set; }
                public int Channel { get; set; }
                public IFormatValue<TS> Format { get; set; }
                public int Codec { get; set; }
                public string StreamName { get; set; }

                public Factory<T, TS> Build()
                {
                    if (Protocol == null)
                        throw new InvalidOperationException("No protocol defined");
                    if (SampleRate == null)
                        throw new InvalidOperationException("No sample rate defined");
                    if (Format == null)
                        throw new InvalidOperationException("No format defined");

                    return new Factory<T, TS>(Protocol, SampleRate, Samples, Channel, Format, Codec,
                        StreamName);
                }
            }
        }
    }
}