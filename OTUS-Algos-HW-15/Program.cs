namespace OTUS_Algos_HW_15;

internal static class Program
{
    private const int NoEdge = -1;

    private static void Main()
    {
        // Граф задан как A[N][Smax].
        // Пустые ячейки отмечены -1, так как вершины нумеруются с 0.
        int[][] adjacencyMatrix =
        {
            new[] { 2, 3, NoEdge },
            new[] { 3, 4, NoEdge },
            new[] { 5, NoEdge, NoEdge },
            new[] { 5, NoEdge, NoEdge },
            new[] { 6, NoEdge, NoEdge },
            new[] { 6, NoEdge, NoEdge },
            new[] { NoEdge, NoEdge, NoEdge }
        };

        int[][] levels = BuildLevelsByDemukron(adjacencyMatrix, NoEdge);
        PrintLevels(levels);
    }

    public static int[][] BuildLevelsByDemukron(int[][] adjacencyMatrix, int noEdge = NoEdge)
    {
        ArgumentNullException.ThrowIfNull(adjacencyMatrix);

        int vertexCount = adjacencyMatrix.Length;
        int[] inDegree = new int[vertexCount];
        bool[] processed = new bool[vertexCount];
        int[][] levelBuffer = new int[vertexCount][];
        int levelCount = 0;
        int remainingVertices = vertexCount;

        for (int vertex = 0; vertex < vertexCount; vertex++)
        {
            int[] neighbours = adjacencyMatrix[vertex] ?? Array.Empty<int>();

            for (int i = 0; i < neighbours.Length; i++)
            {
                int neighbour = neighbours[i];
                if (neighbour == noEdge)
                {
                    continue;
                }

                ValidateVertex(neighbour, vertexCount);
                inDegree[neighbour]++;
            }
        }

        while (remainingVertices > 0)
        {
            int verticesOnLevel = 0;

            for (int vertex = 0; vertex < vertexCount; vertex++)
            {
                if (!processed[vertex] && inDegree[vertex] == 0)
                {
                    verticesOnLevel++;
                }
            }

            if (verticesOnLevel == 0)
            {
                throw new InvalidOperationException("Алгоритм Демукрона применим только к ацикличному графу.");
            }

            int[] currentLevel = new int[verticesOnLevel];
            int currentLevelIndex = 0;

            for (int vertex = 0; vertex < vertexCount; vertex++)
            {
                if (!processed[vertex] && inDegree[vertex] == 0)
                {
                    processed[vertex] = true;
                    currentLevel[currentLevelIndex++] = vertex;
                    remainingVertices--;
                }
            }

            for (int i = 0; i < currentLevel.Length; i++)
            {
                int vertex = currentLevel[i];
                int[] neighbours = adjacencyMatrix[vertex] ?? Array.Empty<int>();

                for (int j = 0; j < neighbours.Length; j++)
                {
                    int neighbour = neighbours[j];
                    if (neighbour == noEdge)
                    {
                        continue;
                    }

                    inDegree[neighbour]--;
                }
            }

            levelBuffer[levelCount++] = currentLevel;
        }

        int[][] levels = new int[levelCount][];
        Array.Copy(levelBuffer, levels, levelCount);
        return levels;
    }

    private static void ValidateVertex(int vertex, int vertexCount)
    {
        if (vertex < 0 || vertex >= vertexCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vertex),
                $"Вершина {vertex} выходит за границы графа 0..{vertexCount - 1}.");
        }
    }

    private static void PrintLevels(int[][] levels)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            Console.Write($"level[{i}] = [");

            for (int j = 0; j < levels[i].Length; j++)
            {
                if (j > 0)
                {
                    Console.Write(", ");
                }

                Console.Write(levels[i][j]);
            }

            Console.WriteLine("]");
        }
    }
}
