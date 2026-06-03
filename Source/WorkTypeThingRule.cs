using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Represents a rule that associates a work type with a set of stat weights for evaluating things.
/// </summary>
[PublicAPI]
public class WorkTypeThingRule : IExposable
{
    /// <summary>
    ///     Stores all relevant <see cref="ThingDef" /> objects for evaluation.
    /// </summary>
    private static HashSet<ThingDef>? _allRelevantThings;

    /// <summary>
    ///     Indicates whether the rule has been initialized.
    /// </summary>
    private bool _isInitialized;

    /// <summary>
    ///     Stores the stat weights associated with this rule, keyed by stat definition name.
    /// </summary>
    private Dictionary<string, StatWeight> _statWeights = new();

    /// <summary>
    ///     The <see cref="WorkTypeDef" /> associated with this rule.
    /// </summary>
    private WorkTypeDef? _workTypeDef;

    /// <summary>
    ///     The name of the work type definition associated with this rule.
    /// </summary>
    private string? _workTypeDefName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WorkTypeThingRule" /> class.
    /// </summary>
    public WorkTypeThingRule() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="WorkTypeThingRule" /> class with the specified work type definition
    ///     name.
    /// </summary>
    /// <param name="workTypeDefName">The name of the work type definition.</param>
    public WorkTypeThingRule(string? workTypeDefName)
    {
        _workTypeDefName = workTypeDefName;
    }

    /// <summary>
    ///     Gets all relevant <see cref="ThingDef" /> objects for evaluation.
    /// </summary>
    private static IEnumerable<ThingDef> AllRelevantThings
    {
        get
        {
            if (_allRelevantThings == null || _allRelevantThings.Count == 0)
                _allRelevantThings =
                [
                    ..DefDatabase<ThingDef>.AllDefs.Where(def =>
                        def.IsWeapon && !def.destroyOnDrop &&
                        (def.statBases != null || def.equippedStatOffsets != null))
                ];
            return _allRelevantThings;
        }
    }

    /// <summary>
    ///     Gets the default rules for all work types defined in <see cref="WorkTypeStatMap.DefaultStatsMap" />.
    /// </summary>
    public static IEnumerable<WorkTypeThingRule> DefaultRules
    {
        get
        {
            var rules = new List<WorkTypeThingRule>();
            if (WorkTypeStatMap.DefaultStatsMap == null)
            {
                Logger.LogError("Tried to get default WorkTypeThingRules with uninitialized WorkTypeStatMap.");
                return rules;
            }
            foreach (var map in WorkTypeStatMap.DefaultStatsMap)
            {
                var rule = new WorkTypeThingRule(map.Key.defName);
                foreach (var statWeight in map.Value.Values)
                {
                    var statDef = statWeight.StatDef;
                    if (statDef != null)
                        rule.SetStatWeight(statDef, statWeight.Weight, statWeight.Protected);
                }
                rules.Add(rule);
            }
            return rules;
        }
    }

    /// <summary>
    ///     Gets the label for this rule, using the short label of the work type if available.
    /// </summary>
    public string? Label =>
        WorkTypeDef != null
            ? WorkTypeDef.labelShort.NullOrEmpty() ? WorkTypeDefName : WorkTypeDef.labelShort.CapitalizeFirst()
            : WorkTypeDefName;

    /// <summary>
    ///     Gets the collection of <see cref="StatWeight" /> objects associated with this rule.
    /// </summary>
    public IEnumerable<StatWeight> StatWeights
    {
        get
        {
            Initialize();
            return _statWeights.Values;
        }
    }

    /// <summary>
    ///     Gets the <see cref="WorkTypeDef" /> associated with this rule.
    /// </summary>
    private WorkTypeDef? WorkTypeDef
    {
        get
        {
            Initialize();
            return _workTypeDef;
        }
    }

    /// <summary>
    ///     Gets the name of the work type definition associated with this rule.
    /// </summary>
    public string? WorkTypeDefName => _workTypeDefName;

    /// <summary>
    ///     Exposes data for serialization.
    /// </summary>
    public void ExposeData()
    {
        Scribe_Values.Look(ref _workTypeDefName, nameof(WorkTypeDefName));
        Scribe_Collections.Look(ref _statWeights, nameof(StatWeights), LookMode.Value, LookMode.Deep);
    }

