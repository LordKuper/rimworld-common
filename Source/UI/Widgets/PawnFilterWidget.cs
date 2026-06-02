using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common.Filters;
using LordKuper.Common.Filters.Limits;
using LordKuper.Common.Helpers;
using RimWorld;
using UnityEngine;
using Verse;
using PawnHealthState = LordKuper.Common.Filters.PawnHealthState;
using Strings = LordKuper.Common.Resources.Strings.PawnFilter;

namespace LordKuper.Common.UI.Widgets;

/// <summary>
///     Provides UI widgets for configuring and displaying pawn filter options.
/// </summary>
[PublicAPI]
public static class PawnFilterWidget
{
    /// <summary>
    ///     Minimum width for the default selector.
    /// </summary>
    private const float DefaultSelectorWidthMin = Layout.GridSize * 40;

    /// <summary>
    ///     List of all possible pawn health states.
    /// </summary>
    private static readonly List<PawnHealthState> AllPawnHealthStates =
    [
        .. Enum.GetValues(typeof(PawnHealthState)).Cast<PawnHealthState>().Where(s => s != PawnHealthState.None)
    ];

    /// <summary>
    ///     List of all possible pawn primary weapon types.
    /// </summary>
    private static readonly List<PawnPrimaryWeaponType> AllPawnPrimaryWeaponTypes =
        [.. Enum.GetValues(typeof(PawnPrimaryWeaponType)).Cast<PawnPrimaryWeaponType>()];

    /// <summary>
    ///     List of all possible pawn types.
    /// </summary>
    private static readonly List<PawnType> AllPawnTypes = [.. Enum.GetValues(typeof(PawnType)).Cast<PawnType>()];

    /// <summary>
    ///     List of all possible work capacities, excluding None and AllWork.
    /// </summary>
    private static readonly List<WorkTags> AllWorkCapacities =
    [
        .. Enum.GetValues(typeof(WorkTags)).Cast<WorkTags>().Where(t => t != WorkTags.None && t != WorkTags.AllWork)
    ];

