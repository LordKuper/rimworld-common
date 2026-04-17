using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Provides access to common resources used throughout the mod.
/// </summary>
public static class Resources
{
    /// <summary>
    ///     Contains string resources for the mod.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        ///     Provides localized strings for common actions.
        /// </summary>
        public static class Actions
        {
            /// <summary>
            ///     Gets the localized string for the "Add" action.
            /// </summary>
            [UsedImplicitly] public static readonly string Add =
                $"{CommonMod.ModId}.{nameof(Actions)}.{nameof(Add)}".Translate();

            /// <summary>
            ///     Gets the localized string for the "Delete" action.
            /// </summary>
            [UsedImplicitly] public static readonly string Delete =
                $"{CommonMod.ModId}.{nameof(Actions)}.{nameof(Delete)}".Translate();

            /// <summary>
            ///     Gets the localized string for the "Edit" action.
            /// </summary>
            [UsedImplicitly] public static readonly string Edit =
                $"{CommonMod.ModId}.{nameof(Actions)}.{nameof(Edit)}".Translate();

            /// <summary>
            ///     Gets the localized string for the "Refresh" action.
            /// </summary>
            [UsedImplicitly] public static readonly string Refresh =
                $"{CommonMod.ModId}.{nameof(Actions)}.{nameof(Refresh)}".Translate();

            /// <summary>
            ///     Gets the localized string for the "Reset" action.
            /// </summary>
            [UsedImplicitly] public static readonly string Reset =
                $"{CommonMod.ModId}.{nameof(Actions)}.{nameof(Reset)}".Translate();

            /// <summary>
            ///     Gets the localized string for the "Select" action.
            /// </summary>
            [UsedImplicitly] public static readonly string Select =
                $"{CommonMod.ModId}.{nameof(Actions)}.{nameof(Select)}".Translate();
        }

        /// <summary>
        ///     Provides string resources for multi-checkbox states.
        /// </summary>
        public static class MultiCheckboxStates
        {
            /// <summary>
            ///     Gets the string representing the "Off" state, colorized.
            /// </summary>
            [UsedImplicitly]
            public static readonly string Off = "X".Colorize(GenColor.FromBytes(217, 38, 38));

            /// <summary>
            ///     Gets the string representing the "On" state, colorized.
            /// </summary>
            [UsedImplicitly]
            public static readonly string On = "✓".Colorize(GenColor.FromBytes(38, 217, 38));

            /// <summary>
            ///     Gets the string representing the "Partial" state, colorized.
            /// </summary>
            [UsedImplicitly]
            public static readonly string Partial = "~".Colorize(GenColor.FromBytes(218, 178, 46));
        }

        /// <summary>
        ///     Provides string resources for passion-related descriptions.
        /// </summary>
        internal static class Passions
        {
            /// <summary>
            ///     Gets the localized description for the forget rate factor.
            /// </summary>
            internal static readonly string ForgetRateFactorDescription =
                $"{CommonMod.ModId}.{nameof(Passions)}.{nameof(ForgetRateFactorDescription)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized description for the learn rate factor.
            /// </summary>
            internal static readonly string LearnRateFactorDescription =
                $"{CommonMod.ModId}.{nameof(Passions)}.{nameof(LearnRateFactorDescription)}"
                    .Translate();
        }

        /// <summary>
        ///     Provides string resources for the PawnFilter component.
        /// </summary>
        internal static class PawnFilter
        {
            private static string _filterPawnCapacitiesTooltip;
            private static string _filterPawnCapacitiesTriStateTooltip;
            private static string _filterPawnHealthStatesTooltip;
            private static string _filterPawnHealthStatesTriStateTooltip;
            private static string _filterPawnPrimaryWeaponTypesTooltip;
            private static string _filterPawnPrimaryWeaponTypesTriStateTooltip;
            private static string _filterPawnSkillsTooltip;
            private static string _filterPawnSkillsTriStateTooltip;
            private static string _filterPawnStatsTooltip;
            private static string _filterPawnStatsTriStateTooltip;
            private static string _filterPawnTraitsTooltip;
            private static string _filterPawnTraitsTriStateTooltip;
            private static string _filterPawnTypesTooltip;
            private static string _filterPawnTypesTriStateTooltip;
            private static string _filterWorkCapacitiesTooltip;
            private static string _filterWorkCapacitiesTriStateTooltip;
            private static string _filterWorkPassionsTooltip;
            private static string _filterWorkPassionsTriStateTooltip;

