using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;
using RimWorld;
using UnityEngine;
using Verse;
using static LordKuper.Common.Resources;

namespace LordKuper.Common.UI.Widgets;

/// <summary>
///     Provides UI widgets for displaying and editing <see cref="WorkTypeThingRule" /> objects.
/// </summary>
[PublicAPI]
public static class WorkTypeThingRuleWidget
{
    /// <summary>
    ///     Draws the bottom part of the widget tab, including the available items section and refresh button.
    ///     When <paramref name="mapThings" /> is non-null and non-empty the bottom band's width is split into
    ///     two disjoint halves: List 1 (globally-available <see cref="ThingDef" /> icons) on the left and
    ///     List 2 (on-map <see cref="Thing" /> instances) on the right, sharing the same row count and header row.
    ///     When <paramref name="mapThings" /> is null or empty only List 1 renders at full width (graceful no-op).
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <param name="refreshAction">Action to invoke when the refresh button is clicked.</param>
    /// <param name="thingIconBoxScrollPosition">Reference to the scroll position for the thing icon box section.</param>
    /// <param name="things">The list of <see cref="ThingDef" /> objects to display.</param>
    /// <param name="mapThingIconBoxScrollPosition">Reference to the scroll position for the on-map thing icon box section.</param>
    /// <param name="mapThings">
    ///     On-map <see cref="Thing" /> instances to display in the second list, or <see langword="null" /> to
    ///     render only List 1 at full width. The caller is responsible for supplying instances in the desired
    ///     descending-score order; the widget renders them as given and does not re-sort.
    /// </param>
    /// <param name="selectedRule">The currently selected <see cref="WorkTypeThingRule" />.</param>
    private static void DoBottomPart(Rect rect, Action refreshAction,
        ref Vector2 thingIconBoxScrollPosition, IReadOnlyList<ThingDef> things,
        ref Vector2 mapThingIconBoxScrollPosition, IReadOnlyList<Thing>? mapThings,
        WorkTypeThingRule? selectedRule)
    {
        if (selectedRule == null) return;
        var showMapList = mapThings is { Count: > 0 };
        if (showMapList)
        {
            // Side-by-side layout: split the band into two equal halves separated by ElementGap.
            // Reserving the gap before halving ensures both columns are identical in width and
            // consistent with the spacing used elsewhere in the layout.
            var halfWidth = (rect.width - Layout.ElementGap) / 2f;
            var list1HeaderRect = new Rect(rect.x, rect.y, halfWidth, rect.height);
            var list2HeaderRect = new Rect(rect.x + halfWidth + Layout.ElementGap, rect.y, halfWidth, rect.height);

            // List 1 header + refresh button
            var header1Rect = Sections.GetSectionHeaderRect(list1HeaderRect, out var rem1Rect);
            var button1Rect =
                Layout.GetRightColumnRect(header1Rect, header1Rect.width / 4f, out header1Rect);
            Layout.GetRightColumnRect(header1Rect, Layout.ElementGap, out header1Rect);
            Sections.DoSectionHeaderLabel(header1Rect,
                Strings.WorkTypeThingRuleWidget.AvailableItemsLabel,
                Strings.WorkTypeThingRuleWidget.AvailableItemsTooltip);
            Buttons.DoActionButton(button1Rect, Strings.Actions.Refresh, refreshAction);
            Layout.DoVerticalGap(rem1Rect, out rem1Rect);

            // List 2 header
            var header2Rect = Sections.GetSectionHeaderRect(list2HeaderRect, out var rem2Rect);
            Sections.DoSectionHeaderLabel(header2Rect,
                Strings.WorkTypeThingRuleWidget.AvailableItemsOnMapLabel,
                Strings.WorkTypeThingRuleWidget.AvailableItemsOnMapTooltip);
            Layout.DoVerticalGap(rem2Rect, out rem2Rect);

            // Draw both boxes in their respective halves
            ThingIconBox.DoThingDefBox(rem1Rect, ref thingIconBoxScrollPosition, things, null,
                def => GetWorkTypeDefTooltip(def, selectedRule));
            // mapThings is non-null here (showMapList == true); ! asserts that to the compiler
            ThingIconBox.DoThingBox(rem2Rect, ref mapThingIconBoxScrollPosition, mapThings!, null,
                thing => GetWorkTypeThingTooltip(thing, selectedRule));
        }
        else
        {
            // Single-list layout: List 1 at full width, exactly as before.
            var headerRect = Sections.GetSectionHeaderRect(rect, out var remRect);
            var buttonRect =
                Layout.GetRightColumnRect(headerRect, headerRect.width / 4f, out headerRect);
            Layout.GetRightColumnRect(headerRect, Layout.ElementGap, out headerRect);
            Sections.DoSectionHeaderLabel(headerRect,
                Strings.WorkTypeThingRuleWidget.AvailableItemsLabel,
                Strings.WorkTypeThingRuleWidget.AvailableItemsTooltip);
            Buttons.DoActionButton(buttonRect, Strings.Actions.Refresh, refreshAction);
            Layout.DoVerticalGap(remRect, out remRect);
            ThingIconBox.DoThingDefBox(remRect, ref thingIconBoxScrollPosition, things, null,
                def => GetWorkTypeDefTooltip(def, selectedRule));
        }
    }