    /// <summary>
    ///     Removes the <see cref="StatWeight" /> associated with the specified stat definition name from this rule.
    /// </summary>
    /// <param name="statDefName">The name of the stat definition to remove.</param>
    public void DeleteStatWeight(string statDefName)
    {
        _ = _statWeights.Remove(statDefName);
    }

    /// <summary>
    ///     Gets all globally available <see cref="ThingDef" /> items relevant to this rule, sorted by score.
    /// </summary>
    /// <returns>
    ///     An enumerable list of <see cref="ThingDef" /> objects sorted descending by their calculated score.
    /// </returns>
    public IEnumerable<ThingDef> GetGloballyAvailableItems()
    {
        var items = new List<ThingDef>();
        items.AddRange(AllRelevantThings.Where(def =>
            (def.statBases ?? []).Union(def.equippedStatOffsets ?? [])
            .Any(sm => _statWeights.ContainsKey(sm.stat.defName))));
        items.SortByDescending(GetThingDefScore);
        return items;
    }

    /// <summary>
    ///     Calculates a score for the specified <see cref="ThingDef" /> based on the stat weights of this rule.
    /// </summary>
    /// <param name="def">The <see cref="ThingDef" /> to score.</param>
    /// <returns>The calculated score.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="def" /> is null.</exception>
    public float GetThingDefScore(ThingDef def)
    {
        return def == null
            ? throw new ArgumentNullException(nameof(def))
            : StatWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
                // Where(sw => sw.StatDef != null) guards non-null; ! asserts that to the compiler
                StatRanges.NormalizeStatValue(statWeight.StatDef!,
                    StatHelper.GetStatValueDeviation(def, statWeight.StatDef!)) * statWeight.Weight);
    }

    /// <summary>
    ///     Calculates a normalized [-1..1] score for the specified <see cref="Thing" /> based on the stat weights of this
    ///     rule.
    /// </summary>
    /// <param name="thing">The thing to score.</param>
    /// <returns>The calculated score.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="thing" /> is null.</exception>
    public float GetThingScore(Thing thing)
    {
        return thing == null
            ? throw new ArgumentNullException(nameof(thing))
            : _statWeights.Values.Where(sw => sw.StatDef != null).Sum(sw =>
                // Where(sw => sw.StatDef != null) guards non-null; ! asserts that to the compiler
                StatRanges.NormalizeStatValue(sw.StatDef!, StatHelper.GetStatValueDeviation(thing, sw.StatDef!)) *
                sw.Weight);
    }

    /// <summary>
    ///     Initializes the rule by loading the work type definition and default stat weights if not already initialized.
    /// </summary>
    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail(_workTypeDefName);
        if (_workTypeDef == null) return;
        if (WorkTypeStatMap.DefaultStatsMap == null)
            throw new NullReferenceException("Tried to access uninitialized WorkTypeStatMap.");
        if (!WorkTypeStatMap.DefaultStatsMap.TryGetValue(_workTypeDef, out var defaultStatWeights))
            return;
        foreach (var kvp in defaultStatWeights.Where(kvp => !_statWeights.ContainsKey(kvp.Key.defName)))
        {
            _statWeights.Add(kvp.Key.defName, new StatWeight(kvp.Key, kvp.Value.Weight, kvp.Value.Protected));
        }
    }

    /// <summary>
    ///     Sets the weight and optional protection status for the specified stat definition.
    /// </summary>
    /// <param name="statDef">The stat definition for which the weight is being set. Cannot be <see langword="null" />.</param>
    /// <param name="weight">The weight value to assign to the stat definition.</param>
    /// <param name="isProtected">
    ///     An optional value indicating whether the stat definition is protected.  If <see langword="null" />, the
    ///     protection status remains unchanged.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="statDef" /> is <see langword="null" />.</exception>
    public void SetStatWeight(StatDef statDef, float weight, bool? isProtected = null)
    {
        if (statDef == null) throw new ArgumentNullException(nameof(statDef));
        if (!_statWeights.TryGetValue(statDef.defName, out var statWeight))
        {
            statWeight = new StatWeight(statDef, false);
            _statWeights.Add(statDef.defName, statWeight);
        }
        statWeight.Weight = weight;
        if (isProtected != null) statWeight.Protected = isProtected.Value;
    }
}