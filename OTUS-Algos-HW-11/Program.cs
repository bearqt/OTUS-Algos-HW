
using System.Diagnostics;
using System.Text;

const int Seed = 42;
var sizes = new[] { 10_000, 50_000, 100_000 };
const int searchQueriesCount = 100_000;
const int bTreeMinDegree = 16;

Warmup();

var results = new List<BenchmarkResult>();
foreach (var size in sizes)
{
    var keys = GenerateShuffledKeys(size, Seed + size);
    var queries = GenerateQueries(keys, searchQueriesCount, Seed + size * 7);

    var treapBuildMs = MeasureBuildWithSort(keys, () => new Treap(Seed + size * 11));
    var bTreeBuildMs = MeasureBuildWithSort(keys, () => new BTree(bTreeMinDegree));

    var treap = BuildTree(ArraySortedCopy(keys), () => new Treap(Seed + size * 13));
    var bTree = BuildTree(ArraySortedCopy(keys), () => new BTree(bTreeMinDegree));

    var treapSearchMs = MeasureSearch(treap, queries);
    var bTreeSearchMs = MeasureSearch(bTree, queries);

    results.Add(new BenchmarkResult(size, treapBuildMs, bTreeBuildMs, treapSearchMs, bTreeSearchMs));
}

var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "ComparisonReport.md");
File.WriteAllText(reportPath, BuildMarkdownReport(results, searchQueriesCount, bTreeMinDegree), Encoding.UTF8);

Console.WriteLine($"Отчет сформирован: {reportPath}");
Console.WriteLine();
Console.WriteLine(BuildConsoleTable(results));

return;

static void Warmup()
{
    var sample = GenerateShuffledKeys(2_000, Seed);
    var sampleQueries = GenerateQueries(sample, 5_000, Seed + 1);

    var treap = BuildTree(ArraySortedCopy(sample), () => new Treap(Seed + 2));
    var bTree = BuildTree(ArraySortedCopy(sample), () => new BTree(8));

    _ = MeasureSearch(treap, sampleQueries);
    _ = MeasureSearch(bTree, sampleQueries);
}

static long MeasureBuildWithSort(int[] inputKeys, Func<ISearchTree> factory)
{
    var sw = Stopwatch.StartNew();
    var sorted = ArraySortedCopy(inputKeys);
    var tree = factory();
    foreach (var key in sorted)
    {
        tree.Insert(key);
    }
    sw.Stop();
    return sw.ElapsedMilliseconds;
}

static ISearchTree BuildTree(int[] sortedKeys, Func<ISearchTree> factory)
{
    var tree = factory();
    foreach (var key in sortedKeys)
    {
        tree.Insert(key);
    }
    return tree;
}

static long MeasureSearch(ISearchTree tree, int[] queries)
{
    var sw = Stopwatch.StartNew();
    var found = 0;
    foreach (var q in queries)
    {
        if (tree.Contains(q))
        {
            found++;
        }
    }
    sw.Stop();
    GC.KeepAlive(found);
    return sw.ElapsedMilliseconds;
}

static int[] GenerateShuffledKeys(int size, int seed)
{
    var keys = Enumerable.Range(1, size).ToArray();
    var random = new Random(seed);
    for (var i = keys.Length - 1; i > 0; i--)
    {
        var j = random.Next(i + 1);
        (keys[i], keys[j]) = (keys[j], keys[i]);
    }
    return keys;
}

static int[] GenerateQueries(int[] keys, int count, int seed)
{
    var random = new Random(seed);
    var queries = new int[count];
    for (var i = 0; i < count; i++)
    {
        queries[i] = i % 2 == 0
            ? keys[random.Next(keys.Length)] // existing key
            : keys.Length + 1 + random.Next(keys.Length); // missing key
    }
    return queries;
}

static int[] ArraySortedCopy(int[] input)
{
    var copy = input.ToArray();
    Array.Sort(copy);
    return copy;
}

