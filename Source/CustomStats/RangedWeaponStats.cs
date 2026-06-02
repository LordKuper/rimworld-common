using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;
using RimWorld;

namespace LordKuper.Common.CustomStats;

/// <summary>
///     Provides utilities for working with custom ranged weapon statistics.
/// </summary>
[PublicAPI]
public static class RangedWeaponStats
{
    /// <summary>
    ///     The category name for custom ranged weapon stats.
    /// </summary>
    private const string Category = "RangedWeapons";

    private static readonly List<StatDef> StatDefList = StatDefNames.Select(defName => new StatDef
    {
        defName = defName,
        label = defName,
        description = string.Empty,
        category = CategoryDef
    }).ToList();

    /// <summary>
    ///     The <see cref="StatCategoryDef" /> for custom ranged weapon stats.
    /// </summary>
    private static StatCategoryDef CategoryDef { get; } = new()
    {
        defName = $"{StatHelper.CustomStatPrefix}_{Category}",
        label = $"{StatHelper.CustomStatPrefix}_{Category}"
    };

    /// <summary>
    ///     Gets the collection of all custom stat definition names.
    /// </summary>
    [NotNull]
    private static IEnumerable<string> StatDefNames =>
        Enum.GetValues(typeof(RangedWeaponStat)).OfType<RangedWeaponStat>().Select(GetStatDefName);

    /// <summary>
    ///     Gets the collection of all <see cref="StatDef" />s for custom ranged weapon stats.
    /// </summary>
    [NotNull]
    [ItemNotNull]
    public static IEnumerable<StatDef> StatDefs => StatDefList;

    /// <summary>
    ///     Gets the stat definition name for a given <see cref="RangedWeaponStat" />.
    /// </summary>
    /// <param name="stat">The custom ranged weapon stat.</param>
    /// <returns>The stat definition name.</returns>
    [NotNull]
    public static string GetStatDefName(RangedWeaponStat stat)
    {
        return $"{StatHelper.CustomStatPrefix}_{Category}_{stat}";
    }

    /// <summary>
    ///     Gets the stat name from a stat definition name.
    /// </summary>
    /// <param name="defName">The stat definition name.</param>
    /// <returns>The stat name, or <c>null</c> if not a custom stat.</returns>
    [CanBeNull]
    public static string GetStatName([NotNull] string defName)
    {
        const string categoryPrefix = $"{StatHelper.CustomStatPrefix}_{Category}_";
        return defName.StartsWith(categoryPrefix, StringComparison.OrdinalIgnoreCase)
            ? defName.Substring(categoryPrefix.Length)
            : null;
    }

    /// <summary>
    ///     Determines whether the specified definition name is a custom stat.
    /// </summary>
    /// <param name="defName">The stat definition name.</param>
    /// <returns><c>true</c> if the definition name is a custom stat; otherwise, <c>false</c>.</returns>
    public static bool IsCustomStat([NotNull] string defName)
    {
        return StatDefNames.Contains(defName);
    }

    /// <summary>
    ///     Updates stat labels and descriptions with translated strings.
    ///     Called from <see cref="StatHelper" /> post-init after language is loaded.
    /// </summary>
    internal static void UpdateLabels()
    {
        foreach (var def in StatDefList)
        {
            def.label = Resources.Strings.Stats.GetLabel(def.defName);
            def.description = Resources.Strings.Stats.GetDescription(def.defName);
        }
    }
}