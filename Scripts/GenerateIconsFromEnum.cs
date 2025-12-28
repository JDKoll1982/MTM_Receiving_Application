#r "nuget: FluentIcons.Common, 1.1.253"
#r "nuget: FluentIcons.WinUI, 1.1.253"

using System;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
using FluentIcons.Common;
using FluentIcons.WinUI;

Console.WriteLine("=== FluentIcons.WinUI Icon Extractor ===\n");

// Try to find the Symbol enum directly
var symbolType = typeof(Symbol);
var assembly = symbolType.Assembly;

Console.WriteLine($"Assembly: {assembly.GetName().Name} v{assembly.GetName().Version}");
Console.WriteLine($"Symbol type: {symbolType.FullName}\n");

if (symbolType.IsEnum)
{
    Console.WriteLine("Symbol is an enum - extracting values...\n");

    var icons = Enum.GetValues(symbolType)
        .Cast<object>()
        .Select(icon =>
        {
            int value = Convert.ToInt32(icon);
            string glyph;

            // Handle high Unicode codepoints (above 0xFFFF) which need surrogate pairs
            if (value > 0xFFFF)
            {
                glyph = char.ConvertFromUtf32(value);
            }
            else
            {
                glyph = ((char)value).ToString();
            }

            return new
            {
                name = icon.ToString(),
                value = value,
                glyph = glyph,
                codepoint = value
            };
        })
        .Where(x => x.value >= 0xE000) // Filter out control characters
        .OrderBy(x => x.name)
        .ToList();

    var json = JsonSerializer.Serialize(icons, new JsonSerializerOptions { WriteIndented = true });

    // Update FluentIcons.json (the main file used by the app)
    File.WriteAllText("Assets/FluentIcons.json", json);

    Console.WriteLine($"✓ SUCCESS: Generated {icons.Count} icons");
    Console.WriteLine($"✓ Updated: Assets/FluentIcons.json");
    Console.WriteLine($"\nFirst 5 icons:");
    foreach (var icon in icons.Take(5))
    {
        Console.WriteLine($"  - {icon.name} (U+{icon.value:X4})");
    }
}
else
{
    Console.WriteLine("Could not find Symbol enum.");
}
