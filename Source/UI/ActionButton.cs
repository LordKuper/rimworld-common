using System;
using JetBrains.Annotations;

namespace LordKuper.Common.UI;

/// <summary>
///     Represents a button with an associated action, label, tooltip, and enabled state.
/// </summary>
[PublicAPI]
public struct ActionButton
{
    /// <summary>
    ///     Gets the action to execute when the button is clicked.
    /// </summary>
    internal Action Action { get; }

    /// <summary>
    ///     Gets the label displayed on the button.
    /// </summary>
    internal string Label { get; }

    /// <summary>
    ///     Gets a value indicating whether the button is enabled.
    /// </summary>
    internal bool IsEnabled { get; }

    /// <summary>
    ///     Gets the tooltip to display for the button.
    /// </summary>
    internal string Tooltip { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ActionButton" /> struct.
    /// </summary>
    /// <param name="label">The label displayed on the button. Cannot be null or empty.</param>
    /// <param name="action">The action to execute when the button is clicked. Cannot be null.</param>
    /// <param name="tooltip">The tooltip to display for the button. Can be null.</param>
    /// <param name="isEnabled">A value indicating whether the button is enabled. Defaults to <c>true</c>.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="label" /> is null or empty, or if <paramref name="action" /> is null.
    /// </exception>
    public ActionButton([NotNull] string label, [NotNull] Action action, [CanBeNull] string tooltip = null,
        bool isEnabled = true)
    {
        if (string.IsNullOrEmpty(label)) throw new ArgumentNullException(nameof(label));
        if (action == null) throw new ArgumentNullException(nameof(action));
        Label = label;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Tooltip = tooltip;
        IsEnabled = isEnabled;
    }
}