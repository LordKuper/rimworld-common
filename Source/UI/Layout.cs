using JetBrains.Annotations;
using UnityEngine;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides layout utilities for UI elements.
/// </summary>
[PublicAPI]
public static class Layout
{
    /// <summary>
    ///     The gap in pixels between UI elements.
    /// </summary>
    public const float ElementGap = GridSize * 3;

    /// <summary>
    ///     The smaller gap in pixels between UI elements.
    /// </summary>
    public const float ElementGapSmall = GridSize * 2;

    /// <summary>
    ///     The smallest gap in pixels between UI elements.
    /// </summary>
    public const float ElementGapTiny = GridSize;

    /// <summary>
    ///     The base grid size in pixels.
    /// </summary>
    internal const float GridSize = 4f;

    /// <summary>
    ///     The height of a horizontal line.
    /// </summary>
    internal const float HorizontalLineHeight = ElementGap;

    /// <summary>
    ///     Represents the size of indentation, in pixels.
    /// </summary>
    internal const float IndentationSize = GridSize * 4;

    /// <summary>
    ///     The default height in pixels for a row.
    /// </summary>
    public const float RowHeight = GridSize * 8;

    /// <summary>
    ///     The height in pixels for a large row.
    /// </summary>
    public const float RowHeightLarge = GridSize * 10;

