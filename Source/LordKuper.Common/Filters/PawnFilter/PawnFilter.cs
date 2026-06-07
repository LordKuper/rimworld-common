using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LordKuper.Common.Filters.Limits;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace LordKuper.Common.Filters;

/// <summary>
///     Provides filtering logic for pawns.
/// </summary>
[PublicAPI]
public class PawnFilter : IExposable
{
    /// <summary>
    ///     The set of allowed pawn health states for filtering.
    /// </summary>
    public PawnHealthState AllowedPawnHealthStates = PawnHealthState.None;

    /// <summary>
    ///     Defines the set of allowed primary weapon types for pawns.
    /// </summary>
    public HashSet<PawnPrimaryWeaponType> AllowedPawnPrimaryWeaponTypes = [];

    /// <summary>
    ///     The set of allowed pawn types for filtering.
    /// </summary>
    public HashSet<PawnType> AllowedPawnTypes = [];

    /// <summary>
    ///     The set of allowed work passions for filtering.
    /// </summary>
    public HashSet<Passion> AllowedWorkPassions = [];

    /// <summary>
    ///     Whether to filter pawns by their capacities.
    /// </summary>
    public bool? FilterPawnCapacities;

    /// <summary>
    ///     Whether to filter pawns by their health states.
    /// </summary>
    public bool? FilterPawnHealthStates;

    /// <summary>
    ///     Gets or sets a value indicating whether to filter pawn primary weapon types.
    /// </summary>
    public bool? FilterPawnPrimaryWeaponTypes;

    /// <summary>
    ///     Indicates whether to filter pawns by their skills.
    /// </summary>
    public bool? FilterPawnSkills;

    /// <summary>
    ///     Indicates whether to filter pawns by their stats.
    /// </summary>
    public bool? FilterPawnStats;

    /// <summary>
    ///     Whether to filter pawns by their traits.
    /// </summary>
    public bool? FilterPawnTraits;

    /// <summary>
    ///     Whether to filter pawns by their types.
    /// </summary>
    public bool? FilterPawnTypes;

    /// <summary>
    ///     Whether to filter pawns by their work capacities.
    /// </summary>
    public bool? FilterWorkCapacities;

    /// <summary>
    ///     Whether to filter pawns by their work passions.
    /// </summary>
    public bool? FilterWorkPassions;

    /// <summary>
    ///     The set of forbidden pawn health states for filtering.
    /// </summary>
    public PawnHealthState ForbiddenPawnHealthStates = PawnHealthState.None;

    /// <summary>
    ///     The set of forbidden pawn types for filtering.
    /// </summary>
    public HashSet<PawnType> ForbiddenPawnTypes = [];

    /// <summary>
    ///     The list of pawn capacity limits for filtering.
    /// </summary>
    public List<PawnCapacityLimit> PawnCapacityLimits = [];

    /// <summary>
    ///     The list of pawn skill limits for filtering.
    /// </summary>
    public List<PawnSkillLimit> PawnSkillLimits = [];

    /// <summary>
    ///     The list of stat limits for filtering pawns.
    /// </summary>
    public List<StatLimit> PawnStatLimits = [];

    /// <summary>
    ///     The list of pawn trait limits for filtering. Each limit indicates whether its trait is required or forbidden.
    /// </summary>
    public List<PawnTraitLimit> PawnTraitLimits = [];

    /// <summary>
    ///     Indicates whether the control is in tri-state mode.
    /// </summary>
    public bool TriStateMode;

    /// <summary>
    ///     The dictionary of work capacity limits for filtering.
    ///     The key is the work capacity name, and the value indicates if the capacity is required (true) or forbidden (false).
    /// </summary>
    public Dictionary<WorkTags, bool> WorkCapacityLimits = [];

