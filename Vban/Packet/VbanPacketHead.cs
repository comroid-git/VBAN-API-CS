using System;
using Vban.Constants;
using Vban.Model;
using static Vban.Util;

namespace Vban.Packet
{
    public class VbanPacketHead : IByteArray
    {
        public static readonly int SIZE = 28;

        internal VbanPacketHead(
                Protocol   protocol,
                SampleRate sampleRate,
                int        samples,
                int        channel,
                Format     format,
                Codec      codec,
                string     streamName,
                int        frame)
        {
            byte[] tmp = new byte[0];
            tmp = AppendByteArray(tmp, ToByteArray("VBAN"));
            tmp = AppendByteArray(tmp, (byte) ((int) protocol | (int) sampleRate));
            tmp = AppendByteArray(tmp,
                    (byte) CheckRange(0, 255, samples), (byte) CheckRange(0, 255, channel));
            tmp   = AppendByteArray(tmp, (byte) ((int) format | (int) codec));
            tmp   = AppendByteArray(tmp, TrimArray(ToByteArray(streamName), 16));
            Bytes = AppendByteArray(tmp, IntToByteArray(frame, 4));
        }

        public VbanPacketHead(
                Protocol   protocol,
                SampleRate sampleRate,
                int        samples,
                int        channel,
                Format     format,
                Codec      codec,
                string     streamName,
                int        frame,
                byte[]     data) : this(protocol, sampleRate, samples, channel, format, codec, streamName, frame)
        {
            Bytes = data;
        }

        public byte[] Bytes { get; }

        public static Factory DefaultTextFactory()
        {
            return new Factory(
                    Protocol.Text,
                    SampleRate.Hz176400,
                    0,
                    0,
                    Format.Int16,
                    Codec.Pcm,
                    "Command1"
            );
        }

        public class Factory : IFactory<VbanPacketHead>
        {
            public Factory(
                    Protocol   protocol,
                    SampleRate sampleRate,
                    int        samples,
                    int        channel,
                    Format     format,
                    Codec      codec,
                    string     streamName)
            {
                Protocol   = protocol;
                SampleRate = sampleRate;
                Samples    = samples;
                Channel    = channel;
                Format     = format;
                Codec      = codec;
                StreamName = streamName;
            }

            public Protocol   Protocol   { get; }
            public SampleRate SampleRate { get; }
            public int        Samples    { get; }
            public int        Channel    { get; }
            public Format     Format     { get; }
            public Codec      Codec      { get; }
            public string     StreamName { get; }

            public VbanPacketHead Create()
            {
                return new VbanPacketHead(
                        Protocol,
                        SampleRate,
                        Samples,
                        Channel,
                        Format,
                        Codec,
                        StreamName ?? NameDefault(),
                        Counter++);
            }

            public int Counter { get; private set; }

            private string NameDefault()
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Protocol)
                {
                    case Protocol.Audio:  return "Stream1";
                    case Protocol.Serial: return "MIDI1";
                    case Protocol.Text:   return "Command1";
                    default:
                        // ReSharper disable once NotResolvedInText
                        throw new ArgumentOutOfRangeException("Protocol",
                                "Cannot get default stream name; unknown protocol type.");
                }
            }

            public class Builder : IBuilder<Factory>
            {
                public Builder()
                {
                    Protocol   = Protocol.Default;
                    SampleRate = SampleRate.Default;
                    Samples    = 255;
                    Channel    = 255;
                    Format     = Format.Default;
                    Codec      = Codec.Default;
                }

                public Protocol   Protocol   { get; set; }
                public SampleRate SampleRate { get; set; }
                public int        Samples    { get; set; }
                public int        Channel    { get; set; }
                public Format     Format     { get; set; }
                public Codec      Codec      { get; set; }
                public string     StreamName { get; set; }

                public Factory Build()
                {
                    return new Factory(
                            Protocol,
                            SampleRate,
                            Samples,
                            Channel,
                            Format,
                            Codec,
                            StreamName);
                }
            }
        }
    }
}