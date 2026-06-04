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
    /// <remarks>
    ///     <para>
    ///         <strong>Adaptive behavior (intentional, by design):</strong> the returned score
    ///         is <em>not stable</em> across differing call sequences or sessions. The per-<see cref="StatDef" />
    ///         min/max range in <see cref="Ranges" /> expands as values are observed, so the normalized
    ///         result for a given <paramref name="value" /> depends on the <em>set and order</em> of all
    ///         values scored so far in the current process. Identical inputs can yield different outputs
    ///         if the observation history differs.
    ///     </para>
    ///     <para>
    ///         This order-dependence is intentional. Unit tests that exercise normalization must
    ///         save and restore <see cref="Ranges" /> to prevent state leaking between tests.
    ///     </para>
    /// </remarks>
    /// <param name="stat">The stat definition to normalize.</param>
    /// <param name="value">The value to normalize.</param>
    /// <returns>
    ///     A normalized value in [0, 1] relative to the min/max range observed so far for
    ///     <paramref name="stat" />. Not reproducible across differing observation histories.
    /// </returns>
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