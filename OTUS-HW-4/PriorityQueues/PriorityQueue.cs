using System;
using System.Collections.Generic;

namespace OTUS_HW_4.PriorityQueues;

public sealed class PriorityQueue<T>
{
    private readonly List<Queue<T>> _queues = [];
    private int _count;

    public int Count => _count;

    public void Enqueue(int priority, T item)
    {
        if (priority < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }

        EnsurePriority(priority);
        _queues[priority].Enqueue(item);
        _count++;
    }

    public T Dequeue()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        for (var i = _queues.Count - 1; i >= 0; i--)
        {
            if (_queues[i].Count > 0)
            {
                _count--;
                return _queues[i].Dequeue();
            }
        }

        throw new InvalidOperationException("Queue is empty.");
    }

    private void EnsurePriority(int priority)
    {
        while (_queues.Count <= priority)
        {
            _queues.Add(new Queue<T>());
        }
    }
}

