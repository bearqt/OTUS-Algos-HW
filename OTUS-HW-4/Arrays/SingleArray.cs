using System;

namespace OTUS_HW_4.Arrays;

public sealed class SingleArray<T> : IDynamicArray<T>
{
    private T[] _array = [];

    public int Length => _array.Length;

    public T Get(int index)
    {
        ValidateIndex(index);
        return _array[index];
    }

    public void Set(T item, int index)
    {
        ValidateIndex(index);
        _array[index] = item;
    }

    public void Add(T item)
    {
        Resize(_array.Length + 1);
        _array[^1] = item;
    }

    public void Add(T item, int index)
    {
        ValidateInsertIndex(index);
        Resize(_array.Length + 1);

        for (var i = _array.Length - 1; i > index; i--)
        {
            _array[i] = _array[i - 1];
        }

        _array[index] = item;
    }

    public T Remove(int index)
    {
        ValidateIndex(index);

        var removed = _array[index];
        for (var i = index; i < _array.Length - 1; i++)
        {
            _array[i] = _array[i + 1];
        }

        Resize(_array.Length - 1);
        return removed;
    }

    private void Resize(int newLength)
    {
        var newArray = new T[newLength];
        Array.Copy(_array, newArray, Math.Min(_array.Length, newLength));
        _array = newArray;
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    private void ValidateInsertIndex(int index)
    {
        if (index < 0 || index > _array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}

