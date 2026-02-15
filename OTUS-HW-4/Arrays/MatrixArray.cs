using System;

namespace OTUS_HW_4.Arrays;

public sealed class MatrixArray<T>(int blockSize = 100) : IDynamicArray<T>
{
    private readonly int _blockSize = blockSize > 0 ? blockSize : throw new ArgumentOutOfRangeException(nameof(blockSize));
    private T[][] _blocks = [];
    private int _length;

    public int Length => _length;

    public T Get(int index)
    {
        ValidateIndex(index);
        var (block, offset) = ToBlockOffset(index);
        return _blocks[block][offset];
    }

    public void Set(T item, int index)
    {
        ValidateIndex(index);
        var (block, offset) = ToBlockOffset(index);
        _blocks[block][offset] = item;
    }

    public void Add(T item)
    {
        EnsureCapacity(_length + 1);
        var (block, offset) = ToBlockOffset(_length);
        _blocks[block][offset] = item;
        _length++;
    }

    public void Add(T item, int index)
    {
        ValidateInsertIndex(index);
        EnsureCapacity(_length + 1);

        for (var i = _length; i > index; i--)
        {
            SetUnchecked(i, GetUnchecked(i - 1));
        }

        SetUnchecked(index, item);
        _length++;
    }

    public T Remove(int index)
    {
        ValidateIndex(index);

        var removed = GetUnchecked(index);
        for (var i = index; i < _length - 1; i++)
        {
            SetUnchecked(i, GetUnchecked(i + 1));
        }

        _length--;
        SetUnchecked(_length, default!);
        TrimEmptyBlocks();
        return removed;
    }

    private T GetUnchecked(int index)
    {
        var (block, offset) = ToBlockOffset(index);
        return _blocks[block][offset];
    }

    private void SetUnchecked(int index, T value)
    {
        var (block, offset) = ToBlockOffset(index);
        _blocks[block][offset] = value;
    }

    private (int block, int offset) ToBlockOffset(int index)
    {
        return (index / _blockSize, index % _blockSize);
    }

    private void EnsureCapacity(int required)
    {
        var requiredBlocks = (required + _blockSize - 1) / _blockSize;
        if (requiredBlocks <= _blocks.Length)
        {
            return;
        }

        var newBlocks = new T[requiredBlocks][];
        Array.Copy(_blocks, newBlocks, _blocks.Length);

        for (var i = _blocks.Length; i < requiredBlocks; i++)
        {
            newBlocks[i] = new T[_blockSize];
        }

        _blocks = newBlocks;
    }

    private void TrimEmptyBlocks()
    {
        var requiredBlocks = (_length + _blockSize - 1) / _blockSize;
        if (requiredBlocks == _blocks.Length)
        {
            return;
        }

        var newBlocks = new T[requiredBlocks][];
        Array.Copy(_blocks, newBlocks, requiredBlocks);
        _blocks = newBlocks;
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

