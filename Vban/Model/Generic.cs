// ReSharper disable UnusedTypeParameter

namespace Vban.Model
{
    public interface IBindable<T>
    {
    }

    public interface IBuilder<out T>
    {
        T Build();
    }

    public interface IByteArray
    {
        byte[] Bytes { get; }
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