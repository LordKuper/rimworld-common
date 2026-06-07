using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides UI field rendering utilities.
/// </summary>
public static partial class Fields
{
    /// <summary>
    ///     The height of a selector row.
    /// </summary>
    private const float SelectorRowHeight = Layout.RowHeight;

    /// <summary>
    ///     Draws a labeled selector field with optional action buttons and icon.
    /// </summary>
    /// <typeparam name="T">The type of the selectable value.</typeparam>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="indentationLevel">The indentation level for the label.</param>
    /// <param name="actionButtons">A collection of icon buttons to display next to the label.</param>
    /// <param name="label">The label text for the field.</param>
    /// <param name="labelTooltip">The tooltip for the label.</param>
    /// <param name="value">The currently selected value. Will be updated if changed.</param>
    /// <param name="options">The available options for selection.</param>
    /// <param name="getLabelAction">A function to get the display label for each option.</param>
    /// <param name="getTooltipAction">A function to get the tooltip for each option, or null if no tooltip is needed.</param>
    /// <param name="selectAction">An action to execute when an option is selected.</param>
    /// <param name="icon">An optional icon to display next to the label.</param>
    /// <param name="remRect">Returns the remaining rectangle after drawing the field.</param>
    /// <returns>The height of the selector row.</returns>
    public static float DoLabeledSelector<T>(Rect rect, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? labelTooltip, T value,
        IReadOnlyCollection<T> options, Func<T, string> getLabelAction,
        Func<T, string>? getTooltipAction, Action<T> selectAction, Texture? icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, SelectorRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon,
            out var inputRect);
        DoSelector(inputRect, value, options, getLabelAction, getTooltipAction, selectAction);
        return SelectorRowHeight;
    }

    /// <summary>
    ///     Draws a selector button that allows the user to choose from a list of options.
    /// </summary>
    /// <typeparam name="T">The type of the selectable value.</typeparam>
    /// <param name="rect">The rectangle in which to draw the selector.</param>
    /// <param name="value">The currently selected value. Will be updated if changed.</param>
    /// <param name="options">The available options for selection.</param>
    /// <param name="getLabelAction">A function to get the display label for each option.</param>
    /// <param name="getTooltipAction">A function to get the tooltip for each option, or null if no tooltip is needed.</param>
    /// <param name="selectAction">An action to execute when an option is selected.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="options" /> or <paramref name="getLabelAction" /> or <paramref name="selectAction" /> is
    ///     null.
    /// </exception>
    private static void DoSelector<T>(Rect rect, T value, IReadOnlyCollection<T> options,
        Func<T, string> getLabelAction, Func<T, string>? getTooltipAction, Action<T> selectAction)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (getLabelAction == null) throw new ArgumentNullException(nameof(getLabelAction));
        if (selectAction == null) throw new ArgumentNullException(nameof(selectAction));
        var optionList = new List<FloatMenuOption>(options.Count);
        foreach (var o in options) optionList.Add(new FloatMenuOption(getLabelAction(o), () => { selectAction(o); }));
        Buttons.DoActionButton(rect, getLabelAction(value),
            () => { Find.WindowStack.Add(new FloatMenu(optionList)); },
            getTooltipAction?.Invoke(value), optionList.Count > 0);
    }
}