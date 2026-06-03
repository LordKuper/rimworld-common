using LordKuper.Common;
using LordKuper.Common.Filters.Limits;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;
using Xunit;

namespace LordKuper.Common.Tests.Filters.Limits;

/// <summary>
///     Tests for <see cref="StatLimit" /> range constraints and value clamping (AC-20).
/// </summary>
[Collection("StaticState")]
public class StatLimitTests : IClassFixture<StaticStateFixture>
{
    private readonly StaticStateFixture _fixture;

    public StatLimitTests(StaticStateFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Ctor_Parameterless_InitializesEmpty()
    {
        // AC-20: Parameterless constructor for serialization
        var limit = new StatLimit();

        Assert.Null(limit.StatDef);
        Assert.Null(limit.StatDefName);
    }

    [Fact]
    public void Ctor_WithDefName_StoresName()
    {
        // AC-20: Constructor with def name stores the name
        var limit = new StatLimit("TestStat");

        Assert.Equal("TestStat", limit.StatDefName);
    }

    [Fact]
    public void Ctor_WithDefNameAndValues_StoresAll()
    {
        // AC-20: Constructor with def name and value bounds
        var limit = new StatLimit("TestStat", 10f, 50f);

        Assert.Equal("TestStat", limit.StatDefName);
        Assert.Equal(10f, limit.MinValue);
        Assert.Equal(50f, limit.MaxValue);
    }

    [Fact]
    public void Ctor_WithStatDef_ConfiguresFromDef()
    {
        // AC-20: Constructor from StatDef configures limits
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);

        Assert.Equal("TestStat", limit.StatDefName);
        Assert.Equal(0f, limit.LimitMinCap);
        Assert.Equal(100f, limit.LimitMaxCap);
    }

    [Fact]
    public void MinValue_SetAndGet_ClampsToBounds()
    {
        // AC-20: MinValue is clamped to the limit caps
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);

        // Set within bounds
        limit.MinValue = 25f;
        Assert.Equal(25f, limit.MinValue);

        // Set below bounds - should clamp
        limit.MinValue = -50f;
        Assert.Equal(0f, limit.MinValue); // Clamped to LimitMinCap
    }

    [Fact]
    public void MaxValue_SetAndGet_ClampsToBounds()
    {
        // AC-20: MaxValue is clamped to the limit caps
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);

        // Set within bounds
        limit.MaxValue = 75f;
        Assert.Equal(75f, limit.MaxValue);

        // Set above bounds - should clamp
        limit.MaxValue = 200f;
        Assert.Equal(100f, limit.MaxValue); // Clamped to LimitMaxCap
    }

    [Fact]
    public void MinValue_Null_ResetsToMinCap()
    {
        // AC-20: Setting MinValue to null resets it to LimitMinCap
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);
        limit.MinValue = 50f;
        Assert.Equal(50f, limit.MinValue);

        // Reset to null
        limit.MinValue = null;
        Assert.Null(limit.MinValue); // Returns null when at LimitMinCap
    }

    [Fact]
    public void MaxValue_Null_ResetsToMaxCap()
    {
        // AC-20: Setting MaxValue to null resets it to LimitMaxCap
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);
        limit.MaxValue = 50f;
        Assert.Equal(50f, limit.MaxValue);

        // Reset to null
        limit.MaxValue = null;
        Assert.Null(limit.MaxValue); // Returns null when at LimitMaxCap
    }

    [Fact]
    public void MinValueBuffer_ParsesValidFloat()
    {
        // AC-20: MinValueBuffer parses valid float input
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);

        limit.MinValueBuffer = "35.5";
        Assert.Equal(35.5f, limit.MinValue ?? 0f, precision: 1);
    }

    [Fact]
    public void MaxValueBuffer_ParsesValidFloat()
    {
        // AC-20: MaxValueBuffer parses valid float input
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);

        limit.MaxValueBuffer = "65.75";
        Assert.Equal(65.75f, limit.MaxValue ?? 0f, precision: 1);
    }

    [Fact]
    public void MinValueBuffer_InvalidText_RetainsText()
    {
        // AC-20: Invalid text is retained in buffer
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);
        var oldValue = limit.MinValue;

        limit.MinValueBuffer = "not a number";
        Assert.Equal(oldValue, limit.MinValue); // Value unchanged
        Assert.Equal("not a number", limit.MinValueBuffer); // Text retained
    }

    [Fact]
    public void Limit_FloatRange_Stores()
    {
        // AC-20: The Limit field stores the FloatRange
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit(statDef);
        limit.MinValue = 25f;
        limit.MaxValue = 75f;

        Assert.Equal(25f, limit.Limit.min);
        Assert.Equal(75f, limit.Limit.max);
    }

    [Fact]
    public void StatDef_LazyInitialization()
    {
        // AC-20: StatDef is lazily initialized from name
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            minValue = 0f,
            maxValue = 100f,
            toStringStyle = ToStringStyle.FloatTwo,
            category = null
        };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var limit = new StatLimit("TestStat");
        var resolved = limit.StatDef;

        Assert.NotNull(resolved);
        Assert.Equal("TestStat", resolved!.defName);
    }

    [Fact]
    public void ExposeData_DoesNotThrow()
    {
        // AC-20: ExposeData can be called without throwing
        var limit = new StatLimit("TestStat", 10f, 50f);

        // Should not throw
        Assert.Null(Record.Exception(() => limit.ExposeData()));
    }
}
