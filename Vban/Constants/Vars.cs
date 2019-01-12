namespace Vban.Constants
{
    public enum SampleRate
    {
        Hz6000   = 0,
        Hz12000  = 1,
        Hz24000  = 2,
        Hz48000  = 3,
        Hz96000  = 4,
        Hz192000 = 5,
        Hz384000 = 6,

        Hz8000   = 7,
        Hz16000  = 8,
        Hz32000  = 9,
        Hz64000  = 10,
        Hz128000 = 11,
        Hz256000 = 12,
        Hz512000 = 13,

        Hz11025  = 14,
        Hz22050  = 15,
        Hz44100  = 16,
        Hz88200  = 17,
        Hz176400 = 18,
        Hz352800 = 19,
        Hz705600 = 20,

        Default = Hz44100
    }

    public enum Protocol
    {
        Audio   = 0x00,
        Serial  = 0x20,
        Text    = 0x40,
        Service = 0x60,

        Default = Audio
    }

    public enum Format
    {
        Byte8   = 0x00,
        Int16   = 0x01,
        Int24   = 0x02,
        Int32   = 0x03,
        Float32 = 0x04,
        Float64 = 0x05,
        Bits12  = 0x06,
        Bits10  = 0x07,

        Default = Int16
    }

    public enum Codec
    {
        Pcm  = 0x00,
        Vbca = 0x10, // VB-Audio AOIP Codec
        Vbcv = 0x20, // VB-Audio VOIP Codec
        User = 0xF0,

        Default = Pcm
    }
}