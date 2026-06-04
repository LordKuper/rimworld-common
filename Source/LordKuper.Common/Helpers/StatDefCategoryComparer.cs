using System;
using System.Collections.Generic;
using RimWorld;

namespace LordKuper.Common.Helpers;

/// <summary>
///     Provides a comparer for <see cref="StatDef" /> objects that sorts first by category label, then by stat label.
/// </summary>
internal class StatDefCategoryComparer : IComparer<StatDef>
{
    /// <summary>
    ///     Compares two <see cref="StatDef" /> instances by their category label and then by their stat label.
    /// </summary>
    /// <param name="x">The first <see cref="StatDef" /> to compare.</param>
    /// <param name="y">The second <see cref="StatDef" /> to compare.</param>
    /// <returns>
    ///     Less than zero if <paramref name="x" /> is less than <paramref name="y" />;
    ///     zero if they are equal;
    ///     greater than zero if <paramref name="x" /> is greater than <paramref name="y" />.
    /// </returns>
    public int Compare(StatDef? x, StatDef? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        var xCategoryLabel = x.category != null ? x.category.GetLabel() : string.Empty;
        var yCategoryLabel = y.category != null ? y.category.GetLabel() : string.Empty;
        var categoryCompare = string.Compare(xCategoryLabel, yCategoryLabel,
            StringComparison.OrdinalIgnoreCase);
        if (categoryCompare != 0)
            return categoryCompare;
        var xLabel = x.GetLabel();
        var yLabel = y.GetLabel();
        return string.Compare(xLabel, yLabel, StringComparison.OrdinalIgnoreCase);
    }
}