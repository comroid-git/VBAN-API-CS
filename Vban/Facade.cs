using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Vban.Model;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
#pragma warning disable 659

namespace Vban
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once ClassNeverInstantiated.Global
    public class VBAN
    {
        public const int DefaultPort = 6980;

        private VBAN()
        {
        }

        public class AnyProtocol
        {
            internal AnyProtocol()
            {
            }

            public int Value { get; internal set; }

            public string Name
            {
                get
                {
                    switch (Value)
                    {
                        case 0x00: return "AUDIO";
                        case 0x20: return "SERIAL";
                        case 0x40: return "TEXT";
                        case 0x60: return "SERVICE";
                        default:
                            throw new InvalidOperationException("Unknown protocol: " + ToString());
                    }
                }
            }

            public bool IsAudio => Value == 0x00;

            public bool IsSerial => Value == 0x20;

            public bool IsText => Value == 0x40;

            public bool IsService => Value == 0x60;

            public override bool Equals(object obj)
            {
                if (obj is AnyProtocol protocol)
                    return protocol.Value == Value;

                return false;
            }

            public override string ToString()
            {
                return $"{Name}-Protocol({Convert.ToString(Value, 16)})";
            }
        }

        public sealed class Protocol<T> : AnyProtocol, IBindable<T>, IIntEnum
        {
            public static readonly Protocol<AudioFrame> Audio =
                new Protocol<AudioFrame>(0x00, AudioFrame.FromBytes);

            public static readonly Protocol<MIDICommand> Serial =
                new Protocol<MIDICommand>(0x20, MIDICommand.FromBytes);

            public static readonly Protocol<string> Text =
                new Protocol<string>(0x40, Encoding.ASCII.GetString);

            public static readonly Protocol<byte[]> Service = new Protocol<byte[]>(0x60, x => x);

            private readonly Func<byte[], T> _mapper;

            private Protocol(int value, Func<byte[], T> mapper)
            {
                _mapper = mapper;
                Value = value;
            }

            internal T CreateDataObject(byte[] bytes)
            {
                return _mapper.Invoke(bytes);
            }

            public static AnyProtocol ByValue(int value)
            {
                switch (value)
                {
                    case 0x00: return Audio;
                    case 0x20: return Serial;
                    case 0x40: return Text;
                    case 0x60: return Service;
                    default:
                        throw new InvalidOperationException(
                            $"Unknown protocol value: {Convert.ToString(value, 16)}");
                }
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        // ReSharper disable once ConvertToStaticClass
        public sealed class Codec
        {
            public const int PCM = 0x00;
            public const int VBCA = 0x10; // VB-Audio AOIP Codec
            public const int VBCV = 0x20; // VB-Audio VOIP Codec
            public const int USER = 0xF0;

            private Codec()
            {
            }
        }

        public sealed class SampleRate : IDataRateValue<AudioFrame>
        {
            public static readonly SampleRate Hz6000 = new SampleRate(0);
            public static readonly SampleRate Hz12000 = new SampleRate(1);
            public static readonly SampleRate Hz24000 = new SampleRate(2);
            public static readonly SampleRate Hz48000 = new SampleRate(3);
            public static readonly SampleRate Hz96000 = new SampleRate(4);
            public static readonly SampleRate Hz192000 = new SampleRate(5);
            public static readonly SampleRate Hz384000 = new SampleRate(6);

            public static readonly SampleRate Hz8000 = new SampleRate(7);
            public static readonly SampleRate Hz16000 = new SampleRate(8);
            public static readonly SampleRate Hz32000 = new SampleRate(9);
            public static readonly SampleRate Hz64000 = new SampleRate(10);
            public static readonly SampleRate Hz128000 = new SampleRate(11);
            public static readonly SampleRate Hz256000 = new SampleRate(12);
            public static readonly SampleRate Hz512000 = new SampleRate(13);

            public static readonly SampleRate Hz11025 = new SampleRate(14);
            public static readonly SampleRate Hz22050 = new SampleRate(15);
            public static readonly SampleRate Hz44100 = new SampleRate(16);
            public static readonly SampleRate Hz88200 = new SampleRate(17);
            public static readonly SampleRate Hz176400 = new SampleRate(18);
            public static readonly SampleRate Hz352800 = new SampleRate(19);
            public static readonly SampleRate Hz705600 = new SampleRate(20);

            private static readonly SampleRate[] Values = new SampleRate[21];

            private SampleRate(int value)
            {
                Value = value;

                Values[value] = this;
            }

            public int Value { get; }

            public static SampleRate ByValue(int value)
            {
                return Values[value];
            }
        }

        public sealed class BitsPerSecond : IDataRateValue<IEnumerable<char>>
        {
            public static readonly BitsPerSecond Bps0 = new BitsPerSecond(0);
            public static readonly BitsPerSecond Bps110 = new BitsPerSecond(1);
            public static readonly BitsPerSecond Bps150 = new BitsPerSecond(2);
            public static readonly BitsPerSecond Bps300 = new BitsPerSecond(3);
            public static readonly BitsPerSecond Bps600 = new BitsPerSecond(4);
            public static readonly BitsPerSecond Bps1200 = new BitsPerSecond(5);
            public static readonly BitsPerSecond Bps2400 = new BitsPerSecond(6);

            public static readonly BitsPerSecond Bps4800 = new BitsPerSecond(7);
            public static readonly BitsPerSecond Bps9600 = new BitsPerSecond(8);
            public static readonly BitsPerSecond Bps14400 = new BitsPerSecond(9);
            public static readonly BitsPerSecond Bps19200 = new BitsPerSecond(10);
            public static readonly BitsPerSecond Bps31250 = new BitsPerSecond(11);
            public static readonly BitsPerSecond Bps38400 = new BitsPerSecond(12);
            public static readonly BitsPerSecond Bps57600 = new BitsPerSecond(13);

            public static readonly BitsPerSecond Bps115200 = new BitsPerSecond(14);
            public static readonly BitsPerSecond Bps128000 = new BitsPerSecond(15);
            public static readonly BitsPerSecond Bps230400 = new BitsPerSecond(16);
            public static readonly BitsPerSecond Bps250000 = new BitsPerSecond(17);
            public static readonly BitsPerSecond Bps256000 = new BitsPerSecond(18);
            public static readonly BitsPerSecond Bps460800 = new BitsPerSecond(19);
            public static readonly BitsPerSecond Bps921600 = new BitsPerSecond(20);

            public static readonly BitsPerSecond Bps1000000 = new BitsPerSecond(21);
            public static readonly BitsPerSecond Bps1500000 = new BitsPerSecond(22);
            public static readonly BitsPerSecond Bps2000000 = new BitsPerSecond(23);
            public static readonly BitsPerSecond Bps3000000 = new BitsPerSecond(24);

            private static readonly BitsPerSecond[] Values = new BitsPerSecond[25];

            private BitsPerSecond(int value)
            {
                Value = value;

                Values[value] = this;
            }

            public int Value { get; }

            public static BitsPerSecond ByValue(int value)
            {
                return Values[value];
            }
        }

        public sealed class AudioFormat : IFormatValue<AudioFrame>
        {
            public static readonly AudioFormat Byte8 = new AudioFormat(0x00);
            public static readonly AudioFormat Int16 = new AudioFormat(0x01);
            public static readonly AudioFormat Int24 = new AudioFormat(0x02);
            public static readonly AudioFormat Int32 = new AudioFormat(0x03);
            public static readonly AudioFormat Float32 = new AudioFormat(0x04);
            public static readonly AudioFormat Float64 = new AudioFormat(0x05);
            public static readonly AudioFormat Bits12 = new AudioFormat(0x06);
            public static readonly AudioFormat Bits10 = new AudioFormat(0x07);

            private static readonly AudioFormat[] Values = new AudioFormat[8];

            private AudioFormat(int value)
            {
                Value = value;

                Values[value] = this;
            }

            public int Value { get; }

            public static AudioFormat ByValue(int value)
            {
                return Values[value];
            }
        }

        public class AnyFormat
        {
        }

        public sealed class CommandFormat<TE> : AnyFormat, IFormatValue<TE> where TE : IEnumerable<char>
        {
            public static readonly CommandFormat<TE> Ascii = new CommandFormat<TE>(0x00);
            public static readonly CommandFormat<TE> Utf8 = new CommandFormat<TE>(0x10);
            public static readonly CommandFormat<TE> Wchar = new CommandFormat<TE>(0x20);

            private static readonly CommandFormat<TE>[] Values = new CommandFormat<TE>[3];
            private static volatile int _vi;

            private CommandFormat(int value)
            {
                Value = value;

                Values[_vi++] = this;
            }

            public int Value { get; }

            public static AnyFormat ByValue(int value)
            {
                foreach (var that in Values)
                    if (that.Value == value)
                        return that;

                throw new InvalidOperationException(
                    "Unknown Format value: " + Convert.ToString(value, 16));
            }
        }

        public sealed class Format<TE> : AnyFormat, IFormatValue<TE> where TE : IByteArray
        {
            public static readonly Format<TE> Byte8 = new Format<TE>(0x00);

            private static readonly Format<TE>[] Values = new Format<TE>[1];
            private static volatile int _vi;

            private Format(int value)
            {
                Value = value;

                Values[_vi++] = this;
            }

            public int Value { get; }

            public static AnyFormat ByValue(int value)
            {
                foreach (var that in Values)
                    if (that.Value == value)
                        return that;

                throw new InvalidOperationException(
                    "Unknown Format value:" + Convert.ToString(value, 16));
            }
        }
    }
}