using System;
using System.Collections.Generic;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;

/// <summary>
/// Chooses a shape and a color for a lot, deterministically and without ever changing one
/// that already exists.
/// </summary>
/// <remarks>
/// A lot is the pair (normalized heat number, supplier group). The hash seeds the choice; it is
/// not the authority. Once a marking has been recorded the stored row is returned untouched, so
/// adding a shape or a color later cannot repaint history.
/// </remarks>
public static class Helper_LotMarkingResolver
{
    /// <summary>
    /// The supplier group used when a receipt carries no supplier, which is the normal case for
    /// a receipt without a purchase order. The literal is part of the contract and must not change.
    /// </summary>
    public const string NoSupplierGroup = "NONPO";

    /// <summary>
    /// How many times the resolver may advance past a marking another live lot already holds
    /// before accepting the one it started with.
    /// </summary>
    public const int MaxProbeAttempts = 64;

    private const uint FnvOffsetBasis = 2166136261;
    private const uint FnvPrime = 16777619;

    /// <summary>
    /// Resolves the marking for a lot.
    /// </summary>
    /// <param name="heatNumber">The heat number as it was captured.</param>
    /// <param name="supplierId">The supplier group, or null to use the shared no-supplier group.</param>
    /// <param name="existingMarkings">
    /// The markings already recorded. A row matching this lot wins outright; any other row
    /// occupies its shape and color when the policy is to probe.
    /// </param>
    /// <param name="policy">What to do when the computed marking is already held.</param>
    /// <param name="shapeCode">The resolved shape code.</param>
    /// <param name="colorCode">The resolved color code.</param>
    /// <returns>
    /// <c>false</c> when the lot has no heat number and therefore receives no marking.
    /// </returns>
    public static bool TryResolve(
        string? heatNumber,
        string? supplierId,
        IReadOnlyList<Model_Tool_LotMarking>? existingMarkings,
        Enum_LotMarkingCollisionPolicy policy,
        out string shapeCode,
        out string colorCode
    )
    {
        shapeCode = string.Empty;
        colorCode = string.Empty;

        if (Helper_LotNumberNormalizer.IsBlank(heatNumber))
        {
            return false;
        }

        var normalizedHeat = Helper_LotNumberNormalizer.Normalize(heatNumber);
        var supplier = NormalizeSupplier(supplierId);
        var markings = existingMarkings ?? [];

        foreach (var marking in markings)
        {
            if (Matches(marking, normalizedHeat, supplier))
            {
                shapeCode = marking.ShapeCode;
                colorCode = marking.ColorCode;
                return true;
            }
        }

        var seed = ComputeSeed(normalizedHeat, supplier);
        var (candidateShape, candidateColor) = ComputeFromSeed(seed);

        if (policy == Enum_LotMarkingCollisionPolicy.Probe)
        {
            var attempt = 0;
            while (
                attempt < MaxProbeAttempts
                && IsTaken(markings, candidateShape, candidateColor)
            )
            {
                attempt++;
                (candidateShape, candidateColor) = ComputeFromSeed(Mix(seed + (uint)attempt));
            }
        }

        shapeCode = candidateShape;
        colorCode = candidateColor;
        return true;
    }

    /// <summary>
    /// Produces the marking a seed maps to. Exposed so a test can assert the mapping directly.
    /// </summary>
    public static (string ShapeCode, string ColorCode) ComputeFromSeed(uint seed)
    {
        var shapes = Model_LotMarkingPalette.Shapes;
        var colors = Model_LotMarkingPalette.Colors;

        var shapeIndex = (int)(seed % (uint)shapes.Count);
        var colorIndex = (int)((seed / (uint)shapes.Count) % (uint)colors.Count);

        return (shapes[shapeIndex].Glyph, colors[colorIndex].Code);
    }

    /// <summary>
    /// Produces the seed for a lot. Deterministic, culture invariant, and independent of the
    /// clock and of who is signed in.
    /// </summary>
    public static uint ComputeSeed(string normalizedHeatNumber, string supplierId)
    {
        var key = string.Concat(normalizedHeatNumber, "\u0001", supplierId);
        return Fnv1a(key);
    }

    /// <summary>
    /// Reduces a supplier to its group key: upper case and trimmed, or the shared no-supplier
    /// group when nothing usable was supplied.
    /// </summary>
    public static string NormalizeSupplier(string? supplierId) =>
        string.IsNullOrWhiteSpace(supplierId)
            ? NoSupplierGroup
            : supplierId.Trim().ToUpperInvariant();

    /// <summary>
    /// <c>true</c> when the two codes are already held by any supplied marking.
    /// </summary>
    public static bool IsTaken(
        IReadOnlyList<Model_Tool_LotMarking> markings,
        string shapeCode,
        string colorCode
    )
    {
        foreach (var marking in markings)
        {
            if (
                string.Equals(marking.ShapeCode, shapeCode, StringComparison.OrdinalIgnoreCase)
                && string.Equals(marking.ColorCode, colorCode, StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }
        }

        return false;
    }

    private static bool Matches(
        Model_Tool_LotMarking marking,
        string normalizedHeat,
        string supplier
    ) =>
        string.Equals(
            marking.HeatNumberNormalized,
            normalizedHeat,
            StringComparison.OrdinalIgnoreCase
        )
        && string.Equals(marking.SupplierId, supplier, StringComparison.OrdinalIgnoreCase);

    private static uint Fnv1a(string value)
    {
        var hash = FnvOffsetBasis;

        foreach (var character in value)
        {
            hash ^= character;
            hash *= FnvPrime;
        }

        return hash;
    }

    /// <summary>
    /// A 32 bit avalanche step, so probe attempts from neighbouring seeds do not land on
    /// neighbouring markings.
    /// </summary>
    private static uint Mix(uint value)
    {
        var mixed = value;
        mixed ^= mixed >> 16;
        mixed *= 0x7FEB352D;
        mixed ^= mixed >> 15;
        mixed *= 0x846CA68B;
        mixed ^= mixed >> 16;
        return mixed;
    }
}
