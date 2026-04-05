using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var algorithms = new[]
{
    new SearchAlgorithm("Полный перебор", StringSearchAlgorithms.NaiveSearch),
    new SearchAlgorithm("КМП (сдвиги по префиксу)", StringSearchAlgorithms.KnuthMorrisPrattSearch),
    new SearchAlgorithm("Хорспул (сдвиги по суффиксу текста)", StringSearchAlgorithms.BoyerMooreHorspoolSearch),
    new SearchAlgorithm("Бойер-Мур", StringSearchAlgorithms.BoyerMooreSearch)
};

PrintSection("Проверка корректности");
var isCorrect = RunCorrectnessTests(algorithms, BuildCorrectnessCases());

PrintSection("Сравнение на разных данных");
RunBenchmarks(algorithms, BuildBenchmarkCases());

Environment.ExitCode = isCorrect ? 0 : 1;

static bool RunCorrectnessTests(
    IReadOnlyList<SearchAlgorithm> algorithms,
    IReadOnlyList<SearchCase> testCases)
{
    var allPassed = true;

    foreach (var testCase in testCases)
    {
        var expectedIndex = testCase.ExpectedIndex
            ?? testCase.Text.IndexOf(testCase.Pattern, StringComparison.Ordinal);

        Console.WriteLine(testCase.Name);
        Console.WriteLine($"  text:    \"{Truncate(testCase.Text, 60)}\"");
        Console.WriteLine($"  pattern: \"{Truncate(testCase.Pattern, 30)}\"");
        Console.WriteLine($"  expected index: {expectedIndex}");

        foreach (var algorithm in algorithms)
        {
            var result = algorithm.Search(testCase.Text, testCase.Pattern);
            var passed = result.Index == expectedIndex;

            Console.WriteLine(
                $"  {algorithm.Name,-38} -> index = {result.Index,4}, comparisons = {result.Comparisons,8}, status = {(passed ? "OK" : "FAIL")}");

            allPassed &= passed;
        }

        Console.WriteLine();
    }

    Console.WriteLine(allPassed
        ? "Все алгоритмы вернули корректный индекс для всех тестовых наборов."
        : "Обнаружены расхождения между реализацией и ожидаемыми результатами.");
    Console.WriteLine();

    return allPassed;
}

static void RunBenchmarks(
    IReadOnlyList<SearchAlgorithm> algorithms,
    IReadOnlyList<BenchmarkCase> benchmarkCases)
{
    foreach (var benchmarkCase in benchmarkCases)
    {
        Console.WriteLine(benchmarkCase.Name);
        Console.WriteLine(
            $"  text length = {benchmarkCase.Text.Length}, pattern length = {benchmarkCase.Pattern.Length}, iterations = {benchmarkCase.Iterations}");

        foreach (var algorithm in algorithms)
        {
            var stopwatch = Stopwatch.StartNew();
            long totalComparisons = 0;
            var lastIndex = -1;

            for (var iteration = 0; iteration < benchmarkCase.Iterations; iteration++)
            {
                var result = algorithm.Search(benchmarkCase.Text, benchmarkCase.Pattern);
                totalComparisons += result.Comparisons;
                lastIndex = result.Index;
            }

            stopwatch.Stop();

            Console.WriteLine(
                $"  {algorithm.Name,-38} -> index = {lastIndex,6}, avg comparisons = {totalComparisons / benchmarkCase.Iterations,10}, total time = {stopwatch.ElapsedMilliseconds,5} ms");
        }

        Console.WriteLine();
    }
}

static IReadOnlyList<SearchCase> BuildCorrectnessCases()
{
    return
    [
        new SearchCase("Пустой шаблон", "abc", string.Empty),
        new SearchCase("Пустой текст", string.Empty, "abc"),
        new SearchCase("Шаблон длиннее текста", "abc", "abcd"),
        new SearchCase("Совпадение в начале", "abracadabra", "abra"),
        new SearchCase("Совпадение в середине", "123needle456", "needle"),
        new SearchCase("Совпадение в конце", "zzzpattern", "pattern"),
        new SearchCase("Нет совпадения", "abcdefgh", "ijk"),
        new SearchCase("Повторяющийся текст", "aaaaaaab", "aaab"),
        new SearchCase("Перекрывающийся префикс", "ababababca", "ababca"),
        new SearchCase("Кириллица", "мама мыла раму", "раму")
    ];
}

