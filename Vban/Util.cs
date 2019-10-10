using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vban.Model;

namespace Vban
{
    public class Util
    {
        public static byte[] SubArray(byte[] src, int iLow, int iHigh)
        {
            var bytes = new byte[iHigh - iLow];

            Array.Copy(src, iLow, bytes, 0, bytes.Length);

            return bytes;
        }

        public static void CheckRange(int check, int from, int to)
        {
            if (check < from || check > to)
                throw new InvalidOperationException($"Integer out of range. [{from};{check};{to}]");
        }

        public static byte[] TrimArray(byte[] src, int size)
        {
            var bytes = new byte[size];
            Array.Copy(src, bytes, src.Length);
            return bytes;
        }

        public static byte[] IntToByteArray(int integer)
        {
            // ReSharper disable once RedundantCast
            return BitConverter.GetBytes((Int32) integer);
        }

        public static string BytesToString(byte[] bytes, Encoding encoding)
        {
            return bytes.TakeWhile(b => b != 0)
                .Aggregate("", (current, b) => current + encoding.GetChars(new[] {b}));
        }

        public static byte[] CreateByteArray<T>(T data)
        {
            // Must support types: IEnumerable<char>, IByteArray

            if (data is IEnumerable<char> seq)
                return Encoding.UTF8.GetBytes(seq.ToString());
            if (data is IByteArray byteArray)
                return byteArray.Bytes;

            throw new InvalidOperationException("Unknown Data Type! Please contact the developer.");
        }
    }
}