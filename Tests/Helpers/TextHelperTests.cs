using System.Text;
using LordKuper.Common.Helpers;
using Xunit;

namespace LordKuper.Common.Tests.Helpers;

/// <summary>
///     Tests for <see cref="TextHelper" /> indentation helpers (AC-17).
/// </summary>
public class TextHelperTests
{
    [Fact]
    public void AppendIndented_WithZeroIndentation_AppendsTextWithoutSpaces()
    {
        // AC-17: Zero indentation level adds no spaces
        var sb = new StringBuilder();

        sb.AppendIndented("Hello", 0);

        Assert.Equal("Hello", sb.ToString());
    }

    [Fact]
    public void AppendIndented_WithPositiveIndentation_AppendsSpacesAndText()
    {
        // AC-17: Positive indentation adds 2*level spaces before text
        var sb = new StringBuilder();

        sb.AppendIndented("Hello", 1);

        Assert.Equal("  Hello", sb.ToString()); // 2 spaces for level 1
    }

    [Fact]
    public void AppendIndented_WithMultipleIndentationLevels()
    {
        // AC-17: Indentation scales with level
        var sb = new StringBuilder();

        sb.AppendIndented("Hello", 3);

        Assert.Equal("      Hello", sb.ToString()); // 6 spaces for level 3
    }

    [Fact]
    public void AppendIndented_NegativeIndentation_Ignores()
    {
        // AC-17: Negative indentation is treated as zero (no special handling)
        var sb = new StringBuilder();

        sb.AppendIndented("Hello", -1);

        Assert.Equal("Hello", sb.ToString()); // No spaces for negative level
    }

    [Fact]
    public void AppendIndented_NullStringBuilder_Throws()
    {
        // AC-17: Null StringBuilder throws ArgumentNullException
        StringBuilder? sb = null;

        Assert.Throws<ArgumentNullException>(() => sb!.AppendIndented("Hello", 0));
    }

    [Fact]
    public void AppendIndented_NullText_Throws()
    {
        // AC-17: Null or empty text throws ArgumentNullException
        var sb = new StringBuilder();

        Assert.Throws<ArgumentNullException>(() => sb.AppendIndented(null!, 0));
        Assert.Throws<ArgumentNullException>(() => sb.AppendIndented("", 0));
    }

    [Fact]
    public void AppendIndented_EmptyText_Throws()
    {
        // AC-17: Empty text throws ArgumentNullException
        var sb = new StringBuilder();

        Assert.Throws<ArgumentNullException>(() => sb.AppendIndented("", 0));
    }

    [Fact]
    public void AppendLineIndented_WithZeroIndentation_AppendsTextAndNewline()
    {
        // AC-17: Zero indentation adds no spaces, but still adds newline
        var sb = new StringBuilder();

        sb.AppendLineIndented("Hello", 0);

        Assert.Equal("Hello\r\n", sb.ToString());
    }

    [Fact]
    public void AppendLineIndented_WithPositiveIndentation_AppendsSpacesTextAndNewline()
    {
        // AC-17: Positive indentation adds spaces, text, and newline
        var sb = new StringBuilder();

        sb.AppendLineIndented("Hello", 1);

        Assert.Equal("  Hello\r\n", sb.ToString()); // 2 spaces for level 1, then text, then newline
    }

    [Fact]
    public void AppendLineIndented_WithMultipleIndentationLevels()
    {
        // AC-17: Line indentation scales with level
        var sb = new StringBuilder();

        sb.AppendLineIndented("Hello", 2);

        Assert.Equal("    Hello\r\n", sb.ToString()); // 4 spaces for level 2
    }

    [Fact]
    public void AppendLineIndented_NegativeIndentation_Ignores()
    {
        // AC-17: Negative indentation is treated as zero
        var sb = new StringBuilder();

        sb.AppendLineIndented("Hello", -1);

        Assert.Equal("Hello\r\n", sb.ToString()); // No spaces, but newline is added
    }

    [Fact]
    public void AppendLineIndented_NullStringBuilder_Throws()
    {
        // AC-17: Null StringBuilder throws ArgumentNullException
        StringBuilder? sb = null;

        Assert.Throws<ArgumentNullException>(() => sb!.AppendLineIndented("Hello", 0));
    }

    [Fact]
    public void AppendLineIndented_NullText_Throws()
    {
        // AC-17: Null text throws ArgumentNullException
        var sb = new StringBuilder();

        Assert.Throws<ArgumentNullException>(() => sb.AppendLineIndented(null!, 0));
    }

    [Fact]
    public void AppendLineIndented_EmptyText_Throws()
    {
        // AC-17: Empty text throws ArgumentNullException
        var sb = new StringBuilder();

        Assert.Throws<ArgumentNullException>(() => sb.AppendLineIndented("", 0));
    }

    [Fact]
    public void AppendIndented_MultipleLines_BuildsUp()
    {
        // AC-17: Multiple calls build up indented content
        var sb = new StringBuilder();

        sb.AppendLineIndented("Line 1", 0);
        sb.AppendLineIndented("Line 2", 1);
        sb.AppendLineIndented("Line 3", 2);

        var result = sb.ToString();
        Assert.Contains("Line 1\r\n", result);
        Assert.Contains("  Line 2\r\n", result);
        Assert.Contains("    Line 3\r\n", result);
    }

    [Fact]
    public void AppendIndented_LargeIndentationLevel()
    {
        // AC-17: Large indentation levels work correctly
        var sb = new StringBuilder();

        sb.AppendIndented("Text", 10);

        // 10 levels * 2 spaces per level = 20 spaces
        Assert.Equal(new string(' ', 20) + "Text", sb.ToString());
    }

    [Fact]
    public void AppendLineIndented_LargeIndentationLevel()
    {
        // AC-17: Large indentation levels work with line indents
        var sb = new StringBuilder();

        sb.AppendLineIndented("Text", 10);

        // 10 levels * 2 spaces per level = 20 spaces
        Assert.Equal(new string(' ', 20) + "Text\r\n", sb.ToString());
    }
}
