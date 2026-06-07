using LordKuper.Common.Helpers;
using RimWorld;

namespace LordKuper.Common.Tests;

/// <summary>
///     Tests for <see cref="StatRanges" /> adaptive normalization behavior.
///     Covers adaptive range expansion and order-dependence of NormalizeStatValue.
///     StatRanges maintains per-stat adaptive ranges and is cleared between tests via StaticStateTestBase's
///     [SetUp]/[TearDown].
/// </summary>
[NonParallelizable]
public class StatRangesTests : StaticStateTestBase
{
    [Test]
    public void NormalizeStatValue_FirstValue_ExpandsRange()
    {
        // First call initializes the range [value, value], then normalizes
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // First call with any value expands range from [value, value]
        var result = StatRanges.NormalizeStatValue(statDef, 50f);

        // Should not throw and return a valid float (preserve original compound check as value form)
        (!float.IsNaN(result) && !float.IsInfinity(result)).Should().BeTrue();
    }

    [Test]
    public void NormalizeStatValue_LargeRanges_Supported()
    {
        // Large value ranges are handled without numeric overflow
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var small = StatRanges.NormalizeStatValue(statDef, 1000f);
        var large = StatRanges.NormalizeStatValue(statDef, 1_000_000f);

        // Both should be valid (no overflow); compound validity check preserved as boolean form
        (!float.IsNaN(small) && !float.IsInfinity(small)).Should().BeTrue();
        (!float.IsNaN(large) && !float.IsInfinity(large)).Should().BeTrue();
    }

    [Test]
    public void NormalizeStatValue_MultipleStats_IndependentRanges()
    {
        // Each stat maintains its own independent range
        var fakeProvider = new FakeDefProvider();
        var statDef1 = new StatDef { defName = "Stat1", label = "Stat 1", category = null };
        var statDef2 = new StatDef { defName = "Stat2", label = "Stat 2", category = null };
        fakeProvider.AddDef(statDef1);
        fakeProvider.AddDef(statDef2);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // Normalize some values for stat1
        StatRanges.NormalizeStatValue(statDef1, 10f);
        var norm120 = StatRanges.NormalizeStatValue(statDef1, 20f);

        // Normalize some values for stat2
        StatRanges.NormalizeStatValue(statDef2, 100f);
        var norm2200 = StatRanges.NormalizeStatValue(statDef2, 200f);

        // Both should succeed independently; compound validity check preserved as boolean form
        (!float.IsNaN(norm120) && !float.IsInfinity(norm120)).Should().BeTrue();
        (!float.IsNaN(norm2200) && !float.IsInfinity(norm2200)).Should().BeTrue();
    }

    [Test]
    public void NormalizeStatValue_NegativeValues_Supported()
    {
        // Negative values are handled in range expansion
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var neg10 = StatRanges.NormalizeStatValue(statDef, -10f);
        var neg5 = StatRanges.NormalizeStatValue(statDef, -5f);
        var zero = StatRanges.NormalizeStatValue(statDef, 0f);

        // All should be valid floats; compound validity check preserved as boolean form
        (!float.IsNaN(neg10) && !float.IsInfinity(neg10)).Should().BeTrue();
        (!float.IsNaN(neg5) && !float.IsInfinity(neg5)).Should().BeTrue();
        (!float.IsNaN(zero) && !float.IsInfinity(zero)).Should().BeTrue();
    }

    [Test]
    public void NormalizeStatValue_SecondValue_UpdatesRange()
    {
        // Second call updates the range if value is outside [min, max]
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var first = StatRanges.NormalizeStatValue(statDef, 10f); // Range: [10, 10]
        var second = StatRanges.NormalizeStatValue(statDef, 20f); // Range: [10, 20]
        var third = StatRanges.NormalizeStatValue(statDef, 15f); // Range: [10, 20]

        // All should be valid normalized values; compound validity check preserved as boolean form
        (!float.IsNaN(first) && !float.IsInfinity(first)).Should().BeTrue();
        (!float.IsNaN(second) && !float.IsInfinity(second)).Should().BeTrue();
        (!float.IsNaN(third) && !float.IsInfinity(third)).Should().BeTrue();
    }

    [Test]
    public void NormalizeStatValue_ZeroValue()
    {
        // Zero value is handled
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var result = StatRanges.NormalizeStatValue(statDef, 0f);
        // compound validity check preserved as boolean form
        (!float.IsNaN(result) && !float.IsInfinity(result)).Should().BeTrue();
    }

    [Test]
    public void NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange()
    {
        // AC-8: Regression test — verifies the fix: first positive observation seeds [v, v],
        // not the buggy [0, v]. Must pass on the fixed code and fail if UpdateStatRange
        // is reverted to `range = new FloatRange(0, value)`.
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // First observation of 50: range becomes [50, 50] (degenerate).
        // NormalizeValue on a degenerate range returns 0 (no width).
        var firstResult = StatRanges.NormalizeStatValue(statDef, 50f);
        firstResult.Should().Be(0f);

        // Expand the range: 50 → 100 expands to [50, 100].
        // NormalizeValue(50, [50, 100]) = (50 - 50) / (100 - 50) = 0.
        var afterSecond = StatRanges.NormalizeStatValue(statDef, 100f);
        StatRanges.NormalizeStatValue(statDef, 50f).Should().Be(0f);

        // NormalizeValue(100, [50, 100]) = (100 - 50) / (100 - 50) = 1.
        afterSecond.Should().Be(1f);
    }

