using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common.CustomStats;
using RimWorld;
using Verse;

namespace LordKuper.Common.Helpers;

/// <summary>
///     Provides helper methods and properties for working with StatDefs and stat values.
/// </summary>
[PublicAPI]
public static class StatHelper
{
    /// <summary>
    ///     Prefix used for custom stat definitions.
    /// </summary>
    internal const string CustomStatPrefix = "LK";

    /// <summary>
    ///     Provides a comparer for sorting StatDefs by category and label.
    /// </summary>
    private static readonly IComparer<StatDef> Comparer = new StatDefCategoryComparer();

    /// <summary>
    ///     Stores stat definitions grouped by their category.
    /// </summary>
    private static readonly Dictionary<StatCategory, SortedSet<StatDef>> Stats = new();

    /// <summary>
    ///     Stores all melee weapon stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _allMeleeWeaponStatDefs = [];

    /// <summary>
    ///     Stores all ranged weapon stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _allRangedWeaponStatDefs = [];

    /// <summary>
    ///     Stores all stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _allStatDefs = [];

    /// <summary>
    ///     Stores all tool stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _allToolStatDefs = [];

    /// <summary>
    ///     Stores apparel category names.
    /// </summary>
    private static HashSet<string> _apparelCategories = [];

    /// <summary>
    ///     Stores custom stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _customStatsDefs = [];

    /// <summary>
    ///     Stores default apparel stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _defaultApparelStatDefs = [];

    /// <summary>
    ///     Stores default pawn stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _defaultPawnStatDefs = [];

    /// <summary>
    ///     Stores default weapon stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _defaultWeaponStatDefs = [];

    /// <summary>
    ///     Stores default work stat definitions.
    /// </summary>
    private static SortedSet<StatDef> _defaultWorkStatDefs = [];

    /// <summary>
    ///     Stores pawn category names.
    /// </summary>
    private static HashSet<string> _pawnCategories = [];

    /// <summary>
    ///     Stores all stat definitions indexed by defName (lowercase) for O(1) lookup.
    /// </summary>
    private static Dictionary<string, StatDef> _statDefsByName = new();

    /// <summary>
    ///     Stores weapon category names.
    /// </summary>
    private static HashSet<string> _weaponCategories = [];

    /// <summary>
    ///     Stores work category names.
    /// </summary>
    private static HashSet<string> _workCategories = [];

    /// <summary>
    ///     Static constructor to initialize stat categories and definitions.
    ///     Delegates to <see cref="Rebuild" /> so test fixtures can re-initialize against a
    ///     swapped <see cref="DefProvider.Current" /> without changing load-time behavior.
    /// </summary>
    static StatHelper()
    {
        Rebuild();
    }

    /// <summary>
    ///     Gets a stat definition by its defName, case-insensitive.
    /// </summary>
    /// <param name="defName">The defName of the stat to find.</param>
    /// <returns>The matching <see cref="StatDef" /> if found; otherwise, <c>null</c>.</returns>
    internal static StatDef? GetStatDef(string? defName)
    {
        if (string.IsNullOrEmpty(defName)) return null;
        // After IsNullOrEmpty returns false, defName is a non-empty, non-null string.
        _statDefsByName.TryGetValue(defName!.ToLowerInvariant(), out var result);
        return result;
    }

    /// <summary>
    ///     Gets the value of a stat for a specific <see cref="Thing" />.
    /// </summary>
    /// <param name="thing">The thing to evaluate.</param>
    /// <param name="statDef">The stat definition.</param>
    /// <returns>The stat value for the specified thing.</returns>
    public static float GetStatValue(Thing thing, StatDef statDef)
    {
        if (thing == null) throw new ArgumentNullException(nameof(thing));
        if (statDef == null) throw new ArgumentNullException(nameof(statDef));
        try
        {
            var offset = 0f;
            var offsets = thing.def.equippedStatOffsets;
            if (offsets != null)
                foreach (var modifier in offsets)
                {
                    if (modifier.stat != statDef) continue;
                    offset = modifier.value;
                    break;
                }

            return thing.GetStatValue(statDef) + offset;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(
                $"Could not evaluate stat '{statDef.LabelCap}' of {thing.LabelCapNoCount}.",
                exception);
            return 0f;
        }
    }

