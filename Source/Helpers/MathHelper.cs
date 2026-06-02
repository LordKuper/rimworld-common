using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.Helpers;

/// <summary>
///     Provides utility methods for mathematical operations.
/// </summary>
[PublicAPI]
public static class MathHelper
{
    /// <summary>
    ///     Normalizes a value within a specified range.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="range">The range to normalize within.</param>
    /// <returns>The normalized value.</returns>
    public static float NormalizeValue(float value, FloatRange range)
    {
        value = Mathf.Clamp(value, range.min, range.max);
        var valueRange = range.max - range.min;
        if (Math.Abs(valueRange) < 0.001f) return 0f;
        var normalizedValue = (value - range.min) / valueRange;
        return range switch
        {
            { min: < 0, max: < 0 } => -1 + normalizedValue,
            { min: < 0, max: > 0 } => -1 + 2 * normalizedValue,
            _ => normalizedValue
        };
    }
}