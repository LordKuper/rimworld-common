using System.Collections.Generic;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Provides functionality for tracking and normalizing stat values using dynamic ranges.
/// </summary>
/// <remarks>
///     <para>
///         <strong>Adaptive, order-dependent normalization (intentional, by design):</strong> the
///         per-<see cref="StatDef" /> min/max range in <see cref="Ranges" /> expands as values are
///         observed. Normalized results for any given value depend on the <em>set and order</em> of all
///         values scored so far in the current process. Identical inputs can yield different outputs
///         if the observation history differs.
///     </para>
///     <para>
///         This class is process-global and static. Consumers that need a clean slate between sessions
///         (e.g. test isolation) must call <see cref="Clear" /> to reset the accumulated ranges.
///     </para>
/// </remarks>
public static class StatRanges
{
    /// <summary>
    ///     Stores the minimum and maximum observed values for each <see cref="StatDef" />.
    /// </summary>
    private static readonly Dictionary<StatDef, FloatRange> Ranges = new();

    /// <summary>
    ///     Clears all accumulated stat ranges, resetting the cache to an empty state.
    /// </summary>
    /// <remarks>
    ///     Use this method to reset the process-global range cache when a clean observation
    ///     baseline is required (for example, between isolated unit tests).
    /// </remarks>
    public static void Clear() => Ranges.Clear();

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
    ///         This order-dependence is intentional. Consumers that need a reproducible baseline
    ///         must call <see cref="Clear" /> before starting a new observation sequence.
    ///     </para>
    /// </remarks>
    /// <param name="stat">The stat definition to normalize.</param>
    /// <param name="value">The value to normalize.</param>
    /// <returns>
    ///     A normalized value in [0, 1] relative to the min/max range observed so far for
    ///     <paramref name="stat" />. Not reproducible across differing observation histories.
    /// </returns>
    public static float NormalizeStatValue(StatDef stat, float value)
    {
        UpdateStatRange(stat, value);
        return MathHelper.NormalizeValue(value, Ranges[stat]);
    }

    /// <summary>
    ///     Updates the observed range for a stat, expanding it if the provided value is outside the current range.
    /// </summary>
    /// <remarks>
    ///     On the first observation of a stat, seeds the range to <c>[value, value]</c> so the
    ///     degenerate single-element range is exact rather than anchored at zero.
    /// </remarks>
    /// <param name="stat">The stat definition to update.</param>
    /// <param name="value">The value to consider for range expansion.</param>
    private static void UpdateStatRange(StatDef stat, float value)
    {
        if (!Ranges.TryGetValue(stat, out var range)) { range = new FloatRange(value, value); }
        if (range.min > value) { range.min = value; }
        if (range.max < value) { range.max = value; }
        Ranges[stat] = range;
    }
}
