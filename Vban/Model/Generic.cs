// ReSharper disable UnusedTypeParameter

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Vban.Model
{
    public interface IBindable<T>
    {
    }

    public interface IBuilder<out T>
    {
        T Build();
    }

    public class ByteArray
    {
        private readonly bool _fixedSize;
        private readonly IList<byte> arr;

        public ByteArray(int length) : this(true, new byte[length]) {}

        public ByteArray(params byte[] bytes) : this(false, bytes)
        {
        }

        public ByteArray(bool fixedSize, params byte[] bytes)
        {
            _fixedSize = fixedSize;
            arr = new List<byte>(bytes);
        }

        public byte[] Bytes => arr.ToArray();
        public int Length => arr.Count;

        public bool Insert(ref int oi, params byte[] bytes)
        {
            if (_fixedSize && oi + bytes.Length > arr.Count)
                return false;
            int rl = bytes.Length;
            int c = 0;
            for (int ri = 0; ri < rl; ri++)
            {
                if (!Set(oi, bytes[ri]))
                    return false;
                oi += 1;
            }
            return true;
        }

        public bool Set(int index, byte byt)
        {
            if (_fixedSize && index > arr.Count + 1)
                return false;
            try
            {
                arr[index] = byt;
            }
            catch (Exception ignored)
            {
                return false;
            }

            return true;
        }

        public bool Append(params byte[] bytes)
        {
            if (_fixedSize)
                return false;
            int i = 0;
            for (; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                try
                {
                    arr.Add(b);
                }
                catch (Exception ignored)
                {
                    return false;
                }
            }
            return true;
        }

        public static implicit operator byte[](ByteArray bytes) => bytes.Bytes;
    }

    public interface IFactory<out T>
    {
        int Counter { get; }
        T Create();
    }

    public interface IIntEnum
    {
        int Value { get; }
    }

    public interface ICharSequence
    {
        int Length { get; }

        char CharAt(int index);

        string Substring(int start, int end);

        string ToString();
    }
}