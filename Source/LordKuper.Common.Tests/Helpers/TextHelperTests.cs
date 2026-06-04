using System.Text;
using LordKuper.Common.Helpers;

namespace LordKuper.Common.Tests.Helpers;

/// <summary>
///     Tests for <see cref="TextHelper" /> indentation helpers.
/// </summary>
public class TextHelperTests
{
    [Test]
    public void AppendIndented_EmptyText_Throws()
    {
        // Empty text throws ArgumentNullException
        var sb = new StringBuilder();
        var act = () => sb.AppendIndented("", 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AppendIndented_LargeIndentationLevel()
    {
        // Large indentation levels work correctly
        var sb = new StringBuilder();
        sb.AppendIndented("Text", 10);

        // 10 levels * 2 spaces per level = 20 spaces
        sb.ToString().Should().Be(new string(' ', 20) + "Text");
    }

    [Test]
    public void AppendIndented_MultipleLines_BuildsUp()
    {
        // Multiple calls build up indented content
        var sb = new StringBuilder();
        sb.AppendLineIndented("Line 1", 0);
        sb.AppendLineIndented("Line 2", 1);
        sb.AppendLineIndented("Line 3", 2);
        var result = sb.ToString();
        result.Should().Contain("Line 1\r\n");
        result.Should().Contain("  Line 2\r\n");
        result.Should().Contain("    Line 3\r\n");
    }

    [Test]
    public void AppendIndented_NegativeIndentation_Ignores()
    {
        // Negative indentation is treated as zero (no special handling)
        var sb = new StringBuilder();
        sb.AppendIndented("Hello", -1);
        sb.ToString().Should().Be("Hello"); // No spaces for negative level
    }

    [Test]
    public void AppendIndented_NullStringBuilder_Throws()
    {
        // Null StringBuilder throws ArgumentNullException
        StringBuilder? sb = null;
        var act = () => sb!.AppendIndented("Hello", 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AppendIndented_NullText_Throws()
    {
        // Null or empty text throws ArgumentNullException
        var sb = new StringBuilder();
        var actNull = () => sb.AppendIndented(null!, 0);
        actNull.Should().Throw<ArgumentNullException>();
        var actEmpty = () => sb.AppendIndented("", 0);
        actEmpty.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AppendIndented_WithMultipleIndentationLevels()
    {
        // Indentation scales with level
        var sb = new StringBuilder();
        sb.AppendIndented("Hello", 3);
        sb.ToString().Should().Be("      Hello"); // 6 spaces for level 3
    }

    [Test]
    public void AppendIndented_WithPositiveIndentation_AppendsSpacesAndText()
    {
        // Positive indentation adds 2*level spaces before text
        var sb = new StringBuilder();
        sb.AppendIndented("Hello", 1);
        sb.ToString().Should().Be("  Hello"); // 2 spaces for level 1
    }

    [Test]
    public void AppendIndented_WithZeroIndentation_AppendsTextWithoutSpaces()
    {
        // Zero indentation level adds no spaces
        var sb = new StringBuilder();
        sb.AppendIndented("Hello", 0);
        sb.ToString().Should().Be("Hello");
    }

    [Test]
    public void AppendLineIndented_EmptyText_Throws()
    {
        // Empty text throws ArgumentNullException
        var sb = new StringBuilder();
        var act = () => sb.AppendLineIndented("", 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AppendLineIndented_LargeIndentationLevel()
    {
        // Large indentation levels work with line indents
        var sb = new StringBuilder();
        sb.AppendLineIndented("Text", 10);

        // 10 levels * 2 spaces per level = 20 spaces
        sb.ToString().Should().Be(new string(' ', 20) + "Text\r\n");
    }

    [Test]
    public void AppendLineIndented_NegativeIndentation_Ignores()
    {
        // Negative indentation is treated as zero
        var sb = new StringBuilder();
        sb.AppendLineIndented("Hello", -1);
        sb.ToString().Should().Be("Hello\r\n"); // No spaces, but newline is added
    }

    [Test]
    public void AppendLineIndented_NullStringBuilder_Throws()
    {
        // Null StringBuilder throws ArgumentNullException
        StringBuilder? sb = null;
        var act = () => sb!.AppendLineIndented("Hello", 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AppendLineIndented_NullText_Throws()
    {
        // Null text throws ArgumentNullException
        var sb = new StringBuilder();
        var act = () => sb.AppendLineIndented(null!, 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AppendLineIndented_WithMultipleIndentationLevels()
    {
        // Line indentation scales with level
        var sb = new StringBuilder();
        sb.AppendLineIndented("Hello", 2);
        sb.ToString().Should().Be("    Hello\r\n"); // 4 spaces for level 2
    }

    [Test]
    public void AppendLineIndented_WithPositiveIndentation_AppendsSpacesTextAndNewline()
    {
        // Positive indentation adds spaces, text, and newline
        var sb = new StringBuilder();
        sb.AppendLineIndented("Hello", 1);
        sb.ToString().Should().Be("  Hello\r\n"); // 2 spaces for level 1, then text, then newline
    }

    [Test]
    public void AppendLineIndented_WithZeroIndentation_AppendsTextAndNewline()
    {
        // Zero indentation adds no spaces, but still adds newline
        var sb = new StringBuilder();
        sb.AppendLineIndented("Hello", 0);
        sb.ToString().Should().Be("Hello\r\n");
    }
}