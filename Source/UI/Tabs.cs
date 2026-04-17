using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods for tabbed UI elements.
/// </summary>
[UsedImplicitly]
public static class Tabs
{
    /// <summary>
    ///     Draws a tabbed UI section with optional top, scrollable, and bottom content regions.
    /// </summary>
    /// <param name="tabRect">
    ///     The rectangle representing the area of the tab.
    /// </param>
    /// <param name="topFixedHeight">
    ///     The height of the top fixed region. Used if <paramref name="doTopContentAction" /> is not null.
    /// </param>
    /// <param name="doTopContentAction">
    ///     Delegate to draw the top fixed region. Should return the height used for the top region.
    ///     If null, the top region is omitted.
    /// </param>
    /// <param name="scrollableContentHeight">
    ///     The height of the scrollable content. Updated by the scroll view to reflect the actual content height.
    /// </param>
    /// <param name="scrollPosition">
    ///     The current scroll position within the scrollable region. Updated by the scroll view.
    /// </param>
    /// <param name="doScrollableContentAction">
    ///     Delegate to draw the scrollable region. Should return the height used for the scrollable content.
    ///     If null, the scrollable region is omitted.
    /// </param>
    /// <param name="bottomFixedHeight">
    ///     The height of the bottom fixed region. Used if <paramref name="doBottomContentAction" /> is not null.
    /// </param>
    /// <param name="doBottomContentAction">
    ///     Delegate to draw the bottom fixed region. Should return the height used for the bottom region.
    ///     If null, the bottom region is omitted.
    /// </param>
    [UsedImplicitly]
    public static void DoTab(Rect tabRect, float topFixedHeight,
        [CanBeNull] Action<Rect> doTopContentAction, float scrollableContentHeight,
        ref Vector2 scrollPosition, [CanBeNull] Action<Rect> doScrollableContentAction,
        float bottomFixedHeight, [CanBeNull] Action<Rect> doBottomContentAction)
    {
        GetTabRects(tabRect, doTopContentAction == null ? 0f : topFixedHeight,
            doBottomContentAction == null ? 0f : bottomFixedHeight, out var topFixedRect,
            out var scrollableRect, out var bottomFixedRect);
        doTopContentAction?.Invoke(topFixedRect);
        if (doScrollableContentAction != null)
            ScrollView.DoScrollableContent(scrollableRect, scrollableContentHeight,
                ref scrollPosition, doScrollableContentAction);
        doBottomContentAction?.Invoke(bottomFixedRect);
    }

    /// <summary>
    ///     Draws a set of tabs within the specified container and returns the rectangle of the active tab area.
    /// </summary>
    /// <param name="container">The rectangle that defines the area for the tabs.</param>
    /// <param name="tabs">The list of <see cref="TabRecord" /> objects representing the tabs to draw.</param>
    /// <returns>
    ///     The rectangle representing the area of the active tab.
    /// </returns>
    [UsedImplicitly]
    public static Rect DoTabs(Rect container, List<TabRecord> tabs)
    {
        var tabDrawerRect = container;
        tabDrawerRect.yMin += 32f;
        _ = TabDrawer.DrawTabs(tabDrawerRect, tabs, 512f);
        var activeTabRect = tabDrawerRect.ContractedBy(12f);
        activeTabRect.xMax += 8f;
        return activeTabRect;
    }

    /// <summary>
    ///     Calculates the rectangles for the top fixed region, scrollable region, and bottom fixed region
    ///     within a tabbed UI area.
    /// </summary>
    /// <param name="tabRect">
    ///     The full rectangle representing the tab area.
    /// </param>
    /// <param name="topFixedHeight">
    ///     The height of the top fixed region. If zero or less, the top region is omitted.
    /// </param>
    /// <param name="bottomFixedHeight">
    ///     The height of the bottom fixed region. If zero or less, the bottom region is omitted.
    /// </param>
    /// <param name="topFixedRect">
    ///     Output parameter. The rectangle representing the top fixed region. If omitted, set to <see cref="Rect.zero" />.
    /// </param>
    /// <param name="scrollableRect">
    ///     Output parameter. The rectangle representing the scrollable region.
    /// </param>
    /// <param name="bottomFixedRect">
    ///     Output parameter. The rectangle representing the bottom fixed region. If omitted, set to <see cref="Rect.zero" />.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if the combined height of the fixed regions exceeds the available tab area.
    /// </exception>
    [UsedImplicitly]
    public static void GetTabRects(Rect tabRect, float topFixedHeight, float bottomFixedHeight,
        out Rect topFixedRect, out Rect scrollableRect, out Rect bottomFixedRect)
    {
        if (topFixedHeight + bottomFixedHeight > tabRect.height - 100f)
            throw new ArgumentOutOfRangeException("Height of fixed regions is too large.",
                (Exception)null);
        var remRect = tabRect;
        if (topFixedHeight > 0)
        {
            topFixedRect = Layout.GetTopRowRect(remRect, topFixedHeight, out remRect);
            remRect = Layout.DoDoubleGapLineHorizontal(remRect);
        }
        else
        {
            topFixedRect = Rect.zero;
        }
        if (bottomFixedHeight > 0)
        {
            bottomFixedRect = Layout.GetBottomRowRect(remRect,
                bottomFixedHeight + Layout.HorizontalLineHeight, out remRect);
            bottomFixedRect = Layout.DoDoubleGapLineHorizontal(bottomFixedRect);
            scrollableRect = remRect;
        }
        else
        {
            scrollableRect = remRect;
            bottomFixedRect = Rect.zero;
        }
    }
}