using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Provides a mapping between <see cref="SkillDef" /> and the set of <see cref="StatDef" />s
///     that are affected by skill need factors and offsets.
/// </summary>
internal static class SkillStatMap
{
    /// <summary>
    ///     Internal storage for the skill-stat mapping.
    /// </summary>
    private static Dictionary<SkillDef, HashSet<StatDef>>? _map;

    /// <summary>
    ///     Gets the mapping between <see cref="SkillDef" /> and the set of <see cref="StatDef" />s
    ///     affected by skill need factors and offsets.
    /// </summary>
    /// <remarks>
    ///     The mapping is built on first access if it has not already been initialized.
    /// </remarks>
    internal static Dictionary<SkillDef, HashSet<StatDef>>? Map
    {
        get
        {
            if (_map == null) BuildMap();
            return _map;
        }
    }

    /// <summary>
    ///     Builds the mapping between <see cref="SkillDef" /> and <see cref="StatDef" />s
    ///     by analyzing all stat definitions for skill need factors and offsets.
    /// </summary>
    private static void BuildMap()
    {
#if DEBUG
        Logger.LogMessage($"Building {nameof(SkillStatMap)}...");
#endif
        var skillDefs = DefDatabase<SkillDef>.AllDefsListForReading;
        var statDefs = DefDatabase<StatDef>.AllDefsListForReading;
        _map = new Dictionary<SkillDef, HashSet<StatDef>>(skillDefs.Count);
        foreach (var skill in skillDefs)
        {
            _map[skill] = [];
        }
        foreach (var stat in statDefs)
        {
            if (stat.skillNeedFactors != null)
                foreach (var needFactor in stat.skillNeedFactors)
                {
                    var stats = _map[needFactor.skill];
                    stats.Add(stat);
                    if (stat.statFactors == null) continue;
                    foreach (var f in stat.statFactors)
                    {
                        stats.Add(f);
                    }
                }
            if (stat.skillNeedOffsets != null)
                foreach (var needOffset in stat.skillNeedOffsets)
                {
                    var stats = _map[needOffset.skill];
                    stats.Add(stat);
                    if (stat.statFactors == null) continue;
                    foreach (var f in stat.statFactors)
                    {
                        stats.Add(f);
                    }
                }
        }
#if DEBUG
        foreach (var kvp in _map)
        {
            Logger.LogMessage($"{kvp.Key.defName}: {string.Join(", ", kvp.Value)}");
        }
#endif
    }
}