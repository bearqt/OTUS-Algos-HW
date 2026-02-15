using System.Numerics;

string? input = Console.ReadLine();
if (!int.TryParse(input, out int position) || position < 0 || position > 63)
{
    return;
}

// Active solution for the current task: knight.
var (moveCount, moves) = CalculateKnightMoves(position);
Console.WriteLine(moveCount);
Console.WriteLine(moves);

static (int count, ulong mask) CalculateKingMoves(int position)
{
    ulong king = 1UL << position;
    const ulong NotAFile = 0xFEFEFEFEFEFEFEFEUL;
    const ulong NotHFile = 0x7F7F7F7F7F7F7F7FUL;

    ulong moves =
        (king << 8) |
        (king >> 8) |
        ((king & NotHFile) << 1) |
        ((king & NotAFile) >> 1) |
        ((king & NotHFile) << 9) |
        ((king & NotAFile) << 7) |
        ((king & NotHFile) >> 7) |
        ((king & NotAFile) >> 9);

    int moveCount = BitOperations.PopCount(moves);
    return (moveCount, moves);
}

static (int count, ulong mask) CalculateKnightMoves(int position)
{
    ulong knight = 1UL << position;

    const ulong NotAFile = 0xFEFEFEFEFEFEFEFEUL;
    const ulong NotABFile = 0xFCFCFCFCFCFCFCFCUL;
    const ulong NotHFile = 0x7F7F7F7F7F7F7F7FUL;
    const ulong NotGHFile = 0x3F3F3F3F3F3F3F3FUL;

    ulong moves =
        ((knight & NotHFile) << 17) |
        ((knight & NotAFile) << 15) |
        ((knight & NotGHFile) << 10) |
        ((knight & NotABFile) << 6) |
        ((knight & NotHFile) >> 15) |
        ((knight & NotAFile) >> 17) |
        ((knight & NotGHFile) >> 6) |
        ((knight & NotABFile) >> 10);

    int moveCount = BitOperations.PopCount(moves);
    return (moveCount, moves);
}

static int CountBitsByShift(ulong value)
{
    int count = 0;
    while (value != 0)
    {
        count += (int)(value & 1UL);
        value >>= 1;
    }

    return count;
}

static int CountBitsByKernighan(ulong value)
{
    int count = 0;
    while (value != 0)
    {
        value &= value - 1;
        count++;
    }

    return count;
}