    /// <summary>
    ///     Draws two horizontal gap lines and returns the remaining rectangle below the lines.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the lines.</param>
    /// <returns>The remaining rectangle below the lines.</returns>
    internal static Rect DoDoubleGapLineHorizontal(Rect rect)
    {
        var color = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.4f);
        var lineRect = GetTopRowRect(rect, ElementGap, out var remRect);
        Verse.Widgets.DrawLineHorizontal(lineRect.x, lineRect.center.y - 2f, lineRect.width);
        Verse.Widgets.DrawLineHorizontal(lineRect.x, lineRect.center.y + 2f, lineRect.width);
        GUI.color = color;
        return remRect;
    }

    /// <summary>
    ///     Draws a horizontal gap line within the specified rectangle and returns the height of the line.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the horizontal line.</param>
    /// <param name="remRect">
    ///     When this method returns, contains the remaining rectangle below the drawn line.
    /// </param>
    /// <returns>
    ///     The height of the line rectangle that was drawn.
    /// </returns>
    internal static float DoGapLineHorizontal(Rect rect, out Rect remRect)
    {
        var color = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.4f);
        var lineRect = GetTopRowRect(rect, ElementGap, out remRect);
        Verse.Widgets.DrawLineHorizontal(lineRect.x, lineRect.center.y, lineRect.width);
        GUI.color = color;
        return lineRect.height;
    }

    /// <summary>
    ///     Draws a vertical gap line within the specified rectangle and returns the remaining rectangle.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the vertical line.</param>
    /// <param name="remRect">
    ///     When this method returns, contains the remaining rectangle to the right of the drawn line.
    /// </param>
    public static void DoGapLineVertical(Rect rect, out Rect remRect)
    {
        var color = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.4f);
        var lineRect = GetLeftColumnRect(rect, ElementGap, out remRect);
        Verse.Widgets.DrawLineVertical(lineRect.center.x, lineRect.y, lineRect.height);
        GUI.color = color;
    }

    /// <summary>
    ///     Creates a vertical gap within the specified rectangle and returns the top gap-sized rectangle.
    /// </summary>
    /// <param name="rect">The original rectangle from which the top row and remaining area are calculated.</param>
    /// <param name="remRect">When this method returns, contains the remaining rectangle after the top row is extracted.</param>
    /// <returns>A <see cref="Rect" /> representing the top row rectangle, adjusted by the vertical gap.</returns>
    public static Rect DoVerticalGap(Rect rect, out Rect remRect)
    {
        return GetTopRowRect(rect, ElementGap, out remRect);
    }

    /// <summary>
    ///     Creates a small vertical gap within the specified rectangle and returns the top gap-sized rectangle.
    /// </summary>
    /// <param name="rect">The original rectangle from which the top row and remaining area are calculated.</param>
    /// <param name="remRect">When this method returns, contains the remaining rectangle after the top row is extracted.</param>
    /// <returns>A <see cref="Rect" /> representing the top row rectangle, adjusted by the small vertical gap.</returns>
    internal static Rect DoVerticalGapSmall(Rect rect, out Rect remRect)
    {
        return GetTopRowRect(rect, ElementGapSmall, out remRect);
    }

    /// <summary>
    ///     Returns the bottom row rectangle of the specified height from the given rectangle.
    /// </summary>
    /// <param name="rect">The original rectangle.</param>
    /// <param name="rowHeight">The height of the bottom row to extract.</param>
    /// <param name="remRect">The remaining rectangle above the bottom row.</param>
    /// <returns>The rectangle representing the bottom row.</returns>
    public static Rect GetBottomRowRect(Rect rect, float rowHeight, out Rect remRect)
    {
        remRect = rect;
        remRect.yMax -= rowHeight;
        return new Rect(rect.x, rect.yMax - rowHeight, rect.width, rowHeight);
    }

    /// <summary>
    ///     Calculates a rectangle of the specified width and height, centered within the given rectangle.
    /// </summary>
    /// <param name="rect">The rectangle within which the new rectangle will be centered.</param>
    /// <param name="width">
    ///     The width of the new rectangle. Must be less than or equal to the width of <paramref name="rect" />
    ///     .
    /// </param>
    /// <param name="height">
    ///     The height of the new rectangle. Must be less than or equal to the height of
    ///     <paramref name="rect" />.
    /// </param>
    /// <returns>
    ///     A new <see cref="Rect" /> that is centered within <paramref name="rect" /> and has the specified width and height.
    ///     If the specified width or height is greater than the dimensions of <paramref name="rect" />, the original
    ///     rectangle is returned.
    /// </returns>
    public static Rect GetCenteredRect(Rect rect, float width, float height)
    {
        if (rect.width <= width || rect.height <= height)
            return rect;
        var x = (rect.width - width) * 0.5f;
        var y = (rect.height - height) * 0.5f;
        return new Rect(rect.x + x, rect.y + y, width, height);
    }

    /// <summary>
    ///     Calculates and returns the centered row rectangle within the given rectangle, splitting the remaining space
    ///     into top and bottom rectangles.
    /// </summary>
    /// <param name="rect">The source rectangle from which the centered row and remaining areas are calculated.</param>
    /// <param name="rowHeight">The height of the centered row to extract. Must be a positive value.</param>
    /// <param name="topRect">When this method returns, contains the rectangle representing the area above the centered row.</param>
    /// <param name="bottomRect">When this method returns, contains the rectangle representing the area below the centered row.</param>
    /// <returns>
    ///     A <see cref="Rect" /> representing the centered row within the given rectangle. If the height of the source
    ///     rectangle is less than or equal to <paramref name="rowHeight" />, the entire rectangle is returned, and both
    ///     <paramref name="topRect" /> and <paramref name="bottomRect" /> are set to <see cref="Rect.zero" />.
    /// </returns>
    public static Rect GetCenterRowRect(Rect rect, float rowHeight, out Rect topRect, out Rect bottomRect)
    {
        if (rect.height <= rowHeight)
        {
            topRect = bottomRect = Rect.zero;
            return rect;
        }
        var offset = (rect.height - rowHeight) / 2f;
        topRect = GetTopRowRect(rect, offset, out var remRect);
        var rowRect = GetTopRowRect(remRect, rowHeight, out bottomRect);
        return rowRect;
    }

    [NotNull]
    internal static Rect[] GetGridRects(Rect rect, float minColumnWidth, float columnGap, float rowHeight, float rowGap,
        int cellCount, out float gridHeight, out Rect remRect)
    {
        var columnCount = Mathf.FloorToInt(rect.width / (minColumnWidth + columnGap));
        if (cellCount < columnCount)
            columnCount = cellCount;
        if (columnCount < 1) columnCount = 1;
        var rowCount = cellCount / columnCount + (cellCount % columnCount > 0 ? 1 : 0);
        var columnWidth = (rect.width - columnGap * (columnCount - 1)) / columnCount;
        gridHeight = rowHeight * rowCount + rowGap * (rowCount - 1);
        GetTopRowRect(rect, gridHeight, out remRect);
        var rects = new Rect[cellCount];
        for (var i = 0; i < cellCount; i++)
        {
            var row = i / columnCount;
            var column = i % columnCount;
            rects[i] = new Rect(rect.x + column * (columnWidth + columnGap), rect.y + row * (rowHeight + rowGap),
                columnWidth, rowHeight);
        }
        return rects;
    }

    /// <summary>
    ///     Returns the left column rectangle of the specified width from the given rectangle.
    /// </summary>
    /// <param name="rect">The original rectangle.</param>
    /// <param name="columnWidth">The width of the left column to extract.</param>
    /// <param name="remRect">The remaining rectangle to the right of the left column.</param>
    /// <returns>The rectangle representing the left column.</returns>
    public static Rect GetLeftColumnRect(Rect rect, float columnWidth, out Rect remRect)
    {
        remRect = rect;
        remRect.xMin += columnWidth;
        return new Rect(rect.x, rect.y, columnWidth, rect.height);
    }

    /// <summary>
    ///     Returns the right column rectangle of the specified width from the given rectangle.
    /// </summary>
    /// <param name="rect">The original rectangle.</param>
    /// <param name="columnWidth">The width of the right column to extract.</param>
    /// <param name="remRect">The remaining rectangle to the left of the right column.</param>
    /// <returns>The rectangle representing the right column.</returns>
    public static Rect GetRightColumnRect(Rect rect, float columnWidth, out Rect remRect)
    {
        remRect = rect;
        remRect.xMax -= columnWidth;
        return new Rect(rect.xMax - columnWidth, rect.y, columnWidth, rect.height);
    }

    /// <summary>
    ///     Returns the top row rectangle of the specified height from the given rectangle.
    /// </summary>
    /// <param name="rect">The original rectangle.</param>
    /// <param name="rowHeight">The height of the top row to extract.</param>
    /// <param name="remRect">The remaining rectangle below the top row.</param>
    /// <returns>The rectangle representing the top row.</returns>
    public static Rect GetTopRowRect(Rect rect, float rowHeight, out Rect remRect)
    {
        remRect = rect;
        remRect.yMin += rowHeight;
        return new Rect(rect.x, rect.y, rect.width, rowHeight);
    }
}