using System.Collections.Generic;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Provides functionality for tracking and normalizing stat values using dynamic ranges.
/// </summary>
internal static class StatRanges
{
    /// <summary>
    ///     Stores the minimum and maximum observed values for each <see cref="StatDef" />.
    /// </summary>
    private static readonly Dictionary<StatDef, FloatRange> Ranges = new();

    /// <summary>
    ///     Normalizes a stat value based on the observed range for the specified stat.
    ///     Updates the range if the value is outside the current bounds.
    /// </summary>
    /// <param name="stat">The stat definition to normalize.</param>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The normalized value in the range [0, 1].</returns>
    internal static float NormalizeStatValue(StatDef stat, float value)
    {
        UpdateStatRange(stat, value);
        return MathHelper.NormalizeValue(value, Ranges[stat]);
    }

    /// <summary>
    ///     Updates the observed range for a stat, expanding it if the provided value is outside the current range.
    /// </summary>
    /// <param name="stat">The stat definition to update.</param>
    /// <param name="value">The value to consider for range expansion.</param>
    private static void UpdateStatRange(StatDef stat, float value)
    {
        if (!Ranges.TryGetValue(stat, out var range)) Ranges[stat] = new FloatRange(value, value);
        if (range.min > value)
        {
            range.min = value;
            Ranges[stat] = range;
        }
        if (range.max < value)
        {
            range.max = value;
            Ranges[stat] = range;
        }
    }
}