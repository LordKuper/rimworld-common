using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods for drawing icon buttons in the UI.
/// </summary>
[PublicAPI]
public static class Buttons
{
    /// <summary>
    ///     The default height of an action button in pixels.
    /// </summary>
    public const float ActionButtonHeight = Layout.GridSize * 8;

    /// <summary>
    ///     The minimum width of an action button in pixels.
    /// </summary>
    private const float ActionButtonWidthMin = Layout.GridSize * 50;

    /// <summary>
    ///     The default size (in pixels) for icon buttons.
    /// </summary>
    public const float IconButtonSize = Layout.GridSize * 6;

    /// <summary>
    ///     Draws a standard action button with a label and invokes the specified action when clicked.
    /// </summary>
    /// <param name="rect">The rectangle area where the button will be drawn.</param>
    /// <param name="label">The text label to display on the button.</param>
    /// <param name="action">The action to invoke when the button is clicked.</param>
    /// <param name="tooltip">The tooltip text to display when hovering over the button. Can be null.</param>
    /// <param name="isEnabled">Indicates whether the button is enabled and interactive. Default is true.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action" /> is null.</exception>
    public static void DoActionButton(Rect rect, [CanBeNull] string label, [NotNull] Action action,
        [CanBeNull] string tooltip = null, bool isEnabled = true)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        var font = Text.Font;
        Text.Font = GameFont.Small;
        var buttonRect = Layout.GetCenterRowRect(rect, ActionButtonHeight, out _, out _);
        if (Verse.Widgets.ButtonText(buttonRect, label, active: isEnabled)) action.Invoke();
        if (!string.IsNullOrEmpty(tooltip)) TooltipHandler.TipRegion(buttonRect, tooltip);
        Text.Font = font;
    }

    /// <summary>
    ///     Draws a row of action buttons and returns the total height used.
    /// </summary>
    /// <param name="rect">The rectangle area where the button row will be drawn.</param>
    /// <param name="actionButtons">An array of <see cref="ActionButton" />s to display.</param>
    /// <param name="remRect">The remaining rectangle after the row is drawn.</param>
    /// <returns>The height of the button row.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="actionButtons" /> is null.</exception>
    public static float DoActionButtonGrid(Rect rect, [NotNull] ActionButton[] actionButtons, out Rect remRect)
    {
        if (actionButtons == null) throw new ArgumentNullException(nameof(actionButtons));
        if (actionButtons.Length == 0)
        {
            remRect = rect;
            return 0f;
        }
        var buttonRects = Layout.GetGridRects(rect, ActionButtonWidthMin, Layout.ElementGapSmall, ActionButtonHeight,
            Layout.ElementGapSmall, actionButtons.Length, out var height, out remRect);
        for (var i = 0; i < actionButtons.Length; i++)
        {
            var button = actionButtons[i];
            var buttonRect = buttonRects[i];
            DoActionButton(buttonRect, button.Label, button.Action, button.Tooltip, button.IsEnabled);
        }
        return height;
    }

    /// <summary>
    ///     Draws an icon button and handles its interaction.
    /// </summary>
    /// <param name="rect">The rectangle area where the button will be drawn.</param>
    /// <param name="iconButton">The <see cref="IconButton" /> to display and handle.</param>
    public static void DoIconButton(Rect rect, IconButton iconButton)
    {
        if (iconButton.IsEnabled)
        {
            if (Verse.Widgets.ButtonImageFitted(rect, iconButton.Icon)) iconButton.Action.Invoke();
        }
        else
        {
            Verse.Widgets.DrawTextureFitted(rect, iconButton.Icon, 1f, 0.25f);
        }
        if (!string.IsNullOrEmpty(iconButton.Tooltip))
            TooltipHandler.TipRegion(rect, iconButton.Tooltip);
    }

    /// <summary>
    ///     Renders a toggleable icon button within the specified rectangle and updates the toggle state.
    /// </summary>
    /// <remarks>
    ///     This method uses the provided <paramref name="getter" /> to determine the current toggle state
    ///     and the  <paramref name="setter" /> to update the state after the button is interacted with. The button displays
    ///     different icons and tooltips depending on whether the toggle is enabled or disabled.
    /// </remarks>
    /// <param name="rect">The screen space rectangle where the button will be drawn.</param>
    /// <param name="getter">A function that retrieves the current toggle state. Must not be <see langword="null" />.</param>
    /// <param name="setter">An action that sets the updated toggle state. Must not be <see langword="null" />.</param>
    /// <param name="enabledTooltip">The tooltip text to display when the toggle is enabled.</param>
    /// <param name="enabledIcon">The icon to display when the toggle is enabled. Must not be <see langword="null" />.</param>
    /// <param name="disabledTooltip">The tooltip text to display when the toggle is disabled.</param>
    /// <param name="disabledIcon">The icon to display when the toggle is disabled. Must not be <see langword="null" />.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="getter" />, <paramref name="setter" />, <paramref name="enabledIcon" />, or
    ///     <paramref
    ///         name="disabledIcon" />
    ///     is <see langword="null" />.
    /// </exception>
    public static void DoIconButtonToggle(Rect rect, [NotNull] Func<bool> getter, [NotNull] Action<bool> setter,
        [CanBeNull] string enabledTooltip, [NotNull] Texture2D enabledIcon, [CanBeNull] string disabledTooltip,
        [NotNull] Texture2D disabledIcon)
    {
        if (getter == null) throw new ArgumentNullException(nameof(getter));
        if (setter == null) throw new ArgumentNullException(nameof(setter));
        if (enabledIcon == null) throw new ArgumentNullException(nameof(enabledIcon));
        if (disabledIcon == null) throw new ArgumentNullException(nameof(disabledIcon));
        var value = getter();
        DoIconButtonToggle(rect, ref value, enabledTooltip, enabledIcon, disabledTooltip, disabledIcon);
        setter(value);
    }

    /// <summary>
    ///     Renders a toggleable icon button within the specified rectangle and updates the toggle state.
    /// </summary>
    /// <remarks>
    ///     The button toggles between two states: enabled and disabled. The appearance and tooltip of
    ///     the button are determined by the current state. Clicking the button switches the state and updates the
    ///     <paramref
    ///         name="value" />
    ///     .
    /// </remarks>
    /// <param name="rect">The screen space rectangle where the button will be drawn.</param>
    /// <param name="value">
    ///     A reference to the current toggle state. Set to <see langword="true" /> for the enabled state or
    ///     <see
    ///         langword="false" />
    ///     for the disabled state. This value will be updated based on user interaction.
    /// </param>
    /// <param name="enabledTooltip">The tooltip text to display when the button is in the enabled state.</param>
    /// <param name="enabledIcon">The icon to display when the button is in the enabled state.</param>
    /// <param name="disabledTooltip">The tooltip text to display when the button is in the disabled state.</param>
    /// <param name="disabledIcon">The icon to display when the button is in the disabled state.</param>
    public static void DoIconButtonToggle(Rect rect, ref bool value, [CanBeNull] string enabledTooltip,
        [NotNull] Texture2D enabledIcon, [CanBeNull] string disabledTooltip, [NotNull] Texture2D disabledIcon)
    {
        if (enabledIcon == null) throw new ArgumentNullException(nameof(enabledIcon));
        if (disabledIcon == null) throw new ArgumentNullException(nameof(disabledIcon));
        var newValue = value;
        var iconButton = value
            ? new IconButton(enabledIcon, () => newValue = false, enabledTooltip)
            : new IconButton(disabledIcon, () => newValue = true, disabledTooltip);
        DoIconButton(rect, iconButton);
        value = newValue;
    }
}