    /// <summary>
    ///     Renders the Pawn Capacities section within the specified rectangular area and updates the associated filter
    ///     state.
    /// </summary>
    /// <remarks>
    ///     This method handles the rendering and interaction logic for the Pawn Capacities section,
    ///     including toggling the filter state, adjusting capacity limits, and adding or removing capacity definitions. It
    ///     updates the provided filter object and tracks whether any changes were made to the filter state.
    /// </remarks>
    /// <param name="rect">The rectangular area where the section will be drawn.</param>
    /// <param name="pawnFilter">The filter object that determines which pawn capacities are displayed and modified.</param>
    /// <param name="inputId">An identifier used for input handling within the section.</param>
    /// <param name="filterChanged">
    ///     A reference to a boolean value that will be set to <see langword="true" /> if the filter state changes as a
    ///     result of user interaction.
    /// </param>
    /// <param name="remRect">
    ///     When the method returns, contains the remaining portion of the rectangle that was not used by the
    ///     section.
    /// </param>
    /// <returns>The vertical space consumed by the section, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoPawnCapacitiesSection(Rect rect, [NotNull] PawnFilter pawnFilter, int inputId,
        ref bool filterChanged, out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterPawnCapacities;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterPawnCapacities,
                Strings.GetFilterPawnCapacitiesTooltip(true), Strings.PawnCapacityLimitsLabel,
                Strings.PawnCapacityLimitsTooltip, out remRect);
            if (oldValue != pawnFilter.FilterPawnCapacities) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterPawnCapacities;
            var enabled = pawnFilter.FilterPawnCapacities == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled, Strings.GetFilterPawnCapacitiesTooltip(false),
                Strings.PawnCapacityLimitsLabel, Strings.PawnCapacityLimitsTooltip, out remRect);
            pawnFilter.FilterPawnCapacities = enabled;
            if (oldValue != pawnFilter.FilterPawnCapacities) filterChanged = true;
        }
        if (pawnFilter.FilterPawnCapacities != true) return y;
        var limits = pawnFilter.PawnCapacityLimits;
        for (var i = 0; i < limits.Count;)
        {
            var limit = limits[i];
            if (limit.Def == null)
            {
                i++;
                continue;
            }
            var index = i;
            var deleted = false;
            var oldValue = limit.Limit;
            y += Fields.DoLabeledPercentRangeSlider(remRect, inputId + i, 0, [
                    new IconButton(Resources.Textures.Actions.Delete, () =>
                    {
                        limits.RemoveAt(index);
                        deleted = true;
                    }, Resources.Strings.Actions.Delete)
                ], limit.Label ?? string.Empty, limit.Def?.description, ref limit.Limit, PawnCapacityLimit.LimitMinCap,
                PawnCapacityLimit.LimitMaxCap, limit.ValueStep, PawnCapacityLimit.ValueStyle, null, out remRect);
            if (oldValue != limit.Limit || deleted) filterChanged = true;
            if (i < limits.Count && limits[i] == limit)
                i++;
        }
        var existingDefNames = new HashSet<string>(limits.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var l in limits)
        {
            existingDefNames.Add(l.DefName);
        }
        var canAdd = false;
        foreach (var def in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
        {
            if (existingDefNames.Contains(def.defName)) continue;
            canAdd = true;
            break;
        }
        if (!canAdd) return y;
        var buttonRect = Layout.GetTopRowRect(remRect, Buttons.ActionButtonHeight, out remRect);
        y += buttonRect.height;
        var added = false;
        Buttons.DoActionButton(buttonRect, Resources.Strings.Actions.Add, () =>
        {
            var options = new List<FloatMenuOption>();
            foreach (var def in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
            {
                if (pawnFilter.PawnCapacityLimits.Any(l =>
                        def.defName.Equals(l.DefName, StringComparison.OrdinalIgnoreCase)))
                    continue;
                var label = def.GetLabel();
                options.Add(new FloatMenuOption(label, () =>
                {
                    pawnFilter.PawnCapacityLimits.Add(new PawnCapacityLimit(def));
                    added = true;
                }));
            }
            options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.CurrentCulture));
            Find.WindowStack.Add(new FloatMenu(options));
        });
        if (added) filterChanged = true;
        return y;
    }

    /// <summary>
    ///     Renders and processes the UI for filtering pawns based on specified sections and criteria.
    /// </summary>
    /// <remarks>
    ///     This method dynamically renders sections of the filter UI based on the specified
    ///     <paramref
    ///         name="sections" />
    ///     flags. Each section corresponds to a specific filtering criterion, such as pawn types, health
    ///     states, skills, or traits. If a section is enabled but its corresponding input ID (e.g.,
    ///     <paramref
    ///         name="pawnSkillInputId" />
    ///     ) is not provided, that section will be skipped.  If the filter settings are modified
    ///     during the UI interaction, the optional <paramref name="filterChangedAction" /> will be invoked to notify the
    ///     caller of the change.
    /// </remarks>
    /// <param name="rect">The rectangular area within which the filter UI is drawn.</param>
    /// <param name="pawnFilter">
    ///     The <see cref="PawnFilter" /> object that stores the current filter settings. Cannot be
    ///     <see langword="null" />.
    /// </param>
    /// <param name="sections">
    ///     The sections of the filter to display, as a combination of <see cref="PawnFilterSections" />
    ///     flags.
    /// </param>
    /// <param name="pawnSkillInputId">
    ///     The ID of the skill to filter by, or <see langword="null" /> if skill filtering is not
    ///     enabled.
    /// </param>
    /// <param name="pawnStatInputId">
    ///     The ID of the stat to filter by, or <see langword="null" /> if stat filtering is not
    ///     enabled.
    /// </param>
    /// <param name="pawnCapacityInputId">
    ///     The ID of the capacity to filter by, or <see langword="null" /> if capacity filtering
    ///     is not enabled.
    /// </param>
    /// <param name="filterChangedAction">An optional callback action to invoke if the filter settings are changed by the user.</param>
    /// <param name="remRect">The remaining rectangular area after the filter UI is drawn.</param>
    /// <returns>The total vertical space used by the filter UI, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    public static float DoPawnFilter(Rect rect, [NotNull] PawnFilter pawnFilter, PawnFilterSections sections,
        int? pawnSkillInputId, int? pawnStatInputId, int? pawnCapacityInputId, [CanBeNull] Action filterChangedAction,
        out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        remRect = rect;
        var y = 0f;
        var filterChanged = false;
        var doGapLine = false;
        var doTypes = sections.HasFlag(PawnFilterSections.PawnTypes);
        var doHealthStates = sections.HasFlag(PawnFilterSections.PawnHealthStates);
        var doWeaponTypes = sections.HasFlag(PawnFilterSections.PawnPrimaryWeaponTypes);
        var doSkills = sections.HasFlag(PawnFilterSections.PawnSkills) && pawnSkillInputId.HasValue;
        var doWorkPassions = sections.HasFlag(PawnFilterSections.WorkPassions);
        var doWorkCapacities = sections.HasFlag(PawnFilterSections.WorkCapacities);
        var doTraits = sections.HasFlag(PawnFilterSections.PawnTraits);
        var doStats = sections.HasFlag(PawnFilterSections.PawnStats) && pawnStatInputId.HasValue;
        var doCapacities = sections.HasFlag(PawnFilterSections.PawnCapacities) && pawnCapacityInputId.HasValue;
        if (doTypes)
        {
            y += DoPawnTypesSection(rect, pawnFilter, ref filterChanged, out remRect);
            doGapLine = true;
        }
        if (doHealthStates)
        {
            if (doGapLine)
                y += Layout.DoGapLineHorizontal(remRect, out remRect);
            y += DoPawnHealthStatesSection(remRect, pawnFilter, ref filterChanged, out remRect);
            doGapLine = true;
        }
        if (doWeaponTypes)
        {
            if (doGapLine)
                y += Layout.DoGapLineHorizontal(remRect, out remRect);
            y += DoPawnPrimaryWeaponTypesSection(remRect, pawnFilter, ref filterChanged, out remRect);
        }
        if (doSkills)
        {
            if (doGapLine)
                y += Layout.DoGapLineHorizontal(remRect, out remRect);
            y += DoPawnSkillsSection(remRect, pawnFilter, pawnSkillInputId.Value, ref filterChanged, out remRect);
            doGapLine = true;
        }
        if (doWorkPassions)
        {
            if (doGapLine)
                y += Layout.DoGapLineHorizontal(remRect, out remRect);
            y += DoWorkPassionsSection(remRect, pawnFilter, ref filterChanged, out remRect);
            doGapLine = true;
        }
        if (doWorkCapacities)
        {
            if (doGapLine)
                y += Layout.DoGapLineHorizontal(remRect, out remRect);
            y += DoWorkCapacitiesSection(remRect, pawnFilter, ref filterChanged, out remRect);
            doGapLine = true;
        }
        if (doTraits)
        {
            if (doGapLine)
                y += Layout.DoGapLineHorizontal(remRect, out remRect);
            y += DoPawnTraitsSection(remRect, pawnFilter, ref filterChanged, out remRect);
            doGapLine = true;
        }
        if (doStats)
        {
            if (doGapLine)
                y += Layout.DoGapLineHorizontal(remRect, out remRect);
            y += DoPawnStatsSection(remRect, pawnFilter, pawnStatInputId.Value, ref filterChanged, out remRect);
            doGapLine = true;
        }
        if (doCapacities)
        {
            if (doGapLine)
                y += Layout.DoGapLineHorizontal(remRect, out remRect);
            y += DoPawnCapacitiesSection(remRect, pawnFilter, pawnCapacityInputId.Value, ref filterChanged,
                out remRect);
        }
        if (filterChanged && filterChangedAction != null) filterChangedAction.Invoke();
        return y;
    }

    /// <summary>
    ///     Renders the health states filter section for a pawn and updates the filter state based on user interaction.
    /// </summary>
    /// <remarks>
    ///     This method dynamically generates UI elements for filtering pawn health states based on the
    ///     provided <paramref name="pawnFilter" />. It supports both tri-state and binary filtering modes, and updates the
    ///     filter's state based on user input. The method also ensures that only relevant health states are displayed and
    ///     interactable.
    /// </remarks>
    /// <param name="rect">The rectangular area within which the section is drawn.</param>
    /// <param name="pawnFilter">The filter object that determines which health states are allowed or forbidden.</param>
    /// <param name="filterChanged">
    ///     A reference to a boolean value that will be set to <see langword="true" /> if the filter state changes as a
    ///     result of user interaction; otherwise, it remains <see langword="false" />.
    /// </param>
    /// <param name="remRect">
    ///     When the method returns, contains the remaining portion of the <paramref name="rect" /> that was not used by the
    ///     section.
    /// </param>
    /// <returns>The vertical space (in pixels) consumed by the section within the provided <paramref name="rect" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoPawnHealthStatesSection(Rect rect, [NotNull] PawnFilter pawnFilter, ref bool filterChanged,
        out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        bool enabled;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterPawnHealthStates;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterPawnHealthStates,
                Strings.GetFilterPawnHealthStatesTooltip(true), Strings.AllowedPawnHealthStatesLabel,
                Strings.AllowedPawnHealthStatesTooltip, out remRect);
            enabled = pawnFilter.FilterPawnHealthStates == true;
            if (oldValue != pawnFilter.FilterPawnHealthStates) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterPawnHealthStates;
            enabled = pawnFilter.FilterPawnHealthStates == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled, Strings.GetFilterPawnHealthStatesTooltip(false),
                Strings.AllowedPawnHealthStatesLabel, Strings.AllowedPawnHealthStatesTooltip, out remRect);
            pawnFilter.FilterPawnHealthStates = enabled;
            if (oldValue != pawnFilter.FilterPawnHealthStates) filterChanged = true;
        }
        if (!enabled) return y;
        var count = 0;
        var relevantHealthStates = new HashSet<PawnHealthState>();
        foreach (var hs in AllPawnHealthStates)
        {
            if (pawnFilter.ForbiddenPawnHealthStates.HasFlag(hs)) continue;
            relevantHealthStates.Add(hs);
            count++;
        }
        if (count != 0)
        {
            var rects = Layout.GetGridRects(remRect, DefaultSelectorWidthMin, Layout.ElementGapSmall, Layout.RowHeight,
                Layout.ElementGapSmall, count, out var gridHeight, out remRect);
            y += gridHeight;
            var i = 0;
            foreach (var healthState in relevantHealthStates)
            {
                var selectorRect = rects[i++];
                var label = Resources.Strings.PawnHealthState.GetLabel(healthState);
                var tooltip = Resources.Strings.PawnHealthState.GetTooltip(healthState);
                var value = pawnFilter.AllowedPawnHealthStates.HasFlag(healthState);
                var oldValue = value;
                Fields.DoLabeledCheckbox(selectorRect, 0, null, ref value, label, tooltip, null, out _);
                if (oldValue == value) continue;
                filterChanged = true;
                if (value)
                    pawnFilter.AllowedPawnHealthStates |= healthState;
                else
                    pawnFilter.AllowedPawnHealthStates &= ~healthState;
            }
        }
        return y;
    }

    /// <summary>
    ///     Renders and processes the section for filtering pawns by their primary weapon types.
    /// </summary>
    /// <remarks>
    ///     This method handles both the header toggle for enabling or disabling the filter and the
    ///     individual checkboxes for selecting specific weapon types. If the filter is disabled, the section will not
    ///     render the individual weapon type options.
    /// </remarks>
    /// <param name="rect">The rectangular area within which the section is drawn.</param>
    /// <param name="pawnFilter">
    ///     The filter object that determines the allowed primary weapon types for pawns. Cannot be
    ///     <see langword="null" />.
    /// </param>
    /// <param name="filterChanged">
    ///     A reference to a boolean value that will be set to <see langword="true" /> if the filter state changes as a
    ///     result of user interaction.
    /// </param>
    /// <param name="remRect">
    ///     When the method returns, contains the remaining portion of the <paramref name="rect" /> that was not used by the
    ///     section.
    /// </param>
    /// <returns>The vertical space consumed by the section, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoPawnPrimaryWeaponTypesSection(Rect rect, [NotNull] PawnFilter pawnFilter,
        ref bool filterChanged, out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        bool enabled;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterPawnPrimaryWeaponTypes;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterPawnPrimaryWeaponTypes,
                Strings.GetFilterPawnPrimaryWeaponTypesTooltip(true), Strings.AllowedPawnPrimaryWeaponTypesLabel,
                Strings.AllowedPawnPrimaryWeaponTypesTooltip, out remRect);
            enabled = pawnFilter.FilterPawnPrimaryWeaponTypes == true;
            if (oldValue != pawnFilter.FilterPawnPrimaryWeaponTypes) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterPawnPrimaryWeaponTypes;
            enabled = pawnFilter.FilterPawnPrimaryWeaponTypes == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled,
                Strings.GetFilterPawnPrimaryWeaponTypesTooltip(false), Strings.AllowedPawnPrimaryWeaponTypesLabel,
                Strings.AllowedPawnPrimaryWeaponTypesTooltip, out remRect);
            pawnFilter.FilterPawnPrimaryWeaponTypes = enabled;
            if (oldValue != pawnFilter.FilterPawnPrimaryWeaponTypes) filterChanged = true;
        }
        if (!enabled) return y;
        var count = AllPawnPrimaryWeaponTypes.Count;
        if (count != 0)
        {
            var rects = Layout.GetGridRects(remRect, DefaultSelectorWidthMin, Layout.ElementGapSmall, Layout.RowHeight,
                Layout.ElementGapSmall, count, out var gridHeight, out remRect);
            y += gridHeight;
            var i = 0;
            foreach (var weaponType in AllPawnPrimaryWeaponTypes)
            {
                var selectorRect = rects[i++];
                var label = Resources.Strings.PawnPrimaryWeaponType.GetLabel(weaponType);
                var tooltip = Resources.Strings.PawnPrimaryWeaponType.GetTooltip(weaponType);
                var value = pawnFilter.AllowedPawnPrimaryWeaponTypes.Contains(weaponType);
                var oldValue = value;
                Fields.DoLabeledCheckbox(selectorRect, 0, null, ref value, label, tooltip, null, out _);
                if (value)
                    pawnFilter.AllowedPawnPrimaryWeaponTypes.Add(weaponType);
                else
                    pawnFilter.AllowedPawnPrimaryWeaponTypes.Remove(weaponType);
                if (oldValue != value) filterChanged = true;
            }
        }
        return y;
    }

    /// <summary>
    ///     Renders the skills filtering section for a pawn and updates the filter state based on user input.
    /// </summary>
    /// <remarks>
    ///     This method handles the rendering and interaction logic for the pawn skills filter section.
    ///     It includes toggleable headers, sliders for skill limits, and buttons for adding or removing skill filters. The
    ///     method updates the provided <paramref name="pawnFilter" /> object based on user input and indicates whether the
    ///     filter state has changed via the <paramref name="filterChanged" /> parameter.
    /// </remarks>
    /// <param name="rect">The rectangular area within which the section is drawn.</param>
    /// <param name="pawnFilter">
    ///     The <see cref="PawnFilter" /> object that holds the current filter settings. Cannot be
    ///     <see langword="null" />.
    /// </param>
    /// <param name="inputId">A unique identifier for input elements within this section, used to manage UI state.</param>
    /// <param name="filterChanged">
    ///     A reference to a boolean that will be set to <see langword="true" /> if the filter state changes as a result of
    ///     user interaction.
    /// </param>
    /// <param name="remRect">The remaining portion of the <paramref name="rect" /> after the section is rendered.</param>
    /// <returns>The vertical space consumed by the section, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoPawnSkillsSection(Rect rect, [NotNull] PawnFilter pawnFilter, int inputId,
        ref bool filterChanged, out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterPawnSkills;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterPawnSkills,
                Strings.GetFilterPawnSkillsTooltip(true), Strings.PawnSkillLimitsLabel, Strings.PawnSkillLimitsTooltip,
                out remRect);
            if (oldValue != pawnFilter.FilterPawnSkills) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterPawnSkills;
            var enabled = pawnFilter.FilterPawnSkills == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled, Strings.GetFilterPawnSkillsTooltip(false),
                Strings.PawnSkillLimitsLabel, Strings.PawnSkillLimitsTooltip, out remRect);
            pawnFilter.FilterPawnSkills = enabled;
            if (oldValue != pawnFilter.FilterPawnSkills) filterChanged = true;
        }
        if (pawnFilter.FilterPawnSkills == true)
        {
            var limits = pawnFilter.PawnSkillLimits;
            for (var i = 0; i < limits.Count;)
            {
                var limit = limits[i];
                if (limit.Def == null)
                {
                    i++;
                    continue;
                }
                var index = i;
                var oldValue = limit.Limit;
                var deleted = false;
                y += Fields.DoLabeledIntegerRangeSlider(remRect, inputId + i, 0, [
                        new IconButton(Resources.Textures.Actions.Delete, () =>
                        {
                            limits.RemoveAt(index);
                            deleted = true;
                        }, Resources.Strings.Actions.Delete)
                    ], limit.Label ?? string.Empty, limit.Def?.description, ref limit.Limit, PawnSkillLimit.LimitMinCap,
                    PawnSkillLimit.LimitMaxCap, PawnSkillLimit.ValueStep, null, out remRect);
                if (oldValue != limit.Limit || deleted)
                    filterChanged = true;
                if (i < limits.Count && ReferenceEquals(limits[i], limit))
                    i++;
            }
            var existingDefNames = new HashSet<string>(limits.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var limit in limits)
            {
                existingDefNames.Add(limit.DefName);
            }
            var canAdd = false;
            foreach (var def in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                if (existingDefNames.Contains(def.defName)) continue;
                canAdd = true;
                break;
            }
            if (!canAdd) return y;
            var buttonRect = Layout.GetTopRowRect(remRect, Buttons.ActionButtonHeight, out remRect);
            y += buttonRect.height;
            var added = false;
            Buttons.DoActionButton(buttonRect, Resources.Strings.Actions.Add, () =>
            {
                var options = new List<FloatMenuOption>();
                foreach (var def in DefDatabase<SkillDef>.AllDefsListForReading)
                {
                    if (existingDefNames.Contains(def.defName)) continue;
                    var label = def.GetLabel();
                    options.Add(new FloatMenuOption(label, () =>
                    {
                        pawnFilter.PawnSkillLimits.Add(new PawnSkillLimit(def));
                        added = true;
                    }));
                }
                options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.CurrentCulture));
                Find.WindowStack.Add(new FloatMenu(options));
            });
            if (added) filterChanged = true;
        }
        return y;
    }

    /// <summary>
    ///     Renders and manages the pawn stats filter section within the specified UI rectangle.
    /// </summary>
    /// <remarks>
    ///     This method handles the rendering of the pawn stats filter section, including toggleable
    ///     headers, range sliders, and input fields for stat limits. It also provides functionality to add or remove stat
    ///     limits dynamically. The method updates the filter settings in the provided <paramref name="pawnFilter" />
    ///     instance and tracks whether any changes were made via the <paramref name="filterChanged" /> parameter.
    /// </remarks>
    /// <param name="rect">The rectangle in which the section is drawn.</param>
    /// <param name="pawnFilter">
    ///     The <see cref="PawnFilter" /> instance containing the filter settings to be displayed and
    ///     modified.
    /// </param>
    /// <param name="inputId">A unique identifier for input elements within this section, used to differentiate UI controls.</param>
    /// <param name="filterChanged">
    ///     A reference to a boolean value that will be set to <see langword="true" /> if any changes are made to the filter
    ///     settings.
    /// </param>
    /// <param name="remRect">An output parameter that returns the remaining rectangle space after the section is rendered.</param>
    /// <returns>The vertical space consumed by the section, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoPawnStatsSection(Rect rect, [NotNull] PawnFilter pawnFilter, int inputId,
        ref bool filterChanged, out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterPawnStats;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterPawnStats,
                Strings.GetFilterPawnStatsTooltip(true), Strings.PawnStatLimitsLabel, Strings.PawnStatLimitsTooltip,
                out remRect);
            if (oldValue != pawnFilter.FilterPawnStats) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterPawnStats;
            var enabled = pawnFilter.FilterPawnStats == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled, Strings.GetFilterPawnStatsTooltip(false),
                Strings.PawnStatLimitsLabel, Strings.PawnStatLimitsTooltip, out remRect);
            pawnFilter.FilterPawnStats = enabled;
            if (oldValue != pawnFilter.FilterPawnStats) filterChanged = true;
        }
        if (pawnFilter.FilterPawnStats != true) return y;
        var statLimits = pawnFilter.PawnStatLimits;
        for (var i = 0; i < statLimits.Count;)
        {
            var limit = statLimits[i];
            if (limit.Def == null)
            {
                i++;
                continue;
            }
            var label = limit.Label ?? string.Empty;
            var description = limit.Def?.description;
            if (limit.ValueStyle is ToStringStyle.PercentZero or ToStringStyle.PercentOne or ToStringStyle.PercentTwo)
            {
                var index = i;
                var deleted = false;
                var oldValue = limit.Limit;
                y += Fields.DoLabeledPercentRangeSlider(remRect, inputId + i, 0, [
                        new IconButton(Resources.Textures.Actions.Delete, () =>
                        {
                            statLimits.RemoveAt(index);
                            deleted = true;
                        }, Resources.Strings.Actions.Delete)
                    ], label, description, ref limit.Limit, limit.LimitMinCap, limit.LimitMaxCap, limit.ValueStep,
                    limit.ValueStyle, null, out remRect);
                if (oldValue != limit.Limit || deleted)
                    filterChanged = true;
            }
            else
            {
                var index = i;
                var minBuffer = limit.MinValueBuffer;
                var maxBuffer = limit.MaxValueBuffer;
                var deleted = false;
                var oldValue = limit.Limit;
                y += Fields.DoLabeledFloatRangeInputs(remRect, 0, [
                        new IconButton(Resources.Textures.Actions.Delete, () =>
                        {
                            statLimits.RemoveAt(index);
                            deleted = true;
                        }, Resources.Strings.Actions.Delete)
                    ], label, description, ref minBuffer, ref maxBuffer, limit.LimitMinCap, limit.LimitMaxCap,
                    limit.ValueStyle, null, out remRect);
                limit.MinValueBuffer = minBuffer;
                limit.MaxValueBuffer = maxBuffer;
                if (oldValue != limit.Limit || deleted) filterChanged = true;
            }
            if (i < statLimits.Count && statLimits[i] == limit)
                i++;
        }
        var statDefNamesSet = new HashSet<string>(statLimits.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var l in statLimits)
        {
            statDefNamesSet.Add(l.DefName);
        }
        var canAdd = false;
        var statsByCategory = StatHelper.GetStatsByCategory(StatCategory.Pawn);
        foreach (var def in statsByCategory)
        {
            if (statDefNamesSet.Contains(def.defName)) continue;
            canAdd = true;
            break;
        }
        if (!canAdd) return y;
        var buttonRect = Layout.GetTopRowRect(remRect, Buttons.ActionButtonHeight, out remRect);
        y += buttonRect.height;
        var added = false;
        Buttons.DoActionButton(buttonRect, Resources.Strings.Actions.Add, () =>
        {
            var options = new List<FloatMenuOption>();
            foreach (var def in statsByCategory)
            {
                if (statDefNamesSet.Contains(def.defName)) continue;
                var label = def.GetLabel();
                options.Add(new FloatMenuOption(label, () =>
                {
                    statLimits.Add(new StatLimit(def));
                    added = true;
                }));
            }
            options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.CurrentCulture));
            Find.WindowStack.Add(new FloatMenu(options));
        });
        if (added) filterChanged = true;
        return y;
    }

    /// <summary>
    ///     Renders the UI section for filtering pawn traits and updates the filter state based on user input.
    /// </summary>
    /// <remarks>
    ///     This method handles the display and interaction logic for the pawn traits filter section. It
    ///     includes a toggleable header to enable or disable trait filtering, and dynamically generates UI elements for
    ///     managing individual trait limits. If the filter is enabled, users can add, remove, or modify trait limits. The
    ///     method ensures that any changes to the filter state are reflected in the <paramref name="filterChanged" />
    ///     parameter.
    /// </remarks>
    /// <param name="rect">The rectangular area within which the section is drawn.</param>
    /// <param name="pawnFilter">
    ///     The <see cref="PawnFilter" /> object that holds the current filter settings. Cannot be
    ///     <see langword="null" />.
    /// </param>
    /// <param name="filterChanged">
    ///     A reference to a boolean value that will be set to <see langword="true" /> if the filter state changes as a
    ///     result of user interaction.
    /// </param>
    /// <param name="remRect">Outputs the remaining rectangle area after the section is drawn.</param>
    /// <returns>The vertical space consumed by the section, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoPawnTraitsSection(Rect rect, [NotNull] PawnFilter pawnFilter, ref bool filterChanged,
        out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterPawnTraits;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterPawnTraits,
                Strings.GetFilterPawnTraitsTooltip(true), Strings.PawnTraitLimitsLabel, Strings.PawnTraitLimitsTooltip,
                out remRect);
            if (oldValue != pawnFilter.FilterPawnTraits) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterPawnTraits;
            var enabled = pawnFilter.FilterPawnTraits == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled, Strings.GetFilterPawnTraitsTooltip(false),
                Strings.PawnTraitLimitsLabel, Strings.PawnTraitLimitsTooltip, out remRect);
            pawnFilter.FilterPawnTraits = enabled;
            if (oldValue != pawnFilter.FilterPawnTraits) filterChanged = true;
        }
        if (pawnFilter.FilterPawnTraits == true)
        {
            var traitLimits = pawnFilter.PawnTraitLimits;
            var traitCount = traitLimits.Count;
            var rects = Layout.GetGridRects(remRect, DefaultSelectorWidthMin, Layout.ElementGapSmall, Layout.RowHeight,
                Layout.ElementGapSmall, traitCount + 1, out var gridHeight, out remRect);
            y += gridHeight;
            for (var i = 0; i < traitLimits.Count;)
            {
                var limit = traitLimits[i];
                var def = limit.Def;
                if (def == null)
                {
                    i++;
                    continue;
                }
                var description = def.description;
                var index = i;
                var oldValue = limit.Limit;
                var deleted = false;
                Fields.DoLabeledCheckbox(rects[i], 0, [
                    new IconButton(Resources.Textures.Actions.Delete, () =>
                    {
                        traitLimits.RemoveAt(index);
                        deleted = true;
                    }, Resources.Strings.Actions.Delete)
                ], ref limit.Limit, limit.Label, description, null, out _);
                if (oldValue != limit.Limit || deleted)
                    filterChanged = true;
                if (i < traitLimits.Count && traitLimits[i] == limit) i++;
            }
            var traitDefNames = new HashSet<string>(traitLimits.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var limit in traitLimits)
            {
                traitDefNames.Add(limit.DefName);
            }
            var canAdd = false;
            foreach (var def in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                if (traitDefNames.Contains(def.defName)) continue;
                canAdd = true;
                break;
            }
            var added = false;
            Buttons.DoActionButton(rects[rects.Length - 1], Resources.Strings.Actions.Add, () =>
            {
                var options = new List<FloatMenuOption>();
                foreach (var def in DefDatabase<TraitDef>.AllDefsListForReading)
                {
                    if (traitDefNames.Contains(def.defName)) continue;
                    var label = def.GetLabel();
                    options.Add(new FloatMenuOption(label, () =>
                    {
                        traitLimits.Add(new PawnTraitLimit(def));
                        added = true;
                    }));
                }
                options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.CurrentCulture));
                Find.WindowStack.Add(new FloatMenu(options));
            }, null, canAdd);
            if (added) filterChanged = true;
        }
        return y;
    }

    /// <summary>
    ///     Renders the pawn types section of a filter UI and updates the filter state based on user interaction.
    /// </summary>
    /// <remarks>
    ///     This method handles the rendering and interaction logic for a section of the UI that allows
    ///     users to configure which pawn types are included or excluded in a filter. It supports both tri-state and binary
    ///     toggle modes, as determined by the <see cref="PawnFilter.TriStateMode" /> property. The method also updates the
    ///     filter's allowed and forbidden pawn types based on user input.
    /// </remarks>
    /// <param name="rect">The rectangular area within which the section is drawn.</param>
    /// <param name="pawnFilter">
    ///     The <see cref="PawnFilter" /> object representing the current filter state. Cannot be
    ///     <see langword="null" />.
    /// </param>
    /// <param name="filterChanged">
    ///     A reference to a boolean value that will be set to <see langword="true" /> if the filter state changes as a
    ///     result of user interaction.
    /// </param>
    /// <param name="remRect">Outputs the remaining portion of the <paramref name="rect" /> that was not used by the section.</param>
    /// <returns>The vertical space consumed by the section, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoPawnTypesSection(Rect rect, [NotNull] PawnFilter pawnFilter, ref bool filterChanged,
        out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        bool enabled;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterPawnTypes;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterPawnTypes,
                Strings.GetFilterPawnTypesTooltip(true), Strings.AllowedPawnTypesLabel, Strings.AllowedPawnTypesTooltip,
                out remRect);
            enabled = pawnFilter.FilterPawnTypes == true;
            if (pawnFilter.FilterPawnTypes != oldValue) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterPawnTypes;
            enabled = pawnFilter.FilterPawnTypes == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled, Strings.GetFilterPawnTypesTooltip(false),
                Strings.AllowedPawnTypesLabel, Strings.AllowedPawnTypesTooltip, out remRect);
            pawnFilter.FilterPawnTypes = enabled;
            if (pawnFilter.FilterPawnTypes != oldValue) filterChanged = true;
        }
        if (!enabled) return y;
        {
            var count = 0;
            foreach (var pt in AllPawnTypes)
            {
                if (!pawnFilter.ForbiddenPawnTypes.Contains(pt)) count++;
            }
            if (count != 0)
            {
                var rects = Layout.GetGridRects(remRect, DefaultSelectorWidthMin, Layout.ElementGapSmall,
                    Layout.RowHeight, Layout.ElementGapSmall, count, out var gridHeight, out remRect);
                y += gridHeight;
                var allowed = pawnFilter.AllowedPawnTypes;
                var i = 0;
                foreach (var pawnType in AllPawnTypes)
                {
                    if (pawnFilter.ForbiddenPawnTypes.Contains(pawnType)) continue;
                    var selectorRect = rects[i++];
                    var label = Resources.Strings.PawnType.GetLabel(pawnType);
                    var value = allowed.Contains(pawnType);
                    var oldValue = value;
                    Fields.DoLabeledCheckbox(selectorRect, 0, null, ref value, label, null, null, out _);
                    if (value)
                        allowed.Add(pawnType);
                    else
                        allowed.Remove(pawnType);
                    if (value != oldValue) filterChanged = true;
                }
            }
            return y;
        }
    }

    /// <summary>
    ///     Renders and manages the UI section for configuring work capacity filters for a given pawn filter.
    /// </summary>
    /// <param name="rect">The rectangular area within which the section is rendered.</param>
    /// <param name="pawnFilter">
    ///     The <see cref="PawnFilter" /> object that holds the work capacity filter settings. Cannot be
    ///     <see
    ///         langword="null" />
    ///     .
    /// </param>
    /// <param name="filterChanged">
    ///     A reference to a boolean value that will be set to <see langword="true" /> if any filter settings are modified
    ///     during the operation.
    /// </param>
    /// <param name="remRect">Outputs the remaining rectangular area after rendering the section.</param>
    /// <returns>The total height of the rendered section, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoWorkCapacitiesSection(Rect rect, [NotNull] PawnFilter pawnFilter, ref bool filterChanged,
        out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterWorkCapacities;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterWorkCapacities,
                Strings.GetFilterWorkCapacitiesTooltip(true), Strings.WorkCapacityLimitsLabel,
                Strings.WorkCapacityLimitsTooltip, out remRect);
            if (oldValue != pawnFilter.FilterWorkCapacities) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterWorkCapacities;
            var enabled = pawnFilter.FilterWorkCapacities == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled, Strings.GetFilterWorkCapacitiesTooltip(false),
                Strings.WorkCapacityLimitsLabel, Strings.WorkCapacityLimitsTooltip, out remRect);
            pawnFilter.FilterWorkCapacities = enabled;
            if (oldValue != pawnFilter.FilterWorkCapacities) filterChanged = true;
        }
        if (pawnFilter.FilterWorkCapacities == true)
        {
            var count = pawnFilter.WorkCapacityLimits.Count;
            var canAdd = false;
            foreach (var wt in AllWorkCapacities)
            {
                if (!pawnFilter.WorkCapacityLimits.ContainsKey(wt))
                {
                    canAdd = true;
                    break;
                }
            }
            var rects = Layout.GetGridRects(remRect, DefaultSelectorWidthMin, Layout.ElementGapSmall, Layout.RowHeight,
                Layout.ElementGapSmall, canAdd ? count + 1 : count, out var gridHeight, out remRect);
            y += gridHeight;
            var i = 0;
            var keys = pawnFilter.WorkCapacityLimits.Keys.ToArray();
            foreach (var key in keys)
            {
                var value = pawnFilter.WorkCapacityLimits[key];
                var oldValue = value;
                var deleted = false;
                Fields.DoLabeledCheckbox(rects[i++], 0, [
                    new IconButton(Resources.Textures.Actions.Delete, () =>
                    {
                        pawnFilter.WorkCapacityLimits.Remove(key);
                        deleted = true;
                    }, Resources.Strings.Actions.Delete)
                ], ref value, key.LabelTranslated(), null, null, out _);
                if (pawnFilter.WorkCapacityLimits.ContainsKey(key))
                {
                    if (value != oldValue)
                        filterChanged = true;
                    pawnFilter.WorkCapacityLimits[key] = value;
                }
                if (deleted)
                    filterChanged = true;
            }
            if (canAdd)
            {
                var added = false;
                Buttons.DoActionButton(rects[i], Resources.Strings.Actions.Add, () =>
                {
                    var options = new List<FloatMenuOption>();
                    foreach (var wt in AllWorkCapacities)
                    {
                        if (!pawnFilter.WorkCapacityLimits.ContainsKey(wt))
                        {
                            var label = wt.LabelTranslated();
                            options.Add(new FloatMenuOption(label, () =>
                            {
                                pawnFilter.WorkCapacityLimits[wt] = true;
                                added = true;
                            }));
                        }
                    }
                    options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.CurrentCulture));
                    Find.WindowStack.Add(new FloatMenu(options));
                });
                if (added) filterChanged = true;
            }
        }
        return y;
    }

    /// <summary>
    ///     Renders and processes the "Work Passions" section of a UI layout, allowing the user to toggle and configure work
    ///     passion filters for a given <see cref="PawnFilter" />.
    /// </summary>
    /// <param name="rect">The rectangular area within which the section is rendered.</param>
    /// <param name="pawnFilter">
    ///     The <see cref="PawnFilter" /> object that holds the current filter settings. Cannot be
    ///     <see langword="null" />.
    /// </param>
    /// <param name="filterChanged">
    ///     A reference to a boolean value that will be set to <see langword="true" /> if any changes are made to the filter
    ///     settings; otherwise, it remains unchanged.
    /// </param>
    /// <param name="remRect">
    ///     Outputs the remaining rectangular area after the section has been rendered. This can be used for rendering
    ///     subsequent UI elements.
    /// </param>
    /// <returns>The total vertical space consumed by the section, in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawnFilter" /> is <see langword="null" />.</exception>
    private static float DoWorkPassionsSection(Rect rect, [NotNull] PawnFilter pawnFilter, ref bool filterChanged,
        out Rect remRect)
    {
        if (pawnFilter == null) throw new ArgumentNullException(nameof(pawnFilter));
        var y = 0f;
        if (pawnFilter.TriStateMode)
        {
            var oldValue = pawnFilter.FilterWorkPassions;
            y += Sections.DoToggleableSectionHeader(rect, ref pawnFilter.FilterWorkPassions,
                Strings.GetFilterWorkPassionsTooltip(true), Strings.AllowedWorkPassionsLabel,
                Strings.AllowedWorkPassionsTooltip, out remRect);
            if (oldValue != pawnFilter.FilterWorkPassions) filterChanged = true;
        }
        else
        {
            var oldValue = pawnFilter.FilterWorkPassions;
            var enabled = pawnFilter.FilterWorkPassions == true;
            y += Sections.DoToggleableSectionHeader(rect, ref enabled, Strings.GetFilterWorkPassionsTooltip(false),
                Strings.AllowedWorkPassionsLabel, Strings.AllowedWorkPassionsTooltip, out remRect);
            pawnFilter.FilterWorkPassions = enabled;
            if (oldValue != pawnFilter.FilterWorkPassions) filterChanged = true;
        }
        if (pawnFilter.FilterWorkPassions == true)
        {
            var passionCaches = PassionHelper.Passions;
            var rects = Layout.GetGridRects(remRect, DefaultSelectorWidthMin, Layout.ElementGapSmall, Layout.RowHeight,
                Layout.ElementGapSmall, passionCaches.Count, out var gridHeight, out remRect);
            y += gridHeight;
            for (var i = 0; i < passionCaches.Count; i++)
            {
                var passionCache = passionCaches[i];
                var selectorRect = rects[i];
                var value = pawnFilter.AllowedWorkPassions.Contains(passionCache.Passion);
                var oldValue = value;
                Fields.DoLabeledCheckbox(selectorRect, 0, null, ref value, passionCache.Label, passionCache.Description,
                    passionCache.Icon, out _);
                if (value)
                    pawnFilter.AllowedWorkPassions.Add(passionCache.Passion);
                else
                    pawnFilter.AllowedWorkPassions.Remove(passionCache.Passion);
                if (oldValue != value) filterChanged = true;
            }
        }
        return y;
    }
}