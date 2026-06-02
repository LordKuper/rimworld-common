using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods for drawing icons in the UI.
/// </summary>
internal static class Icons
{
    /// <summary>
    ///     The default size (in pixels) for info icons.
    /// </summary>
    internal const float InfoIconSize = Layout.GridSize * 4;

    /// <summary>
    ///     Draws an icon texture within the specified rectangle and optionally displays a tooltip.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the icon.</param>
    /// <param name="icon">The texture to draw as the icon.</param>
    /// <param name="tooltip">An optional tooltip to display when hovering over the icon.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="icon" /> is null.</exception>
    internal static void DoIcon(Rect rect, [NotNull] Texture2D icon, [CanBeNull] string tooltip = null)
    {
        if (icon == null) throw new ArgumentNullException(nameof(icon));
        Verse.Widgets.DrawTextureFitted(rect, icon, 1f);
        if (!string.IsNullOrEmpty(tooltip)) TooltipHandler.TipRegion(rect, tooltip);
    }
}