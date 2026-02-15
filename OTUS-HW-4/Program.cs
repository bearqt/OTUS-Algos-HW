using OTUS_HW_4.Benchmarking;
using OTUS_HW_4.PriorityQueues;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("OTUS HW-4");
Console.WriteLine();

BenchmarkRunner.RunAndSave("benchmark-results.md");

Console.WriteLine();
Console.WriteLine("PriorityQueue demo:");

var queue = new PriorityQueue<string>();
queue.Enqueue(1, "low");
queue.Enqueue(3, "high");
queue.Enqueue(2, "medium");

while (queue.Count > 0)
{
    Console.WriteLine(queue.Dequeue());
}

