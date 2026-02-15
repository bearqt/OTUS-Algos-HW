using System.Diagnostics;
using System.Globalization;
using System.Text;

var sizes = new[] { 100, 1_000, 10_000, 100_000, 1_000_000 };
var algorithms = new (string Name, Action<int[]> Sort)[]
{
    ("CountingSort", SortAlgorithms.CountingSort),
    ("RadixSort", SortAlgorithms.RadixSort),
    ("BucketSort", SortAlgorithms.BucketSort)
};

var results = new List<BenchmarkResult>();
var invariant = CultureInfo.InvariantCulture;

foreach (var size in sizes)
{
    var source = GenerateRandomArray(size, 0, 999, seed: 42 + size);

    foreach (var algorithm in algorithms)
    {
        var data = (int[])source.Clone();
        var elapsed = await MeasureAsync(
            () => algorithm.Sort(data),
            timeout: TimeSpan.FromMinutes(2));

        if (elapsed is null)
        {
            results.Add(new BenchmarkResult(size, algorithm.Name, null, TimedOut: true));
            continue;
        }

        if (!IsSorted(data))
        {
            throw new InvalidOperationException(
                $"Algorithm {algorithm.Name} failed to sort array of size {size}.");
        }

        results.Add(new BenchmarkResult(size, algorithm.Name, elapsed.Value.TotalMilliseconds, TimedOut: false));
        Console.WriteLine($"{algorithm.Name} | n={size} | {elapsed.Value.TotalMilliseconds.ToString("F3", invariant)} ms");
    }
}

var markdown = BuildMarkdownTable(results, sizes, algorithms.Select(a => a.Name).ToArray(), invariant);
var outputPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SORTING_BENCHMARK.md");
outputPath = Path.GetFullPath(outputPath);
File.WriteAllText(outputPath, markdown, Encoding.UTF8);

Console.WriteLine();
Console.WriteLine($"Benchmark table written to: {outputPath}");

static async Task<TimeSpan?> MeasureAsync(Action action, TimeSpan timeout)
{
    using var cts = new CancellationTokenSource(timeout);
    var sw = Stopwatch.StartNew();
    var task = Task.Run(action, cts.Token);

    try
    {
        await task.WaitAsync(cts.Token);
        sw.Stop();
        return sw.Elapsed;
    }
    catch (OperationCanceledException)
    {
        return null;
    }
}

static int[] GenerateRandomArray(int size, int minInclusive, int maxInclusive, int seed)
{
    var random = new Random(seed);
    var result = new int[size];

    for (var i = 0; i < result.Length; i++)
    {
        result[i] = random.Next(minInclusive, maxInclusive + 1);
    }

    return result;
}

static bool IsSorted(int[] data)
{
    for (var i = 1; i < data.Length; i++)
    {
        if (data[i] < data[i - 1])
        {
            return false;
        }
    }

    return true;
}

static string BuildMarkdownTable(
    List<BenchmarkResult> results,
    int[] sizes,
    string[] algorithmNames,
    CultureInfo culture)
{
    var sb = new StringBuilder();
    sb.AppendLine("# Сравнение времени сортировок");
    sb.AppendLine();
    sb.AppendLine("Условия:");
    sb.AppendLine("- Массивы случайных чисел в диапазоне `0..999`.");
    sb.AppendLine("- Размеры: `10^2`, `10^3`, `10^4`, `10^5`, `10^6`.");
    sb.AppendLine("- Таймаут на один запуск алгоритма: `2 минуты`.");
    sb.AppendLine();

    sb.Append("| Размер массива | ");
    sb.Append(string.Join(" | ", algorithmNames));
    sb.AppendLine(" |");

    sb.Append("| --- | ");
    sb.Append(string.Join(" | ", algorithmNames.Select(_ => "---")));
    sb.AppendLine(" |");

    foreach (var size in sizes)
    {
        sb.Append($"| {size} | ");

        var rowValues = new List<string>();
        foreach (var algorithm in algorithmNames)
        {
            var item = results.Single(r => r.Size == size && r.Algorithm == algorithm);
            rowValues.Add(item.TimedOut ? "TIMEOUT" : $"{item.Milliseconds!.Value.ToString("F3", culture)} ms");
        }

        sb.Append(string.Join(" | ", rowValues));
        sb.AppendLine(" |");
    }

    sb.AppendLine();
    sb.AppendLine($"Дата запуска: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    return sb.ToString();
}

internal static class SortAlgorithms
{
    public static void CountingSort(int[] data)
    {
        if (data.Length == 0)
        {
            return;
        }

        const int minValue = 0;
        const int maxValue = 999;
        var counts = new int[maxValue - minValue + 1];

        for (var i = 0; i < data.Length; i++)
        {
            counts[data[i] - minValue]++;
        }

        var index = 0;
        for (var value = 0; value < counts.Length; value++)
        {
            var count = counts[value];
            while (count-- > 0)
            {
                data[index++] = value + minValue;
            }
        }
    }

    public static void RadixSort(int[] data)
    {
        if (data.Length == 0)
        {
            return;
        }

        var max = data.Max();
        var output = new int[data.Length];
        var count = new int[10];

        for (var exp = 1; max / exp > 0; exp *= 10)
        {
            Array.Clear(count);

            for (var i = 0; i < data.Length; i++)
            {
                var digit = (data[i] / exp) % 10;
                count[digit]++;
            }

            for (var i = 1; i < count.Length; i++)
            {
                count[i] += count[i - 1];
            }

            for (var i = data.Length - 1; i >= 0; i--)
            {
                var digit = (data[i] / exp) % 10;
                output[--count[digit]] = data[i];
            }

            output.CopyTo(data, 0);
        }
    }

    public static void BucketSort(int[] data)
    {
        if (data.Length == 0)
        {
            return;
        }

        const int minValue = 0;
        const int maxValue = 999;
        const int bucketCount = 10;
        const int bucketSize = (maxValue - minValue + 1) / bucketCount;

        var buckets = new List<int>[bucketCount];
        for (var i = 0; i < bucketCount; i++)
        {
            buckets[i] = new List<int>();
        }

        for (var i = 0; i < data.Length; i++)
        {
            var bucketIndex = Math.Min((data[i] - minValue) / bucketSize, bucketCount - 1);
            buckets[bucketIndex].Add(data[i]);
        }

        var index = 0;
        for (var i = 0; i < bucketCount; i++)
        {
            buckets[i].Sort();
            foreach (var value in buckets[i])
            {
                data[index++] = value;
            }
        }
    }
}

internal sealed record BenchmarkResult(int Size, string Algorithm, double? Milliseconds, bool TimedOut);
