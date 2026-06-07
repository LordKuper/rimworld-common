using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods for drawing labeled UI sections with headers and outlined backgrounds.
/// </summary>
[PublicAPI]
public static class Sections
{
    /// <summary>
    ///     The background color used for section boxes.
    /// </summary>
    private static readonly Color BackgroundColor = new(1f, 1f, 1f, 0.05f);

    /// <summary>
    ///     The outline color used for section boxes.
    /// </summary>
    private static readonly Color OutlineColor = new(1f, 1f, 1f, 0.4f);

    /// <summary>
    ///     Draws a labeled section with a header, optional tooltip, and outlined background.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the section.</param>
    /// <param name="contentHeight">The height of the section's content area.</param>
    /// <param name="header">The header text for the section. Must not be null or whitespace.</param>
    /// <param name="tooltip">An optional tooltip for the header.</param>
    /// <param name="sectionContentRect">Outputs the rectangle for the section's content area.</param>
    /// <param name="remRect">Outputs the remaining rectangle after the section.</param>
    /// <returns>The total vertical space consumed by the section, including header and gaps.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="header" /> is null or whitespace.</exception>
    public static float DoLabeledSectionBox(Rect rect, float contentHeight, string header,
        string? tooltip, out Rect sectionContentRect, out Rect remRect)
    {
        if (string.IsNullOrWhiteSpace(header)) throw new ArgumentNullException(nameof(header));
        var y = 0f;
        y += DoSectionHeader(rect, header, tooltip, out remRect);
        var outlineRect = Layout.GetTopRowRect(remRect, contentHeight + Layout.ElementGapSmall * 2,
            out remRect);
        y += outlineRect.height;
        Verse.Widgets.DrawBoxSolidWithOutline(outlineRect, BackgroundColor, OutlineColor);
        sectionContentRect = outlineRect.ContractedBy(Layout.ElementGapSmall);
        y += Layout.DoVerticalGap(remRect, out remRect).height;
        return y;
    }

    /// <summary>
    ///     Draws the section header and returns the vertical space consumed.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the header.</param>
    /// <param name="header">The header text. Must not be null or whitespace.</param>
    /// <param name="tooltip">An optional tooltip for the header.</param>
    /// <param name="remRect">Outputs the remaining rectangle after the header.</param>
    /// <returns>The vertical space consumed by the header and gap.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="header" /> is null or whitespace.</exception>
    public static float DoSectionHeader(Rect rect, string header, string? tooltip, out Rect remRect)
    {
        if (string.IsNullOrWhiteSpace(header)) throw new ArgumentNullException(nameof(header));
        var y = 0f;
        var headerRect = GetSectionHeaderRect(rect, out remRect);
        y += headerRect.height;
        DoSectionHeaderLabel(headerRect, header, tooltip);
        y += Layout.DoVerticalGapSmall(remRect, out remRect).height;
        return y;
    }

    /// <summary>
    ///     Draws the section header label and optional tooltip icon.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the label.</param>
    /// <param name="header">The header text.</param>
    /// <param name="tooltip">An optional tooltip for the header.</param>
    public static void DoSectionHeaderLabel(Rect rect, string header, string? tooltip = null)
    {
        var font = Text.Font;
        var anchor = Text.Anchor;
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleLeft;
        Verse.Widgets.Label(rect, header);
        if (!string.IsNullOrWhiteSpace(tooltip))
        {
            var headerWidth = Labels.GetTextWidth(header, Text.Font);
            Layout.GetLeftColumnRect(rect, headerWidth + Layout.ElementGapSmall,
                out var tooltipRect);
            var buttonRect = Layout.GetLeftColumnRect(tooltipRect, Icons.InfoIconSize, out _);
            Icons.DoIcon(buttonRect, Resources.Textures.InfoIcon, tooltip);
        }

        Text.Font = font;
        Text.Anchor = anchor;
    }

