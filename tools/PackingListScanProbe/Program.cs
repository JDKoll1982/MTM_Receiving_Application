using System.Globalization;
using System.Text.Json;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MtmTools.PackingListScanProbe;

internal static class Program
{
    private const double DefaultPdfDpi = 400;

    /// <summary>400 dpi is the point where faint copier scans stop losing the item table.</summary>
    public const double DefaultPdfDpiValue = DefaultPdfDpi;

    private static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine(Usage);
            return 2;
        }

        var mode = args[0];
        if (mode is not ("text" or "dump" or "extract"))
        {
            Console.Error.WriteLine($"unknown mode '{mode}'\n\n{Usage}");
            return 2;
        }

        var options = new ScanOptions();
        var inputs = new List<string>();
        for (var i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--dpi" when i + 1 < args.Length:
                    options.Dpi = double.Parse(args[++i], CultureInfo.InvariantCulture);
                    break;
                case "--scale" when i + 1 < args.Length:
                    options.Scale = double.Parse(args[++i], CultureInfo.InvariantCulture);
                    break;
                case "--page" when i + 1 < args.Length:
                    options.Pages = args[++i]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(v => int.Parse(v, CultureInfo.InvariantCulture))
                        .ToHashSet();
                    break;
                case "--crop" when i + 1 < args.Length:
                    options.Crop = args[++i]
                        .Split(',')
                        .Select(v => double.Parse(v, CultureInfo.InvariantCulture))
                        .ToArray();
                    break;
                default:
                    inputs.Add(args[i]);
                    break;
            }
        }

        if (inputs.Count == 0)
        {
            Console.Error.WriteLine(Usage);
            return 2;
        }

        var engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en-US"))
                     ?? OcrEngine.TryCreateFromUserProfileLanguages();

        if (engine is null)
        {
            Console.Error.WriteLine(
                "No OCR engine available. Install an OCR language pack, for example in an elevated PowerShell:\n" +
                "  $cap = Get-WindowsCapability -Online | Where-Object { $_.Name -like 'Language.OCR*en-US*' }\n" +
                "  $cap | Add-WindowsCapability -Online");
            return 3;
        }

        Console.Error.WriteLine(
            $"# engine={engine.RecognizerLanguage.LanguageTag} maxImageDim={OcrEngine.MaxImageDimension} " +
            $"dpi={options.Dpi} scale={options.Scale}");

        var allFields = new List<ScanFields>();

        foreach (var input in inputs)
        {
            foreach (var (pageLabel, bitmap) in await LoadPagesAsync(input, options))
            {
                var result = await engine.RecognizeAsync(bitmap);
                var words = result.Lines
                    .SelectMany(line => line.Words.Select(word => new OcrWord(
                        word.Text,
                        word.BoundingRect.X,
                        word.BoundingRect.Y,
                        word.BoundingRect.Width,
                        word.BoundingRect.Height)))
                    .ToList();

                Console.WriteLine(
                    $"\n## {Path.GetFileName(input)}{pageLabel}  {bitmap.PixelWidth}x{bitmap.PixelHeight}  " +
                    $"lines={result.Lines.Count} words={words.Count}");

                switch (mode)
                {
                    case "dump":
                        foreach (var word in words)
                        {
                            Console.WriteLine($"  {word.Y,6:F0} {word.X,6:F0} {word.Width,5:F0}  {word.Text}");
                        }

                        break;

                    case "text":
                        foreach (var line in result.Lines)
                        {
                            var rect = line.Words.Count > 0 ? line.Words[0].BoundingRect : default;
                            Console.WriteLine($"  [{rect.Y,6:F0} {rect.X,6:F0}] {line.Text}");
                        }

                        break;

                    default:
                        var fields = ScanExtractor.Extract(words);
                        fields.Source = Path.GetFileName(input);
                        fields.Page = pageLabel;
                        fields.WordCount = words.Count;
                        allFields.Add(fields);

                        if (inputs.Count == 1)
                        {
                            Console.WriteLine(JsonSerializer.Serialize(fields, JsonOptions));
                        }

                        break;
                }
            }
        }

        if (mode == "extract")
        {
            if (inputs.Count > 1)
            {
                Console.WriteLine(JsonSerializer.Serialize(allFields, JsonOptions));
            }

            if (allFields.Any(f => f.Warnings.Count > 0))
            {
                Console.Error.WriteLine("\n# warnings:");
                foreach (var field in allFields.Where(f => f.Warnings.Count > 0))
                {
                    foreach (var warning in field.Warnings)
                    {
                        Console.Error.WriteLine($"  {field.Source}{field.Page}: {warning}");
                    }
                }
            }
        }

        return 0;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private const string Usage = """
        usage: PackingListScanProbe <text|dump|extract> <file...> [options]

          text      print OCR lines with their position (readable transcript)
          dump      print every OCR word with x/y/width (debugging)
          extract   print structured fields as JSON

        options
          --dpi N          PDF rasterization DPI (default 300)
          --scale N        multiplier for image inputs (default 1 = native pixels)
          --page N[,N]     only these PDF pages (1-based; images ignore this)
          --crop l,t,r,b   crop as fractions of the page, e.g. 0,0.2,0.6,0.6
        """;

    private static async Task<List<(string Label, SoftwareBitmap Bitmap)>> LoadPagesAsync(
        string path,
        ScanOptions options)
    {
        var full = Path.GetFullPath(path);
        if (!File.Exists(full))
        {
            throw new FileNotFoundException($"input not found: {full}");
        }

        if (Path.GetExtension(full).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return await LoadPdfPagesAsync(full, options);
        }

        var image = await LoadImageAsync(full, options.Scale, options.Crop);
        return [(string.Empty, image)];
    }

    private static async Task<List<(string, SoftwareBitmap)>> LoadPdfPagesAsync(string path, ScanOptions options)
    {
        var file = await StorageFile.GetFileFromPathAsync(path);
        var document = await PdfDocument.LoadFromFileAsync(file);
        var pages = new List<(string, SoftwareBitmap)>();

        for (uint index = 0; index < document.PageCount; index++)
        {
            var pageNumber = (int)index + 1;
            if (options.Pages is { Count: > 0 } && !options.Pages.Contains(pageNumber))
            {
                continue;
            }

            using var page = document.GetPage(index);
            var scale = options.Dpi / 96.0;
            var renderOptions = new PdfPageRenderOptions
            {
                DestinationWidth = ClampToOcrLimit(page.Size.Width * scale),
                DestinationHeight = ClampToOcrLimit(page.Size.Height * scale),
            };

            using var stream = new InMemoryRandomAccessStream();
            await page.RenderToStreamAsync(stream, renderOptions);
            stream.Seek(0);

            var decoder = await BitmapDecoder.CreateAsync(stream);
            var bitmap = await DecodeAsync(decoder, options.Crop);
            pages.Add(($" page {pageNumber}", bitmap));
        }

        return pages;
    }

    private static uint ClampToOcrLimit(double value)
    {
        var limit = (double)OcrEngine.MaxImageDimension;
        return (uint)Math.Max(1, Math.Min(value, limit));
    }

    private static async Task<SoftwareBitmap> LoadImageAsync(string path, double scale, double[]? crop)
    {
        var file = await StorageFile.GetFileFromPathAsync(path);
        using var stream = await file.OpenAsync(FileAccessMode.Read);
        var decoder = await BitmapDecoder.CreateAsync(stream);

        var limit = (double)OcrEngine.MaxImageDimension;
        var width = (crop is null ? decoder.PixelWidth : decoder.PixelWidth * (uint)((crop[2] - crop[0]) * 1000) / 1000.0);
        var height = (crop is null ? decoder.PixelHeight : decoder.PixelHeight * (crop[3] - crop[1]));
        var effectiveScale = Math.Min(scale, Math.Min(limit / width, limit / height));

        var bounds = crop is null
            ? new BitmapBounds { X = 0, Y = 0, Width = decoder.PixelWidth, Height = decoder.PixelHeight }
            : new BitmapBounds
            {
                X = (uint)(decoder.PixelWidth * crop[0]),
                Y = (uint)(decoder.PixelHeight * crop[1]),
                Width = (uint)(decoder.PixelWidth * (crop[2] - crop[0])),
                Height = (uint)(decoder.PixelHeight * (crop[3] - crop[1])),
            };

        var transform = new BitmapTransform
        {
            Bounds = bounds,
            ScaledWidth = Math.Max(1, (uint)Math.Round(bounds.Width * effectiveScale)),
            ScaledHeight = Math.Max(1, (uint)Math.Round(bounds.Height * effectiveScale)),
            InterpolationMode = BitmapInterpolationMode.Fant,
        };

        return await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            transform,
            ExifOrientationMode.RespectExifOrientation,
            ColorManagementMode.DoNotColorManage);
    }

    private static async Task<SoftwareBitmap> DecodeAsync(BitmapDecoder decoder, double[]? crop)
    {
        if (crop is null)
        {
            return await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                new BitmapTransform(),
                ExifOrientationMode.RespectExifOrientation,
                ColorManagementMode.DoNotColorManage);
        }

        var transform = new BitmapTransform
        {
            Bounds = new BitmapBounds
            {
                X = (uint)(decoder.PixelWidth * crop[0]),
                Y = (uint)(decoder.PixelHeight * crop[1]),
                Width = (uint)(decoder.PixelWidth * (crop[2] - crop[0])),
                Height = (uint)(decoder.PixelHeight * (crop[3] - crop[1])),
            },
        };

        return await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            transform,
            ExifOrientationMode.RespectExifOrientation,
            ColorManagementMode.DoNotColorManage);
    }
}

internal sealed class ScanOptions
{
    public double Dpi { get; set; } = Program.DefaultPdfDpiValue;
    public double Scale { get; set; } = 1.0;
    public HashSet<int>? Pages { get; set; }
    public double[]? Crop { get; set; }
}

internal sealed record OcrWord(string Text, double X, double Y, double Width, double Height);
