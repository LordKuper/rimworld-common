using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Provides a mapping between <see cref="WorkTypeDef" /> and relevant <see cref="StatDef" />s with associated weights.
///     Used to determine which stats are important for each work type in the game.
/// </summary>
[PublicAPI]
public class WorkTypeStatMap
{
    /// <summary>
    ///     Default stat weights for specific work types, keyed by work type defName and stat defName.
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, float>> DefaultWorkTypeStats =
        new()
        {
            {
                "Cooking", new Dictionary<string, float>
                {
                    { "FoodPoisonChance", 2f }, { "DrugCookingSpeed", 1f },
                    { "ButcheryFleshSpeed", 1f },
                    { "ButcheryFleshEfficiency", 1.5f }, { "CookSpeed", 1f }
                }
            },
            { "Hunting", new Dictionary<string, float> { { "HuntingStealth", 2f } } },
            {
                "Doctor",
                new Dictionary<string, float>
                    { { "MedicalTendQualityOffset", 2f }, { "MedicalPotency", 2f } }
            }
        };

    /// <summary>
    ///     Stores a mapping from <see cref="WorkTypeDef" /> to a set of <see cref="StatDef" />s that are used for
    ///     auto-switching.
    /// </summary>
    private static Dictionary<WorkTypeDef, HashSet<StatDef>>? _autoSwitchStatsMap;

    /// <summary>
    ///     Stores a mapping from <see cref="WorkTypeDef" /> to a dictionary of <see cref="StatDef" /> and their associated
    ///     <see cref="StatWeight" />.
    /// </summary>
    private static Dictionary<WorkTypeDef, Dictionary<StatDef, StatWeight>>? _defaultStatsMap;

    /// <summary>
    ///     Gets a mapping from <see cref="WorkTypeDef" /> to a set of <see cref="StatDef" />s used for auto-switching.
    ///     The map is built on first access if not already initialized.
    /// </summary>
    public static Dictionary<WorkTypeDef, HashSet<StatDef>>? AutoSwitchStatsMap
    {
        get
        {
            if (_autoSwitchStatsMap == null) BuildMap();
            return _autoSwitchStatsMap;
        }
    }

    /// <summary>
    ///     Gets a mapping from <see cref="WorkTypeDef" /> to a dictionary of <see cref="StatDef" /> and their associated
    ///     <see cref="StatWeight" />.
    ///     The map is built on first access if not already initialized.
    /// </summary>
    internal static Dictionary<WorkTypeDef, Dictionary<StatDef, StatWeight>>? DefaultStatsMap
    {
        get
        {
            if (_defaultStatsMap == null) BuildMap();
            return _defaultStatsMap;
        }
    }

    /// <summary>
    ///     Builds the mapping between work types and their relevant stats on first access.
    ///     Delegates to <see cref="Rebuild" />.
    /// </summary>
    private static void BuildMap()
    {
        Rebuild();
    }

    /// <summary>
    ///     Builds (or rebuilds) the work-type → stat mappings against the active
    ///     <see cref="DefProvider.Current" />.
    ///     Called on first access of <see cref="AutoSwitchStatsMap" /> / <see cref="DefaultStatsMap" />
    ///     and by test fixtures after swapping the provider.
    ///     Produces the same state as the original <c>BuildMap</c>.
    /// </summary>
    internal static void Rebuild()
    {
        IReadOnlyList<WorkTypeDef> workTypes;
        IReadOnlyList<RecipeDef> allRecipes;
        try
        {
            workTypes = DefProvider.Current.WorkTypeDefsInPriorityOrder();
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"{nameof(WorkTypeStatMap)}.{nameof(Rebuild)}: " +
                "failed to read WorkTypeDefs from DefProvider.", ex);
            workTypes = [];
        }

        try
        {
            allRecipes = DefProvider.Current.AllDefsListForReading<RecipeDef>();
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"{nameof(WorkTypeStatMap)}.{nameof(Rebuild)}: " +
                "failed to read RecipeDefs from DefProvider.", ex);
            allRecipes = [];
        }

        var workTypeStatDefs = StatHelper.GetStatsByCategory(StatCategory.Work);
        _defaultStatsMap =
            new Dictionary<WorkTypeDef, Dictionary<StatDef, StatWeight>>(workTypes.Count);
        _autoSwitchStatsMap = new Dictionary<WorkTypeDef, HashSet<StatDef>>(workTypes.Count);
        foreach (var workType in workTypes)
        {
            var statWeights = new Dictionary<StatDef, StatWeight>();
            var autoSwitchStats = new HashSet<StatDef>();
            _defaultStatsMap.Add(workType, statWeights);
            _autoSwitchStatsMap.Add(workType, autoSwitchStats);
            if (DefaultWorkTypeStats.TryGetValue(workType.defName, out var defaultStatWeights))
                foreach (var kvp in defaultStatWeights)
                {
                    var statDef = DefProvider.Current.GetNamedSilentFail<StatDef>(kvp.Key);
                    if (statDef != null)
                        statWeights[statDef] = new StatWeight(statDef, kvp.Value, true);
                    else
                        Logger.LogWarning($"{nameof(WorkTypeStatMap)}.{nameof(Rebuild)}: " +
                                          $"StatDef '{kvp.Key}' referenced by work type '{workType.defName}' could not be resolved.");
                }

            var skillStatMap = SkillStatMap.Map;
            var relevantSkills = workType.relevantSkills;
            if (relevantSkills != null && skillStatMap != null)
                foreach (var skill in relevantSkills)
                {
                    if (!skillStatMap.TryGetValue(skill, out var stats)) continue;
                    foreach (var statDef in stats)
                    {
                        autoSwitchStats.Add(statDef);
                        if (!statWeights.ContainsKey(statDef))
                            statWeights.Add(statDef, new StatWeight(statDef, 1f, true));
                    }
                }

            foreach (var recipe in allRecipes)
            {
                if (recipe.requiredGiverWorkType != workType) continue;
                var effStat = recipe.efficiencyStat;
                var speedStat = recipe.workSpeedStat;
                var tableEffStat = recipe.workTableEfficiencyStat;
                var tableSpeedStat = recipe.workTableSpeedStat;
                if (effStat != null && !statWeights.ContainsKey(effStat))
                    statWeights.Add(effStat, new StatWeight(effStat, 0.8f, true));
                if (speedStat != null)
                {
                    autoSwitchStats.Add(speedStat);
                    if (!statWeights.ContainsKey(speedStat))
                        statWeights.Add(speedStat, new StatWeight(speedStat, 0.5f, true));
                }

                if (tableEffStat != null && !statWeights.ContainsKey(tableEffStat))
                    statWeights.Add(tableEffStat, new StatWeight(tableEffStat, 0.8f, true));
                if (tableSpeedStat != null && !statWeights.ContainsKey(tableSpeedStat))
                    statWeights.Add(tableSpeedStat, new StatWeight(tableSpeedStat, 0.5f, true));
            }

            var toRemove = new List<StatDef>();
            foreach (var def in statWeights.Keys)
                if (!workTypeStatDefs.Contains(def))
                    toRemove.Add(def);
            foreach (var def in toRemove) statWeights.Remove(def);
        }
    }
}