            /// <summary>
            ///     Gets the localized label for allowed pawn health states.
            /// </summary>
            public static readonly string AllowedPawnHealthStatesLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(AllowedPawnHealthStatesLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for allowed pawn health states.
            /// </summary>
            public static readonly string AllowedPawnHealthStatesTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(AllowedPawnHealthStatesTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized label for allowed pawn primary weapon types.
            /// </summary>
            public static readonly string AllowedPawnPrimaryWeaponTypesLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(AllowedPawnPrimaryWeaponTypesLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for allowed pawn primary weapon types.
            /// </summary>
            public static readonly string AllowedPawnPrimaryWeaponTypesTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(AllowedPawnPrimaryWeaponTypesTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized label for allowed pawn types.
            /// </summary>
            public static readonly string AllowedPawnTypesLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(AllowedPawnTypesLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for allowed pawn types.
            /// </summary>
            public static readonly string AllowedPawnTypesTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(AllowedPawnTypesTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized label for allowed work passions.
            /// </summary>
            public static readonly string AllowedWorkPassionsLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(AllowedWorkPassionsLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for allowed work passions.
            /// </summary>
            public static readonly string AllowedWorkPassionsTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(AllowedWorkPassionsTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of pawn capacities filter.
            /// </summary>
            public static readonly string FilterPawnCapacitiesOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnCapacitiesOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of pawn capacities filter.
            /// </summary>
            public static readonly string FilterPawnCapacitiesOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnCapacitiesOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of pawn health states filter.
            /// </summary>
            public static readonly string FilterPawnHealthStatesOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnHealthStatesOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of pawn health states filter.
            /// </summary>
            public static readonly string FilterPawnHealthStatesOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnHealthStatesOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of pawn primary weapon types filter.
            /// </summary>
            public static readonly string FilterPawnPrimaryWeaponTypesOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnPrimaryWeaponTypesOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of pawn primary weapon types filter.
            /// </summary>
            public static readonly string FilterPawnPrimaryWeaponTypesOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnPrimaryWeaponTypesOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of pawn skills filter.
            /// </summary>
            public static readonly string FilterPawnSkillsOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnSkillsOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of pawn skills filter.
            /// </summary>
            public static readonly string FilterPawnSkillsOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnSkillsOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of pawn stats filter.
            /// </summary>
            public static readonly string FilterPawnStatsOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnStatsOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of pawn stats filter.
            /// </summary>
            public static readonly string FilterPawnStatsOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnStatsOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of pawn traits filter.
            /// </summary>
            public static readonly string FilterPawnTraitsOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnTraitsOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of pawn traits filter.
            /// </summary>
            public static readonly string FilterPawnTraitsOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnTraitsOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of pawn types filter.
            /// </summary>
            public static readonly string FilterPawnTypesOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnTypesOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of pawn types filter.
            /// </summary>
            public static readonly string FilterPawnTypesOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterPawnTypesOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of work capacities filter.
            /// </summary>
            public static readonly string FilterWorkCapacitiesOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterWorkCapacitiesOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of work capacities filter.
            /// </summary>
            public static readonly string FilterWorkCapacitiesOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterWorkCapacitiesOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "Off" state of work passions filter.
            /// </summary>
            public static readonly string FilterWorkPassionsOffTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterWorkPassionsOffTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for the "On" state of work passions filter.
            /// </summary>
            public static readonly string FilterWorkPassionsOnTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(FilterWorkPassionsOnTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the translation key for the ignored filter section.
            /// </summary>
            public static readonly string IgnoreFilter =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(IgnoreFilter)}".Translate();

            /// <summary>
            ///     Gets the localized label for pawn capacity limits.
            /// </summary>
            public static readonly string PawnCapacityLimitsLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(PawnCapacityLimitsLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for pawn capacity limits.
            /// </summary>
            public static readonly string PawnCapacityLimitsTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(PawnCapacityLimitsTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized label for pawn skill limits.
            /// </summary>
            public static readonly string PawnSkillLimitsLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(PawnSkillLimitsLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for pawn skill limits.
            /// </summary>
            public static readonly string PawnSkillLimitsTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(PawnSkillLimitsTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized label for pawn stat limits.
            /// </summary>
            public static readonly string PawnStatLimitsLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(PawnStatLimitsLabel)}".Translate();

            /// <summary>
            ///     Gets the localized tooltip for pawn stat limits.
            /// </summary>
            public static readonly string PawnStatLimitsTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(PawnStatLimitsTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized label for pawn trait limits.
            /// </summary>
            public static readonly string PawnTraitLimitsLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(PawnTraitLimitsLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for pawn trait limits.
            /// </summary>
            public static readonly string PawnTraitLimitsTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(PawnTraitLimitsTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip text for an undefined filter.
            /// </summary>
            public static readonly string UndefinedFilterTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(UndefinedFilterTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized label for work capacity limits.
            /// </summary>
            public static readonly string WorkCapacityLimitsLabel =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(WorkCapacityLimitsLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for work capacity limits.
            /// </summary>
            public static readonly string WorkCapacityLimitsTooltip =
                $"{CommonMod.ModId}.{nameof(PawnFilter)}.{nameof(WorkCapacityLimitsTooltip)}"
                    .Translate();

            /// <summary>
            ///     Appends a tooltip message indicating an undefined filter state to the specified tooltip text.
            /// </summary>
            /// <param name="tooltip">
            ///     The existing tooltip text to which the undefined filter message will be appended. Cannot be <see langword="null" />
            ///     .
            /// </param>
            /// <returns>
            ///     A new string that combines the original tooltip text with the undefined filter state message, separated by a
            ///     newline.
            /// </returns>
            [NotNull]
            private static string AppendUndefinedFilterTooltip(string tooltip)
            {
                return
                    $"{tooltip}{Environment.NewLine}{MultiCheckboxStates.Partial}: {UndefinedFilterTooltip}";
            }

            /// <summary>
            ///     Gets the tooltip for pawn capacities filter, optionally including the tri-state description.
            /// </summary>
            /// <param name="triState">If true, includes the partial state description.</param>
            /// <returns>The tooltip string for pawn capacities filter.</returns>
            [NotNull]
            public static string GetFilterPawnCapacitiesTooltip(bool triState)
            {
                if (!triState)
                    return _filterPawnCapacitiesTooltip ??= string.Concat(MultiCheckboxStates.On,
                        ": ", FilterPawnCapacitiesOnTooltip, Environment.NewLine,
                        MultiCheckboxStates.Off, ": ", FilterPawnCapacitiesOffTooltip);
                if (_filterPawnCapacitiesTriStateTooltip != null)
                    return _filterPawnCapacitiesTriStateTooltip;
                var baseTooltip = GetFilterPawnCapacitiesTooltip(false);
                _filterPawnCapacitiesTriStateTooltip = AppendUndefinedFilterTooltip(baseTooltip);
                return _filterPawnCapacitiesTriStateTooltip;
            }

            /// <summary>
            ///     Gets the tooltip for pawn health states filter, optionally including the tri-state description.
            /// </summary>
            /// <param name="triState">If true, includes the partial state description.</param>
            /// <returns>The tooltip string for pawn health states filter.</returns>
            [NotNull]
            public static string GetFilterPawnHealthStatesTooltip(bool triState)
            {
                if (!triState)
                    return _filterPawnHealthStatesTooltip ??= string.Concat(MultiCheckboxStates.On,
                        ": ", FilterPawnHealthStatesOnTooltip, Environment.NewLine,
                        MultiCheckboxStates.Off, ": ", FilterPawnHealthStatesOffTooltip);
                if (_filterPawnHealthStatesTriStateTooltip != null)
                    return _filterPawnHealthStatesTriStateTooltip;
                var baseTooltip = GetFilterPawnHealthStatesTooltip(false);
                _filterPawnHealthStatesTriStateTooltip = AppendUndefinedFilterTooltip(baseTooltip);
                return _filterPawnHealthStatesTriStateTooltip;
            }

            /// <summary>
            ///     Generates a tooltip describing the filter states for pawn primary weapon types.
            /// </summary>
            /// <param name="triState">
            ///     A value indicating whether the tooltip should include a tri-state description. If
            ///     <see
            ///         langword="true" />
            ///     , the tooltip includes an additional "undefined" state. If <see langword="false" />, the
            ///     tooltip describes only the "on" and "off" states.
            /// </param>
            /// <returns>
            ///     A string containing the tooltip text for the filter states. The tooltip includes descriptions for the
            ///     "on" and "off" states, and optionally the "undefined" state if <paramref name="triState" /> is
            ///     <see
            ///         langword="true" />
            ///     .
            /// </returns>
            [NotNull]
            public static string GetFilterPawnPrimaryWeaponTypesTooltip(bool triState)
            {
                if (!triState)
                    return _filterPawnPrimaryWeaponTypesTooltip ??= string.Concat(
                        MultiCheckboxStates.On, ": ", FilterPawnPrimaryWeaponTypesOnTooltip,
                        Environment.NewLine, MultiCheckboxStates.Off, ": ",
                        FilterPawnPrimaryWeaponTypesOffTooltip);
                if (_filterPawnPrimaryWeaponTypesTriStateTooltip != null)
                    return _filterPawnPrimaryWeaponTypesTriStateTooltip;
                var baseTooltip = GetFilterPawnPrimaryWeaponTypesTooltip(false);
                _filterPawnPrimaryWeaponTypesTriStateTooltip =
                    AppendUndefinedFilterTooltip(baseTooltip);
                return _filterPawnPrimaryWeaponTypesTriStateTooltip;
            }

            /// <summary>
            ///     Gets the tooltip for pawn skills filter, optionally including the tri-state description.
            /// </summary>
            /// <param name="triState">If true, includes the partial state description.</param>
            /// <returns>The tooltip string for pawn skills filter.</returns>
            [NotNull]
            public static string GetFilterPawnSkillsTooltip(bool triState)
            {
                if (!triState)
                    return _filterPawnSkillsTooltip ??= string.Concat(MultiCheckboxStates.On, ": ",
                        FilterPawnSkillsOnTooltip, Environment.NewLine, MultiCheckboxStates.Off,
                        ": ", FilterPawnSkillsOffTooltip);
                if (_filterPawnSkillsTriStateTooltip != null)
                    return _filterPawnSkillsTriStateTooltip;
                var baseTooltip = GetFilterPawnSkillsTooltip(false);
                _filterPawnSkillsTriStateTooltip = AppendUndefinedFilterTooltip(baseTooltip);
                return _filterPawnSkillsTriStateTooltip;
            }

            /// <summary>
            ///     Gets the tooltip for pawn stats filter, optionally including the tri-state description.
            /// </summary>
            /// <param name="triState">If true, includes the partial state description.</param>
            /// <returns>The tooltip string for pawn stats filter.</returns>
            [NotNull]
            public static string GetFilterPawnStatsTooltip(bool triState)
            {
                if (!triState)
                    return _filterPawnStatsTooltip ??= string.Concat(MultiCheckboxStates.On, ": ",
                        FilterPawnStatsOnTooltip, Environment.NewLine, MultiCheckboxStates.Off,
                        ": ", FilterPawnStatsOffTooltip);
                if (_filterPawnStatsTriStateTooltip != null) return _filterPawnStatsTriStateTooltip;
                var baseTooltip = GetFilterPawnStatsTooltip(false);
                _filterPawnStatsTriStateTooltip = AppendUndefinedFilterTooltip(baseTooltip);
                return _filterPawnStatsTriStateTooltip;
            }

            /// <summary>
            ///     Gets the tooltip for pawn traits filter, optionally including the tri-state description.
            /// </summary>
            /// <param name="triState">If true, includes the partial state description.</param>
            /// <returns>The tooltip string for pawn traits filter.</returns>
            [NotNull]
            public static string GetFilterPawnTraitsTooltip(bool triState)
            {
                if (!triState)
                    return _filterPawnTraitsTooltip ??= string.Concat(MultiCheckboxStates.On, ": ",
                        FilterPawnTraitsOnTooltip, Environment.NewLine, MultiCheckboxStates.Off,
                        ": ", FilterPawnTraitsOffTooltip);
                if (_filterPawnTraitsTriStateTooltip != null)
                    return _filterPawnTraitsTriStateTooltip;
                var baseTooltip = GetFilterPawnTraitsTooltip(false);
                _filterPawnTraitsTriStateTooltip = AppendUndefinedFilterTooltip(baseTooltip);
                return _filterPawnTraitsTriStateTooltip;
            }

            /// <summary>
            ///     Generates a tooltip string describing the filter states for pawn types.
            /// </summary>
            /// <param name="triState">
            ///     A value indicating whether the filter supports a tri-state mode. If <see langword="true" />, the tooltip
            ///     includes descriptions for "On", "Off", and "Partial" states. If <see langword="false" />, the tooltip
            ///     includes descriptions for only "On" and "Off" states.
            /// </param>
            /// <returns>
            ///     A string containing the tooltip text for the filter states. The string includes state names and their
            ///     corresponding descriptions, separated by new lines.
            /// </returns>
            [NotNull]
            public static string GetFilterPawnTypesTooltip(bool triState)
            {
                if (!triState)
                    return _filterPawnTypesTooltip ??= string.Concat(MultiCheckboxStates.On, ": ",
                        FilterPawnTypesOnTooltip, Environment.NewLine, MultiCheckboxStates.Off,
                        ": ", FilterPawnTypesOffTooltip);
                if (_filterPawnTypesTriStateTooltip != null) return _filterPawnTypesTriStateTooltip;
                var baseTooltip = GetFilterPawnTypesTooltip(false);
                _filterPawnTypesTriStateTooltip = AppendUndefinedFilterTooltip(baseTooltip);
                return _filterPawnTypesTriStateTooltip;
            }

            /// <summary>
            ///     Gets the tooltip for work capacities filter, optionally including the tri-state description.
            /// </summary>
            /// <param name="triState">If true, includes the partial state description.</param>
            /// <returns>The tooltip string for work capacities filter.</returns>
            [NotNull]
            public static string GetFilterWorkCapacitiesTooltip(bool triState)
            {
                if (!triState)
                    return _filterWorkCapacitiesTooltip ??= string.Concat(MultiCheckboxStates.On,
                        ": ", FilterWorkCapacitiesOnTooltip, Environment.NewLine,
                        MultiCheckboxStates.Off, ": ", FilterWorkCapacitiesOffTooltip);
                if (_filterWorkCapacitiesTriStateTooltip != null)
                    return _filterWorkCapacitiesTriStateTooltip;
                var baseTooltip = GetFilterWorkCapacitiesTooltip(false);
                _filterWorkCapacitiesTriStateTooltip = AppendUndefinedFilterTooltip(baseTooltip);
                return _filterWorkCapacitiesTriStateTooltip;
            }

            /// <summary>
            ///     Gets the tooltip for work passions filter, optionally including the tri-state description.
            /// </summary>
            /// <param name="triState">If true, includes the partial state description.</param>
            /// <returns>The tooltip string for work passions filter.</returns>
            [NotNull]
            public static string GetFilterWorkPassionsTooltip(bool triState)
            {
                if (!triState)
                    return _filterWorkPassionsTooltip ??= string.Concat(MultiCheckboxStates.On,
                        ": ", FilterWorkPassionsOnTooltip, Environment.NewLine,
                        MultiCheckboxStates.Off, ": ", FilterWorkPassionsOffTooltip);
                if (_filterWorkPassionsTriStateTooltip != null)
                    return _filterWorkPassionsTriStateTooltip;
                var baseTooltip = GetFilterWorkPassionsTooltip(false);
                _filterWorkPassionsTriStateTooltip = AppendUndefinedFilterTooltip(baseTooltip);
                return _filterWorkPassionsTriStateTooltip;
            }
        }

        /// <summary>
        ///     Provides string resources and label caching for pawn health states.
        /// </summary>
        internal static class PawnHealthState
        {
            /// <summary>
            ///     Caches localized labels for each <see cref="Filters.PawnHealthState" /> value.
            /// </summary>
            private static readonly ConcurrentDictionary<Filters.PawnHealthState, string> Labels =
                new();

            /// <summary>
            ///     A thread-safe dictionary that maps <see cref="Filters.PawnHealthState" /> values to their corresponding
            ///     tooltip strings.
            /// </summary>
            private static readonly ConcurrentDictionary<Filters.PawnHealthState, string> Tooltips =
                new();

            /// <summary>
            ///     Gets the localized label for the specified <see cref="Filters.PawnHealthState" /> value.
            ///     The result is cached for future calls.
            /// </summary>
            /// <param name="healthState">The pawn health state for which to get the label.</param>
            /// <returns>The localized label string for the specified health state.</returns>
            public static string GetLabel(Filters.PawnHealthState healthState)
            {
                return Labels.GetOrAdd(healthState,
                    hs => $"{CommonMod.ModId}.{nameof(PawnHealthState)}.{hs}.Label".Translate());
            }

            /// <summary>
            ///     Retrieves the localized tooltip text for the specified pawn health state.
            /// </summary>
            /// <param name="healthState">The health state of the pawn for which the tooltip is being retrieved.</param>
            /// <returns>A localized string representing the tooltip for the given health state.</returns>
            public static string GetTooltip(Filters.PawnHealthState healthState)
            {
                return Tooltips.GetOrAdd(healthState,
                    hs => $"{CommonMod.ModId}.{nameof(PawnHealthState)}.{hs}.Tooltip".Translate());
            }
        }

        /// <summary>
        ///     Provides string resources and label/tooltip caching for pawn primary weapon types.
        /// </summary>
        internal static class PawnPrimaryWeaponType
        {
            /// <summary>
            ///     Caches localized labels for each <see cref="Filters.PawnPrimaryWeaponType" /> value.
            /// </summary>
            private static readonly ConcurrentDictionary<Filters.PawnPrimaryWeaponType, string>
                Labels = new();

            /// <summary>
            ///     Caches localized tooltips for each <see cref="Filters.PawnPrimaryWeaponType" /> value.
            /// </summary>
            private static readonly ConcurrentDictionary<Filters.PawnPrimaryWeaponType, string>
                Tooltips = new();

            /// <summary>
            ///     Gets the localized label for the specified <see cref="Filters.PawnPrimaryWeaponType" /> value.
            ///     The result is cached for future calls.
            /// </summary>
            /// <param name="weaponType">The primary weapon type for which to get the label.</param>
            /// <returns>The localized label string for the specified weapon type.</returns>
            public static string GetLabel(Filters.PawnPrimaryWeaponType weaponType)
            {
                return Labels.GetOrAdd(weaponType,
                    wt => $"{CommonMod.ModId}.{nameof(PawnPrimaryWeaponType)}.{wt}.Label"
                        .Translate());
            }

            /// <summary>
            ///     Gets the localized tooltip for the specified <see cref="Filters.PawnPrimaryWeaponType" /> value.
            ///     The result is cached for future calls.
            /// </summary>
            /// <param name="weaponType">The primary weapon type for which to get the tooltip.</param>
            /// <returns>The localized tooltip string for the specified weapon type.</returns>
            public static string GetTooltip(Filters.PawnPrimaryWeaponType weaponType)
            {
                return Tooltips.GetOrAdd(weaponType,
                    wt => $"{CommonMod.ModId}.{nameof(PawnPrimaryWeaponType)}.{wt}.Tooltip"
                        .Translate());
            }
        }

        /// <summary>
        ///     Provides string resources and label caching for pawn types.
        /// </summary>
        internal static class PawnType
        {
            /// <summary>
            ///     Caches localized labels for each <see cref="Filters.PawnType" /> value.
            /// </summary>
            private static readonly ConcurrentDictionary<Filters.PawnType, string> Labels = new();

            /// <summary>
            ///     Gets the localized label for the specified <see cref="Filters.PawnType" /> value.
            ///     The result is cached for future calls.
            /// </summary>
            /// <param name="pawnType">The pawn type for which to get the label.</param>
            /// <returns>The localized label string for the specified pawn type.</returns>
            public static string GetLabel(Filters.PawnType pawnType)
            {
                return Labels.GetOrAdd(pawnType,
                    pt => $"{CommonMod.ModId}.{nameof(PawnType)}.{pt}.Label".Translate());
            }
        }

        /// <summary>
        ///     Provides methods for retrieving stat-related string resources.
        /// </summary>
        internal static class Stats
        {
            /// <summary>
            ///     Gets the localized description for a stat by its definition name.
            /// </summary>
            /// <param name="defName">The definition name of the stat.</param>
            /// <returns>The localized description string for the stat.</returns>
            [NotNull]
            public static string GetDescription(string defName)
            {
                return $"{CommonMod.ModId}.{nameof(Stats)}.{defName}.Description".Translate();
            }

            /// <summary>
            ///     Gets the localized label for a stat by its definition name.
            /// </summary>
            /// <param name="defName">The definition name of the stat.</param>
            /// <returns>The localized label string for the stat.</returns>
            [NotNull]
            public static string GetLabel(string defName)
            {
                return $"{CommonMod.ModId}.{nameof(Stats)}.{defName}.Label".Translate();
            }
        }

        /// <summary>
        ///     Provides string resources for the WorkTypeThingRuleWidget UI component.
        /// </summary>
        internal static class WorkTypeThingRuleWidget
        {
            /// <summary>
            ///     Gets the localized label for available items.
            /// </summary>
            public static readonly string AvailableItemsLabel =
                $"{CommonMod.ModId}.{nameof(WorkTypeThingRuleWidget)}.{nameof(AvailableItemsLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for available items.
            /// </summary>
            public static readonly string AvailableItemsTooltip =
                $"{CommonMod.ModId}.{nameof(WorkTypeThingRuleWidget)}.{nameof(AvailableItemsTooltip)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized string indicating that no rule is selected.
            /// </summary>
            public static readonly string NoRuleSelected =
                $"{CommonMod.ModId}.{nameof(WorkTypeThingRuleWidget)}.{nameof(NoRuleSelected)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized label for stat weights.
            /// </summary>
            public static readonly string StatWeightsLabel =
                $"{CommonMod.ModId}.{nameof(WorkTypeThingRuleWidget)}.{nameof(StatWeightsLabel)}"
                    .Translate();

            /// <summary>
            ///     Gets the localized tooltip for stat weights.
            /// </summary>
            public static readonly string StatWeightsTooltip =
                $"{CommonMod.ModId}.{nameof(WorkTypeThingRuleWidget)}.{nameof(StatWeightsTooltip)}"
                    .Translate();
        }
    }

    /// <summary>
    ///     Provides access to commonly used textures for the mod.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Textures
    {
        /// <summary>
        ///     Gets a reference to a texture used to indicate a bad or missing resource.
        /// </summary>
        [UsedImplicitly]
        public static readonly Texture2D BadTexture =
            ContentFinder<Texture2D>.Get("UI/Misc/BadTexture");

        /// <summary>
        ///     Gets a reference to the info icon texture.
        /// </summary>
        internal static readonly Texture2D InfoIcon = TexButton.Info;

        /// <summary>
        ///     Provides access to textures related to common actions.
        /// </summary>
        [StaticConstructorOnStartup]
        internal static class Actions
        {
            /// <summary>
            ///     Gets a reference to the texture used for the delete action.
            /// </summary>
            internal static readonly Texture2D Delete = TexUI.DismissTex;
        }

        /// <summary>
        ///     Provides access to textures representing passion states.
        /// </summary>
        [StaticConstructorOnStartup]
        internal static class Passions
        {
            /// <summary>
            ///     Gets a reference to the major passion icon texture.
            /// </summary>
            internal static readonly Texture2D PassionIconMajor =
                ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");

            /// <summary>
            ///     Gets a reference to the minor passion icon texture.
            /// </summary>
            internal static readonly Texture2D PassionIconMinor =
                ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");

            /// <summary>
            ///     Gets a reference to the texture used for no passion (transparent).
            /// </summary>
            internal static readonly Texture2D PassionIconNone =
                ContentFinder<Texture2D>.Get("Things/Mote/Transparent");
        }
    }
}