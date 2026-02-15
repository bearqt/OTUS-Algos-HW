using System.Diagnostics;
using System.Text;

var sizes = new[] { 100, 1_000, 10_000, 100_000, 1_000_000 };
var algorithms = new (string Name, Action<int[], TimeSpan> Sort)[]
{
    ("QuickSort", SortingAlgorithms.QuickSort),
    ("MergeSort", SortingAlgorithms.MergeSort),
};

var random = new Random(42);
var timeout = TimeSpan.FromMinutes(2);
var results = new List<BenchmarkResult>();

foreach (var size in sizes)
{
    Console.WriteLine($"Preparing array: {size:N0}");
    var source = CreateRandomArray(size, random);

    foreach (var algorithm in algorithms)
    {
        Console.WriteLine($"Running {algorithm.Name} for n={size:N0}...");
        var testArray = (int[])source.Clone();
        var outcome = Benchmark(algorithm.Sort, testArray, timeout);
        results.Add(new BenchmarkResult(algorithm.Name, size, outcome));
    }
}

var markdownPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SortingComparison.md");
markdownPath = Path.GetFullPath(markdownPath);
File.WriteAllText(markdownPath, BuildMarkdown(results, timeout), Encoding.UTF8);

Console.WriteLine();
Console.WriteLine($"Markdown table saved to: {markdownPath}");

static int[] CreateRandomArray(int size, Random random)
{
    var array = new int[size];
    for (var i = 0; i < size; i++)
    {
        array[i] = random.Next();
    }

    return array;
}

static BenchmarkOutcome Benchmark(Action<int[], TimeSpan> sort, int[] input, TimeSpan timeout)
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        sort(input, timeout);
        stopwatch.Stop();
    }
    catch (TimeoutException)
    {
        stopwatch.Stop();
        return new BenchmarkOutcome(false, true, stopwatch.Elapsed);
    }

    var sorted = IsSorted(input);
    return new BenchmarkOutcome(sorted, false, stopwatch.Elapsed);
}

static bool IsSorted(int[] array)
{
    for (var i = 1; i < array.Length; i++)
    {
        if (array[i - 1] > array[i])
        {
            return false;
        }
    }

    return true;
}

static string BuildMarkdown(List<BenchmarkResult> results, TimeSpan timeout)
{
    var sb = new StringBuilder();
    sb.AppendLine("# Сравнение времени сортировки");
    sb.AppendLine();
    sb.AppendLine($"Ограничение по времени на один запуск: **{timeout.TotalMinutes:0} минуты**.");
    sb.AppendLine();
    sb.AppendLine("| Алгоритм | Размер массива | Время | Статус |");
    sb.AppendLine("|---|---:|---:|---|");

    foreach (var result in results
                 .OrderBy(r => r.Algorithm, StringComparer.Ordinal)
                 .ThenBy(r => r.Size))
    {
        var timeText = result.Outcome.TimedOut ? "TIMEOUT" : $"{result.Outcome.Elapsed.TotalMilliseconds:F2} ms";
        var status = result.Outcome.TimedOut
            ? "Timeout"
            : (result.Outcome.IsSorted ? "OK" : "Ошибка сортировки");

        sb.AppendLine($"| {result.Algorithm} | {result.Size} | {timeText} | {status} |");
    }

    return sb.ToString();
}

readonly record struct BenchmarkResult(string Algorithm, int Size, BenchmarkOutcome Outcome);
readonly record struct BenchmarkOutcome(bool IsSorted, bool TimedOut, TimeSpan Elapsed);

static class SortingAlgorithms
{
    public static void QuickSort(int[] array, TimeSpan timeout)
    {
        if (array.Length <= 1)
        {
            return;
        }

        var start = Stopwatch.StartNew();
        QuickSortInternal(array, 0, array.Length - 1, start, timeout);
    }

    public static void MergeSort(int[] array, TimeSpan timeout)
    {
        if (array.Length <= 1)
        {
            return;
        }

        var start = Stopwatch.StartNew();
        var buffer = new int[array.Length];
        MergeSortInternal(array, buffer, 0, array.Length - 1, start, timeout);
    }

    private static void QuickSortInternal(int[] array, int left, int right, Stopwatch start, TimeSpan timeout)
    {
        ThrowIfTimeout(start, timeout);

        var i = left;
        var j = right;
        var pivot = array[left + ((right - left) / 2)];

        while (i <= j)
        {
            while (array[i] < pivot)
            {
                i++;
            }

            while (array[j] > pivot)
            {
                j--;
            }

            if (i <= j)
            {
                (array[i], array[j]) = (array[j], array[i]);
                i++;
                j--;
            }
        }

        if (left < j)
        {
            QuickSortInternal(array, left, j, start, timeout);
        }

        if (i < right)
        {
            QuickSortInternal(array, i, right, start, timeout);
        }
    }

    private static void MergeSortInternal(int[] array, int[] buffer, int left, int right, Stopwatch start, TimeSpan timeout)
    {
        ThrowIfTimeout(start, timeout);

        if (left >= right)
        {
            return;
        }

        var middle = left + ((right - left) / 2);
        MergeSortInternal(array, buffer, left, middle, start, timeout);
        MergeSortInternal(array, buffer, middle + 1, right, start, timeout);
        Merge(array, buffer, left, middle, right);
    }

    private static void Merge(int[] array, int[] buffer, int left, int middle, int right)
    {
        var i = left;
        var j = middle + 1;
        var k = left;

        while (i <= middle && j <= right)
        {
            if (array[i] <= array[j])
            {
                buffer[k++] = array[i++];
            }
            else
            {
                buffer[k++] = array[j++];
            }
        }

        while (i <= middle)
        {
            buffer[k++] = array[i++];
        }

        while (j <= right)
        {
            buffer[k++] = array[j++];
        }

        for (var index = left; index <= right; index++)
        {
            array[index] = buffer[index];
        }
    }

    private static void ThrowIfTimeout(Stopwatch start, TimeSpan timeout)
    {
        if (start.Elapsed > timeout)
        {
            throw new TimeoutException("Sorting timed out.");
        }
    }
}
