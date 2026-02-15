using System.Diagnostics;
using System.Text;

const int N = 25000;
const int SearchAndRemoveFraction = 10;
const string ReportFileName = "PERFORMANCE_REPORT.md";

RunCorrectnessTests();

int[] orderedValues = Enumerable.Range(1, N).ToArray();
int[] randomValues = orderedValues.ToArray();
Shuffle(randomValues, new Random(42));

int operationsCount = N / SearchAndRemoveFraction;
int[] searchValues = GetRandomSample(orderedValues, operationsCount, new Random(7));
int[] removeValues = GetRandomSample(orderedValues, operationsCount, new Random(123));

var randomTreeResult = BenchmarkScenario("Случайная вставка", randomValues, searchValues, removeValues);
var orderedTreeResult = BenchmarkScenario("Возрастающая вставка", orderedValues, searchValues, removeValues);

string report = BuildMarkdownReport(N, operationsCount, randomTreeResult, orderedTreeResult);
File.WriteAllText(ReportFileName, report, Encoding.UTF8);

Console.WriteLine($"Отчет записан в {ReportFileName}");
Console.WriteLine();
Console.WriteLine(report);

static void RunCorrectnessTests()
{
    var tree = new BinarySearchTree();
    int[] values = [8, 3, 10, 1, 6, 14, 4, 7, 13];

    foreach (int value in values)
    {
        tree.insert(value);
    }

    Assert(tree.search(6), "Элемент 6 должен быть найден");
    Assert(!tree.search(2), "Элемент 2 не должен быть найден");

    tree.remove(1);  // Лист
    Assert(!tree.search(1), "Лист 1 должен быть удален");

    tree.remove(14); // Узел с одним потомком
    Assert(!tree.search(14), "Узел 14 должен быть удален");
    Assert(tree.search(13), "Потомок 13 должен остаться после удаления 14");

    tree.remove(3);  // Узел с двумя потомками
    Assert(!tree.search(3), "Узел 3 должен быть удален");
    Assert(tree.search(4) && tree.search(6) && tree.search(7), "Поддерево после удаления 3 должно сохраниться");
}

static BenchmarkResult BenchmarkScenario(
    string name,
    int[] insertValues,
    int[] searchValues,
    int[] removeValues)
{
    var tree = new BinarySearchTree();

    long insertMs = Measure(() =>
    {
        foreach (int value in insertValues)
        {
            tree.insert(value);
        }
    });

    int found = 0;
    long searchMs = Measure(() =>
    {
        foreach (int value in searchValues)
        {
            if (tree.search(value))
            {
                found++;
            }
        }
    });

    long removeMs = Measure(() =>
    {
        foreach (int value in removeValues)
        {
            tree.remove(value);
        }
    });

    return new BenchmarkResult(name, insertMs, searchMs, removeMs, found);
}

static long Measure(Action action)
{
    var sw = Stopwatch.StartNew();
    action();
    sw.Stop();
    return sw.ElapsedMilliseconds;
}

static int[] GetRandomSample(int[] source, int count, Random random)
{
    int[] copy = source.ToArray();
    Shuffle(copy, random);
    return copy.Take(count).ToArray();
}

static void Shuffle(int[] array, Random random)
{
    for (int i = array.Length - 1; i > 0; i--)
    {
        int j = random.Next(i + 1);
        (array[i], array[j]) = (array[j], array[i]);
    }
}

static string BuildMarkdownReport(int n, int opCount, BenchmarkResult randomTree, BenchmarkResult orderedTree)
{
    long randomTotal = randomTree.InsertMs + randomTree.SearchMs + randomTree.RemoveMs;
    long orderedTotal = orderedTree.InsertMs + orderedTree.SearchMs + orderedTree.RemoveMs;

    var sb = new StringBuilder();
    sb.AppendLine("# Отчет по производительности бинарного дерева поиска");
    sb.AppendLine();
    sb.AppendLine($"Дата запуска: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine();
    sb.AppendLine("## Параметры эксперимента");
    sb.AppendLine();
    sb.AppendLine($"- Размер дерева `N`: {n}");
    sb.AppendLine($"- Поисков: `N/10`: {opCount}");
    sb.AppendLine($"- Удалений: `N/10`: {opCount}");
    sb.AppendLine("- Дерево 1: вставка в случайном порядке");
    sb.AppendLine("- Дерево 2: вставка в возрастающем порядке");
    sb.AppendLine();
    sb.AppendLine("## Таблица производительности");
    sb.AppendLine();
    sb.AppendLine("| Сценарий | Insert (мс) | Search N/10 (мс) | Remove N/10 (мс) | Total (мс) | Найдено |" );
    sb.AppendLine("|---|---:|---:|---:|---:|---:|");
    sb.AppendLine($"| {randomTree.Name} | {randomTree.InsertMs} | {randomTree.SearchMs} | {randomTree.RemoveMs} | {randomTotal} | {randomTree.FoundCount}/{opCount} |");
    sb.AppendLine($"| {orderedTree.Name} | {orderedTree.InsertMs} | {orderedTree.SearchMs} | {orderedTree.RemoveMs} | {orderedTotal} | {orderedTree.FoundCount}/{opCount} |");
    sb.AppendLine();
    sb.AppendLine("## Вывод");
    sb.AppendLine();
    sb.AppendLine("При случайной вставке дерево остается ближе к сбалансированному, поэтому операции работают заметно быстрее.");
    sb.AppendLine("При возрастающей вставке дерево вырождается в связный список, из-за чего операции становятся линейными по времени и суммарное время резко растет.");
    sb.AppendLine("Эксперимент подтверждает, что для предсказуемой производительности на отсортированных данных нужно самобалансирующееся дерево (AVL/Red-Black). ");

    return sb.ToString();
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException($"Тест не пройден: {message}");
    }
}

internal sealed class BinarySearchTree
{
    private Node? _root;

    public void insert(int x)
    {
        if (_root is null)
        {
            _root = new Node(x);
            return;
        }

        Node current = _root;
        while (true)
        {
            if (x < current.Value)
            {
                if (current.Left is null)
                {
                    current.Left = new Node(x);
                    return;
                }

                current = current.Left;
            }
            else if (x > current.Value)
            {
                if (current.Right is null)
                {
                    current.Right = new Node(x);
                    return;
                }

                current = current.Right;
            }
            else
            {
                return; // Дубликаты не добавляем.
            }
        }
    }

    public bool search(int x)
    {
        Node? current = _root;

        while (current is not null)
        {
            if (x < current.Value)
            {
                current = current.Left;
            }
            else if (x > current.Value)
            {
                current = current.Right;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public void remove(int x)
    {
        Node? parent = null;
        Node? current = _root;

        while (current is not null && current.Value != x)
        {
            parent = current;
            current = x < current.Value ? current.Left : current.Right;
        }

        if (current is null)
        {
            return;
        }

        if (current.Left is not null && current.Right is not null)
        {
            Node successorParent = current;
            Node successor = current.Right;

            while (successor.Left is not null)
            {
                successorParent = successor;
                successor = successor.Left;
            }

            current.Value = successor.Value;
            current = successor;
            parent = successorParent;
        }

        Node? replacement = current.Left ?? current.Right;

        if (parent is null)
        {
            _root = replacement;
        }
        else if (parent.Left == current)
        {
            parent.Left = replacement;
        }
        else
        {
            parent.Right = replacement;
        }
    }

    private sealed class Node(int value)
    {
        public int Value { get; set; } = value;
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }
}

internal sealed record BenchmarkResult(
    string Name,
    long InsertMs,
    long SearchMs,
    long RemoveMs,
    int FoundCount);
