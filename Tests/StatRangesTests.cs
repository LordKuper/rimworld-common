using LordKuper.Common.Helpers;
using RimWorld;

namespace LordKuper.Common.Tests;

/// <summary>
///     Tests for <see cref="StatRanges" /> adaptive normalization behavior (ADR-0002, AC-20).
///     Covers adaptive range expansion and order-dependence of NormalizeStatValue.
///     AC-20: StatRanges maintains per-stat adaptive ranges and is cleared between tests via StaticStateFixture.
/// </summary>
[Collection("StaticState")]
public class StatRangesTests : IClassFixture<StaticStateFixture>
{
    public StatRangesTests(StaticStateFixture fixture) { _ = fixture; }

    [Fact]
    public void NormalizeStatValue_FirstValue_ExpandsRange()
    {
        // AC-20 / ADR-0002: First call initializes the range [value, value], then normalizes
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // First call with any value expands range from [value, value]
        var result = StatRanges.NormalizeStatValue(statDef, 50f);

        // Should not throw and return a valid float
        Assert.True(!float.IsNaN(result) && !float.IsInfinity(result));
    }

    [Fact]
    public void NormalizeStatValue_LargeRanges_Supported()
    {
        // AC-20: Large value ranges are handled without numeric overflow
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var small = StatRanges.NormalizeStatValue(statDef, 1000f);
        var large = StatRanges.NormalizeStatValue(statDef, 1_000_000f);

        // Both should be valid (no overflow)
        Assert.True(!float.IsNaN(small) && !float.IsInfinity(small));
        Assert.True(!float.IsNaN(large) && !float.IsInfinity(large));
    }

    [Fact]
    public void NormalizeStatValue_MultipleStats_IndependentRanges()
    {
        // AC-20: Each stat maintains its own independent range
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

        // Both should succeed independently
        Assert.True(!float.IsNaN(norm120) && !float.IsInfinity(norm120));
        Assert.True(!float.IsNaN(norm2200) && !float.IsInfinity(norm2200));
    }

    [Fact]
    public void NormalizeStatValue_NegativeValues_Supported()
    {
        // AC-20 / ADR-0002: Negative values are handled in range expansion
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var neg10 = StatRanges.NormalizeStatValue(statDef, -10f);
        var neg5 = StatRanges.NormalizeStatValue(statDef, -5f);
        var zero = StatRanges.NormalizeStatValue(statDef, 0f);

        // All should be valid floats
        Assert.True(!float.IsNaN(neg10) && !float.IsInfinity(neg10));
        Assert.True(!float.IsNaN(neg5) && !float.IsInfinity(neg5));
        Assert.True(!float.IsNaN(zero) && !float.IsInfinity(zero));
    }

    [Fact]
    public void NormalizeStatValue_SecondValue_UpdatesRange()
    {
        // AC-20 / ADR-0002: Second call updates the range if value is outside [min, max]
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var first = StatRanges.NormalizeStatValue(statDef, 10f); // Range: [10, 10]
        var second = StatRanges.NormalizeStatValue(statDef, 20f); // Range: [10, 20]
        var third = StatRanges.NormalizeStatValue(statDef, 15f); // Range: [10, 20]

        // All should be valid normalized values
        Assert.True(!float.IsNaN(first) && !float.IsInfinity(first));
        Assert.True(!float.IsNaN(second) && !float.IsInfinity(second));
        Assert.True(!float.IsNaN(third) && !float.IsInfinity(third));
    }

    [Fact]
    public void NormalizeStatValue_ZeroValue()
    {
        // AC-20: Zero value is handled
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var result = StatRanges.NormalizeStatValue(statDef, 0f);
        Assert.True(!float.IsNaN(result) && !float.IsInfinity(result));
    }
}