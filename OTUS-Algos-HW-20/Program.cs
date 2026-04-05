using System.Text;

return App.Run(args);

static class App
{
    private static readonly byte[] Magic = [(byte)'R', (byte)'L', (byte)'E', (byte)'2'];
    private const byte FormatVersion = 1;

    public static int Run(string[] args)
    {
        if (args.Length == 0 || IsHelpCommand(args[0]))
        {
            PrintUsage();
            return 0;
        }

        if (args.Length is < 2 or > 3)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0].Trim().ToLowerInvariant();
        var inputPath = Path.GetFullPath(args[1]);
        var outputPath = Path.GetFullPath(args.Length == 3 ? args[2] : GetDefaultOutputPath(command, inputPath));

        try
        {
            if (string.Equals(inputPath, outputPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Входной и выходной файлы должны отличаться.");
            }

            return command switch
            {
                "compress" or "c" => CompressFile(inputPath, outputPath),
                "decompress" or "d" => DecompressFile(inputPath, outputPath),
                _ => UnknownCommand()
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Ошибка: {ex.Message}");
            return 1;
        }
    }

    private static int CompressFile(string inputPath, string outputPath)
    {
        EnsureInputExists(inputPath);
        EnsureOutputDirectory(outputPath);

        var sourceBytes = File.ReadAllBytes(inputPath);
        var packedBytes = ImprovedRle.Compress(sourceBytes);

        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);

        writer.Write(Magic);
        writer.Write(FormatVersion);
        writer.Write(sourceBytes.LongLength);
        writer.Write(packedBytes);

        PrintStats("Сжатие завершено", inputPath, outputPath, sourceBytes.LongLength, stream.Length);
        return 0;
    }

    private static int DecompressFile(string inputPath, string outputPath)
    {
        EnsureInputExists(inputPath);
        EnsureOutputDirectory(outputPath);

        using var stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);

        var header = reader.ReadBytes(Magic.Length);
        if (!header.SequenceEqual(Magic))
        {
            throw new InvalidDataException("Файл не похож на архив улучшенного RLE формата.");
        }

        var version = reader.ReadByte();
        if (version != FormatVersion)
        {
            throw new InvalidDataException($"Неподдерживаемая версия формата: {version}.");
        }

        var originalLength = reader.ReadInt64();
        if (originalLength < 0)
        {
            throw new InvalidDataException("Некорректная длина исходных данных в заголовке.");
        }

        var packedBytes = reader.ReadBytes(checked((int)(stream.Length - stream.Position)));
        var restoredBytes = ImprovedRle.Decompress(packedBytes, originalLength);

        File.WriteAllBytes(outputPath, restoredBytes);

