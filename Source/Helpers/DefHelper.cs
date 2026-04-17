using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace LordKuper.Common.Helpers;

/// <summary>
///     Provides extension methods for retrieving user-friendly labels from <see cref="Def" /> instances.
/// </summary>
public static class DefHelper
{
    /// <summary>
    ///     A cache for storing user-friendly labels by <c>defName</c>.
    /// </summary>
    private static readonly Dictionary<string, string> LabelCache = new();

    /// <summary>
    ///     Gets a user-friendly label for a <see cref="WorkTypeDef" /> instance.
    /// </summary>
    /// <param name="def">The <see cref="WorkTypeDef" /> instance. Must not be null.</param>
    /// <returns>
    ///     The capitalized label if available, otherwise the short label if available, otherwise the <c>defName</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="def" /> is null.</exception>
    private static string GetLabel([NotNull] this WorkTypeDef def)
    {
        if (def == null) throw new ArgumentNullException(nameof(def), "Def cannot be null.");
        return !string.IsNullOrWhiteSpace(def.LabelCap) ? def.LabelCap :
            !string.IsNullOrWhiteSpace(def.labelShort) ? def.labelShort : def.defName;
    }

    /// <summary>
    ///     Gets a user-friendly label for a <see cref="Def" /> instance, using a cache by defName.
    /// </summary>
    /// <param name="def">The <see cref="Def" /> instance. Must not be null.</param>
    /// <returns>
    ///     The capitalized label if available, otherwise the <c>defName</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="def" /> is null.</exception>
    [UsedImplicitly]
    public static string GetLabel([NotNull] this Def def)
    {
        if (def == null) throw new ArgumentNullException(nameof(def), "Def cannot be null.");
        if (LabelCache.TryGetValue(def.defName, out var label)) return label;
        label = def is WorkTypeDef wtd ? wtd.GetLabel() :
            !string.IsNullOrWhiteSpace(def.LabelCap) ? def.LabelCap : def.defName;
        LabelCache[def.defName] = label;
        return label;
    }
}