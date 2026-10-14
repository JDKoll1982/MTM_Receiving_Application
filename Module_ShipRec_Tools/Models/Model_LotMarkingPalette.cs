using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One shape a marking can use. The glyph is the value persisted in the ledger, so a lot
/// marked today still reads the same after any change to this list.
/// </summary>
/// <param name="Glyph">The emoji drawn for the shape, and the value stored against the lot.</param>
/// <param name="DisplayName">The accessible label, used as the tooltip and screen-reader text.</param>
/// <param name="SortOrder">The order shapes are listed in.</param>
public sealed record Model_LotMarkingShape(string Glyph, string DisplayName, int SortOrder);

/// <summary>
/// One color a marking can use.
/// </summary>
/// <param name="Code">The stable code persisted in the ledger. Never a display name.</param>
/// <param name="DisplayName">The name a user reads.</param>
/// <param name="HexToken">The color used to draw the swatch. Never persisted.</param>
/// <param name="SortOrder">The order colors are listed in.</param>
public sealed record Model_LotMarkingColor(
    string Code,
    string DisplayName,
    string HexToken,
    int SortOrder
);

/// <summary>
/// The fixed set of shapes and colors a marking can use, with lookups by code.
/// </summary>
public static class Model_LotMarkingPalette
{
    /// <summary>
    /// The shapes, in listing order. Every value is an emoji that Segoe UI Emoji draws, so no
    /// shape can render as a missing-glyph box.
    /// </summary>
    /// <remarks>
    /// Only shapes with a long-established emoji are offered. Cylinder, hexagon and pentagon were
    /// dropped because no dependable emoji exists for them, and oval became a circle because the
    /// emoji set has no oval. Changing this list never alters a marking already recorded, because
    /// the stored value is the glyph itself.
    /// </remarks>
    public static IReadOnlyList<Model_LotMarkingShape> Shapes { get; } =
    [
        new("⚡", "Lightning Bolt", 1),
        new("💗", "Heart", 2),
        new("🌙", "Crescent Moon", 3),
        new("⭐", "Star", 4),
        new("💎", "Diamond", 5),
        new("⬛", "Square", 6),
        new("🔺", "Triangle", 7),
        new("⭕", "Circle", 8),
        new("❌", "Cross", 9),
        new("➡️", "Arrow", 10),
    ];

    /// <summary>
    /// The colors, in listing order.
    /// </summary>
    /// <remarks>
    /// White and black carry the same swatch treatment as every other color and rely on the view's
    /// outline to stay visible against both light and dark backgrounds. Draw the outline from the
    /// palette value rather than a fixed border, or one of the two disappears on one theme.
    /// </remarks>
    public static IReadOnlyList<Model_LotMarkingColor> Colors { get; } =
    [
        new("BLUE", "Blue", "#1E6FD9", 1),
        new("RED", "Red", "#D93025", 2),
        new("GREEN", "Green", "#1E8E3E", 3),
        new("WHITE", "White", "#FFFFFF", 4),
        new("YELLOW", "Yellow", "#F9AB00", 5),
        new("BLACK", "Black", "#202124", 6),
        new("PURPLE", "Purple", "#7B1FA2", 7),
        new("ORANGE", "Orange", "#E8710A", 8),
        new("PINK", "Pink", "#E91E63", 9),
        new("GRAY", "Gray", "#80868B", 10),
        new("BROWN", "Brown", "#8D6E63", 11),
        new("TEAL", "Teal", "#00897B", 12),
    ];

    /// <summary>
    /// The number of distinct shape and color pairs a marking can take.
    /// </summary>
    public static int CombinationCount => Shapes.Count * Colors.Count;

    /// <summary>
    /// Finds a shape by its glyph. Returns <c>null</c> for a glyph that is not offered.
    /// </summary>
    public static Model_LotMarkingShape? FindShape(string? glyph)
    {
        if (string.IsNullOrWhiteSpace(glyph))
        {
            return null;
        }

        foreach (var shape in Shapes)
        {
            if (string.Equals(shape.Glyph, glyph, StringComparison.Ordinal))
            {
                return shape;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a color by its code, ignoring case. Returns <c>null</c> for an unknown code.
    /// </summary>
    public static Model_LotMarkingColor? FindColor(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        foreach (var color in Colors)
        {
            if (string.Equals(color.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return color;
            }
        }

        return null;
    }
}
