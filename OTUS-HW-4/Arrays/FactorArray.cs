using System;

namespace OTUS_HW_4.Arrays;

public sealed class FactorArray<T>(int factor = 2, int initialCapacity = 10) : IDynamicArray<T>
{
    private readonly int _factor = factor > 1 ? factor : throw new ArgumentOutOfRangeException(nameof(factor));
    private T[] _array = new T[Math.Max(1, initialCapacity)];
    private int _length;

    public int Length => _length;

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
        EnsureCapacity(_length + 1);
        _array[_length++] = item;
    }

    public void Add(T item, int index)
    {
        ValidateInsertIndex(index);
        EnsureCapacity(_length + 1);

        for (var i = _length; i > index; i--)
        {
            _array[i] = _array[i - 1];
        }

        _array[index] = item;
        _length++;
    }

    public T Remove(int index)
    {
        ValidateIndex(index);

        var removed = _array[index];
        for (var i = index; i < _length - 1; i++)
        {
            _array[i] = _array[i + 1];
        }

        _length--;
        _array[_length] = default!;
        return removed;
    }

    private void EnsureCapacity(int required)
    {
        if (required <= _array.Length)
        {
            return;
        }

        var newCapacity = _array.Length * _factor;
        while (newCapacity < required)
        {
            newCapacity *= _factor;
        }

        var newArray = new T[newCapacity];
        Array.Copy(_array, newArray, _length);
        _array = newArray;
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    private void ValidateInsertIndex(int index)
    {
        if (index < 0 || index > _length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}

