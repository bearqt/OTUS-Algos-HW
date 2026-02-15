int[] sumCounts = new int[28];

for (int a = 0; a <= 9; a++)
{
    for (int b = 0; b <= 9; b++)
    {
        for (int c = 0; c <= 9; c++)
        {
            sumCounts[a + b + c]++;
        }
    }
}

long luckyTickets = 0;
for (int s = 0; s <= 27; s++)
{
    luckyTickets += (long)sumCounts[s] * sumCounts[s];
}

Console.WriteLine(luckyTickets);