    /// <summary>
    ///     Gets the value of a stat for a specific <see cref="ThingDef" />.
    /// </summary>
    /// <param name="def">The thing definition to evaluate.</param>
    /// <param name="statDef">The stat definition.</param>
    /// <returns>The stat value for the specified thing definition.</returns>
    private static float GetStatValue(ThingDef def, StatDef statDef)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
        if (statDef == null) throw new ArgumentNullException(nameof(statDef));
        try
        {
            var baseValue = 0f;
            var statBases = def.statBases;
            if (statBases != null)
                foreach (var modifier in statBases)
                {
                    if (modifier.stat != statDef) continue;
                    baseValue = modifier.value;
                    break;
                }

            var offset = 0f;
            var offsets = def.equippedStatOffsets;
            if (offsets != null)
                foreach (var modifier in offsets)
                {
                    if (modifier.stat != statDef) continue;
                    offset = modifier.value;
                    break;
                }

            return baseValue + offset;
        }
        catch (Exception exception)
        {
            Logger.LogWarning($"Could not evaluate stat '{statDef.LabelCap}' of {def.LabelCap}",
                exception);
            return 0f;
        }
    }

    /// <summary>
    ///     Gets the deviation of a stat value from its default base value for a <see cref="ThingDef" />.
    /// </summary>
    /// <param name="def">The thing definition.</param>
    /// <param name="statDef">The stat definition.</param>
    /// <returns>The deviation from the default base value.</returns>
    public static float GetStatValueDeviation(ThingDef def, StatDef statDef)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
        if (statDef == null) throw new ArgumentNullException(nameof(statDef));
        return GetStatValue(def, statDef) - statDef.defaultBaseValue;
    }

    /// <summary>
    ///     Gets the deviation of a stat value from its default base value for a <see cref="Thing" />.
    /// </summary>
    /// <param name="thing">The thing.</param>
    /// <param name="statDef">The stat definition.</param>
    /// <returns>The deviation from the default base value.</returns>
    public static float GetStatValueDeviation(Thing thing, StatDef statDef)
    {
        if (thing == null) throw new ArgumentNullException(nameof(thing));
        if (statDef == null) throw new ArgumentNullException(nameof(statDef));
        return GetStatValue(thing, statDef) - statDef.defaultBaseValue;
    }

    /// <summary>
    ///     Gets the stat definitions for a given category.
    /// </summary>
    /// <param name="category">The stat category.</param>
    /// <returns>A read-only collection of <see cref="StatDef" /> for the specified category.</returns>
    public static IReadOnlyCollection<StatDef> GetStatsByCategory(StatCategory category)
    {
        if (Stats.TryGetValue(category, out var stats)) return stats;
        var source = category switch
        {
            StatCategory.Pawn => _defaultPawnStatDefs,
            StatCategory.Apparel => _defaultApparelStatDefs,
            StatCategory.Weapon => _defaultWeaponStatDefs,
            StatCategory.WeaponMelee => _allMeleeWeaponStatDefs,
            StatCategory.WeaponRanged => _allRangedWeaponStatDefs,
            StatCategory.Tool => _allToolStatDefs,
            StatCategory.Work => _defaultWorkStatDefs,
            StatCategory.All => _allStatDefs,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
        stats = new SortedSet<StatDef>(source, Comparer);
        Stats[category] = stats;
        return stats;
    }

    /// <summary>
    ///     Initializes the apparel category names.
    /// </summary>
    private static void InitializeApparelCategories()
    {
        _apparelCategories =
        [
            nameof(StatCategoryDefOf.Basics),
            nameof(StatCategoryDefOf.BasicsImportant),
            nameof(StatCategoryDefOf.BasicsNonPawnImportant),
            nameof(StatCategoryDefOf.BasicsNonPawn),
            nameof(StatCategoryDefOf.PawnWork),
            nameof(StatCategoryDefOf.Apparel)
        ];
    }

    /// <summary>
    ///     Initializes custom stat definitions.
    /// </summary>
    private static void InitializeCustomStats()
    {
        _customStatsDefs = new SortedSet<StatDef>(
            MeleeWeaponStats.StatDefs.Concat(RangedWeaponStats.StatDefs).Concat(ToolStats.StatDefs),
            Comparer);
    }

    /// <summary>
    ///     Initializes default stat definitions for pawns, apparel, weapons, and work.
    /// </summary>
    private static void InitializeDefaultStats()
    {
        IReadOnlyList<StatDef> defs;
        try
        {
            defs = DefProvider.Current.AllDefsListForReading<StatDef>();
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"{nameof(StatHelper)}.{nameof(InitializeDefaultStats)}: " +
                "failed to read StatDef list from DefProvider.", ex);
            defs = [];
        }

        _defaultPawnStatDefs = new SortedSet<StatDef>(Comparer);
        _defaultApparelStatDefs = new SortedSet<StatDef>(Comparer);
        _defaultWeaponStatDefs = new SortedSet<StatDef>(Comparer);
        _defaultWorkStatDefs = new SortedSet<StatDef>(Comparer);
        foreach (var def in defs)
        {
            if (def == null) continue; // guard: oblivious Verse API may include null entries
            var category = def.category;
            if (category == null) continue;
            var defName = category.defName;
            if (_pawnCategories.Contains(defName))
                _defaultPawnStatDefs.Add(def);
            if (_apparelCategories.Contains(defName))
                _defaultApparelStatDefs.Add(def);
            if (_weaponCategories.Contains(defName))
                _defaultWeaponStatDefs.Add(def);
            if (_workCategories.Contains(defName))
                _defaultWorkStatDefs.Add(def);
        }
    }

    /// <summary>
    ///     Initializes pawn category names.
    /// </summary>
    private static void InitializePawnCategories()
    {
        _pawnCategories =
        [
            nameof(StatCategoryDefOf.BasicsPawn),
            nameof(StatCategoryDefOf.BasicsPawnImportant),
            nameof(StatCategoryDefOf.PawnCombat),
            nameof(StatCategoryDefOf.PawnResistances),
            nameof(StatCategoryDefOf.PawnHealth),
            nameof(StatCategoryDefOf.PawnFood),
            nameof(StatCategoryDefOf.PawnPsyfocus),
            nameof(StatCategoryDefOf.PawnMisc),
            nameof(StatCategoryDefOf.Animals),
            nameof(StatCategoryDefOf.PawnSocial),
            nameof(StatCategoryDefOf.PawnWork),
            nameof(StatCategoryDefOf.Basics)
        ];
    }

    /// <summary>
    ///     Initializes the union of all stat definitions, including custom stats.
    /// </summary>
    private static void InitializeUnionStats()
    {
        IReadOnlyList<StatDef> allDefs;
        try
        {
            allDefs = DefProvider.Current.AllDefsListForReading<StatDef>();
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"{nameof(StatHelper)}.{nameof(InitializeUnionStats)}: " +
                "failed to read StatDef list from DefProvider.", ex);
            allDefs = [];
        }

        var allStatDefsSet = new HashSet<StatDef>(allDefs);
        allStatDefsSet.UnionWith(_customStatsDefs);
        _allStatDefs = new SortedSet<StatDef>(allStatDefsSet, Comparer);
        _statDefsByName = allStatDefsSet.ToDictionary(d => d.defName.ToLowerInvariant());
        var meleeSet = new HashSet<StatDef>(MeleeWeaponStats.StatDefs);
        meleeSet.UnionWith(_defaultWeaponStatDefs);
        _allMeleeWeaponStatDefs = new SortedSet<StatDef>(meleeSet, Comparer);
        var rangedSet = new HashSet<StatDef>(RangedWeaponStats.StatDefs);
        rangedSet.UnionWith(_defaultWeaponStatDefs);
        _allRangedWeaponStatDefs = new SortedSet<StatDef>(rangedSet, Comparer);
        var toolSet = new HashSet<StatDef>(ToolStats.StatDefs);
        toolSet.UnionWith(_defaultWeaponStatDefs);
        _allToolStatDefs = new SortedSet<StatDef>(toolSet, Comparer);
    }

    /// <summary>
    ///     Initializes weapon category names.
    /// </summary>
    private static void InitializeWeaponCategories()
    {
        _weaponCategories =
        [
            nameof(StatCategoryDefOf.Basics),
            nameof(StatCategoryDefOf.BasicsImportant),
            nameof(StatCategoryDefOf.BasicsNonPawnImportant),
            nameof(StatCategoryDefOf.BasicsNonPawn),
            nameof(StatCategoryDefOf.Weapon),
            nameof(StatCategoryDefOf.Weapon_Melee),
            nameof(StatCategoryDefOf.Weapon_Ranged),
            nameof(StatCategoryDefOf.PawnWork)
        ];
    }

    /// <summary>
    ///     Initializes work category names.
    /// </summary>
    private static void InitializeWorkCategories()
    {
        _workCategories =
        [
            nameof(StatCategoryDefOf.PawnWork),
            nameof(StatCategoryDefOf.PawnSocial)
        ];
    }

    /// <summary>
    ///     Initializes (or re-initializes) all stat-helper state against the active
    ///     <see cref="DefProvider.Current" />.
    ///     Called by the static constructor at game load and by test fixtures after swapping
    ///     the provider.  Produces the same state as the original static constructor.
    /// </summary>
    internal static void Rebuild()
    {
        InitializePawnCategories();
        InitializeWorkCategories();
        InitializeApparelCategories();
        InitializeWeaponCategories();
        InitializeDefaultStats();
        InitializeCustomStats();
        InitializeUnionStats();
        Stats.Clear();
    }

    /// <summary>
    ///     Rebuilds custom and union stat collections after labels are updated.
    /// </summary>
    private static void ReinitializeLabels()
    {
        InitializeCustomStats();
        InitializeUnionStats();
        Stats.Clear();
    }

    /// <summary>
    ///     Runs after all defs and language files are loaded.
    ///     Updates custom stat labels/descriptions and rebuilds sorted collections.
    /// </summary>
    [StaticConstructorOnStartup]
    private static class PostInit
    {
        static PostInit()
        {
            MeleeWeaponStats.UpdateLabels();
            RangedWeaponStats.UpdateLabels();
            ToolStats.UpdateLabels();
            ReinitializeLabels();
        }
    }
}