        PrintStats("Распаковка завершена", inputPath, outputPath, stream.Length, restoredBytes.LongLength);
        return 0;
    }

    private static int UnknownCommand()
    {
        Console.Error.WriteLine("Неизвестная команда.");
        PrintUsage();
        return 1;
    }

    private static bool IsHelpCommand(string value) =>
        value.Equals("help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("--help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("-h", StringComparison.OrdinalIgnoreCase)
        || value.Equals("/?", StringComparison.OrdinalIgnoreCase);

    private static string GetDefaultOutputPath(string command, string inputPath) =>
        command switch
        {
            "compress" or "c" => $"{inputPath}.rle",
            "decompress" or "d" => GetDefaultDecompressedPath(inputPath),
            _ => inputPath
        };

    private static string GetDefaultDecompressedPath(string inputPath)
    {
        var directory = Path.GetDirectoryName(inputPath) ?? Directory.GetCurrentDirectory();
        var fileName = Path.GetFileName(inputPath);

        if (fileName.EndsWith(".rle", StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(directory, Path.GetFileNameWithoutExtension(fileName));
        }

        return Path.Combine(directory, $"{fileName}.decoded");
    }

    private static void EnsureInputExists(string inputPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Входной файл не найден.", inputPath);
        }
    }

    private static void EnsureOutputDirectory(string outputPath)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static void PrintStats(string title, string inputPath, string outputPath, long inputSize, long outputSize)
    {
        Console.WriteLine(title);
        Console.WriteLine($"Вход:  {inputPath}");
        Console.WriteLine($"Выход: {outputPath}");
        Console.WriteLine($"Размер: {inputSize} -> {outputSize} байт");

        if (inputSize > 0)
        {
            var ratio = (double)outputSize / inputSize;
            Console.WriteLine($"Коэффициент: {ratio:F3}");
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("RLE архиватор файлов");
        Console.WriteLine("Используется улучшенный RLE: повторы и неповторяющиеся байты кодируются разными блоками.");
        Console.WriteLine();
        Console.WriteLine("Запуск:");
        Console.WriteLine("  dotnet run -- compress <inputFile> [outputFile]");
        Console.WriteLine("  dotnet run -- decompress <inputFile> [outputFile]");
        Console.WriteLine();
        Console.WriteLine("Короткие команды:");
        Console.WriteLine("  dotnet run -- c <inputFile> [outputFile]");
        Console.WriteLine("  dotnet run -- d <inputFile> [outputFile]");
        Console.WriteLine();
        Console.WriteLine("Примеры:");
        Console.WriteLine("  dotnet run -- compress data.bin");
        Console.WriteLine("  dotnet run -- decompress data.bin.rle restored.bin");
    }
}

static class ImprovedRle
{
    private const int MaxBlockLength = 128;
    private const int RepeatMask = 0b1000_0000;
    private const int MinRepeatRunLength = 3;

    public static byte[] Compress(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0)
        {
            return [];
        }

        var result = new List<byte>(data.Length);
        var index = 0;

        while (index < data.Length)
        {
            var repeatLength = CountRepeatedBytes(data, index);
            if (repeatLength >= MinRepeatRunLength)
            {
                index = WriteRepeatBlocks(data[index], repeatLength, result, index);
                continue;
            }

            var literalStart = index;
            var literalLength = 0;

            while (index < data.Length && literalLength < MaxBlockLength)
            {
                repeatLength = CountRepeatedBytes(data, index);
                if (repeatLength >= MinRepeatRunLength)
                {
                    break;
                }

                index++;
                literalLength++;
            }

            WriteLiteralBlock(data, literalStart, literalLength, result);
        }

        return [.. result];
    }

    public static byte[] Decompress(byte[] data, long? expectedLength = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (expectedLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedLength));
        }

        var capacity = expectedLength switch
        {
            null => data.Length,
            > int.MaxValue => throw new InvalidDataException("Размер распакованных данных превышает поддерживаемый массив byte[]."),
            _ => (int)expectedLength.Value
        };

        var result = new List<byte>(capacity);
        var index = 0;

        while (index < data.Length)
        {
            var header = data[index++];
            var blockLength = (header & 0b0111_1111) + 1;
            var isRepeatBlock = (header & RepeatMask) != 0;

            if (isRepeatBlock)
            {
                if (index >= data.Length)
                {
                    throw new InvalidDataException("Поврежденные данные: отсутствует байт для блока повторов.");
                }

                var value = data[index++];
                for (var i = 0; i < blockLength; i++)
                {
                    result.Add(value);
                }

                continue;
            }

            if (index + blockLength > data.Length)
            {
                throw new InvalidDataException("Поврежденные данные: литеральный блок выходит за границы массива.");
            }

            for (var i = 0; i < blockLength; i++)
            {
                result.Add(data[index + i]);
            }

            index += blockLength;
        }

        if (expectedLength.HasValue && result.Count != expectedLength.Value)
        {
            throw new InvalidDataException(
                $"Ожидалось {expectedLength} байт после распаковки, получено {result.Count}.");
        }

        return [.. result];
    }

    private static int WriteRepeatBlocks(byte value, int repeatLength, List<byte> result, int index)
    {
        var remaining = repeatLength;

        while (remaining > 0)
        {
            var chunkLength = Math.Min(remaining, MaxBlockLength);
            result.Add((byte)(RepeatMask | (chunkLength - 1)));
            result.Add(value);
            remaining -= chunkLength;
            index += chunkLength;
        }

        return index;
    }

    private static void WriteLiteralBlock(byte[] data, int start, int length, List<byte> result)
    {
        result.Add((byte)(length - 1));

        for (var i = 0; i < length; i++)
        {
            result.Add(data[start + i]);
        }
    }

    private static int CountRepeatedBytes(byte[] data, int start)
    {
        var value = data[start];
        var length = 1;

        while (start + length < data.Length && data[start + length] == value)
        {
            length++;
        }

        return length;
    }
}