    [Test]
    public void NormalizeStatValue_NegativeSequence_RangeExpansion()
    {
        // AC-2, AC-7: Exact-bound test for negative range expansion.
        // Observe -10, then -5; verify range updates from degenerate to [-10, -5].
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "NegStat", label = "Neg Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // First observation: -10 seeds [-10, -10] (degenerate).
        // NormalizeValue(-10, [-10, -10]) = 0 (zero width).
        var first = StatRanges.NormalizeStatValue(statDef, -10f);
        first.Should().Be(0f);

        // Second observation: -5 expands to [-10, -5].
        // NormalizeValue(-5, [-10, -5]):
        // normalizedValue = (-5 - (-10)) / (-5 - (-10)) = 5 / 5 = 1.
        // min < 0, max < 0 => -1 + 1 = 0.
        var second = StatRanges.NormalizeStatValue(statDef, -5f);
        second.Should().Be(0f);

        // Third observation: 1 expands to [-10, 1] (truly mixed).
        // NormalizeValue(1, [-10, 1]):
        // normalizedValue = (1 - (-10)) / (1 - (-10)) = 11 / 11 = 1.
        // min < 0, max > 0 => -1 + 2 * 1 = 1.
        var third = StatRanges.NormalizeStatValue(statDef, 1f);
        third.Should().Be(1f);

        // Verify -10 maps correctly in [-10, 1]:
        // NormalizeValue(-10, [-10, 1]):
        // normalizedValue = (-10 - (-10)) / (1 - (-10)) = 0 / 11 = 0.
        // min < 0, max > 0 => -1 + 2 * 0 = -1.
        var neg10Final = StatRanges.NormalizeStatValue(statDef, -10f);
        neg10Final.Should().Be(-1f);
    }

    [Test]
    public void NormalizeStatValue_PositiveSequence_ExactBounds()
    {
        // AC-2, AC-7: Exact-bound test for positive sequence.
        // Observe 50, then 100; verify degenerate initial range and expansion to [50, 100].
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // First observation: 50 seeds [50, 50] (degenerate).
        // NormalizeValue(50, [50, 50]) = 0 (zero width).
        var first = StatRanges.NormalizeStatValue(statDef, 50f);
        first.Should().Be(0f);

        // Second observation: 100 expands to [50, 100].
        // NormalizeValue(100, [50, 100]) = (100 - 50) / (100 - 50) = 1.
        var second = StatRanges.NormalizeStatValue(statDef, 100f);
        second.Should().Be(1f);

        // Verify 50 still maps to 0 after expansion:
        // NormalizeValue(50, [50, 100]) = (50 - 50) / (100 - 50) = 0.
        var fifty = StatRanges.NormalizeStatValue(statDef, 50f);
        fifty.Should().Be(0f);

        // Verify 100 still maps to 1:
        var hundred = StatRanges.NormalizeStatValue(statDef, 100f);
        hundred.Should().Be(1f);
    }

    [Test]
    public void NormalizeStatValue_NegativeSequenceToZero_ExactBounds()
    {
        // AC-2, AC-7: Exact-bound test for the exact AC-2 negative sequence.
        // Observe -10, then -5, then 0; verify range expansion from [-10, -10] to [-10, -5] to [-10, 0].
        // Verify exact normalized values: -10 → 0 and 0 → 1 when range is [-10, 0].
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "NegToZeroStat", label = "Neg to Zero Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // First observation: -10 seeds [-10, -10] (degenerate).
        // NormalizeValue(-10, [-10, -10]) = 0 (zero width).
        var first = StatRanges.NormalizeStatValue(statDef, -10f);
        first.Should().Be(0f);

        // Second observation: -5 expands to [-10, -5].
        // NormalizeValue(-5, [-10, -5]):
        // normalizedValue = (-5 - (-10)) / (-5 - (-10)) = 5 / 5 = 1.
        // min < 0, max < 0 => -1 + 1 = 0.
        var second = StatRanges.NormalizeStatValue(statDef, -5f);
        second.Should().Be(0f);

        // Third observation: 0 expands to [-10, 0].
        // NormalizeValue(0, [-10, 0]):
        // normalizedValue = (0 - (-10)) / (0 - (-10)) = 10 / 10 = 1.
        // min < 0, max = 0 (not > 0) => default case => normalizedValue = 1.
        var third = StatRanges.NormalizeStatValue(statDef, 0f);
        third.Should().Be(1f);

        // Verify endpoints in the final range [-10, 0].
        // Re-observe -10 and 0 (they are at the current min/max, so calling them does not expand the range).
        // NormalizeValue(-10, [-10, 0]) = (-10 - (-10)) / (0 - (-10)) = 0 / 10 = 0.
        // min < 0, max = 0 (not > 0) => default case => 0.
        var endpointMin = StatRanges.NormalizeStatValue(statDef, -10f);
        endpointMin.Should().Be(0f);

        // NormalizeValue(0, [-10, 0]) = (0 - (-10)) / (0 - (-10)) = 10 / 10 = 1.
        // min < 0, max = 0 (not > 0) => default case => 1.
        var endpointMax = StatRanges.NormalizeStatValue(statDef, 0f);
        endpointMax.Should().Be(1f);
    }
}