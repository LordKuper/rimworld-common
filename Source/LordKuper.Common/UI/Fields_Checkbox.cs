using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides UI field rendering utilities for labeled checkboxes.
/// </summary>
public static partial class Fields
{
    private const float CheckboxRowHeight = Layout.RowHeight;

    /// <summary>
    ///     Draws a labeled checkbox field with optional indentation, action buttons, tooltip, and icon.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="indentationLevel">The indentation level for the field row.</param>
    /// <param name="actionButtons">A collection of icon buttons to display next to the field.</param>
    /// <param name="value">A reference to the boolean value to be toggled by the checkbox.</param>
    /// <param name="label">The label to display next to the checkbox.</param>
    /// <param name="labelTooltip">The tooltip to display for the label, if any.</param>
    /// <param name="icon">An optional icon to display next to the label.</param>
    /// <param name="remRect">The remaining rectangle after rendering the field row.</param>
    /// <returns>The height of the rendered checkbox row.</returns>
    public static float DoLabeledCheckbox(Rect rect, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, ref bool value, string? label,
        string? labelTooltip, Texture? icon, out Rect remRect)
    {
        if (indentationLevel < 0)
            throw new ArgumentOutOfRangeException(nameof(indentationLevel),
                "Indentation level must be non-negative.");
        var font = Text.Font;
        var anchor = Text.Anchor;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        var rowRect = GetFieldRowRect(rect, CheckboxRowHeight, out remRect);
        if (indentationLevel > 0)
            Layout.GetLeftColumnRect(rowRect, indentationLevel * Layout.IndentationSize,
                out rowRect);
        DoFieldActionButtons(rowRect, actionButtons, out rowRect);
        if (!string.IsNullOrEmpty(labelTooltip)) TooltipHandler.TipRegion(rowRect, labelTooltip);
        var checkBoxRect = Layout.GetLeftColumnRect(rowRect, rowRect.height, out var labelRect);
        Checkboxes.DoCheckbox(checkBoxRect, ref value);
        Layout.GetLeftColumnRect(labelRect, Layout.ElementGap, out labelRect);
        if (icon != null)
        {
            var iconRect = Layout.GetLeftColumnRect(labelRect, rowRect.height, out labelRect);
            DoFieldLabelIcon(iconRect, icon);
            Layout.GetLeftColumnRect(labelRect, Layout.ElementGapSmall, out labelRect);
        }
        Verse.Widgets.LabelFit(labelRect, label);
        Text.Font = font;
        Text.Anchor = anchor;
        return CheckboxRowHeight;
    }

    /// <summary>
    ///     Draws a labeled checkbox field for a nullable boolean value, with optional indentation, action buttons, tooltip,
    ///     and icon.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="indentationLevel">The indentation level for the field row.</param>
    /// <param name="actionButtons">A collection of icon buttons to display next to the field.</param>
    /// <param name="value">A reference to the nullable boolean value to be toggled by the checkbox.</param>
    /// <param name="label">The label to display next to the checkbox.</param>
    /// <param name="labelTooltip">The tooltip to display for the label, if any.</param>
    /// <param name="icon">An optional icon to display next to the label.</param>
    /// <param name="remRect">The remaining rectangle after rendering the field row.</param>
    /// <returns>The height of the rendered checkbox row.</returns>
    public static float DoLabeledCheckbox(Rect rect, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, ref bool? value, string? label,
        string? labelTooltip, Texture? icon, out Rect remRect)
    {
        if (indentationLevel < 0)
            throw new ArgumentOutOfRangeException(nameof(indentationLevel),
                "Indentation level must be non-negative.");
        var font = Text.Font;
        var anchor = Text.Anchor;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        var rowRect = GetFieldRowRect(rect, CheckboxRowHeight, out remRect);
        if (indentationLevel > 0)
            Layout.GetLeftColumnRect(rowRect, indentationLevel * Layout.IndentationSize,
                out rowRect);
        DoFieldActionButtons(rowRect, actionButtons, out rowRect);
        if (!string.IsNullOrEmpty(labelTooltip)) TooltipHandler.TipRegion(rowRect, labelTooltip);
        var checkBoxRect = Layout.GetLeftColumnRect(rowRect, rowRect.height, out var labelRect);
        Checkboxes.DoCheckbox(checkBoxRect, ref value);
        Layout.GetLeftColumnRect(labelRect, Layout.ElementGap, out labelRect);
        if (icon != null)
        {
            var iconRect = Layout.GetLeftColumnRect(labelRect, rowRect.height, out labelRect);
            DoFieldLabelIcon(iconRect, icon);
            Layout.GetLeftColumnRect(labelRect, Layout.ElementGapSmall, out labelRect);
        }
        Verse.Widgets.LabelFit(labelRect, label);
        Text.Font = font;
        Text.Anchor = anchor;
        return CheckboxRowHeight;
    }
}