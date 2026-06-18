using web.av.AngelVisionLpr;
using web.av.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;

var arguments = DemoArguments.Parse(args);

if (arguments.ShowHelp)
{
    DemoArguments.PrintUsage();
    return 0;
}

try
{
    NativeLibraryLoader.Configure();

    if (arguments.Threads > 0)
    {
        // Глобальная настройка SDK: сколько рабочих потоков может использовать распознавание.
        NativeMethods.SetNumberOfThreads(arguments.Threads);
    }

    using var frame = BmpFrameReader.Read(arguments.ImagePath);

    var options = new AitAvOptions
    {
        Type = arguments.Type,
        Tracking = arguments.Tracking,
        FramesBeforeLosing = arguments.FramesBeforeLosing,
        MinPlateWidth = arguments.MinPlateWidth,
        MaxPlateWidth = arguments.MaxPlateWidth,
        MinProbability = arguments.MinProbability,
        Area = arguments.Area ?? new AitRect(0, 0, frame.Width, frame.Height)
    };

    using var recognizer = new LprRecognizer(options);

    // Данные кадра должны оставаться по одному и тому же адресу в памяти на время native-вызова.
    // Поэтому managed byte[] фиксируется через GCHandle и освобождается сразу после Recognize.
    using var pinnedFrame = PinnedFrame.Pin(frame);
    using var plateBuffer = recognizer.Recognize(pinnedFrame.Image, arguments.FrameId, arguments.Info);   

    var found = false;

    foreach (var plate in plateBuffer.PopAll())
    {
        found = true;
        PrintPlate(plate);        
        PlateNativeMemory.ReleaseOwnedBuffers(plate);
    }

    if (!found)
    {
        Console.WriteLine("Номера не найдены.");
    }

    return 0;
}
catch (DllNotFoundException exception)
{
    Console.Error.WriteLine("Не удалось загрузить native-библиотеку AngelVision LPR.");
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine("Положите libav_lpr_c.dll рядом с приложением или в папку libs/win-x64.");
    return 2;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}

static void PrintPlate(AitAvLicensePlate plate)
{
    var data = plate.Data;

    Console.WriteLine($"State: {plate.State}");
    Console.WriteLine($"Plate: {data.PlateText}");
    Console.WriteLine($"Country: {plate.Country}");
    Console.WriteLine($"Probability: {data.Probability:P1}");
    Console.WriteLine($"Object id: {data.Identifier}");
    Console.WriteLine($"Frame id: {plate.FrameId}");
    Console.WriteLine($"Rect: x={data.Position.X}, y={data.Position.Y}, w={data.Position.Width}, h={data.Position.Height}");
    Console.WriteLine();
}

internal sealed class DemoArguments
{
    public string ImagePath { get; private init; } = string.Empty;
    public int MinPlateWidth { get; private init; } = 40;
    public int MaxPlateWidth { get; private init; } = 600;
    public bool Tracking { get; private init; } = true;
    public int FramesBeforeLosing { get; private init; } = 5;
    public double MinProbability { get; private init; } = 0.6;
    public TypeRecognizer Type { get; private init; } = TypeRecognizer.CldDnn;
    public int Threads { get; private init; } = Environment.ProcessorCount;
    public ulong FrameId { get; private init; } = 1;
    public string Info { get; private init; } = string.Empty;
    public AitRect? Area { get; private init; }
    public bool ShowHelp { get; private init; }

    public static DemoArguments Parse(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase) || args.Contains("-h", StringComparer.OrdinalIgnoreCase))
        {
            return new DemoArguments { ShowHelp = true };
        }

        var result = new DemoArguments { ImagePath = args[0] };

        for (var i = 1; i < args.Length; i++)
        {
            var name = args[i];
            var value = ReadValue(args, ref i, name);

            result = name.ToLowerInvariant() switch
            {
                "--min-width" => result.WithValue(minPlateWidth: int.Parse(value, CultureInfo.InvariantCulture)),
                "--max-width" => result.WithValue(maxPlateWidth: int.Parse(value, CultureInfo.InvariantCulture)),
                "--min-probability" => result.WithValue(minProbability: double.Parse(value, CultureInfo.InvariantCulture)),
                "--tracking" => result.WithValue(tracking: bool.Parse(value)),
                "--frames-before-losing" => result.WithValue(framesBeforeLosing: int.Parse(value, CultureInfo.InvariantCulture)),
                "--type" => result.WithValue(type: Enum.Parse<TypeRecognizer>(value, ignoreCase: true)),
                "--threads" => result.WithValue(threads: int.Parse(value, CultureInfo.InvariantCulture)),
                "--frame-id" => result.WithValue(frameId: ulong.Parse(value, CultureInfo.InvariantCulture)),
                "--info" => result.WithValue(info: value),
                "--area" => result.WithValue(area: ParseArea(value)),
                _ => throw new ArgumentException($"Неизвестный аргумент: {name}")
            };
        }

        return result;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("AngelVision LPR demo");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project samples/AngelVisionLprDemo -- image.bmp [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --min-width 40");
        Console.WriteLine("  --max-width 600");
        Console.WriteLine("  --min-probability 0.6");
        Console.WriteLine("  --tracking true");
        Console.WriteLine("  --frames-before-losing 5");
        Console.WriteLine("  --type CldDnn");
        Console.WriteLine("  --threads 8");
        Console.WriteLine("  --frame-id 1");
        Console.WriteLine("  --info camera-1");
        Console.WriteLine("  --area x,y,width,height");
    }

    private static string ReadValue(string[] args, ref int index, string name)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Для аргумента {name} не задано значение.");
        }

        index++;
        return args[index];
    }

    private static AitRect ParseArea(string value)
    {
        var parts = value.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 4)
        {
            throw new ArgumentException("Area задается в формате x,y,width,height.");
        }

        return new AitRect(
            int.Parse(parts[0], CultureInfo.InvariantCulture),
            int.Parse(parts[1], CultureInfo.InvariantCulture),
            int.Parse(parts[2], CultureInfo.InvariantCulture),
            int.Parse(parts[3], CultureInfo.InvariantCulture));
    }

    private DemoArguments WithValue(
        string? imagePath = null,
        int? minPlateWidth = null,
        int? maxPlateWidth = null,
        bool? tracking = null,
        int? framesBeforeLosing = null,
        double? minProbability = null,
        TypeRecognizer? type = null,
        int? threads = null,
        ulong? frameId = null,
        string? info = null,
        AitRect? area = null)
    {
        return new DemoArguments
        {
            ImagePath = imagePath ?? ImagePath,
            MinPlateWidth = minPlateWidth ?? MinPlateWidth,
            MaxPlateWidth = maxPlateWidth ?? MaxPlateWidth,
            Tracking = tracking ?? Tracking,
            FramesBeforeLosing = framesBeforeLosing ?? FramesBeforeLosing,
            MinProbability = minProbability ?? MinProbability,
            Type = type ?? Type,
            Threads = threads ?? Threads,
            FrameId = frameId ?? FrameId,
            Info = info ?? Info,
            Area = area ?? Area
        };
    }
}
