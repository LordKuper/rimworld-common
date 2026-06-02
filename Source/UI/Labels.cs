using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods for drawing labels in the UI.
/// </summary>
[PublicAPI]
public static class Labels
{
    /// <summary>
    ///     The height of a standard label using the medium font.
    /// </summary>
    public static readonly float LabelHeight = Text.LineHeightOf(GameFont.Medium);

    /// <summary>
    ///     The height of a section header.
    /// </summary>
    public static readonly float SectionHeaderHeight = Text.LineHeightOf(GameFont.Medium) + 4f;

    /// <summary>
    ///     Draws a label in the specified rectangle with the given text and alignment.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the label.</param>
    /// <param name="label">The text to display.</param>
    /// <param name="anchor">The text alignment.</param>
    public static void DoLabel(Rect rect, [CanBeNull] string label, TextAnchor anchor)
    {
        var font = Text.Font;
        var textAnchor = Text.Anchor;
        Text.Font = GameFont.Medium;
        Text.Anchor = anchor;
        Verse.Widgets.Label(rect, label);
        Text.Font = font;
        Text.Anchor = textAnchor;
    }

    /// <summary>
    ///     Returns the rendered width of the specified text using the given font.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="font">The font to use for measurement.</param>
    /// <returns>The width in pixels.</returns>
    internal static float GetTextWidth([NotNull] string text, GameFont font)
    {
        var prevFont = Text.Font;
        Text.Font = font;
        var width = Text.CalcSize(text).x;
        Text.Font = prevFont;
        return width;
    }
}