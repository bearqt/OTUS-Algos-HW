var table = new HashTable<string, int>();
table.Add("apple", 10);
table.Add("banana", 20);
table["orange"] = 30;
table["banana"] = 25;

Console.WriteLine($"Count: {table.Count}");
Console.WriteLine($"Contains 'apple': {table.ContainsKey("apple")}");
Console.WriteLine($"Contains 'grape': {table.ContainsKey("grape")}");

if (table.TryGetValue("banana", out var bananaValue))
{
    Console.WriteLine($"banana = {bananaValue}");
}

table.Remove("apple");
Console.WriteLine($"Count after remove: {table.Count}");
