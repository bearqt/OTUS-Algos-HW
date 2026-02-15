namespace OTUS_HW_4.Arrays;

public interface IDynamicArray<T>
{
    int Length { get; }

    T Get(int index);

    void Set(T item, int index);

    void Add(T item);

    void Add(T item, int index);

    T Remove(int index);
}