    /// <summary>
    ///     Draws a label indicating that no rule is selected.
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <returns>The height of the label drawn.</returns>
    private static float DoNoRuleSelectedLabel(Rect rect)
    {
        var labelRect = Layout.GetTopRowRect(rect, Labels.LabelHeight, out _);
        Labels.DoLabel(labelRect, Strings.WorkTypeThingRuleWidget.NoRuleSelected,
            TextAnchor.MiddleCenter);
        return labelRect.height;
    }

    /// <summary>
    ///     Draws the stat weights section for a rule, including sliders and add/delete actions.
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <param name="stats">The available stat definitions.</param>
    /// <param name="statWeights">The current stat weights for the rule.</param>
    /// <param name="addAction">Action to add a stat weight.</param>
    /// <param name="deleteAction">Action to delete a stat weight by stat definition name.</param>
    /// <returns>The total height of the section drawn.</returns>
    private static float DoRuleStatWeights(Rect rect, IEnumerable<StatDef> stats,
        IEnumerable<StatWeight> statWeights, Action<StatDef> addAction, Action<string> deleteAction)
    {
        var y = 0f;
        var headerRect = Sections.GetSectionHeaderRect(rect, out var remRect);
        y += headerRect.height;
        var buttonRect =
            Layout.GetRightColumnRect(headerRect, headerRect.width / 4f, out headerRect);
        Layout.GetRightColumnRect(headerRect, Layout.ElementGap, out headerRect);
        Sections.DoSectionHeaderLabel(headerRect, Strings.WorkTypeThingRuleWidget.StatWeightsLabel,
            Strings.WorkTypeThingRuleWidget.StatWeightsTooltip);
        Buttons.DoActionButton(buttonRect, Strings.Actions.Add,
            () =>
            {
                Find.WindowStack.Add(new FloatMenu(stats
                    .Where(s => statWeights.All(weight => weight.StatDefName != s.defName))
                    .Select(s =>
                        new FloatMenuOption(
                            $"{s.LabelCap} [{s.category?.LabelCap ?? "No category"}]",
                            () => addAction(s))).ToList()));
            });
        var gapRect = Layout.DoVerticalGap(remRect, out remRect);
        y += gapRect.height;
        foreach (var statWeight in statWeights)
            y += Fields.DoLabeledFloatSlider(remRect, 0, [
                    // StatDefName is the dictionary key for this StatWeight — always non-null for an existing entry
                    new IconButton(Textures.Actions.Delete,
                        () => deleteAction(statWeight.StatDefName!),
                        isEnabled: !statWeight.Protected)
                ], statWeight.StatDef?.LabelCap ?? statWeight.StatDefName ?? string.Empty,
                statWeight.StatDef?.description, ref statWeight.Weight, -1 * StatWeight.WeightCap,
                StatWeight.WeightCap, 0.1f, null, out remRect);
        return y;
    }

