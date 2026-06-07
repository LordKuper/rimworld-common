using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods and constants for rendering input fields in the UI.
/// </summary>
[PublicAPI]
public static partial class Fields
{
    /// <summary>
    ///     Draws a set of action buttons within the specified rectangle and outputs the remaining rectangle.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the action buttons.</param>
    /// <param name="actionButtons">A collection of <see cref="IconButton" />s to display.</param>
    /// <param name="remRect">Outputs the remaining rectangle after the buttons are drawn.</param>
    private static void DoFieldActionButtons(Rect rect,
        IReadOnlyCollection<IconButton>? actionButtons, out Rect remRect)
    {
        if (actionButtons == null || actionButtons.Count == 0)
        {
            remRect = rect;
            return;
        }

        var buttonsRect = Layout.GetLeftColumnRect(rect,
            (Buttons.IconButtonSize + Layout.ElementGapSmall) * actionButtons.Count, out remRect);
        foreach (var actionButton in actionButtons)
        {
            var buttonRect =
                Layout.GetLeftColumnRect(buttonsRect, Buttons.IconButtonSize, out buttonsRect);
            Buttons.DoIconButton(buttonRect, actionButton);
            Layout.GetLeftColumnRect(buttonsRect, Layout.ElementGapSmall, out buttonsRect);
        }
    }

    /// <summary>
    ///     Draws a label within the specified rectangle and attaches a tooltip if provided.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the label.</param>
    /// <param name="label">The text to display as the label.</param>
    private static void DoFieldLabel(Rect rect, string label)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentNullException(nameof(label));
        var font = Text.Font;
        var anchor = Text.Anchor;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        Verse.Widgets.LabelFit(rect, label);
        Text.Font = font;
        Text.Anchor = anchor;
    }

    /// <summary>
    ///     Draws a label and optional icon within the specified rectangles, and attaches a tooltip if provided.
    /// </summary>
    /// <param name="labelRect">The rectangle in which to draw the label.</param>
    /// <param name="label">The text to display as the label.</param>
    /// <param name="iconRect">The rectangle in which to draw the icon.</param>
    /// <param name="icon">The icon texture to draw, or null for none.</param>
    private static void DoFieldLabel(Rect labelRect, string label, Rect iconRect, Texture? icon)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentNullException(nameof(label));
        if (icon != null) DoFieldLabelIcon(iconRect, icon);
        DoFieldLabel(labelRect, label);
    }

    /// <summary>
    ///     Draws an icon within the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the icon.</param>
    /// <param name="icon">The icon texture to draw.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="icon" /> is null or <paramref name="rect" /> is zero.</exception>
    private static void DoFieldLabelIcon(Rect rect, Texture icon)
    {
        if (icon == null) throw new ArgumentNullException(nameof(icon));
        if (rect == Rect.zero)
            throw new ArgumentNullException(nameof(rect),
                "Icon rectangle must not be zero when an icon is provided.");
        Verse.Widgets.DrawTextureFitted(rect, icon, 1f);
    }

    /// <summary>
    ///     Displays a labeled field with associated action buttons within the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle within which the field and buttons are displayed.</param>
    /// <param name="indentationLevel">The level of indentation applied to the field label.</param>
    /// <param name="actionButtons">A collection of action buttons to display alongside the field label.</param>
    /// <param name="label">The text to display as the field label.</param>
    /// <param name="tooltip">The tooltip text to display when hovering over the field label.</param>
    /// <param name="icon">The icon to display next to the label, or null for none.</param>
    /// <param name="remRect">The remaining rectangle after the field label and buttons are rendered.</param>
    private static void DoFieldLabelWithButtons(Rect rect, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? tooltip,
        Texture? icon, out Rect remRect)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentNullException(nameof(label));
        GetLabeledFieldRects(rect, 0.5f, indentationLevel, icon != null, out var labelRect,
            out remRect, out var iconRect, out var tooltipRect);
        DoFieldActionButtons(labelRect, actionButtons, out labelRect);
        DoFieldLabel(labelRect, label, iconRect, icon);
        if (!string.IsNullOrWhiteSpace(tooltip))
            TooltipHandler.TipRegion(tooltipRect, tooltip);
    }

    /// <summary>
    ///     Gets the top row rectangle for a field and outputs the remaining rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to split.</param>
    /// <param name="rowHeight">The height of the row to extract.</param>
    /// <param name="remRect">Outputs the remaining rectangle after extracting the row.</param>
    /// <returns>The contracted top row rectangle for the field.</returns>
    private static Rect GetFieldRowRect(Rect rect, float rowHeight, out Rect remRect)
    {
        var rowRect = Layout.GetTopRowRect(rect, rowHeight, out remRect);
        Verse.Widgets.DrawHighlightIfMouseover(rowRect);
        return rowRect.ContractedBy(Layout.ElementGapTiny);
    }

    /// <summary>
    ///     Calculates and returns the rectangular areas for a labeled field within a given row.
    /// </summary>
    /// <param name="rowRect">The rectangle representing the row in which the labeled field is located.</param>
    /// <param name="labelWidthFactor">
    ///     The proportion of the row's width allocated to the label. Must be in the range
    ///     [0.25..0.75].
    /// </param>
    /// <param name="indentationLevel">The number of indentation levels applied to the label. Must be non-negative.</param>
    /// <param name="icon">Whether to allocate space for an icon next to the label.</param>
    /// <param name="labelRect">When the method returns, contains the rectangle allocated for the label.</param>
    /// <param name="inputRect">When the method returns, contains the rectangle allocated for the input field.</param>
    /// <param name="iconRect">
    ///     When the method returns, contains the rectangle allocated for the icon, or
    ///     <see cref="Rect.zero" /> if <paramref name="icon" /> is false.
    /// </param>
    /// <param name="tooltipRect">When the method returns, contains the rectangle allocated for the tooltip region.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if <paramref name="labelWidthFactor" /> is outside the range [0.25..0.75] or if
    ///     <paramref name="indentationLevel" /> is negative.
    /// </exception>
    private static void GetLabeledFieldRects(Rect rowRect, float labelWidthFactor,
        int indentationLevel, bool icon, out Rect labelRect, out Rect inputRect, out Rect iconRect,
        out Rect tooltipRect)
    {
        if (labelWidthFactor is < 0.25f or > 0.75f)
            throw new ArgumentOutOfRangeException(nameof(labelWidthFactor),
                "Label width factor must be in [0.25..0.75] range.");
        if (indentationLevel < 0)
            throw new ArgumentOutOfRangeException(nameof(indentationLevel),
                "Indentation level must be non-negative.");
        labelRect =
            Layout.GetLeftColumnRect(rowRect, rowRect.width * labelWidthFactor, out inputRect);
        if (indentationLevel > 0)
            Layout.GetLeftColumnRect(labelRect, indentationLevel * Layout.IndentationSize,
                out labelRect);
        tooltipRect = labelRect;
        if (icon)
        {
            iconRect = Layout.GetLeftColumnRect(labelRect, labelRect.height, out labelRect);
            Layout.GetLeftColumnRect(labelRect, Layout.ElementGapSmall, out labelRect);
        }
        else
        {
            iconRect = Rect.zero;
        }

        Layout.GetLeftColumnRect(inputRect, Layout.ElementGap, out inputRect);
    }
}