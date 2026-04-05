using System.Text;

var prefixFunctionCases = new[]
{
    "aabaaab",
    "abcdabca",
    "aaaaaa",
    "abacabadabacaba",
    "abcaby",
};

var searchCases = new[]
{
    new SearchCase("ababcabcabababd", "ababd", new[] { 10 }),
    new SearchCase("aaaaa", "aa", new[] { 0, 1, 2, 3 }),
    new SearchCase("abcabcabcabc", "abcabc", new[] { 0, 3, 6 }),
    new SearchCase("needle in a haystack", "stack", new[] { 15 }),
    new SearchCase("mississippi", "issi", new[] { 1, 4 }),
    new SearchCase("abcdef", "gh", Array.Empty<int>()),
    new SearchCase("pattern", "pattern", new[] { 0 }),
    new SearchCase("short", "longer-pattern", Array.Empty<int>()),
};

Console.OutputEncoding = Encoding.UTF8;

PrintHeader("Проверка префикс-функции");
foreach (var pattern in prefixFunctionCases)
{
    var slow = KnuthMorrisPratt.ComputePrefixFunctionSlow(pattern);
    var fast = KnuthMorrisPratt.ComputePrefixFunctionFast(pattern);

    EnsureEqual(slow, fast, $"Префикс-функции не совпали для шаблона \"{pattern}\".");

    Console.WriteLine($"Шаблон: \"{pattern}\"");
    Console.WriteLine($"Медленно: [{string.Join(", ", slow)}]");
    Console.WriteLine($"Быстро : [{string.Join(", ", fast)}]");
    Console.WriteLine();
}

PrintHeader("Переходы конечного автомата");
var automatonPattern = "ababd";
var alphabet = "abcd".ToCharArray();
var automaton = KnuthMorrisPratt.BuildAutomaton(automatonPattern, alphabet);
Console.WriteLine($"Шаблон: \"{automatonPattern}\"");
Console.WriteLine($"Алфавит: {string.Join(", ", alphabet)}");
Console.WriteLine("Состояние | " + string.Join(" | ", alphabet.Select(symbol => $"'{symbol}'")));
for (var state = 0; state < automatonPattern.Length + 1; state++)
{
    var transitions = alphabet.Select(symbol => automaton[state][symbol]);
    Console.WriteLine($"{state,9} | {string.Join(" | ", transitions.Select(value => value.ToString().PadLeft(2)))}");
}

Console.WriteLine();

PrintHeader("Проверка поиска Кнута-Морриса-Пратта");
foreach (var searchCase in searchCases)
{
    var kmpMatches = KnuthMorrisPratt.FindAllMatches(searchCase.Text, searchCase.Pattern);
    var automatonMatches = KnuthMorrisPratt.FindAllMatchesWithAutomaton(searchCase.Text, searchCase.Pattern);

    EnsureEqual(searchCase.ExpectedMatches, kmpMatches, "KMP вернул неверный результат.");
    EnsureEqual(searchCase.ExpectedMatches, automatonMatches, "Автомат вернул неверный результат.");

    Console.WriteLine($"Текст   : \"{searchCase.Text}\"");
    Console.WriteLine($"Шаблон  : \"{searchCase.Pattern}\"");
    Console.WriteLine($"Позиции : [{string.Join(", ", kmpMatches)}]");
    Console.WriteLine();
}

Console.WriteLine("Все проверки пройдены.");

static void PrintHeader(string title)
{
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

static void EnsureEqual(IReadOnlyList<int> expected, IReadOnlyList<int> actual, string errorMessage)
{
    if (expected.Count != actual.Count)
    {
        throw new InvalidOperationException($"{errorMessage} Ожидалось {Format(expected)}, получено {Format(actual)}.");
    }

    for (var i = 0; i < expected.Count; i++)
    {
        if (expected[i] != actual[i])
        {
            throw new InvalidOperationException($"{errorMessage} Ожидалось {Format(expected)}, получено {Format(actual)}.");
        }
    }
}

static string Format(IReadOnlyList<int> values) => $"[{string.Join(", ", values)}]";

static class KnuthMorrisPratt
{
    public static int[] ComputePrefixFunctionSlow(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var prefix = new int[value.Length];

        for (var i = 0; i < value.Length; i++)
        {
            var currentLength = i + 1;

            for (var candidateLength = currentLength - 1; candidateLength > 0; candidateLength--)
            {
                if (HasEqualPrefixAndSuffix(value, currentLength, candidateLength))
                {
                    prefix[i] = candidateLength;
                    break;
                }
            }
        }

        return prefix;
    }

    public static int[] ComputePrefixFunctionFast(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var prefix = new int[value.Length];

        for (var i = 1; i < value.Length; i++)
        {
            var current = prefix[i - 1];

            while (current > 0 && value[i] != value[current])
            {
                current = prefix[current - 1];
            }

            if (value[i] == value[current])
            {
                current++;
            }

            prefix[i] = current;
        }

        return prefix;
    }

    public static List<int> FindAllMatches(string text, string pattern)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern.Length == 0)
        {
            return Enumerable.Range(0, text.Length + 1).ToList();
        }

        if (pattern.Length > text.Length)
        {
            return new List<int>();
        }

        var prefix = ComputePrefixFunctionFast(pattern);
        var matches = new List<int>();
        var matched = 0;

        for (var i = 0; i < text.Length; i++)
        {
            while (matched > 0 && text[i] != pattern[matched])
            {
                matched = prefix[matched - 1];
            }

            if (text[i] == pattern[matched])
            {
                matched++;
            }

            if (matched == pattern.Length)
            {
                matches.Add(i - pattern.Length + 1);
                matched = prefix[matched - 1];
            }
        }

        return matches;
    }

    public static Dictionary<int, Dictionary<char, int>> BuildAutomaton(string pattern, IReadOnlyCollection<char> alphabet)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(alphabet);

        var prefix = ComputePrefixFunctionFast(pattern);
        var automaton = new Dictionary<int, Dictionary<char, int>>();

        for (var state = 0; state <= pattern.Length; state++)
        {
            automaton[state] = new Dictionary<char, int>();

            foreach (var symbol in alphabet)
            {
                var nextState = state;

                while (nextState > 0 && (nextState == pattern.Length || pattern[nextState] != symbol))
                {
                    nextState = prefix[nextState - 1];
                }

                if (nextState < pattern.Length && pattern[nextState] == symbol)
                {
                    nextState++;
                }

                automaton[state][symbol] = nextState;
            }
        }

        return automaton;
    }

    public static List<int> FindAllMatchesWithAutomaton(string text, string pattern)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern.Length == 0)
        {
            return Enumerable.Range(0, text.Length + 1).ToList();
        }

        var alphabet = text.Concat(pattern).Distinct().ToArray();
        var automaton = BuildAutomaton(pattern, alphabet);
        var prefix = ComputePrefixFunctionFast(pattern);
        var matches = new List<int>();
        var state = 0;

        for (var i = 0; i < text.Length; i++)
        {
            state = automaton[state][text[i]];

            if (state == pattern.Length)
            {
                matches.Add(i - pattern.Length + 1);
                state = prefix[state - 1];
            }
        }

        return matches;
    }

    private static bool HasEqualPrefixAndSuffix(string value, int currentLength, int candidateLength)
    {
        for (var i = 0; i < candidateLength; i++)
        {
            var suffixIndex = currentLength - candidateLength + i;
            if (value[i] != value[suffixIndex])
            {
                return false;
            }
        }

        return true;
    }
}

record SearchCase(string Text, string Pattern, IReadOnlyList<int> ExpectedMatches);
