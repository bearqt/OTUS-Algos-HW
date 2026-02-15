// O(N): multiply base by itself exponent times.
static long PowerIterative(long number, int exponent)
{
    if (exponent < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent must be non-negative.");
    }

    long result = 1;
    for (var i = 0; i < exponent; i++)
    {
        result *= number;
    }

    return result;
}

// O(2^N): naive recursion by definition.
static long FibonacciRecursive(int n)
{
    if (n < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(n), "n must be non-negative.");
    }

    if (n <= 1)
    {
        return n;
    }

    return FibonacciRecursive(n - 1) + FibonacciRecursive(n - 2);
}

// O(N): iterative accumulation.
static long FibonacciIterative(int n)
{
    if (n < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(n), "n must be non-negative.");
    }

    if (n <= 1)
    {
        return n;
    }

    long prev = 0;
    long current = 1;

    for (var i = 2; i <= n; i++)
    {
        var next = prev + current;
        prev = current;
        current = next;
    }

    return current;
}

// O(N^2): for each number from 2 to n, test all divisors from 2 to number-1.
static int CountPrimesByDivisors(int n)
{
    if (n < 2)
    {
        return 0;
    }

    var count = 0;

    for (var number = 2; number <= n; number++)
    {
        var isPrime = true;

        for (var divisor = 2; divisor < number; divisor++)
        {
            if (number % divisor == 0)
            {
                isPrime = false;
                break;
            }
        }

        if (isPrime)
        {
            count++;
        }
    }

    return count;
}

Console.WriteLine("1) Iterative O(N) power:");
Console.WriteLine($"2^10 = {PowerIterative(2, 10)}");
Console.WriteLine($"5^3 = {PowerIterative(5, 3)}");

Console.WriteLine();
Console.WriteLine("2) Fibonacci:");
var n = 10;
Console.WriteLine($"Recursive O(2^N), F({n}) = {FibonacciRecursive(n)}");
Console.WriteLine($"Iterative O(N), F({n}) = {FibonacciIterative(n)}");

Console.WriteLine();
Console.WriteLine("3) Count primes with divisor enumeration O(N^2):");
var limit = 100;
Console.WriteLine($"Primes count in [1..{limit}] = {CountPrimesByDivisors(limit)}");