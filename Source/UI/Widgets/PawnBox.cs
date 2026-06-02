using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LordKuper.Common.UI.Widgets;

/// <summary>
///     Provides UI functionality for displaying a scrollable box of pawn entries.
/// </summary>
[PublicAPI]
public static class PawnBox
{
    /// <summary>
    ///     Minimum width for each pawn entry in the grid.
    /// </summary>
    private const float EntryWidthMin = Layout.GridSize * 30;

    /// <summary>
    ///     Represents the background color used for rendering a thing icon box.
    /// </summary>
    private static readonly Color BackgroundColor = new(1f, 1f, 1f, 0.05f);

    /// <summary>
    ///     Represents the color used for outlining a thing icon box.
    /// </summary>
    private static readonly Color OutlineColor = new(1f, 1f, 1f, 0.4f);

    /// <summary>
    ///     The size in pixels of each thing icon.
    /// </summary>
    private static readonly float RowHeight = Text.LineHeightOf(GameFont.Small);

    /// <summary>
    ///     Draws a scrollable box containing a list of pawns, each as a clickable entry.
    /// </summary>
    /// <param name="rect">The rectangle area in which to draw the pawn box.</param>
    /// <param name="scrollPosition">Reference to the current scroll position.</param>
    /// <param name="pawns">The list of pawns to display.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawns" /> is null.</exception>
    public static void DoPawnBox(Rect rect, ref Vector2 scrollPosition, IReadOnlyList<Pawn> pawns)
    {
        if (pawns == null) throw new ArgumentNullException(nameof(pawns));
        var font = Text.Font;
        var anchor = Text.Anchor;
        var color = GUI.color;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        const float elementGapTiny = Layout.ElementGapTiny;
        var horizontalMargin = GUI.skin.verticalScrollbar.fixedWidth + elementGapTiny * 2;
        Verse.Widgets.DrawBoxSolidWithOutline(rect, BackgroundColor, OutlineColor);
        var outRect = new Rect(rect.x + elementGapTiny, rect.y + elementGapTiny, rect.width - elementGapTiny * 1.5f,
            rect.height - elementGapTiny * 2);
        var gridRect = new Rect(outRect.x, outRect.y, rect.width - horizontalMargin, outRect.height);
        var entryRects = Layout.GetGridRects(gridRect, EntryWidthMin, elementGapTiny, RowHeight, elementGapTiny,
            pawns.Count, out var gridHeight, out _);
        var boxRect = new Rect(gridRect.x, gridRect.y, gridRect.width, gridHeight);
        Verse.Widgets.BeginScrollView(outRect, ref scrollPosition, boxRect);
        var evt = Event.current;
        var isMouseDown = evt.type == EventType.MouseDown && evt.button == 0;
        var mousePos = evt.mousePosition;
        for (var i = 0; i < pawns.Count; i++)
        {
            var pawn = pawns[i];
            var entryRect = entryRects[i];
            var mouseOver = entryRect.Contains(mousePos);
            GUI.color = mouseOver ? GenUI.MouseoverColor : Color.white;
            _ = Verse.Widgets.LabelFit(entryRect, pawn.LabelShortCap);
            MouseoverSounds.DoRegion(entryRect);
            TooltipHandler.TipRegion(entryRect, pawn.NameFullColored);
            if (isMouseDown && mouseOver) Find.WindowStack.Add(new Dialog_InfoCard(pawn));
        }
        Verse.Widgets.EndScrollView();
        Text.Font = font;
        Text.Anchor = anchor;
        GUI.color = color;
    }

    /// <summary>
    ///     Calculates the total height required to display a given number of pawn rows in the box.
    /// </summary>
    /// <param name="rowCount">The number of rows to display.</param>
    /// <returns>The total height in pixels for the pawn box.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="rowCount" /> is less than or equal to zero.</exception>
    public static float GetPawnBoxHeight(int rowCount)
    {
        if (rowCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(rowCount), "Row count must be a positive number.");
        return rowCount * RowHeight + (rowCount + 1) * Layout.ElementGapTiny;
    }
}