static string BuildMarkdownReport(List<BenchmarkResult> results, int queriesCount, int bTreeDegree)
{
    var sb = new StringBuilder();
    sb.AppendLine("# Сравнение декартового дерева и B-дерева");
    sb.AppendLine();
    sb.AppendLine($"Дата запуска: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine();
    sb.AppendLine("## Методика");
    sb.AppendLine();
    sb.AppendLine("- Язык: C#");
    sb.AppendLine("- Деревья:");
    sb.AppendLine("  - Декартово дерево поиска (Treap, случайные приоритеты)");
    sb.AppendLine($"  - B-дерево (минимальная степень t = {bTreeDegree})");
    sb.AppendLine("- Для каждого размера набора данных:");
    sb.AppendLine("  - Генерируются уникальные ключи 1..N в случайном порядке");
    sb.AppendLine("  - Измеряется время построения `с учетом сортировки` (копия массива + `Array.Sort` + вставка в дерево)");
    sb.AppendLine($"  - Измеряется время поиска на {queriesCount:N0} запросах (50% успешных, 50% неуспешных)");
    sb.AppendLine();
    sb.AppendLine("## Результаты");
    sb.AppendLine();
    sb.AppendLine("| N ключей | Построение Treap, мс | Построение B-Tree, мс | Поиск Treap, мс | Поиск B-Tree, мс |");
    sb.AppendLine("|---:|---:|---:|---:|---:|");
    foreach (var r in results)
    {
        sb.AppendLine($"| {r.Size:N0} | {r.TreapBuildMs} | {r.BTreeBuildMs} | {r.TreapSearchMs} | {r.BTreeSearchMs} |");
    }

    sb.AppendLine();
    sb.AppendLine("## Вывод");
    sb.AppendLine();
    var treapBuildWins = results.Count(r => r.TreapBuildMs < r.BTreeBuildMs);
    var bTreeBuildWins = results.Count - treapBuildWins;
    var treapSearchWins = results.Count(r => r.TreapSearchMs < r.BTreeSearchMs);
    var bTreeSearchWins = results.Count - treapSearchWins;
    sb.AppendLine($"- По времени построения (включая сортировку): Treap быстрее в {treapBuildWins} случаях, B-Tree быстрее в {bTreeBuildWins} случаях.");
    sb.AppendLine($"- По времени поиска: Treap быстрее в {treapSearchWins} случаях, B-Tree быстрее в {bTreeSearchWins} случаях.");
    sb.AppendLine("- Итог зависит от размера данных и влияния констант в конкретной реализации.");
    return sb.ToString();
}

static string BuildConsoleTable(List<BenchmarkResult> results)
{
    var sb = new StringBuilder();
    sb.AppendLine("N\tTreap build(ms)\tB-Tree build(ms)\tTreap search(ms)\tB-Tree search(ms)");
    foreach (var r in results)
    {
        sb.AppendLine($"{r.Size}\t{r.TreapBuildMs}\t{r.BTreeBuildMs}\t{r.TreapSearchMs}\t{r.BTreeSearchMs}");
    }
    return sb.ToString();
}

file sealed record BenchmarkResult(
    int Size,
    long TreapBuildMs,
    long BTreeBuildMs,
    long TreapSearchMs,
    long BTreeSearchMs);

file interface ISearchTree
{
    void Insert(int key);
    bool Contains(int key);
}

file sealed class Treap : ISearchTree
{
    private readonly Random _random;
    private Node? _root;

    public Treap(int seed) => _random = new Random(seed);

    public void Insert(int key)
    {
        var newNode = new Node(key, _random.Next());
        _root = Insert(_root, newNode);
    }

    public bool Contains(int key)
    {
        var current = _root;
        while (current is not null)
        {
            if (key == current.Key)
            {
                return true;
            }
            current = key < current.Key ? current.Left : current.Right;
        }
        return false;
    }

    private static Node Insert(Node? root, Node newNode)
    {
        if (root is null)
        {
            return newNode;
        }
        if (newNode.Key == root.Key)
        {
            return root;
        }

        if (newNode.Priority > root.Priority)
        {
            Split(root, newNode.Key, out var left, out var right);
            newNode.Left = left;
            newNode.Right = right;
            return newNode;
        }

        if (newNode.Key < root.Key)
        {
            root.Left = Insert(root.Left, newNode);
        }
        else
        {
            root.Right = Insert(root.Right, newNode);
        }
        return root;
    }

    private static void Split(Node? root, int key, out Node? left, out Node? right)
    {
        if (root is null)
        {
            left = null;
            right = null;
            return;
        }

        if (key < root.Key)
        {
            Split(root.Left, key, out left, out var tmp);
            root.Left = tmp;
            right = root;
        }
        else
        {
            Split(root.Right, key, out var tmp, out right);
            root.Right = tmp;
            left = root;
        }
    }

    private sealed class Node
    {
        public int Key { get; }
        public int Priority { get; }
        public Node? Left { get; set; }
        public Node? Right { get; set; }

        public Node(int key, int priority)
        {
            Key = key;
            Priority = priority;
        }
    }
}

file sealed class BTree : ISearchTree
{
    private readonly int _t;
    private BTreeNode _root;

    public BTree(int minDegree)
    {
        if (minDegree < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(minDegree), "B-Tree min degree must be >= 2.");
        }
        _t = minDegree;
        _root = new BTreeNode(_t, isLeaf: true);
    }

    public void Insert(int key)
    {
        if (_root.IsFull)
        {
            var newRoot = new BTreeNode(_t, isLeaf: false);
            newRoot.Children.Add(_root);
            SplitChild(newRoot, 0);
            _root = newRoot;
        }

        InsertNonFull(_root, key);
    }

    public bool Contains(int key)
    {
        return Search(_root, key);
    }

    private bool Search(BTreeNode node, int key)
    {
        var i = 0;
        while (i < node.Keys.Count && key > node.Keys[i])
        {
            i++;
        }

        if (i < node.Keys.Count && key == node.Keys[i])
        {
            return true;
        }

        if (node.IsLeaf)
        {
            return false;
        }

        return Search(node.Children[i], key);
    }

    private void InsertNonFull(BTreeNode node, int key)
    {
        var i = node.Keys.Count - 1;
        if (node.IsLeaf)
        {
            node.Keys.Add(0);
            while (i >= 0 && key < node.Keys[i])
            {
                node.Keys[i + 1] = node.Keys[i];
                i--;
            }
            node.Keys[i + 1] = key;
            return;
        }

        while (i >= 0 && key < node.Keys[i])
        {
            i--;
        }
        i++;

        if (node.Children[i].IsFull)
        {
            SplitChild(node, i);
            if (key > node.Keys[i])
            {
                i++;
            }
        }
        InsertNonFull(node.Children[i], key);
    }

    private void SplitChild(BTreeNode parent, int index)
    {
        var fullChild = parent.Children[index];
        var newRight = new BTreeNode(_t, fullChild.IsLeaf);

        var median = fullChild.Keys[_t - 1];

        for (var j = 0; j < _t - 1; j++)
        {
            newRight.Keys.Add(fullChild.Keys[j + _t]);
        }
        fullChild.Keys.RemoveRange(_t - 1, _t);

        if (!fullChild.IsLeaf)
        {
            for (var j = 0; j < _t; j++)
            {
                newRight.Children.Add(fullChild.Children[j + _t]);
            }
            fullChild.Children.RemoveRange(_t, _t);
        }

        parent.Keys.Insert(index, median);
        parent.Children.Insert(index + 1, newRight);
    }

    private sealed class BTreeNode
    {
        public List<int> Keys { get; } = new();
        public List<BTreeNode> Children { get; } = new();
        public bool IsLeaf { get; }
        public bool IsFull => Keys.Count == (2 * _t) - 1;
        private readonly int _t;

        public BTreeNode(int minDegree, bool isLeaf)
        {
            _t = minDegree;
            IsLeaf = isLeaf;
        }
    }
}
