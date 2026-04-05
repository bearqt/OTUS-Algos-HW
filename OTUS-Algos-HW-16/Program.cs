const int Empty = -1;

int[,] adjacency =
{
    { 1, 2, Empty, Empty },
    { 0, 2, 3, Empty },
    { 0, 1, 3, 4 },
    { 1, 2, 4, Empty },
    { 2, 3, Empty, Empty }
};

int[,] weights =
{
    { 4, 2, Empty, Empty },
    { 4, 1, 5, Empty },
    { 2, 1, 8, 10 },
    { 5, 8, 2, Empty },
    { 10, 2, Empty, Empty }
};

Edge[] edges = KruskalAlgorithm.FindMinimumSpanningTree(adjacency, weights, Empty);

Console.WriteLine("Minimum spanning tree edges:");
for (int i = 0; i < edges.Length; i++)
{
    Console.WriteLine(edges[i]);
}

public sealed class Edge
{
    public int v1;
    public int v2;

    public Edge(int v1, int v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }

    public override string ToString()
    {
        return $"{v1} - {v2}";
    }
}

internal sealed class WeightedEdge
{
    public int V1;
    public int V2;
    public int Weight;

    public WeightedEdge(int v1, int v2, int weight)
    {
        V1 = v1;
        V2 = v2;
        Weight = weight;
    }
}

internal static class KruskalAlgorithm
{
    public static Edge[] FindMinimumSpanningTree(int[,] adjacency, int[,] weights, int emptyValue = -1)
    {
        ValidateInput(adjacency, weights);

        int verticesCount = adjacency.GetLength(0);
        WeightedEdge[] weightedEdges = ExtractUniqueEdges(adjacency, weights, emptyValue);

        Array.Sort(weightedEdges, CompareByWeight);

        Edge[] result = new Edge[Math.Max(0, verticesCount - 1)];
        UnionFind unionFind = new(verticesCount);
        int resultCount = 0;

        for (int i = 0; i < weightedEdges.Length && resultCount < result.Length; i++)
        {
            WeightedEdge edge = weightedEdges[i];

            if (!unionFind.Union(edge.V1, edge.V2))
            {
                continue;
            }

            result[resultCount] = new Edge(edge.V1, edge.V2);
            resultCount++;
        }

        Array.Resize(ref result, resultCount);
        return result;
    }

    private static void ValidateInput(int[,] adjacency, int[,] weights)
    {
        if (adjacency.GetLength(0) != weights.GetLength(0) || adjacency.GetLength(1) != weights.GetLength(1))
        {
            throw new ArgumentException("Matrices adjacency and weights must have the same size.");
        }
    }

    private static WeightedEdge[] ExtractUniqueEdges(int[,] adjacency, int[,] weights, int emptyValue)
    {
        int verticesCount = adjacency.GetLength(0);
        int maxAdjacentVertices = adjacency.GetLength(1);
        WeightedEdge[] buffer = new WeightedEdge[verticesCount * maxAdjacentVertices];
        int count = 0;

        for (int from = 0; from < verticesCount; from++)
        {
            for (int column = 0; column < maxAdjacentVertices; column++)
            {
                int to = adjacency[from, column];
                if (to == emptyValue)
                {
                    continue;
                }

                if (to < 0 || to >= verticesCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(adjacency), $"Vertex index {to} is out of range.");
                }

                int weight = weights[from, column];
                if (weight == emptyValue)
                {
                    throw new ArgumentException("Each adjacency entry must have a corresponding weight.");
                }

                int v1 = Math.Min(from, to);
                int v2 = Math.Max(from, to);
                buffer[count] = new WeightedEdge(v1, v2, weight);
                count++;
            }
        }

        Array.Resize(ref buffer, count);
        Array.Sort(buffer, CompareByVertices);

        WeightedEdge[] uniqueEdges = new WeightedEdge[buffer.Length];
        int uniqueCount = 0;

        for (int i = 0; i < buffer.Length; i++)
        {
            WeightedEdge edge = buffer[i];

            if (edge.V1 == edge.V2)
            {
                continue;
            }

            if (uniqueCount > 0)
            {
                WeightedEdge previous = uniqueEdges[uniqueCount - 1];
                if (previous.V1 == edge.V1 && previous.V2 == edge.V2)
                {
                    if (previous.Weight != edge.Weight)
                    {
                        throw new ArgumentException(
                            $"Edge {edge.V1}-{edge.V2} has different weights in the adjacency vector.");
                    }

                    continue;
                }
            }

            uniqueEdges[uniqueCount] = edge;
            uniqueCount++;
        }

        Array.Resize(ref uniqueEdges, uniqueCount);
        return uniqueEdges;
    }

    private static int CompareByVertices(WeightedEdge? left, WeightedEdge? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        int byFirstVertex = left.V1.CompareTo(right.V1);
        if (byFirstVertex != 0)
        {
            return byFirstVertex;
        }

        int bySecondVertex = left.V2.CompareTo(right.V2);
        if (bySecondVertex != 0)
        {
            return bySecondVertex;
        }

        return left.Weight.CompareTo(right.Weight);
    }

    private static int CompareByWeight(WeightedEdge? left, WeightedEdge? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        int byWeight = left.Weight.CompareTo(right.Weight);
        if (byWeight != 0)
        {
            return byWeight;
        }

        int byFirstVertex = left.V1.CompareTo(right.V1);
        if (byFirstVertex != 0)
        {
            return byFirstVertex;
        }

        return left.V2.CompareTo(right.V2);
    }
}

internal sealed class UnionFind
{
    private readonly int[] parents;
    private readonly int[] ranks;

    public UnionFind(int size)
    {
        parents = new int[size];
        ranks = new int[size];

        for (int i = 0; i < size; i++)
        {
            parents[i] = i;
        }
    }

    public int Find(int vertex)
    {
        if (parents[vertex] != vertex)
        {
            parents[vertex] = Find(parents[vertex]);
        }

        return parents[vertex];
    }

    public bool Union(int left, int right)
    {
        int leftRoot = Find(left);
        int rightRoot = Find(right);

        if (leftRoot == rightRoot)
        {
            return false;
        }

        if (ranks[leftRoot] < ranks[rightRoot])
        {
            parents[leftRoot] = rightRoot;
        }
        else if (ranks[leftRoot] > ranks[rightRoot])
        {
            parents[rightRoot] = leftRoot;
        }
        else
        {
            parents[rightRoot] = leftRoot;
            ranks[leftRoot]++;
        }

        return true;
    }
}
