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
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <param name="refreshAction">Action to invoke when the refresh button is clicked.</param>
    /// <param name="thingIconBoxScrollPosition">Reference to the scroll position for the thing icon box section.</param>
    /// <param name="things">The list of <see cref="ThingDef" /> objects to display.</param>
    /// <param name="selectedRule">The currently selected <see cref="WorkTypeThingRule" />.</param>
    private static void DoBottomPart(Rect rect, Action refreshAction, ref Vector2 thingIconBoxScrollPosition,
        IReadOnlyList<ThingDef> things, WorkTypeThingRule? selectedRule)
    {
        if (selectedRule == null) return;
        var headerRect = Sections.GetSectionHeaderRect(rect, out var remRect);
        var buttonRect = Layout.GetRightColumnRect(headerRect, headerRect.width / 4f, out headerRect);
        Layout.GetRightColumnRect(headerRect, Layout.ElementGap, out headerRect);
        Sections.DoSectionHeaderLabel(headerRect, Strings.WorkTypeThingRuleWidget.AvailableItemsLabel,
            Strings.WorkTypeThingRuleWidget.AvailableItemsTooltip);
        Buttons.DoActionButton(buttonRect, Strings.Actions.Refresh, refreshAction);
        Layout.DoVerticalGap(remRect, out remRect);
        ThingIconBox.DoThingDefBox(remRect, ref thingIconBoxScrollPosition, things, null,
            def => GetWorkTypeDefTooltip(def, selectedRule));
    }

    /// <summary>
    ///     Draws a label indicating that no rule is selected.
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <returns>The height of the label drawn.</returns>
    private static float DoNoRuleSelectedLabel(Rect rect)
    {
        var labelRect = Layout.GetTopRowRect(rect, Labels.LabelHeight, out _);
        Labels.DoLabel(labelRect, Strings.WorkTypeThingRuleWidget.NoRuleSelected, TextAnchor.MiddleCenter);
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
    private static float DoRuleStatWeights(Rect rect, IEnumerable<StatDef> stats, IEnumerable<StatWeight> statWeights,
        Action<StatDef> addAction, Action<string> deleteAction)
    {
        var y = 0f;
        var headerRect = Sections.GetSectionHeaderRect(rect, out var remRect);
        y += headerRect.height;
        var buttonRect = Layout.GetRightColumnRect(headerRect, headerRect.width / 4f, out headerRect);
        Layout.GetRightColumnRect(headerRect, Layout.ElementGap, out headerRect);
        Sections.DoSectionHeaderLabel(headerRect, Strings.WorkTypeThingRuleWidget.StatWeightsLabel,
            Strings.WorkTypeThingRuleWidget.StatWeightsTooltip);
        Buttons.DoActionButton(buttonRect, Strings.Actions.Add,
            () =>
            {
                Find.WindowStack.Add(new FloatMenu(stats
                    .Where(s => statWeights.All(weight => weight.StatDefName != s.defName)).Select(s =>
                        new FloatMenuOption($"{s.LabelCap} [{s.category?.LabelCap ?? "No category"}]",
                            () => addAction(s))).ToList()));
            });
        var gapRect = Layout.DoVerticalGap(remRect, out remRect);
        y += gapRect.height;
        foreach (var statWeight in statWeights)
        {
            y += Fields.DoLabeledFloatSlider(remRect, 0, [
                    new IconButton(Textures.Actions.Delete, () => deleteAction(statWeight.StatDefName),
                        isEnabled: !statWeight.Protected)
                ], statWeight.StatDef?.LabelCap ?? statWeight.StatDefName ?? string.Empty,
                statWeight.StatDef?.description,
                ref statWeight.Weight, -1 * StatWeight.WeightCap, StatWeight.WeightCap, 0.1f, null, out remRect);
        }
        return y;
    }

    /// <summary>
    ///     Draws the scrollable part of the widget tab, including stat weights or a no-rule label.
    /// </summary>
    /// <param name="rect">The rectangle area to draw in.</param>
    /// <param name="selectedRule">The currently selected rule.</param>
    /// <param name="updateThingsAction">Action to invoke when things need updating.</param>
    /// <param name="contentHeight">Reference to the content height to update.</param>
    private static void DoScrollablePart(Rect rect, WorkTypeThingRule? selectedRule, Action updateThingsAction,
        ref float contentHeight)
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
    private static void DoTopPart(Rect rect, IEnumerable<WorkTypeThingRule> rules, WorkTypeThingRule? selectedRule,
        Action<WorkTypeThingRule> selectRuleAction)
    {
        if (Verse.Widgets.ButtonText(rect, selectedRule == null ? Strings.Actions.Select : selectedRule.Label))
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
    public static void DoWidgetTab(Rect rect, ref float scrollableContentHeight, ref Vector2 scrollPosition,
        int thingIconBoxRowCount, IReadOnlyCollection<WorkTypeThingRule> workTypeRules,
        WorkTypeThingRule? selectedWorkTypeRule, Action<WorkTypeThingRule> selectRuleAction, Action updateThingsAction,
        ref Vector2 thingIconBoxScrollPosition, IReadOnlyList<ThingDef> things)
    {
        var contentHeight = scrollableContentHeight;
        var thingScrollPosition = thingIconBoxScrollPosition;
        Tabs.DoTab(rect, GetTopPartHeight(), r => DoTopPart(r, workTypeRules, selectedWorkTypeRule, selectRuleAction),
            scrollableContentHeight, ref scrollPosition,
            r => { DoScrollablePart(r, selectedWorkTypeRule, updateThingsAction, ref contentHeight); },
            GetBottomPartHeight(thingIconBoxRowCount),
            r => DoBottomPart(r, updateThingsAction, ref thingScrollPosition, things, selectedWorkTypeRule));
        scrollableContentHeight = contentHeight;
        thingIconBoxScrollPosition = thingScrollPosition;
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
        var stats = rule.StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef).ToHashSet();
        if (!stats.Any()) return stringBuilder.ToString();
        _ = stringBuilder.AppendLine();
        var thing = def.MadeFromStuff
            ? ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def))
            : ThingMaker.MakeThing(def);
        foreach (var stat in stats)
        {
            _ = stringBuilder.AppendLine($"- {stat.LabelCap} = {StatHelper.GetStatValue(thing, stat):N2}");
        }
        return stringBuilder.ToString();
    }
}