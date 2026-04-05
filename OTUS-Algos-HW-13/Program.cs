public static class Program
{
    public static void Main()
    {
        RunExampleFromTask();
        Console.WriteLine();
        RunAdditionalExample();
    }

    private static void RunExampleFromTask()
    {
        var trie = new Trie();

        Console.WriteLine("Example 1");
        ExecuteInsert(trie, "apple");
        ExecuteSearch(trie, "apple");
        ExecuteSearch(trie, "app");
        ExecuteStartsWith(trie, "app");
        ExecuteInsert(trie, "app");
        ExecuteSearch(trie, "app");
    }

    private static void RunAdditionalExample()
    {
        var trie = new Trie();

        Console.WriteLine("Example 2");
        ExecuteInsert(trie, "cat");
        ExecuteInsert(trie, "car");
        ExecuteInsert(trie, "cart");
        ExecuteSearch(trie, "car");
        ExecuteSearch(trie, "cap");
        ExecuteStartsWith(trie, "ca");
        ExecuteStartsWith(trie, "do");
    }

    private static void ExecuteInsert(Trie trie, string word)
    {
        trie.Insert(word);
        Console.WriteLine($"insert(\"{word}\")");
    }

    private static void ExecuteSearch(Trie trie, string word)
    {
        Console.WriteLine($"search(\"{word}\") = {FormatBool(trie.Search(word))}");
    }

    private static void ExecuteStartsWith(Trie trie, string prefix)
    {
        Console.WriteLine($"startsWith(\"{prefix}\") = {FormatBool(trie.StartsWith(prefix))}");
    }

    private static string FormatBool(bool value)
    {
        return value ? "true" : "false";
    }
}

public sealed class Trie
{
    private sealed class Node
    {
        public Node?[] Children { get; } = new Node?[26];
        public bool IsWord { get; set; }
    }

    private readonly Node _root = new();

    public Trie()
    {
    }

    public void Insert(string word)
    {
        var current = _root;

        foreach (var ch in word)
        {
            var index = ch - 'a';

            current.Children[index] ??= new Node();
            current = current.Children[index]!;
        }

        current.IsWord = true;
    }

    public bool Search(string word)
    {
        return TryGetNode(word, out var node) && node.IsWord;
    }

    public bool StartsWith(string prefix)
    {
        return TryGetNode(prefix, out _);
    }

    private bool TryGetNode(string value, out Node node)
    {
        var current = _root;

        foreach (var ch in value)
        {
            var index = ch - 'a';
            var next = current.Children[index];

            if (next is null)
            {
                node = _root;
                return false;
            }

            current = next;
        }

        node = current;
        return true;
    }
}