    /// <summary>
    ///     Exposes the filter data for saving and loading.
    /// </summary>
    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving) Validate();
        Scribe_Values.Look(ref TriStateMode, nameof(TriStateMode));
        Scribe_Values.Look(ref FilterPawnTypes, nameof(FilterPawnTypes));
        Scribe_Collections.Look(ref AllowedPawnTypes, nameof(AllowedPawnTypes), LookMode.Value);
        Scribe_Collections.Look(ref ForbiddenPawnTypes, nameof(ForbiddenPawnTypes), LookMode.Value);
        Scribe_Values.Look(ref FilterPawnHealthStates, nameof(FilterPawnHealthStates));
        Scribe_Values.Look(ref AllowedPawnHealthStates, nameof(AllowedPawnHealthStates));
        Scribe_Values.Look(ref ForbiddenPawnHealthStates, nameof(ForbiddenPawnHealthStates));
        Scribe_Values.Look(ref FilterWorkPassions, nameof(FilterWorkPassions));
        Scribe_Collections.Look(ref AllowedWorkPassions, nameof(AllowedWorkPassions),
            LookMode.Value);
        Scribe_Values.Look(ref FilterPawnTraits, nameof(FilterPawnTraits));
        Scribe_Collections.Look(ref PawnTraitLimits, nameof(PawnTraitLimits), LookMode.Deep);
        Scribe_Values.Look(ref FilterPawnCapacities, nameof(FilterPawnCapacities));
        Scribe_Collections.Look(ref PawnCapacityLimits, nameof(PawnCapacityLimits), LookMode.Deep);
        Scribe_Values.Look(ref FilterWorkCapacities, nameof(FilterWorkCapacities));
        Scribe_Collections.Look(ref WorkCapacityLimits, nameof(WorkCapacityLimits), LookMode.Value,
            LookMode.Value);
        Scribe_Values.Look(ref FilterPawnSkills, nameof(FilterPawnSkills));
        Scribe_Collections.Look(ref PawnSkillLimits, nameof(PawnSkillLimits), LookMode.Deep);
        Scribe_Values.Look(ref FilterPawnStats, nameof(FilterPawnStats));
        Scribe_Collections.Look(ref PawnStatLimits, nameof(PawnStatLimits), LookMode.Deep);
        Scribe_Values.Look(ref FilterPawnPrimaryWeaponTypes, nameof(FilterPawnPrimaryWeaponTypes));
        Scribe_Collections.Look(ref AllowedPawnPrimaryWeaponTypes,
            nameof(AllowedPawnPrimaryWeaponTypes), LookMode.Value);
    }

    /// <summary>
    ///     Combines two <see cref="PawnFilter" /> instances, preferring values from <paramref name="main" /> when available,
    ///     and falling back to <paramref name="fallback" /> otherwise.
    /// </summary>
    /// <param name="main">
    ///     The primary <see cref="PawnFilter" /> whose values take precedence if set.
    /// </param>
    /// <param name="fallback">
    ///     The fallback <see cref="PawnFilter" /> whose values are used if <paramref name="main" /> does not specify them.
    /// </param>
    /// <returns>
    ///     A new <see cref="PawnFilter" /> instance containing the combined filter settings.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if either <paramref name="main" /> or <paramref name="fallback" /> is <c>null</c>.
    /// </exception>
    public static PawnFilter Combine(PawnFilter main, PawnFilter fallback)
    {
        if (main == null) throw new ArgumentNullException(nameof(main));
        if (fallback == null) throw new ArgumentNullException(nameof(fallback));
        var combined = new PawnFilter { TriStateMode = false };
        CombinePawnTypes(combined, main, fallback);
        CombinePawnHealthStates(combined, main, fallback);
        CombineWorkPassions(combined, main, fallback);
        CombinePawnTraits(combined, main, fallback);
        CombinePawnCapacities(combined, main, fallback);
        CombineWorkCapacities(combined, main, fallback);
        CombinePawnSkills(combined, main, fallback);
        CombinePawnStats(combined, main, fallback);
        CombinePawnPrimaryWeaponTypes(combined, main, fallback);
        combined.Validate();
        return combined;
    }

    private static void CombinePawnCapacities(PawnFilter combined, PawnFilter main,
        PawnFilter fallback)
    {
        var source = main.FilterPawnCapacities.HasValue ? main : fallback;
        combined.FilterPawnCapacities = source.FilterPawnCapacities;
        combined.PawnCapacityLimits = [.. source.PawnCapacityLimits];
    }

    private static void CombinePawnHealthStates(PawnFilter combined, PawnFilter main,
        PawnFilter fallback)
    {
        var source = main.FilterPawnHealthStates.HasValue ? main : fallback;
        combined.FilterPawnHealthStates = source.FilterPawnHealthStates;
        combined.AllowedPawnHealthStates = source.AllowedPawnHealthStates;
        combined.ForbiddenPawnHealthStates = source.ForbiddenPawnHealthStates;
    }

    private static void CombinePawnPrimaryWeaponTypes(PawnFilter combined, PawnFilter main,
        PawnFilter fallback)
    {
        var source = main.FilterPawnPrimaryWeaponTypes.HasValue ? main : fallback;
        combined.FilterPawnPrimaryWeaponTypes = source.FilterPawnPrimaryWeaponTypes;
        combined.AllowedPawnPrimaryWeaponTypes = [.. source.AllowedPawnPrimaryWeaponTypes];
    }

    private static void CombinePawnSkills(PawnFilter combined, PawnFilter main, PawnFilter fallback)
    {
        var source = main.FilterPawnSkills.HasValue ? main : fallback;
        combined.FilterPawnSkills = source.FilterPawnSkills;
        combined.PawnSkillLimits = [.. source.PawnSkillLimits];
    }

    private static void CombinePawnStats(PawnFilter combined, PawnFilter main, PawnFilter fallback)
    {
        var source = main.FilterPawnStats.HasValue ? main : fallback;
        combined.FilterPawnStats = source.FilterPawnStats;
        combined.PawnStatLimits = [.. source.PawnStatLimits];
    }

    private static void CombinePawnTraits(PawnFilter combined, PawnFilter main, PawnFilter fallback)
    {
        var source = main.FilterPawnTraits.HasValue ? main : fallback;
        combined.FilterPawnTraits = source.FilterPawnTraits;
        combined.PawnTraitLimits = [.. source.PawnTraitLimits];
    }

    private static void CombinePawnTypes(PawnFilter combined, PawnFilter main, PawnFilter fallback)
    {
        var source = main.FilterPawnTypes.HasValue ? main : fallback;
        combined.FilterPawnTypes = source.FilterPawnTypes;
        combined.AllowedPawnTypes = [.. source.AllowedPawnTypes];
        combined.ForbiddenPawnTypes = [.. source.ForbiddenPawnTypes];
    }

    private static void CombineWorkCapacities(PawnFilter combined, PawnFilter main,
        PawnFilter fallback)
    {
        var source = main.FilterWorkCapacities.HasValue ? main : fallback;
        combined.FilterWorkCapacities = source.FilterWorkCapacities;
        combined.WorkCapacityLimits = new Dictionary<WorkTags, bool>(source.WorkCapacityLimits);
    }

    private static void CombineWorkPassions(PawnFilter combined, PawnFilter main,
        PawnFilter fallback)
    {
        var source = main.FilterWorkPassions.HasValue ? main : fallback;
        combined.FilterWorkPassions = source.FilterWorkPassions;
        combined.AllowedWorkPassions = [.. source.AllowedWorkPassions];
    }

    /// <summary>
    ///     Creates a deep copy of the current <see cref="PawnFilter" /> instance.
    /// </summary>
    /// <remarks>
    ///     The returned copy includes deep copies of collections and complex objects, ensuring that
    ///     changes to the copy do not affect the original instance, and vice versa.
    /// </remarks>
    /// <returns>A new <see cref="PawnFilter" /> instance with the same configuration and state as the current instance.</returns>
    public PawnFilter Copy()
    {
        return new PawnFilter
        {
            AllowedPawnHealthStates = AllowedPawnHealthStates,
            AllowedPawnTypes = [.. AllowedPawnTypes],
            AllowedWorkPassions = [.. AllowedWorkPassions],
            FilterPawnCapacities = FilterPawnCapacities,
            FilterPawnHealthStates = FilterPawnHealthStates,
            FilterPawnSkills = FilterPawnSkills,
            FilterPawnStats = FilterPawnStats,
            FilterPawnTraits = FilterPawnTraits,
            FilterPawnTypes = FilterPawnTypes,
            FilterWorkCapacities = FilterWorkCapacities,
            FilterWorkPassions = FilterWorkPassions,
            ForbiddenPawnHealthStates = ForbiddenPawnHealthStates,
            ForbiddenPawnTypes = [.. ForbiddenPawnTypes],
            PawnCapacityLimits =
            [
                // Where(l => l != null) filters nulls; ! asserts non-null to the compiler
                .. PawnCapacityLimits.Select(l =>
                {
                    var def = l.Def;
                    return def == null ? null : new PawnCapacityLimit(def) { Limit = l.Limit };
                }).Where(l => l != null).Select(l => l!)
            ],
            PawnSkillLimits =
            [
                // Where(l => l != null) filters nulls; ! asserts non-null to the compiler
                .. PawnSkillLimits.Select(l =>
                {
                    var def = l.Def;
                    return def == null ? null : new PawnSkillLimit(def) { Limit = l.Limit };
                }).Where(l => l != null).Select(l => l!)
            ],
            PawnStatLimits =
            [
                // Where(l => l != null) filters nulls; ! asserts non-null to the compiler
                .. PawnStatLimits.Select(l =>
                {
                    var def = l.Def;
                    return def == null
                        ? null
                        : new StatLimit(def)
                        {
                            Limit = l.Limit, LimitMaxCap = l.LimitMaxCap,
                            LimitMinCap = l.LimitMinCap,
                            ValueStyle = l.ValueStyle
                        };
                }).Where(l => l != null).Select(l => l!)
            ],
            PawnTraitLimits =
            [
                // Where(l => l != null) filters nulls; ! asserts non-null to the compiler
                .. PawnTraitLimits.Select(l =>
                {
                    var def = l.Def;
                    return def == null ? null : new PawnTraitLimit(def) { Limit = l.Limit };
                }).Where(l => l != null).Select(l => l!)
            ],
            TriStateMode = TriStateMode,
            WorkCapacityLimits = new Dictionary<WorkTags, bool>(WorkCapacityLimits),
            FilterPawnPrimaryWeaponTypes = FilterPawnPrimaryWeaponTypes,
            AllowedPawnPrimaryWeaponTypes = [.. AllowedPawnPrimaryWeaponTypes]
        };
    }

    /// <summary>
    ///     Returns a set of pawns from the given maps that match the pawn filter settings.
    /// </summary>
    /// <param name="maps">The maps to search for pawns.</param>
    /// <param name="workType">The work type to filter by passion, or null to ignore passion filtering.</param>
    /// <returns>A set of filtered pawns.</returns>
    public HashSet<Pawn> GetFilteredPawns(IEnumerable<Map> maps, WorkTypeDef? workType)
    {
        if (maps == null) throw new ArgumentNullException(nameof(maps));
        var pawns = new HashSet<Pawn>();
        foreach (var map in maps)
        {
            var allPawns = map.mapPawns.AllPawnsSpawned;
            foreach (var pawn in allPawns)
            {
                if (!SatisfiesFilter(pawn, workType)) continue;
                pawns.Add(pawn);
            }
        }

        return pawns;
    }

    /// <summary>
    ///     Generates a detailed summary of the current pawn filter configuration, formatted with the specified indentation
    ///     level.
    /// </summary>
    /// <remarks>
    ///     The summary includes only the filter criteria that are currently active. Each section is indented
    ///     according to the specified <paramref name="indentationLevel" /> and formatted for readability. This method is
    ///     useful
    ///     for displaying the filter configuration in a user interface.
    /// </remarks>
    /// <param name="indentationLevel">
    ///     The number of indentation levels to apply to the formatted summary. Must be a
    ///     non-negative integer.
    /// </param>
    /// <returns>A string containing the formatted summary of the pawn filter configuration.</returns>
    public string GetSummary(int indentationLevel)
    {
        var stringBuilder = new StringBuilder();
        var anyValue = false;
        if (FilterPawnTypes.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.AllowedPawnTypesLabel}: ".Colorize(ColoredText
                    .ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterPawnTypes.Value
                ? string.Join(", ", AllowedPawnTypes.Select(Resources.Strings.PawnType.GetLabel))
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (FilterPawnHealthStates.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.AllowedPawnHealthStatesLabel}: ".Colorize(
                    ColoredText.ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterPawnHealthStates.Value
                ? string.Join(", ",
                    EnumHelper.GetUniqueFlags(AllowedPawnHealthStates, ForbiddenPawnHealthStates)
                        .Select(Resources.Strings.PawnHealthState.GetLabel))
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (FilterPawnPrimaryWeaponTypes.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.AllowedPawnPrimaryWeaponTypesLabel}: ".Colorize(
                    ColoredText.ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterPawnPrimaryWeaponTypes.Value
                ? string.Join(", ",
                    AllowedPawnPrimaryWeaponTypes.Select(Resources.Strings.PawnPrimaryWeaponType
                        .GetLabel))
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (FilterPawnSkills.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.PawnSkillLimitsLabel}: ".Colorize(ColoredText
                    .ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterPawnSkills.Value
                ? string.Join(", ",
                    PawnSkillLimits.Select(l =>
                        $"{l.Label} [{l.Limit.TrueMin:N0}..{l.Limit.TrueMax:N0}]"))
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (FilterWorkPassions.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.AllowedWorkPassionsLabel}: ".Colorize(ColoredText
                    .ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterWorkPassions.Value
                ? string.Join(", ",
                    AllowedWorkPassions.Select(PassionHelper.GetPassionCache).Where(p => p != null)
                        .Select(p =>
                            p!.Label)) // Where(p => p != null) guards non-null; ! asserts that to the compiler
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (FilterWorkCapacities.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.WorkCapacityLimitsLabel}: ".Colorize(ColoredText
                    .ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterWorkCapacities.Value
                ? string.Join(", ",
                    WorkCapacityLimits.Select(l =>
                        $"{(l.Value ? "+" : "-")}{l.Key.LabelTranslated()}"))
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (FilterPawnTraits.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.PawnTraitLimitsLabel}: ".Colorize(ColoredText
                    .ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterPawnTraits.Value
                ? string.Join(", ",
                    PawnTraitLimits.Select(l => $"{(l.Limit ? "+" : "-")}{l.Label}"))
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (FilterPawnStats.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.PawnStatLimitsLabel}: ".Colorize(ColoredText
                    .ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterPawnStats.Value
                ? string.Join(", ",
                    PawnStatLimits.Select(l =>
                        $"{l.Label} [{l.Limit.TrueMin.ToStringByStyle(l.ValueStyle)}..{l.Limit.TrueMax.ToStringByStyle(l.ValueStyle)}]"))
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (FilterPawnCapacities.HasValue)
        {
            anyValue = true;
            stringBuilder.AppendIndented(
                $"{Resources.Strings.PawnFilter.PawnCapacityLimitsLabel}: ".Colorize(ColoredText
                    .ExpectationsColor), indentationLevel);
            stringBuilder.AppendLine(FilterPawnCapacities.Value
                ? string.Join(", ",
                    PawnCapacityLimits.Select(l =>
                        $"{l.Label} [{l.Limit.TrueMin.ToStringByStyle(PawnCapacityLimit.ValueStyle)}..{l.Limit.TrueMax.ToStringByStyle(PawnCapacityLimit.ValueStyle)}]"))
                : Resources.Strings.PawnFilter.IgnoreFilter);
        }

        if (!anyValue)
            stringBuilder.AppendIndented(Resources.Strings.PawnFilter.UndefinedFilterTooltip,
                indentationLevel);
        return stringBuilder.ToString();
    }

    /// <summary>
    ///     Determines whether the specified <paramref name="pawn" /> satisfies the defined filters and constraints.
    /// </summary>
    /// <remarks>
    ///     This method evaluates the <paramref name="pawn" /> against multiple filters, including pawn
    ///     type, health state, work passions, traits, capacities, skills, and stats. Each filter is applied only if it is
    ///     enabled and configured. If any filter is not satisfied, the method returns <see langword="false" />. If all
    ///     applicable filters are satisfied, the method returns <see langword="true" />.
    /// </remarks>
    /// <param name="pawn">The pawn to evaluate. Cannot be <see langword="null" />.</param>
    /// <param name="workType">
    ///     The work type to consider when filtering by work passions. Can be <see langword="null" /> if work passion
    ///     filtering is not required.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the <paramref name="pawn" /> meets all the specified filters and constraints;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawn" /> is <see langword="null" />.</exception>
    public bool SatisfiesFilter(Pawn pawn, WorkTypeDef? workType)
    {
        if (pawn == null) throw new ArgumentNullException(nameof(pawn));
        if (FilterPawnTypes == true)
        {
            var pawnType = PawnHelper.GetPawnType(pawn);
            if (!AllowedPawnTypes.Contains(pawnType)) return false;
        }

        if (FilterPawnHealthStates == true)
        {
            var healthState = PawnHelper.GetPawnHealthState(pawn);
            if (EnumHelper.HasAnyFlag(healthState, ForbiddenPawnHealthStates) ||
                !EnumHelper.HasAnyFlag(healthState, AllowedPawnHealthStates))
            {
#if DEBUG
                Logger.LogMessage(
                    $"{pawn.LabelShort} doesn't satisfy filter. Reason = Pawn Health State ({healthState}).");
#endif
                return false;
            }
        }

        if (FilterPawnPrimaryWeaponTypes == true)
        {
            var weaponType = PawnHelper.GetPrimaryWeaponType(pawn);
            if (!AllowedPawnPrimaryWeaponTypes.Contains(weaponType))
            {
#if DEBUG
                Logger.LogMessage(
                    $"{pawn.LabelShort} doesn't satisfy filter. Reason = Primary Weapon ({weaponType}).");
#endif
                return false;
            }
        }

        if (FilterWorkPassions == true && workType != null)
        {
            var passion = PawnHelper.GetWorkPassion(pawn, workType);
            if (!AllowedWorkPassions.Contains(passion))
            {
#if DEBUG
                Logger.LogMessage(
                    $"{pawn.LabelShort} doesn't satisfy filter. Reason = Work Passion ({passion}).");
#endif
                return false;
            }
        }

        if (FilterPawnTraits == true && PawnTraitLimits.Count > 0)
        {
            var traits = pawn.story?.traits;
            if (traits == null) return false;
            bool enabledSatisfied = true, disabledSatisfied = true, hasEnabled = false;
            foreach (var limit in PawnTraitLimits)
            {
                if (limit.Def == null) continue;
                if (limit.Limit)
                {
                    if (!hasEnabled)
                    {
                        enabledSatisfied = false;
                        hasEnabled = true;
                    }

                    if (!enabledSatisfied && traits.HasTrait(limit.Def))
                        enabledSatisfied = true;
                }
                else
                {
                    if (traits.HasTrait(limit.Def))
                    {
                        disabledSatisfied = false;
                        break;
                    }
                }
            }

            if (!enabledSatisfied || !disabledSatisfied)
            {
#if DEBUG
                Logger.LogMessage(
                    $"{pawn.LabelShort} doesn't satisfy filter. Reason = Pawn Traits.");
#endif
                return false;
            }
        }

        if (FilterPawnCapacities == true && PawnCapacityLimits.Count > 0)
        {
            if (pawn.health?.capacities == null) return false;
            var satisfied = true;
            foreach (var limit in PawnCapacityLimits)
            {
                if (limit.Def == null) continue;
                if (!limit.Limit.Includes(pawn.health.capacities.GetLevel(limit.Def)))
                {
                    satisfied = false;
                    break;
                }
            }

            if (!satisfied)
            {
#if DEBUG
                Logger.LogMessage(
                    $"{pawn.LabelShort} doesn't satisfy filter. Reason = Pawn Capacities.");
#endif
                return false;
            }
        }

        if (FilterWorkCapacities == true && WorkCapacityLimits.Count > 0)
        {
            var satisfied = true;
            foreach (var limit in WorkCapacityLimits)
                if (pawn.WorkTagIsDisabled(limit.Key) == limit.Value)
                {
                    satisfied = false;
                    break;
                }

            if (!satisfied)
            {
#if DEBUG
                Logger.LogMessage(
                    $"{pawn.LabelShort} doesn't satisfy filter. Reason = Work Capacities.");
#endif
                return false;
            }
        }

        if (FilterPawnSkills == true && PawnSkillLimits.Count > 0)
        {
            if (pawn.skills == null) return false;
            var satisfied = true;
            foreach (var limit in PawnSkillLimits)
            {
                if (limit.Def == null) continue;
                var value = pawn.skills.GetSkill(limit.Def).Level;
                if (value < limit.Limit.TrueMin || value > limit.Limit.TrueMax)
                {
                    satisfied = false;
                    break;
                }
            }

            if (!satisfied)
            {
#if DEBUG
                Logger.LogMessage(
                    $"{pawn.LabelShort} doesn't satisfy filter. Reason = Pawn Skills.");
#endif
                return false;
            }
        }

        if (FilterPawnStats == true && PawnStatLimits.Count > 0)
        {
            var satisfied = true;
            foreach (var limit in PawnStatLimits)
            {
                if (limit.Def == null) continue;
                var statValue = pawn.GetStatValue(limit.Def);
                if (statValue < limit.Limit.TrueMin || statValue > limit.Limit.TrueMax)
                {
                    satisfied = false;
                    break;
                }
            }

            if (!satisfied)
            {
#if DEBUG
                Logger.LogMessage(
                    $"{pawn.LabelShort} doesn't satisfy filter. Reason = Pawn Stats.");
#endif
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Ensures that all filter-related properties are set to valid default values  when <see cref="TriStateMode" /> is
    ///     disabled.
    /// </summary>
    /// <remarks>
    ///     This method initializes filter properties to <see langword="false" /> if they are null  and
    ///     <see cref="TriStateMode" /> is not enabled. It is intended to maintain consistent  state for filter-related
    ///     settings.
    /// </remarks>
    public void Validate()
    {
        if (!TriStateMode)
        {
            FilterPawnTypes ??= false;
            FilterWorkPassions ??= false;
            FilterPawnCapacities ??= false;
            FilterPawnHealthStates ??= false;
            FilterPawnSkills ??= false;
            FilterPawnStats ??= false;
            FilterPawnTraits ??= false;
            FilterWorkCapacities ??= false;
            FilterPawnPrimaryWeaponTypes ??= false;
        }
    }
}