    /// <summary>
    ///     Draws the scrollable part of the widget tab, including stat weights or a no-rule label.
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <param name="selectedRule">The currently selected rule.</param>
    /// <param name="updateThingsAction">Action to invoke when things need updating.</param>
    /// <param name="contentHeight">Reference to the content height to update.</param>
    private static void DoScrollablePart(Rect rect, WorkTypeThingRule? selectedRule,
        Action updateThingsAction, ref float contentHeight)
    {
        var y = 0f;
        if (selectedRule == null)
            y += DoNoRuleSelectedLabel(rect);
        else
            y += DoRuleStatWeights(rect, StatHelper.GetStatsByCategory(StatCategory.Work),
                selectedRule.StatWeights.ToList(), s =>
                {
                    selectedRule.SetStatWeight(s, 0f);
                    updateThingsAction.Invoke();
                }, statDefName =>
                {
                    selectedRule.DeleteStatWeight(statDefName);
                    updateThingsAction.Invoke();
                });
        if (Event.current.type == EventType.Layout) contentHeight = y;
    }

    /// <summary>
    ///     Draws the top part of the widget tab, including the rule selection button.
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <param name="rules">The available rules to select from.</param>
    /// <param name="selectedRule">The currently selected rule.</param>
    /// <param name="selectRuleAction">Action to invoke when a rule is selected.</param>
    private static void DoTopPart(Rect rect, IEnumerable<WorkTypeThingRule> rules,
        WorkTypeThingRule? selectedRule, Action<WorkTypeThingRule> selectRuleAction)
    {
        if (Verse.Widgets.ButtonText(rect,
                selectedRule == null ? Strings.Actions.Select : selectedRule.Label))
            Find.WindowStack.Add(new FloatMenu(rules.Where(r => r != selectedRule).Select(r =>
                new FloatMenuOption(r.Label, () => selectRuleAction(r))).ToList()));
    }

    /// <summary>
    ///     Draws the complete widget tab for work type thing rules.
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <param name="scrollableContentHeight">
    ///     Reference to the scrollable content height, which will be updated based on the content drawn.
    /// </param>
    /// <param name="scrollPosition">
    ///     Reference to the scroll position for the scrollable area.
    /// </param>
    /// <param name="thingIconBoxRowCount">
    ///     The number of rows to display in the thing icon box section.
    /// </param>
    /// <param name="workTypeRules">
    ///     The collection of available <see cref="WorkTypeThingRule" /> objects to select from.
    /// </param>
    /// <param name="selectedWorkTypeRule">
    ///     The currently selected <see cref="WorkTypeThingRule" /> object.
    /// </param>
    /// <param name="selectRuleAction">
    ///     Action to invoke when a rule is selected from the list.
    /// </param>
    /// <param name="updateThingsAction">
    ///     Action to invoke when the list of things needs to be updated (e.g., after stat weight changes).
    /// </param>
    /// <param name="thingIconBoxScrollPosition">
    ///     Reference to the scroll position for the thing icon box section.
    /// </param>
    /// <param name="things">
    ///     The list of <see cref="ThingDef" /> objects to display in the thing icon box section.
    /// </param>
    /// <param name="mapThingIconBoxScrollPosition">
    ///     Reference to the scroll position for the on-map thing icon box section.
    ///     Must be provided by the caller even when <paramref name="mapThings" /> is <see langword="null" />.
    /// </param>
    /// <param name="mapThings">
    ///     On-map <see cref="Thing" /> instances to display in the second list alongside the globally-available
    ///     <see cref="ThingDef" /> list. Pass <see langword="null" /> or an empty list to render only List 1 at
    ///     full width (graceful no-op that reproduces the single-list behavior).
    ///     <para>
    ///         <strong>Pre-sort contract (caller responsibility):</strong> the caller must supply instances
    ///         already sorted in descending score order (e.g. using
    ///         <see cref="WorkTypeThingRule.GetThingScore" />). The widget renders items in the supplied order
    ///         and does not re-sort; this keeps the render pass free of side effects on the shared adaptive
    ///         stat-range history.
    ///     </para>
    /// </param>
    public static void DoWidgetTab(Rect rect, ref float scrollableContentHeight,
        ref Vector2 scrollPosition, int thingIconBoxRowCount,
        IReadOnlyCollection<WorkTypeThingRule> workTypeRules,
        WorkTypeThingRule? selectedWorkTypeRule, Action<WorkTypeThingRule> selectRuleAction,
        Action updateThingsAction, ref Vector2 thingIconBoxScrollPosition,
        IReadOnlyList<ThingDef> things, ref Vector2 mapThingIconBoxScrollPosition,
        IReadOnlyList<Thing>? mapThings = null)
    {
        var contentHeight = scrollableContentHeight;
        var thingScrollPosition = thingIconBoxScrollPosition;
        var mapThingScrollPosition = mapThingIconBoxScrollPosition;
        Tabs.DoTab(rect, GetTopPartHeight(),
            r => DoTopPart(r, workTypeRules, selectedWorkTypeRule, selectRuleAction),
            scrollableContentHeight, ref scrollPosition,
            r => { DoScrollablePart(r, selectedWorkTypeRule, updateThingsAction, ref contentHeight); },
            GetBottomPartHeight(thingIconBoxRowCount),
            r => DoBottomPart(r, updateThingsAction, ref thingScrollPosition, things,
                ref mapThingScrollPosition, mapThings, selectedWorkTypeRule));
        scrollableContentHeight = contentHeight;
        thingIconBoxScrollPosition = thingScrollPosition;
        mapThingIconBoxScrollPosition = mapThingScrollPosition;
    }

