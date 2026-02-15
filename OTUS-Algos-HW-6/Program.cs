
using System.Diagnostics;
using System.Text;

var sizes = new[] { 100, 1_000, 10_000 };
var random = new Random(42);

var results = new List<SortBenchmarkResult>();

foreach (var size in sizes)
{
    var source = CreateRandomArray(size, random);

    results.Add(Measure("BubbleSort", size, source, BubbleSort));
    results.Add(Measure("InsertionSort", size, source, InsertionSort));
    results.Add(Measure("ShellSort", size, source, ShellSort));
}

var markdown = BuildMarkdownTable(results);
File.WriteAllText("SortingBenchmark.md", markdown, Encoding.UTF8);

Console.WriteLine(markdown);

static int[] CreateRandomArray(int size, Random random)
{
    var array = new int[size];
    for (var i = 0; i < size; i++)
    {
        array[i] = random.Next(0, 100_000);
    }

    return array;
}

static SortBenchmarkResult Measure(string algorithm, int size, int[] source, Action<int[]> sort)
{
    var working = (int[])source.Clone();
    var sw = Stopwatch.StartNew();
    sort(working);
    sw.Stop();

    if (!IsSorted(working))
    {
        throw new InvalidOperationException($"{algorithm} failed for size {size}.");
    }

    return new SortBenchmarkResult(algorithm, size, sw.Elapsed.TotalMilliseconds);
}

static bool IsSorted(int[] array)
{
    for (var i = 1; i < array.Length; i++)
    {
        if (array[i] < array[i - 1])
        {
            return false;
        }
    }

    return true;
}

static void BubbleSort(int[] array)
{
    for (var i = 0; i < array.Length - 1; i++)
    {
        var swapped = false;
        for (var j = 0; j < array.Length - 1 - i; j++)
        {
            if (array[j] > array[j + 1])
            {
                (array[j], array[j + 1]) = (array[j + 1], array[j]);
                swapped = true;
            }
        }

        if (!swapped)
        {
            break;
        }
    }
}

static void InsertionSort(int[] array)
{
    for (var i = 1; i < array.Length; i++)
    {
        var key = array[i];
        var j = i - 1;

        while (j >= 0 && array[j] > key)
        {
            array[j + 1] = array[j];
            j--;
        }

        array[j + 1] = key;
    }
}

static void ShellSort(int[] array)
{
    for (var gap = array.Length / 2; gap > 0; gap /= 2)
    {
        for (var i = gap; i < array.Length; i++)
        {
            var temp = array[i];
            var j = i;

            while (j >= gap && array[j - gap] > temp)
            {
                array[j] = array[j - gap];
                j -= gap;
            }

            array[j] = temp;
        }
    }
}

static string BuildMarkdownTable(List<SortBenchmarkResult> results)
{
    var sb = new StringBuilder();
    sb.AppendLine("# Сравнение алгоритмов сортировки");
    sb.AppendLine();
    sb.AppendLine("| Алгоритм | Размер массива | Время, мс |");
    sb.AppendLine("|---|---:|---:|");

    foreach (var row in results
                 .OrderBy(r => r.Algorithm)
                 .ThenBy(r => r.Size))
    {
        sb.AppendLine($"| {row.Algorithm} | {row.Size} | {row.ElapsedMs:F3} |");
    }

    return sb.ToString();
}

internal readonly record struct SortBenchmarkResult(
    string Algorithm,
    int Size,
    double ElapsedMs);
