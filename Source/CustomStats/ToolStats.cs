using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;
using RimWorld;

namespace LordKuper.Common.CustomStats;

/// <summary>
///     Provides utilities for working with custom tool statistics.
/// </summary>
[UsedImplicitly]
public static class ToolStats
{
    /// <summary>
    ///     The category name for custom tool stats.
    /// </summary>
    private const string Category = "Tools";

    /// <summary>
    ///     The stat category definition for custom tool stats.
    /// </summary>
    private static StatCategoryDef CategoryDef { get; } = new()
    {
        defName = $"{StatHelper.CustomStatPrefix}_{Category}",
        label = $"{StatHelper.CustomStatPrefix}_{Category}"
    };

    /// <summary>
    ///     Gets the collection of custom tool stat definition names.
    /// </summary>
    [NotNull]
    private static IEnumerable<string> StatDefNames =>
        Enum.GetValues(typeof(ToolStat)).OfType<ToolStat>().Select(GetStatDefName);

    /// <summary>
    ///     Gets the collection of custom tool stat definitions.
    /// </summary>
    public static IEnumerable<StatDef> StatDefs { get; } = StatDefNames.Select(defName =>
        new StatDef
        {
            defName = defName,
            label = Resources.Strings.Stats.GetLabel(defName),
            description = Resources.Strings.Stats.GetDescription(defName),
            category = CategoryDef
        });

    /// <summary>
    ///     Gets the stat definition name for a given <see cref="ToolStat" />.
    /// </summary>
    /// <param name="stat">The custom tool stat.</param>
    /// <returns>The stat definition name.</returns>
    [NotNull]
    public static string GetStatDefName(ToolStat stat)
    {
        return $"{StatHelper.CustomStatPrefix}_{Category}_{stat}";
    }

    /// <summary>
    ///     Gets the stat name from a stat definition name.
    /// </summary>
    /// <param name="defName">The stat definition name.</param>
    /// <returns>The stat name if the definition name matches the custom tool stat pattern; otherwise, <c>null</c>.</returns>
    [CanBeNull]
    public static string GetStatName([NotNull] string defName)
    {
        const string categoryPrefix = $"{StatHelper.CustomStatPrefix}_{Category}_";
        return defName.StartsWith(categoryPrefix, StringComparison.OrdinalIgnoreCase)
            ? defName.Substring(categoryPrefix.Length)
            : null;
    }

    /// <summary>
    ///     Determines whether the specified definition name is a custom tool stat.
    /// </summary>
    /// <param name="defName">The stat definition name.</param>
    /// <returns><c>true</c> if the definition name is a custom tool stat; otherwise, <c>false</c>.</returns>
    public static bool IsCustomStat(string defName)
    {
        return StatDefNames.Contains(defName);
    }
}