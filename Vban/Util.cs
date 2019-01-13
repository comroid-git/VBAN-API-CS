using System;

namespace Vban
{
    public class Util
    {
        public static byte[] AppendByteArray(byte[] src, params byte[] trg)
        {
            int    tl = src.Length + trg.Length;
            byte[] nw = new byte[tl];
            Array.Copy(src, nw, src.Length);
            Array.Copy(trg, 0,  nw, src.Length, trg.Length);
            return nw;
        }

        public static int CheckRange(int from, int to, int check)
        {
            if (check < from || check > to)
                throw new ArgumentOutOfRangeException(
                        string.Format("Integer out of range. [%d;%d;%d]", from, check, to));
            return check;
        }

        public static byte[] ToByteArray(string txt)
        {
            byte[] bytes = new byte[txt.Length];
            char[] chars = txt.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                bytes[i] = (byte) chars[i];
            return bytes;
        }

        public static byte[] TrimArray(byte[] bytes, int size)
        {
            bool app = bytes.Length < size;
            byte[] nw = new byte[size];
            Array.Copy(bytes, nw, app ? bytes.Length : size);
            if (app) Array.Fill(nw, (byte) 0, bytes.Length, size - bytes.Length);
            return nw;
        }

        public static byte[] IntToByteArray(int integer, int size)
        {
            return TrimArray(BitConverter.GetBytes(size), size);
        }

        public static byte[] GetBytes(object o)
        {
            if (o is string) return ToByteArray((string) o);
            return ToByteArray(o.ToString());
        }
    }
}