using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var graph = BipartiteGraph.CreateSampleA34();
graph.Validate(expectedLeftCount: 3, expectedRightCount: 4, expectedEdgeCount: 5);
var dotContent = graph.ToDot();
var dotFilePath = Path.Combine(Directory.GetCurrentDirectory(), "graph.dot");
File.WriteAllText(dotFilePath, dotContent, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

PrintSection("Двудольный граф A(3,4) с 5 рёбрами");
Console.WriteLine("Левая доля : " + string.Join(", ", graph.LeftPart));
Console.WriteLine("Правая доля: " + string.Join(", ", graph.RightPart));
Console.WriteLine("Рёбра      : " + string.Join(", ", graph.Edges.Select(edge => edge.ToString())));

PrintSection("1) Структура данных и заполнение");
Console.WriteLine(graph.ToCodeRepresentation());

PrintSection("Текстовое представление графа");
Console.WriteLine(graph.ToAsciiDescription());

PrintSection("2) Перечисление множеств");
Console.WriteLine(graph.ToSetRepresentation());

PrintSection("3) Матрица смежности");
var adjacencyMatrix = graph.BuildAdjacencyMatrix();
PrintMatrix(adjacencyMatrix, graph.Vertices, graph.Vertices, "V");

PrintSection("4) Матрица инцидентности");
var incidenceMatrix = graph.BuildIncidenceMatrix();
var edgeHeaders = graph.Edges
    .Select((edge, index) => $"e{index + 1}")
    .ToArray();
PrintMatrix(incidenceMatrix, graph.Vertices, edgeHeaders, "V/E");
Console.WriteLine();
Console.WriteLine("Расшифровка столбцов матрицы инцидентности:");
for (var i = 0; i < graph.Edges.Count; i++)
{
    Console.WriteLine($"e{i + 1} = {graph.Edges[i]}");
}

PrintSection("DOT-представление для Graphviz");
Console.WriteLine(dotContent);
Console.WriteLine();
Console.WriteLine($"DOT-файл сохранён: {dotFilePath}");

static void PrintSection(string title)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', title.Length));
    Console.WriteLine(title);
    Console.WriteLine(new string('=', title.Length));
}

static void PrintMatrix(int[,] matrix, IReadOnlyList<string> rowHeaders, IReadOnlyList<string> columnHeaders, string cornerHeader)
{
    const int cellWidth = 6;

    Console.Write($"{cornerHeader,-cellWidth}");
    foreach (var columnHeader in columnHeaders)
    {
        Console.Write($"{columnHeader,-cellWidth}");
    }

    Console.WriteLine();

    for (var row = 0; row < matrix.GetLength(0); row++)
    {
        Console.Write($"{rowHeaders[row],-cellWidth}");
        for (var column = 0; column < matrix.GetLength(1); column++)
        {
            Console.Write($"{matrix[row, column],-cellWidth}");
        }

        Console.WriteLine();
    }
}

sealed class BipartiteGraph
{
    private readonly Dictionary<string, int> _vertexIndex;

    public IReadOnlyList<string> LeftPart { get; }
    public IReadOnlyList<string> RightPart { get; }
    public IReadOnlyList<Edge> Edges { get; }
    public IReadOnlyList<string> Vertices { get; }

    public BipartiteGraph(IEnumerable<string> leftPart, IEnumerable<string> rightPart, IEnumerable<Edge> edges)
    {
        LeftPart = leftPart.ToArray();
        RightPart = rightPart.ToArray();
        Edges = edges.ToArray();
        Vertices = LeftPart.Concat(RightPart).ToArray();
        _vertexIndex = Vertices
            .Select((vertex, index) => new { vertex, index })
            .ToDictionary(item => item.vertex, item => item.index);
    }

    public static BipartiteGraph CreateSampleA34() =>
        new(
            leftPart: ["A1", "A2", "A3"],
            rightPart: ["B1", "B2", "B3", "B4"],
            edges:
            [
                new Edge("A1", "B1"),
                new Edge("A1", "B3"),
                new Edge("A2", "B2"),
                new Edge("A3", "B2"),
                new Edge("A3", "B4")
            ]);

