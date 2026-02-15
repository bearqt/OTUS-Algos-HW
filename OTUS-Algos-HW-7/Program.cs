using System;

var source = new[] { 64, 25, 12, 22, 11, 90, 3 };

var selectionSorted = (int[])source.Clone();
SortAlgorithms.SelectionSort(selectionSorted);

var heapSorted = (int[])source.Clone();
SortAlgorithms.HeapSort(heapSorted);

Console.WriteLine($"Original:      {string.Join(", ", source)}");
Console.WriteLine($"SelectionSort: {string.Join(", ", selectionSorted)}");
Console.WriteLine($"HeapSort:      {string.Join(", ", heapSorted)}");

static class SortAlgorithms
{
    public static void SelectionSort(int[] array)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        for (int i = 0; i < array.Length - 1; i++)
        {
            int minIndex = i;

            for (int j = i + 1; j < array.Length; j++)
            {
                if (array[j] < array[minIndex])
                {
                    minIndex = j;
                }
            }

            if (minIndex != i)
            {
                Swap(array, i, minIndex);
            }
        }
    }

    public static void HeapSort(int[] array)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        int n = array.Length;

        for (int i = n / 2 - 1; i >= 0; i--)
        {
            Heapify(array, n, i);
        }

        for (int i = n - 1; i > 0; i--)
        {
            Swap(array, 0, i);
            Heapify(array, i, 0);
        }
    }

    private static void Heapify(int[] array, int heapSize, int rootIndex)
    {
        int largest = rootIndex;
        int left = 2 * rootIndex + 1;
        int right = 2 * rootIndex + 2;

        if (left < heapSize && array[left] > array[largest])
        {
            largest = left;
        }

        if (right < heapSize && array[right] > array[largest])
        {
            largest = right;
        }

        if (largest != rootIndex)
        {
            Swap(array, rootIndex, largest);
            Heapify(array, heapSize, largest);
        }
    }

    private static void Swap(int[] array, int i, int j)
    {
        (array[i], array[j]) = (array[j], array[i]);
    }
}
