using System.Numerics;

var taskInput = Console.In.ReadToEnd();

if (args.Length == 0)
{
    Console.Error.WriteLine("Specify the task number: 1, 2, 3 or 4.");
    return;
}

switch (args[0])
{
    case "1":
        SolveFractionSum(taskInput);
        break;
    case "2":
        SolveTreeGarland(taskInput);
        break;
    case "3":
        SolveFiveAndEight(taskInput);
        break;
    case "4":
        SolveBigIsland(taskInput);
        break;
    default:
        Console.Error.WriteLine("Unknown task number. Use 1, 2, 3 or 4.");
        break;
}

static void SolveFractionSum(string input)
{
    var expression = input.Trim();

    if (expression.Length == 0)
    {
        return;
    }

    var parts = expression.Split('+');
    var firstFraction = parts[0].Split('/');
    var secondFraction = parts[1].Split('/');

    long a = long.Parse(firstFraction[0]);
    long b = long.Parse(firstFraction[1]);
    long c = long.Parse(secondFraction[0]);
    long d = long.Parse(secondFraction[1]);

    long numerator = a * d + c * b;
    long denominator = b * d;
    long gcd = Gcd(numerator, denominator);

    Console.WriteLine($"{numerator / gcd}/{denominator / gcd}");
}

static void SolveTreeGarland(string input)
{
    var reader = new TokenReader(input);
    int height = reader.NextInt();
    var dp = new int[height][];

    for (int row = 0; row < height; row++)
    {
        dp[row] = new int[row + 1];

        for (int col = 0; col <= row; col++)
        {
            int value = reader.NextInt();

            if (row == 0)
            {
                dp[row][col] = value;
                continue;
            }

            int bestParent = int.MinValue;

            if (col < row)
            {
                bestParent = Math.Max(bestParent, dp[row - 1][col]);
            }

            if (col > 0)
            {
                bestParent = Math.Max(bestParent, dp[row - 1][col - 1]);
            }

            dp[row][col] = bestParent + value;
        }
    }

    Console.WriteLine(dp[height - 1].Max());
}

static void SolveFiveAndEight(string input)
{
    var reader = new TokenReader(input);
    int n = reader.NextInt();

    BigInteger sequencesEndingWithRunLengthOne = 2;
    BigInteger sequencesEndingWithRunLengthTwo = 0;

    for (int length = 2; length <= n; length++)
    {
        BigInteger nextRunLengthOne = sequencesEndingWithRunLengthOne + sequencesEndingWithRunLengthTwo;
        BigInteger nextRunLengthTwo = sequencesEndingWithRunLengthOne;

        sequencesEndingWithRunLengthOne = nextRunLengthOne;
        sequencesEndingWithRunLengthTwo = nextRunLengthTwo;
    }

    Console.WriteLine(sequencesEndingWithRunLengthOne + sequencesEndingWithRunLengthTwo);
}

static void SolveBigIsland(string input)
{
    var reader = new TokenReader(input);
    int size = reader.NextInt();
    var grid = new int[size, size];
    var visited = new bool[size, size];

    for (int row = 0; row < size; row++)
    {
        for (int col = 0; col < size; col++)
        {
            grid[row, col] = reader.NextInt();
        }
    }

    int islands = 0;
    int[] dRow = [-1, 1, 0, 0];
    int[] dCol = [0, 0, -1, 1];
    var queue = new Queue<(int Row, int Col)>();

    for (int row = 0; row < size; row++)
    {
        for (int col = 0; col < size; col++)
        {
            if (grid[row, col] == 0 || visited[row, col])
            {
                continue;
            }

            islands++;
            visited[row, col] = true;
            queue.Enqueue((row, col));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                for (int direction = 0; direction < 4; direction++)
                {
                    int nextRow = current.Row + dRow[direction];
                    int nextCol = current.Col + dCol[direction];

                    if (nextRow < 0 || nextRow >= size || nextCol < 0 || nextCol >= size)
                    {
                        continue;
                    }

                    if (grid[nextRow, nextCol] == 0 || visited[nextRow, nextCol])
                    {
                        continue;
                    }

                    visited[nextRow, nextCol] = true;
                    queue.Enqueue((nextRow, nextCol));
                }
            }
        }
    }

    Console.WriteLine(islands);
}

static long Gcd(long x, long y)
{
    while (y != 0)
    {
        (x, y) = (y, x % y);
    }

    return Math.Abs(x);
}

sealed class TokenReader(string input)
{
    private readonly string _input = input;
    private int _position;

    public int NextInt() => int.Parse(NextToken());

    public string NextToken()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            _position++;
        }

        int start = _position;

        while (_position < _input.Length && !char.IsWhiteSpace(_input[_position]))
        {
            _position++;
        }

        return start < _position
            ? _input[start.._position]
            : throw new InvalidOperationException("Unexpected end of input.");
    }
}
