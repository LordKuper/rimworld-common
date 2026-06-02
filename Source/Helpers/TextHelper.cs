using System;
using System.Text;
using JetBrains.Annotations;
using Verse;

namespace LordKuper.Common.Helpers;

/// <summary>
///     Provides helper methods for appending indented text to a <see cref="StringBuilder" />.
/// </summary>
[PublicAPI]
public static class TextHelper
{
    /// <summary>
    ///     The number of spaces per indentation level.
    /// </summary>
    private const int IndentSize = 2;

    /// <summary>
    ///     Appends spaces to the <see cref="StringBuilder" /> according to the specified indentation level.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder" /> to append to.</param>
    /// <param name="indentationLevel">The indentation level (number of indents).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sb" /> is <c>null</c>.</exception>
    private static void AppendIndentation(this StringBuilder sb, int indentationLevel)
    {
        if (sb == null) throw new ArgumentNullException(nameof(sb));
        if (indentationLevel <= 0) return;
        sb.Append(' ', IndentSize * indentationLevel);
    }

    /// <summary>
    ///     Appends the specified text to the <see cref="StringBuilder" />, preceded by indentation.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder" /> to append to.</param>
    /// <param name="text">The text to append.</param>
    /// <param name="indentationLevel">The indentation level (number of indents).</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="sb" /> is <c>null</c> or <paramref name="text" /> is <c>null</c> or empty.
    /// </exception>
    public static void AppendIndented(this StringBuilder sb, string text, int indentationLevel)
    {
        if (sb == null) throw new ArgumentNullException(nameof(sb));
        if (text.NullOrEmpty()) throw new ArgumentNullException(nameof(text));
        sb.AppendIndentation(indentationLevel);
        sb.Append(text);
    }

    /// <summary>
    ///     Appends the specified text followed by a line terminator to the <see cref="StringBuilder" />, preceded by
    ///     indentation.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder" /> to append to.</param>
    /// <param name="text">The text to append.</param>
    /// <param name="indentationLevel">The indentation level (number of indents).</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="sb" /> is <c>null</c> or <paramref name="text" /> is <c>null</c> or empty.
    /// </exception>
    public static void AppendLineIndented(this StringBuilder sb, string text, int indentationLevel)
    {
        if (sb == null) throw new ArgumentNullException(nameof(sb));
        if (text.NullOrEmpty()) throw new ArgumentNullException(nameof(text));
        sb.AppendIndentation(indentationLevel);
        sb.AppendLine(text);
    }
}