using System;
using System.Collections;

namespace OTUS_HW_4.Arrays;

public sealed class ArrayListWrapper<T> : IDynamicArray<T>
{
    private readonly ArrayList _list = new();

    public int Length => _list.Count;

    public T Get(int index)
    {
        ValidateIndex(index);
        return (T)_list[index]!;
    }

    public void Set(T item, int index)
    {
        ValidateIndex(index);
        _list[index] = item;
    }

    public void Add(T item)
    {
        _list.Add(item);
    }

    public void Add(T item, int index)
    {
        ValidateInsertIndex(index);
        _list.Insert(index, item);
    }

    public T Remove(int index)
    {
        ValidateIndex(index);
        var removed = (T)_list[index]!;
        _list.RemoveAt(index);
        return removed;
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    private void ValidateInsertIndex(int index)
    {
        if (index < 0 || index > _list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}

