using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods for rendering checkboxes in the UI.
/// </summary>
[PublicAPI]
public static class Checkboxes
{
    /// <summary>
    ///     The default size of a checkbox in pixels.
    /// </summary>
    private const float CheckboxSize = 24f;

    /// <summary>
    ///     Cycles the state of a multi-state checkbox.
    /// </summary>
    /// <param name="state">The current <see cref="MultiCheckboxState" /> of the checkbox.</param>
    /// <returns>
    ///     The next state as a nullable boolean:
    ///     <list type="bullet">
    ///         <item><c>true</c> for <see cref="MultiCheckboxState.Partial" /></item>
    ///         <item><c>false</c> for <see cref="MultiCheckboxState.On" /></item>
    ///         <item><c>null</c> for <see cref="MultiCheckboxState.Off" /></item>
    ///     </list>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if the state is not a valid <see cref="MultiCheckboxState" />.
    /// </exception>
    private static bool? CycleCheckboxState(MultiCheckboxState state)
    {
        return state switch
        {
            MultiCheckboxState.On => false,
            MultiCheckboxState.Off => null,
            MultiCheckboxState.Partial => true,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    /// <summary>
    ///     Draws a checkbox at the center of the specified rectangle and updates the referenced boolean value.
    ///     Optionally displays a tooltip when hovering over the checkbox.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the checkbox.</param>
    /// <param name="value">A reference to the boolean value to update.</param>
    /// <param name="tooltip">An optional tooltip to display when hovering over the checkbox.</param>
    public static void DoCheckbox(Rect rect, ref bool value, string? tooltip = null)
    {
        var size = GetCheckboxSize(rect);
        var checkBoxRect = Layout.GetCenteredRect(rect, size, size);
        Verse.Widgets.Checkbox(checkBoxRect.x, checkBoxRect.y, ref value, size, paintable: true);
        if (!string.IsNullOrEmpty(tooltip)) TooltipHandler.TipRegion(rect, tooltip);
    }

    /// <summary>
    ///     Draws a tri-state checkbox at the center of the specified rectangle and updates the referenced nullable boolean
    ///     value.
    ///     Optionally displays a tooltip when hovering over the checkbox.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the checkbox.</param>
    /// <param name="value">A reference to the nullable boolean value to update.</param>
    /// <param name="tooltip">An optional tooltip to display when hovering over the checkbox.</param>
    public static void DoCheckbox(Rect rect, ref bool? value, string? tooltip = null)
    {
        var size = GetCheckboxSize(rect);
        var checkBoxRect = Layout.GetCenteredRect(rect, size, size);
        var state = GetCheckboxState(value);
        var newState = Verse.Widgets.CheckboxMulti(checkBoxRect, state);
        if (newState != state) value = CycleCheckboxState(state);
        if (!string.IsNullOrEmpty(tooltip)) TooltipHandler.TipRegion(rect, tooltip);
    }

    /// <summary>
    ///     Calculates the appropriate size for a checkbox based on the provided rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to use for size calculation.</param>
    /// <returns>The size of the checkbox in pixels.</returns>
    private static float GetCheckboxSize(Rect rect)
    {
        return Mathf.Min(CheckboxSize, rect.width, rect.height);
    }

    /// <summary>
    ///     Converts a nullable boolean value to a <see cref="MultiCheckboxState" />.
    /// </summary>
    /// <param name="value">The nullable boolean value to convert.</param>
    /// <returns>
    ///     <see cref="MultiCheckboxState.Partial" /> if <c>null</c>,
    ///     <see cref="MultiCheckboxState.Off" /> if <c>false</c>,
    ///     <see cref="MultiCheckboxState.On" /> if <c>true</c>.
    /// </returns>
    private static MultiCheckboxState GetCheckboxState(bool? value)
    {
        return value switch
        {
            null => MultiCheckboxState.Partial,
            false => MultiCheckboxState.Off,
            true => MultiCheckboxState.On
        };
    }
}