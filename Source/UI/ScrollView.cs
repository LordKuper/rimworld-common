using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods for rendering scrollable UI sections.
/// </summary>
[UsedImplicitly]
public class ScrollView
{
    /// <summary>
    ///     Renders a scrollable section within the specified container rectangle.
    /// </summary>
    /// <param name="containerRect">
    ///     The rectangle that defines the bounds of the scrollable area.
    /// </param>
    /// <param name="contentHeight">
    ///     The height of the content to be displayed within the scrollable area.
    /// </param>
    /// <param name="scrollPosition">
    ///     Reference to the current scroll position. This value is updated as the user scrolls.
    /// </param>
    /// <param name="doContentAction">
    ///     A delegate that renders the content.
    /// </param>
    internal static void DoScrollableContent(Rect containerRect, float contentHeight,
        ref Vector2 scrollPosition, [NotNull] Action<Rect> doContentAction)
    {
        var scrollViewRect = new Rect(containerRect.x, containerRect.y,
            containerRect.width - GUI.skin.verticalScrollbar.fixedWidth - 4f, contentHeight);
        Verse.Widgets.BeginScrollView(containerRect, ref scrollPosition, scrollViewRect);
        doContentAction(scrollViewRect);
        Verse.Widgets.EndScrollView();
    }
}