static IReadOnlyList<BenchmarkCase> BuildBenchmarkCases()
{
    const string alphabet = "abcdefghijklmnopqrstuvwxyz";
    const string pattern = "needlepattern";

    var random = new Random(42);
    var prefix = GenerateRandomText(35_000, alphabet, random);
    var suffix = GenerateRandomText(10_000, alphabet, random);
    var tailMatchText = prefix + pattern + suffix;

    var repetitiveText = new string('a', 30_000) + "b";
    var repetitivePattern = new string('a', 200) + "c";

    var startMatchText = pattern + GenerateRandomText(45_000, alphabet, random);

    return
    [
        new BenchmarkCase("Случайный текст, совпадение ближе к концу", tailMatchText, pattern, 20),
        new BenchmarkCase("Повторяющийся текст, совпадения нет", repetitiveText, repetitivePattern, 5),
        new BenchmarkCase("Совпадение сразу в начале", startMatchText, pattern, 50)
    ];
}

static string GenerateRandomText(int length, string alphabet, Random random)
{
    var chars = new char[length];

    for (var i = 0; i < chars.Length; i++)
    {
        chars[i] = alphabet[random.Next(alphabet.Length)];
    }

    return new string(chars);
}

static string Truncate(string value, int maxLength)
{
    if (value.Length <= maxLength)
    {
        return value;
    }

    return value[..(maxLength - 3)] + "...";
}

static void PrintSection(string title)
{
    Console.WriteLine(title);
    Console.WriteLine(new string('=', title.Length));
    Console.WriteLine();
}

delegate SearchMetrics SearchDelegate(string text, string pattern);

readonly record struct SearchAlgorithm(string Name, SearchDelegate Search);

readonly record struct SearchMetrics(int Index, long Comparisons);

readonly record struct SearchCase(string Name, string Text, string Pattern, int? ExpectedIndex = null);

readonly record struct BenchmarkCase(string Name, string Text, string Pattern, int Iterations);

static class StringSearchAlgorithms
{
    public static SearchMetrics NaiveSearch(string text, string pattern)
    {
        if (pattern.Length == 0)
        {
            return new SearchMetrics(0, 0);
        }

        if (pattern.Length > text.Length)
        {
            return new SearchMetrics(-1, 0);
        }

        long comparisons = 0;
        var lastStart = text.Length - pattern.Length;

        for (var start = 0; start <= lastStart; start++)
        {
            var matched = 0;

            while (matched < pattern.Length)
            {
                comparisons++;

                if (text[start + matched] != pattern[matched])
                {
                    break;
                }

                matched++;
            }

            if (matched == pattern.Length)
            {
                return new SearchMetrics(start, comparisons);
            }
        }

        return new SearchMetrics(-1, comparisons);
    }

    public static SearchMetrics KnuthMorrisPrattSearch(string text, string pattern)
    {
        if (pattern.Length == 0)
        {
            return new SearchMetrics(0, 0);
        }

        if (pattern.Length > text.Length)
        {
            return new SearchMetrics(-1, 0);
        }

        var prefix = BuildPrefixFunction(pattern);
        long comparisons = 0;
        var matched = 0;

        for (var index = 0; index < text.Length; index++)
        {
            var currentChar = text[index];
            var isMatch = false;

            while (matched > 0)
            {
                comparisons++;

                if (pattern[matched] == currentChar)
                {
                    isMatch = true;
                    break;
                }

                matched = prefix[matched - 1];
            }

            if (matched == 0)
            {
                comparisons++;
                isMatch = pattern[0] == currentChar;
            }

            if (!isMatch)
            {
                continue;
            }

            matched++;

            if (matched == pattern.Length)
            {
                return new SearchMetrics(index - pattern.Length + 1, comparisons);
            }
        }

        return new SearchMetrics(-1, comparisons);
    }

