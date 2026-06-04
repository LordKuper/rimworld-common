using LordKuper.Common.Helpers;
using Verse;

namespace LordKuper.Common.Tests.Helpers;

/// <summary>
///     Tests for <see cref="MathHelper" /> pure paths (normalization, clamping).
/// </summary>
public class MathHelperTests
{
    [Test]
    public void NormalizeValue_MaxValue_ReturnsOne()
    {
        var range = new FloatRange(0f, 100f);
        var result = MathHelper.NormalizeValue(100f, range);
        result.Should().Be(1f);
    }

    [Test]
    public void NormalizeValue_MinValue_ReturnsZero()
    {
        var range = new FloatRange(0f, 100f);
        var result = MathHelper.NormalizeValue(0f, range);
        result.Should().Be(0f);
    }

    [Test]
    public void NormalizeValue_MixedRange_NegativeValue_ReturnsNegative()
    {
        var range = new FloatRange(-50f, 50f);
        var result = MathHelper.NormalizeValue(-25f, range);
        // normalized = (-25 - (-50)) / 100 = 25 / 100 = 0.25
        // then adjust: -1 + 2*0.25 = -0.5
        result.Should().Be(-0.5f);
    }

    [Test]
    public void NormalizeValue_MixedRange_PositiveValue_ReturnsPositive()
    {
        var range = new FloatRange(-50f, 50f);
        var result = MathHelper.NormalizeValue(25f, range);
        // normalized = (25 - (-50)) / 100 = 75 / 100 = 0.75
        // then adjust: -1 + 2*0.75 = 0.5
        result.Should().Be(0.5f);
    }

    [Test]
    public void NormalizeValue_MixedRange_ReturnsNormalizedValue()
    {
        // Range spans negative and positive: [-50, 50]
        var range = new FloatRange(-50f, 50f);
        var result = MathHelper.NormalizeValue(0f, range);
        // normalized = (0 - (-50)) / 100 = 50 / 100 = 0.5
        // then adjust: -1 + 2*0.5 = 0
        result.Should().Be(0f);
    }

    [Test]
    public void NormalizeValue_NegativeRange_MaxValue_ReturnsZero()
    {
        var range = new FloatRange(-100f, -10f);
        var result = MathHelper.NormalizeValue(-10f, range);
        result.Should().Be(0f);
    }

    [Test]
    public void NormalizeValue_NegativeRange_MinValue_ReturnsMinusOne()
    {
        var range = new FloatRange(-100f, -10f);
        var result = MathHelper.NormalizeValue(-100f, range);
        result.Should().Be(-1f);
    }

    [Test]
    public void NormalizeValue_NegativeRange_ReturnsNormalizedValue()
    {
        // Both min and max are negative: range [-100, -10]
        var range = new FloatRange(-100f, -10f);
        var result = MathHelper.NormalizeValue(-55f, range);
        // normalized = (value - min) / range = (-55 - (-100)) / 90 = 45 / 90 = 0.5
        // then adjust: -1 + 0.5 = -0.5
        result.Should().Be(-0.5f);
    }

    [Test]
    public void NormalizeValue_PositiveRange_ReturnsNormalizedValue()
    {
        var range = new FloatRange(0f, 100f);
        var result = MathHelper.NormalizeValue(50f, range);
        result.Should().Be(0.5f);
    }

    // [TestCase] only — no standalone [Test] on this parameterized method.
    // xUnit precision:4 rounds to 4 decimal places (band ±0.5e-4); faithful equivalent is 5e-5f.
    [TestCase(10f, 10f, 20f, 0f)]
    [TestCase(15f, 10f, 20f, 0.5f)]
    [TestCase(20f, 10f, 20f, 1f)]
    [TestCase(5f, 10f, 20f, 0f)] // clamped below
    [TestCase(25f, 10f, 20f, 1f)] // clamped above
    public void NormalizeValue_Theory(float value, float min, float max, float expected)
    {
        var range = new FloatRange(min, max);
        var result = MathHelper.NormalizeValue(value, range);
        // Faithful equivalent of xUnit precision:4 (rounds to 4 decimal places, band ±0.5e-4)
        result.Should().BeApproximately(expected, 5e-5f);
    }

    [Test]
    public void NormalizeValue_ValueAboveRange_ClampsAndNormalizes()
    {
        var range = new FloatRange(10f, 20f);
        var result = MathHelper.NormalizeValue(50f, range);
        result.Should().Be(1f); // clamped to 20, normalized to 1
    }

    [Test]
    public void NormalizeValue_ValueBelowRange_ClampsAndNormalizes()
    {
        var range = new FloatRange(10f, 20f);
        var result = MathHelper.NormalizeValue(-5f, range);
        result.Should().Be(0f); // clamped to 10, normalized to 0
    }

    [Test]
    public void NormalizeValue_ZeroRange_ReturnsZero()
    {
        // When min == max, range is near-zero and should return 0 regardless of value
        var range = new FloatRange(5f, 5.0001f);
        var result = MathHelper.NormalizeValue(5f, range);
        result.Should().Be(0f);
    }
}