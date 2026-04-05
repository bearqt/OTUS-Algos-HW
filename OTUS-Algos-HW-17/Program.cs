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

int startVertex = 0;
Edge[] edges = DijkstraAlgorithm.FindShortestPathTree(adjacency, weights, startVertex, Empty);

Console.WriteLine($"Shortest path tree from vertex {startVertex}:");
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

internal static class DijkstraAlgorithm
{
    public static Edge[] FindShortestPathTree(int[,] adjacency, int startVertex, int emptyValue = -1)
    {
        int[,] unitWeights = BuildUnitWeights(adjacency, emptyValue);
        return FindShortestPathTree(adjacency, unitWeights, startVertex, emptyValue);
    }

    public static Edge[] FindShortestPathTree(int[,] adjacency, int[,] weights, int startVertex, int emptyValue = -1)
    {
        ArgumentNullException.ThrowIfNull(adjacency);
        ArgumentNullException.ThrowIfNull(weights);

        ValidateInput(adjacency, weights);

        int verticesCount = adjacency.GetLength(0);
        if (verticesCount == 0)
        {
            return Array.Empty<Edge>();
        }

        ValidateVertex(startVertex, verticesCount);

        int maxAdjacentVertices = adjacency.GetLength(1);
        int[] distances = new int[verticesCount];
        int[] previous = new int[verticesCount];
        bool[] visited = new bool[verticesCount];
        int[] treeVertices = new int[Math.Max(0, verticesCount - 1)];
        int treeVertexCount = 0;

        Array.Fill(distances, int.MaxValue);
        Array.Fill(previous, -1);
        distances[startVertex] = 0;

        for (int step = 0; step < verticesCount; step++)
        {
            int currentVertex = FindNearestUnvisitedVertex(distances, visited);
            if (currentVertex == -1)
            {
                break;
            }

            visited[currentVertex] = true;

            if (currentVertex != startVertex && previous[currentVertex] != -1)
            {
                treeVertices[treeVertexCount] = currentVertex;
                treeVertexCount++;
            }

            for (int column = 0; column < maxAdjacentVertices; column++)
            {
                int neighbour = adjacency[currentVertex, column];
                if (neighbour == emptyValue)
                {
                    continue;
                }

                ValidateVertex(neighbour, verticesCount);

                int weight = weights[currentVertex, column];
                if (weight == emptyValue)
                {
                    throw new ArgumentException("Each adjacency entry must have a corresponding weight.");
                }

                if (weight < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(weights), "Dijkstra algorithm requires non-negative edge weights.");
                }

                if (visited[neighbour])
                {
                    continue;
                }

                long candidateDistance = (long)distances[currentVertex] + weight;
                if (candidateDistance > int.MaxValue)
                {
                    throw new OverflowException("The shortest path length exceeds Int32.MaxValue.");
                }

                if (candidateDistance < distances[neighbour])
                {
                    distances[neighbour] = (int)candidateDistance;
                    previous[neighbour] = currentVertex;
                }
            }
        }

        return BuildResult(previous, treeVertices, treeVertexCount);
    }

    private static Edge[] BuildResult(int[] previous, int[] treeVertices, int treeVertexCount)
    {
        Edge[] edges = new Edge[treeVertexCount];

        for (int i = 0; i < treeVertexCount; i++)
        {
            int vertex = treeVertices[i];
            int parent = previous[vertex];
            edges[i] = new Edge(parent, vertex);
        }

        return edges;
    }

    private static int FindNearestUnvisitedVertex(int[] distances, bool[] visited)
    {
        int bestVertex = -1;

        for (int vertex = 0; vertex < distances.Length; vertex++)
        {
            if (visited[vertex])
            {
                continue;
            }

            if (bestVertex == -1 || distances[vertex] < distances[bestVertex])
            {
                bestVertex = vertex;
            }
        }

        if (bestVertex == -1 || distances[bestVertex] == int.MaxValue)
        {
            return -1;
        }

        return bestVertex;
    }

    private static int[,] BuildUnitWeights(int[,] adjacency, int emptyValue)
    {
        int rows = adjacency.GetLength(0);
        int columns = adjacency.GetLength(1);
        int[,] weights = new int[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                weights[row, column] = adjacency[row, column] == emptyValue ? emptyValue : 1;
            }
        }

        return weights;
    }

    private static void ValidateInput(int[,] adjacency, int[,] weights)
    {
        if (adjacency.GetLength(0) != weights.GetLength(0) || adjacency.GetLength(1) != weights.GetLength(1))
        {
            throw new ArgumentException("Matrices adjacency and weights must have the same size.");
        }
    }

    private static void ValidateVertex(int vertex, int verticesCount)
    {
        if (vertex < 0 || vertex >= verticesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(vertex), $"Vertex index {vertex} is out of range 0..{verticesCount - 1}.");
        }
    }
}