    public static SearchMetrics BoyerMooreHorspoolSearch(string text, string pattern)
    {
        if (pattern.Length == 0)
        {
            return new SearchMetrics(0, 0);
        }

        if (pattern.Length > text.Length)
        {
            return new SearchMetrics(-1, 0);
        }

        var shiftTable = BuildHorspoolShiftTable(pattern);
        long comparisons = 0;
        var start = 0;
        var patternLength = pattern.Length;

        while (start <= text.Length - patternLength)
        {
            var patternIndex = patternLength - 1;

            while (patternIndex >= 0)
            {
                comparisons++;

                if (pattern[patternIndex] != text[start + patternIndex])
                {
                    break;
                }

                patternIndex--;
            }

            if (patternIndex < 0)
            {
                return new SearchMetrics(start, comparisons);
            }

            start += shiftTable[text[start + patternLength - 1]];
        }

        return new SearchMetrics(-1, comparisons);
    }

    public static SearchMetrics BoyerMooreSearch(string text, string pattern)
    {
        if (pattern.Length == 0)
        {
            return new SearchMetrics(0, 0);
        }

        if (pattern.Length > text.Length)
        {
            return new SearchMetrics(-1, 0);
        }

        var lastOccurrence = BuildBadCharacterTable(pattern);
        var goodSuffixShift = BuildGoodSuffixShiftTable(pattern);

        long comparisons = 0;
        var start = 0;
        var patternLength = pattern.Length;

        while (start <= text.Length - patternLength)
        {
            var patternIndex = patternLength - 1;

            while (patternIndex >= 0)
            {
                comparisons++;

                if (pattern[patternIndex] != text[start + patternIndex])
                {
                    break;
                }

                patternIndex--;
            }

            if (patternIndex < 0)
            {
                return new SearchMetrics(start, comparisons);
            }

            var badCharacterShift = patternIndex - lastOccurrence[text[start + patternIndex]];

            if (badCharacterShift < 1)
            {
                badCharacterShift = 1;
            }

            start += Math.Max(goodSuffixShift[patternIndex + 1], badCharacterShift);
        }

        return new SearchMetrics(-1, comparisons);
    }

    private static int[] BuildPrefixFunction(string pattern)
    {
        var prefix = new int[pattern.Length];
        var matched = 0;

        for (var index = 1; index < pattern.Length; index++)
        {
            while (matched > 0 && pattern[index] != pattern[matched])
            {
                matched = prefix[matched - 1];
            }

            if (pattern[index] == pattern[matched])
            {
                matched++;
            }

            prefix[index] = matched;
        }

        return prefix;
    }

    private static int[] BuildHorspoolShiftTable(string pattern)
    {
        var table = new int[char.MaxValue + 1];
        Array.Fill(table, pattern.Length);

        for (var index = 0; index < pattern.Length - 1; index++)
        {
            table[pattern[index]] = pattern.Length - 1 - index;
        }

        return table;
    }

    private static int[] BuildBadCharacterTable(string pattern)
    {
        var table = new int[char.MaxValue + 1];
        Array.Fill(table, -1);

        for (var index = 0; index < pattern.Length; index++)
        {
            table[pattern[index]] = index;
        }

        return table;
    }

    private static int[] BuildGoodSuffixShiftTable(string pattern)
    {
        var patternLength = pattern.Length;
        var shift = new int[patternLength + 1];
        var border = new int[patternLength + 1];

        // Предобработка хорошего суффикса для классического Бойера-Мура.
        var i = patternLength;
        var j = patternLength + 1;
        border[i] = j;

        while (i > 0)
        {
            while (j <= patternLength && pattern[i - 1] != pattern[j - 1])
            {
                if (shift[j] == 0)
                {
                    shift[j] = j - i;
                }

                j = border[j];
            }

            i--;
            j--;
            border[i] = j;
        }

        j = border[0];

        for (i = 0; i <= patternLength; i++)
        {
            if (shift[i] == 0)
            {
                shift[i] = j;
            }

            if (i == j)
            {
                j = border[j];
            }
        }

        return shift;
    }
}