    public void Validate(int expectedLeftCount, int expectedRightCount, int expectedEdgeCount)
    {
        if (LeftPart.Count != expectedLeftCount)
        {
            throw new InvalidOperationException($"Ожидалось {expectedLeftCount} вершин в левой доле, получено {LeftPart.Count}.");
        }

        if (RightPart.Count != expectedRightCount)
        {
            throw new InvalidOperationException($"Ожидалось {expectedRightCount} вершин в правой доле, получено {RightPart.Count}.");
        }

        if (Edges.Count != expectedEdgeCount)
        {
            throw new InvalidOperationException($"Ожидалось {expectedEdgeCount} рёбер, получено {Edges.Count}.");
        }

        var leftSet = LeftPart.ToHashSet();
        var rightSet = RightPart.ToHashSet();

        if (leftSet.Count != LeftPart.Count || rightSet.Count != RightPart.Count)
        {
            throw new InvalidOperationException("Вершины в долях должны быть уникальными.");
        }

        if (leftSet.Overlaps(rightSet))
        {
            throw new InvalidOperationException("Доли двудольного графа не должны пересекаться.");
        }

        var uniqueEdges = new HashSet<Edge>();
        foreach (var edge in Edges)
        {
            if (!leftSet.Contains(edge.Left))
            {
                throw new InvalidOperationException($"Вершина {edge.Left} не принадлежит левой доле.");
            }

            if (!rightSet.Contains(edge.Right))
            {
                throw new InvalidOperationException($"Вершина {edge.Right} не принадлежит правой доле.");
            }

            if (!uniqueEdges.Add(edge))
            {
                throw new InvalidOperationException($"Обнаружено повторяющееся ребро {edge}.");
            }
        }
    }

    public int[,] BuildAdjacencyMatrix()
    {
        var matrix = new int[Vertices.Count, Vertices.Count];

        foreach (var edge in Edges)
        {
            var leftIndex = _vertexIndex[edge.Left];
            var rightIndex = _vertexIndex[edge.Right];

            matrix[leftIndex, rightIndex] = 1;
            matrix[rightIndex, leftIndex] = 1;
        }

        return matrix;
    }

    public int[,] BuildIncidenceMatrix()
    {
        var matrix = new int[Vertices.Count, Edges.Count];

        for (var edgeIndex = 0; edgeIndex < Edges.Count; edgeIndex++)
        {
            var edge = Edges[edgeIndex];
            matrix[_vertexIndex[edge.Left], edgeIndex] = 1;
            matrix[_vertexIndex[edge.Right], edgeIndex] = 1;
        }

        return matrix;
    }

    public string ToSetRepresentation()
    {
        var lines = new[]
        {
            $"U = {{{string.Join(", ", LeftPart)}}}",
            $"V = {{{string.Join(", ", RightPart)}}}",
            $"E = {{{string.Join(", ", Edges.Select(edge => edge.ToString()))}}}"
        };

        return string.Join(Environment.NewLine, lines);
    }

    public string ToCodeRepresentation()
    {
        var lines = new List<string>
        {
            "var graph = new BipartiteGraph(",
            $"    leftPart: [{string.Join(", ", LeftPart.Select(vertex => $"\"{vertex}\""))}],",
            $"    rightPart: [{string.Join(", ", RightPart.Select(vertex => $"\"{vertex}\""))}],",
            "    edges:",
            "    ["
        };

        lines.AddRange(Edges.Select(edge => $"        new Edge(\"{edge.Left}\", \"{edge.Right}\"),"));
        lines.Add("    ]);");

        return string.Join(Environment.NewLine, lines);
    }

    public string ToAsciiDescription()
    {
        var lines = new List<string>
        {
            "Доли графа:",
            $"[{string.Join("]   [", LeftPart)}]      [{string.Join("]   [", RightPart)}]",
            string.Empty,
            "Связи между долями:"
        };

        lines.AddRange(Edges.Select(edge => $"{edge.Left} --- {edge.Right}"));

        return string.Join(Environment.NewLine, lines);
    }

    public string ToDot()
    {
        var lines = new List<string>
        {
            "graph A34 {",
            "    rankdir=LR;",
            "    node [shape=circle];",
            $"    {{ rank=same; {string.Join("; ", LeftPart)}; }}",
            $"    {{ rank=same; {string.Join("; ", RightPart)}; }}"
        };

        lines.AddRange(Edges.Select(edge => $"    {edge.Left} -- {edge.Right};"));
        lines.Add("}");

        return string.Join(Environment.NewLine, lines);
    }
}

readonly record struct Edge(string Left, string Right)
{
    public override string ToString() => $"({Left}, {Right})";
}
