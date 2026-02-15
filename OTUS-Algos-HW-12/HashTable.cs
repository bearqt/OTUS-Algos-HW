public sealed class HashTable<TKey, TValue> where TKey : notnull
{
    private const double LoadFactor = 0.75;

    private Node?[] _buckets;

    public HashTable(int capacity = 16)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than 0.");
        }

        _buckets = new Node[capacity];
    }

    public int Count { get; private set; }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException($"Key '{key}' was not found.");
        }
        set => AddOrUpdate(key, value);
    }

    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        EnsureCapacity();

        var bucketIndex = GetBucketIndex(key, _buckets.Length);
        var current = _buckets[bucketIndex];

        while (current is not null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                throw new ArgumentException("An item with the same key has already been added.", nameof(key));
            }

            current = current.Next;
        }

        _buckets[bucketIndex] = new Node(key, value, _buckets[bucketIndex]);
        Count++;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        var bucketIndex = GetBucketIndex(key, _buckets.Length);
        var current = _buckets[bucketIndex];

        while (current is not null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                value = current.Value;
                return true;
            }

            current = current.Next;
        }

        value = default!;
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var bucketIndex = GetBucketIndex(key, _buckets.Length);
        Node? previous = null;
        var current = _buckets[bucketIndex];

        while (current is not null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                if (previous is null)
                {
                    _buckets[bucketIndex] = current.Next;
                }
                else
                {
                    previous.Next = current.Next;
                }

                Count--;
                return true;
            }

            previous = current;
            current = current.Next;
        }

        return false;
    }

    private void AddOrUpdate(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        EnsureCapacity();

        var bucketIndex = GetBucketIndex(key, _buckets.Length);
        var current = _buckets[bucketIndex];

        while (current is not null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                current.Value = value;
                return;
            }

            current = current.Next;
        }

        _buckets[bucketIndex] = new Node(key, value, _buckets[bucketIndex]);
        Count++;
    }

    private void EnsureCapacity()
    {
        if ((Count + 1) <= _buckets.Length * LoadFactor)
        {
            return;
        }

        Resize(_buckets.Length * 2);
    }

    private void Resize(int newSize)
    {
        var newBuckets = new Node[newSize];

        foreach (var bucket in _buckets)
        {
            var current = bucket;
            while (current is not null)
            {
                var next = current.Next;
                var newBucketIndex = GetBucketIndex(current.Key, newSize);

                current.Next = newBuckets[newBucketIndex];
                newBuckets[newBucketIndex] = current;
                current = next;
            }
        }

        _buckets = newBuckets;
    }

    private static int GetBucketIndex(TKey key, int bucketCount)
    {
        var hash = EqualityComparer<TKey>.Default.GetHashCode(key) & int.MaxValue;
        return hash % bucketCount;
    }

    private sealed class Node
    {
        public Node(TKey key, TValue value, Node? next)
        {
            Key = key;
            Value = value;
            Next = next;
        }

        public TKey Key { get; }

        public TValue Value { get; set; }

        public Node? Next { get; set; }
    }
}
