using System;
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
            public static readonly Protocol<AudioFrame> Audio = new(0x00, AudioFrame.FromBytes);

            public static readonly Protocol<MIDICommand> Serial = new(0x20, MIDICommand.FromBytes);

            public static readonly Protocol<string> Text = new(0x40, Encoding.ASCII.GetString);

            public static readonly Protocol<byte[]> Service = new(0x60, x => x);

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
            public static readonly SampleRate Hz6000 = new(0);
            public static readonly SampleRate Hz12000 = new(1);
            public static readonly SampleRate Hz24000 = new(2);
            public static readonly SampleRate Hz48000 = new(3);
            public static readonly SampleRate Hz96000 = new(4);
            public static readonly SampleRate Hz192000 = new(5);
            public static readonly SampleRate Hz384000 = new(6);

            public static readonly SampleRate Hz8000 = new(7);
            public static readonly SampleRate Hz16000 = new(8);
            public static readonly SampleRate Hz32000 = new(9);
            public static readonly SampleRate Hz64000 = new(10);
            public static readonly SampleRate Hz128000 = new(11);
            public static readonly SampleRate Hz256000 = new(12);
            public static readonly SampleRate Hz512000 = new(13);

            public static readonly SampleRate Hz11025 = new(14);
            public static readonly SampleRate Hz22050 = new(15);
            public static readonly SampleRate Hz44100 = new(16);
            public static readonly SampleRate Hz88200 = new(17);
            public static readonly SampleRate Hz176400 = new(18);
            public static readonly SampleRate Hz352800 = new(19);
            public static readonly SampleRate Hz705600 = new(20);

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
            public static readonly BitsPerSecond Bps0 = new(0);
            public static readonly BitsPerSecond Bps110 = new(1);
            public static readonly BitsPerSecond Bps150 = new(2);
            public static readonly BitsPerSecond Bps300 = new(3);
            public static readonly BitsPerSecond Bps600 = new(4);
            public static readonly BitsPerSecond Bps1200 = new(5);
            public static readonly BitsPerSecond Bps2400 = new(6);

            public static readonly BitsPerSecond Bps4800 = new(7);
            public static readonly BitsPerSecond Bps9600 = new(8);
            public static readonly BitsPerSecond Bps14400 = new(9);
            public static readonly BitsPerSecond Bps19200 = new(10);
            public static readonly BitsPerSecond Bps31250 = new(11);
            public static readonly BitsPerSecond Bps38400 = new(12);
            public static readonly BitsPerSecond Bps57600 = new(13);

            public static readonly BitsPerSecond Bps115200 = new(14);
            public static readonly BitsPerSecond Bps128000 = new(15);
            public static readonly BitsPerSecond Bps230400 = new(16);
            public static readonly BitsPerSecond Bps250000 = new(17);
            public static readonly BitsPerSecond Bps256000 = new(18);
            public static readonly BitsPerSecond Bps460800 = new(19);
            public static readonly BitsPerSecond Bps921600 = new(20);

            public static readonly BitsPerSecond Bps1000000 = new(21);
            public static readonly BitsPerSecond Bps1500000 = new(22);
            public static readonly BitsPerSecond Bps2000000 = new(23);
            public static readonly BitsPerSecond Bps3000000 = new(24);

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
            public static readonly AudioFormat Byte8 = new(0x00);
            public static readonly AudioFormat Int16 = new(0x01);
            public static readonly AudioFormat Int24 = new(0x02);
            public static readonly AudioFormat Int32 = new(0x03);
            public static readonly AudioFormat Float32 = new(0x04);
            public static readonly AudioFormat Float64 = new(0x05);
            public static readonly AudioFormat Bits12 = new(0x06);
            public static readonly AudioFormat Bits10 = new(0x07);

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

        public sealed class CommandFormat : AnyFormat, IFormatValue<IEnumerable<char>>
        {
            public static readonly CommandFormat Ascii = new(0x00);
            public static readonly CommandFormat Utf8 = new(0x10);
            public static readonly CommandFormat Wchar = new(0x20);

            private static readonly CommandFormat[] Values = new CommandFormat[3];
            private static volatile int _vi;

            private CommandFormat(int value)
            {
                Value = value;

                Values[_vi++] = this;
            }

            public int Value { get; }

            public static AnyFormatValue ByValue(int value)
            {
                foreach (var that in Values)
                    if (that.Value == value)
                        return that;

                throw new InvalidOperationException(
                    "Unknown Format value: " + Convert.ToString(value, 16));
            }
        }

        public sealed class Format : AnyFormat, IFormatValue<IByteArray>
        {
            public static readonly Format Byte8 = new(0x00);

            private static readonly Format[] Values = new Format[1];
            private static volatile int _vi;

            private Format(int value)
            {
                Value = value;

                Values[_vi++] = this;
            }

            public int Value { get; }

            public static AnyFormatValue ByValue(int value)
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