using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Vban.Model
{
    public class AudioFrame : ByteArray
    {
        private readonly byte[] _bytes;

        private AudioFrame(byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte[] Bytes => this;

        public static AudioFrame FromBytes(byte[] bytes)
        {
            return new AudioFrame(bytes);
        }

        public static implicit operator byte[](AudioFrame frame)
        {
            return frame._bytes;
        }
    }

    // ReSharper disable once InconsistentNaming
    public class MIDICommand : IEnumerable<char>
    {
        private readonly string _str;

        private MIDICommand(string str)
        {
            _str = str;
        }

        public int Length => _str.Length;

        public IEnumerator<char> GetEnumerator()
        {
            return _str.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public char CharAt(int index)
        {
            return _str.ToCharArray()[index];
        }

        public string Substring(int start, int end)
        {
            return _str.Substring(start, end - start);
        }

        public override string ToString()
        {
            return _str;
        }

        public static MIDICommand FromBytes(byte[] bytes)
        {
            return new MIDICommand(Encoding.ASCII.GetString(bytes));
        }
    }
}