    /// <summary>
    ///     Calculates the height of the bottom part of the widget tab based on the number of thing icon box rows.
    /// </summary>
    /// <param name="thingIconBoxRowCount">The number of thing icon box rows.</param>
    /// <returns>The calculated height.</returns>
    private static float GetBottomPartHeight(int thingIconBoxRowCount)
    {
        var thingIconBoxHeight = ThingIconBox.GetThingIconBoxHeight(thingIconBoxRowCount);
        return thingIconBoxHeight + Labels.SectionHeaderHeight + Layout.ElementGap;
    }

    /// <summary>
    ///     Gets the height of the top part of the widget tab.
    /// </summary>
    /// <returns>The height of the top part.</returns>
    private static float GetTopPartHeight()
    {
        return Buttons.ActionButtonHeight;
    }

    /// <summary>
    ///     Gets the tooltip string for a live <see cref="Thing" /> instance based on the rule's stat weights.
    ///     Reads stats directly from the instance (including <c>equippedStatOffsets</c>) via
    ///     <see cref="StatHelper.GetStatValue(Thing, StatDef)" />; no temporary thing is synthesised.
    ///     Returns an empty string when <see cref="Current.Game" /> is <see langword="null" />.
    /// </summary>
    /// <param name="thing">The <see cref="Thing" /> instance to get the tooltip for.</param>
    /// <param name="rule">The selected <see cref="WorkTypeThingRule" />.</param>
    /// <returns>The tooltip string describing the instance's actual stat values.</returns>
    private static string GetWorkTypeThingTooltip(Thing thing, WorkTypeThingRule rule)
    {
        var stringBuilder = new StringBuilder();
        _ = stringBuilder.AppendLine(thing.LabelCapNoCount);
        if (Current.Game == null) return stringBuilder.ToString();
        // Where(sw => sw.StatDef != null) guards non-null before Select; ! asserts that to the compiler
        var stats = rule.StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef!)
            .ToHashSet();
        if (!stats.Any()) return stringBuilder.ToString();
        _ = stringBuilder.AppendLine();
        foreach (var stat in stats)
            _ = stringBuilder.AppendLine(
                $"- {stat.LabelCap} = {StatHelper.GetStatValue(thing, stat):N2}");
        return stringBuilder.ToString();
    }

    /// <summary>
    ///     Gets the tooltip string for a <see cref="ThingDef" /> based on the selected rule's stat weights.
    /// </summary>
    /// <param name="def">The <see cref="ThingDef" /> to get the tooltip for.</param>
    /// <param name="rule">The selected <see cref="WorkTypeThingRule" />.</param>
    /// <returns>The tooltip string describing the thing's stats.</returns>
    private static string GetWorkTypeDefTooltip(ThingDef def, WorkTypeThingRule rule)
    {
        var stringBuilder = new StringBuilder();
        _ = stringBuilder.AppendLine(def.LabelCap);
        if (Current.Game == null) return stringBuilder.ToString();
        // Where(sw => sw.StatDef != null) guards non-null before Select; ! asserts that to the compiler
        var stats = rule.StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef!)
            .ToHashSet();
        if (!stats.Any()) return stringBuilder.ToString();
        _ = stringBuilder.AppendLine();
        var thing = def.MadeFromStuff
            ? ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def))
            : ThingMaker.MakeThing(def);
        foreach (var stat in stats)
            _ = stringBuilder.AppendLine(
                $"- {stat.LabelCap} = {StatHelper.GetStatValue(thing, stat):N2}");
        return stringBuilder.ToString();
    }
}