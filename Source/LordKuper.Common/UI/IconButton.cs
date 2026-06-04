using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LordKuper.Common.UI;

/// <summary>
///     Represents an icon button to be displayed next to a field label.
/// </summary>
[PublicAPI]
public struct IconButton
{
    /// <summary>
    ///     Gets the icon to display on the button.
    /// </summary>
    internal Texture2D? Icon { get; }

    /// <summary>
    ///     Gets the action to execute when the button is clicked.
    /// </summary>
    internal Action Action { get; }

    /// <summary>
    ///     Gets a value indicating whether the button is enabled.
    /// </summary>
    internal bool IsEnabled { get; }

    /// <summary>
    ///     Gets the tooltip to display for the button.
    /// </summary>
    internal string? Tooltip { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconButton" /> struct.
    /// </summary>
    /// <param name="icon">The icon to display on the button.</param>
    /// <param name="action">The action to execute when the button is clicked.</param>
    /// <param name="tooltip">The tooltip to display for the button.</param>
    /// <param name="isEnabled">Whether the button is enabled.</param>
    public IconButton(Texture2D? icon, Action action, string? tooltip = null, bool isEnabled = true)
    {
        Icon = icon;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Tooltip = tooltip;
        IsEnabled = isEnabled;
    }
}