    /// <summary>
    ///     Draws a toggleable section header with a checkbox and label, and outputs the remaining rectangle.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the header.</param>
    /// <param name="isEnabled">A reference to the toggle state controlled by the header checkbox.</param>
    /// <param name="checkBoxTooltip">An optional tooltip for the toggle checkbox.</param>
    /// <param name="header">The header text.</param>
    /// <param name="headerTooltip">An optional tooltip for the header.</param>
    /// <param name="remRect">The remaining rectangle below the header.</param>
    /// <returns>The vertical space consumed by the header, including the trailing gap.</returns>
    internal static float DoToggleableSectionHeader(Rect rect, ref bool isEnabled,
        string? checkBoxTooltip, string header, string? headerTooltip, out Rect remRect)
    {
        if (string.IsNullOrWhiteSpace(header)) throw new ArgumentNullException(nameof(header));
        var y = 0f;
        var headerRect = GetSectionHeaderRect(rect, out remRect);
        y += headerRect.height;
        var toggleRect = Layout.GetLeftColumnRect(headerRect, headerRect.height, out headerRect);
        Checkboxes.DoCheckbox(toggleRect, ref isEnabled, checkBoxTooltip);
        Layout.GetLeftColumnRect(headerRect, Layout.ElementGapSmall, out headerRect);
        DoSectionHeaderLabel(headerRect, header, headerTooltip);
        y += Layout.DoVerticalGapSmall(remRect, out remRect).height;
        return y;
    }

    /// <summary>
    ///     Renders a toggleable section header with an optional checkbox and label, and calculates the remaining layout
    ///     rectangle.
    /// </summary>
    /// <param name="rect">The rectangle within which the section header is rendered.</param>
    /// <param name="isEnabled">
    ///     A nullable boolean indicating the state of the checkbox.  If <see langword="null" />, the checkbox is rendered in
    ///     an indeterminate state.  The value is updated based on user interaction.
    /// </param>
    /// <param name="checkBoxTooltip">
    ///     An optional tooltip displayed when hovering over the checkbox. Can be
    ///     <see langword="null" />.
    /// </param>
    /// <param name="header">The text displayed as the section header. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="headerTooltip">
    ///     An optional tooltip displayed when hovering over the section header. Can be
    ///     <see langword="null" />.
    /// </param>
    /// <param name="remRect">The remaining rectangle after rendering the section header, for further layout purposes.</param>
    /// <returns>The vertical space occupied by the section header, including any gaps or padding.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="header" /> is <see langword="null" /> or whitespace.</exception>
    internal static float DoToggleableSectionHeader(Rect rect, ref bool? isEnabled,
        string? checkBoxTooltip, string header, string? headerTooltip, out Rect remRect)
    {
        if (string.IsNullOrWhiteSpace(header)) throw new ArgumentNullException(nameof(header));
        var y = 0f;
        var headerRect = GetSectionHeaderRect(rect, out remRect);
        y += headerRect.height;
        var toggleRect = Layout.GetLeftColumnRect(headerRect, headerRect.height, out headerRect);
        Checkboxes.DoCheckbox(toggleRect, ref isEnabled, checkBoxTooltip);
        Layout.GetLeftColumnRect(headerRect, Layout.ElementGapSmall, out headerRect);
        DoSectionHeaderLabel(headerRect, header, headerTooltip);
        y += Layout.DoVerticalGapSmall(remRect, out remRect).height;
        return y;
    }

    /// <summary>
    ///     Calculates the rectangle for a section header within the specified area.
    /// </summary>
    /// <param name="rect">The total area from which the section header rectangle is derived.</param>
    /// <param name="remRect">When this method returns, contains the remaining area after the section header is excluded.</param>
    /// <returns>
    ///     A <see cref="Rect" /> representing the area allocated for the section header. The height of the rectangle is
    ///     determined by the predefined section header height.
    /// </returns>
    public static Rect GetSectionHeaderRect(Rect rect, out Rect remRect)
    {
        return Layout.GetTopRowRect(rect, Labels.SectionHeaderHeight, out remRect);
    }
}