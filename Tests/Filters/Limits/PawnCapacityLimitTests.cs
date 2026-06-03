using LordKuper.Common;
using LordKuper.Common.Filters.Limits;
using Verse;
using Xunit;

namespace LordKuper.Common.Tests.Filters.Limits;

/// <summary>
///     Tests for <see cref="PawnCapacityLimit" /> capacity range constraints (AC-20).
/// </summary>
[Collection("StaticState")]
public class PawnCapacityLimitTests : IClassFixture<StaticStateFixture>
{
    private readonly StaticStateFixture _fixture;

    public PawnCapacityLimitTests(StaticStateFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Ctor_Parameterless_InitializesEmpty()
    {
        // AC-20: Parameterless constructor
        var limit = new PawnCapacityLimit();

        Assert.Null(limit.PawnCapacityDef);
        Assert.Null(limit.PawnCapacityDefName);
    }

    [Fact]
    public void Ctor_WithDefName_StoresName()
    {
        // AC-20: Constructor with def name
        var limit = new PawnCapacityLimit("Sight");

        Assert.Equal("Sight", limit.PawnCapacityDefName);
    }

    [Fact]
    public void Ctor_WithDefNameAndValues_StoresAll()
    {
        // AC-20: Constructor with def name and bounds
        var limit = new PawnCapacityLimit("Sight", 0.5f, 1.5f);

        Assert.Equal("Sight", limit.PawnCapacityDefName);
        Assert.Equal(0.5f, limit.MinValue);
        Assert.Equal(1.5f, limit.MaxValue);
    }

    [Fact]
    public void Ctor_WithPawnCapacityDef_StoresName()
    {
        // AC-20: Constructor from PawnCapacityDef
        var def = new PawnCapacityDef { defName = "Sight" };

        var limit = new PawnCapacityLimit(def);

        Assert.Equal("Sight", limit.PawnCapacityDefName);
    }

    [Fact]
    public void MinValue_SetAndGet_ClampsToBounds()
    {
        // AC-20: MinValue is clamped between 0 and 5
        var limit = new PawnCapacityLimit("Sight");

        limit.MinValue = 2.0f;
        Assert.Equal(2.0f, limit.MinValue);

        // Below min cap
        limit.MinValue = -1f;
        Assert.Equal(0f, limit.MinValue); // Clamped to LimitMinCap (0)

        // Above max cap
        limit.MinValue = 10f;
        Assert.Equal(5f, limit.MinValue); // Clamped to LimitMaxCap (5)
    }

    [Fact]
    public void MaxValue_SetAndGet_ClampsToBounds()
    {
        // AC-20: MaxValue is clamped between 0 and 5
        var limit = new PawnCapacityLimit("Sight");

        limit.MaxValue = 3.0f;
        Assert.Equal(3.0f, limit.MaxValue);

        // Below min cap
        limit.MaxValue = -1f;
        Assert.Equal(0f, limit.MaxValue); // Clamped to LimitMinCap

        // Above max cap
        limit.MaxValue = 10f;
        Assert.Equal(5f, limit.MaxValue); // Clamped to LimitMaxCap (5)
    }

    [Fact]
    public void MinValue_Null_ResetsToMinCap()
    {
        // AC-20: Setting MinValue to null resets it to LimitMinCap (0)
        var limit = new PawnCapacityLimit("Sight");

        limit.MinValue = 2.0f;
        Assert.Equal(2.0f, limit.MinValue);

        limit.MinValue = null;
        Assert.Null(limit.MinValue); // Null when at LimitMinCap
    }

    [Fact]
    public void MaxValue_Null_ResetsToMaxCap()
    {
        // AC-20: Setting MaxValue to null resets it to LimitMaxCap (5)
        var limit = new PawnCapacityLimit("Sight");

        limit.MaxValue = 2.0f;
        Assert.Equal(2.0f, limit.MaxValue);

        limit.MaxValue = null;
        Assert.Null(limit.MaxValue); // Null when at LimitMaxCap
    }

    [Fact]
    public void MinValueBuffer_ParsesValidFloat()
    {
        // AC-20: MinValueBuffer parses valid float
        var limit = new PawnCapacityLimit("Sight");

        limit.MinValueBuffer = "1.5";
        Assert.Equal(1.5f, limit.MinValue);
    }

    [Fact]
    public void MaxValueBuffer_ParsesValidFloat()
    {
        // AC-20: MaxValueBuffer parses valid float
        var limit = new PawnCapacityLimit("Sight");

        limit.MaxValueBuffer = "3.5";
        Assert.Equal(3.5f, limit.MaxValue);
    }

    [Fact]
    public void MinValueBuffer_InvalidText_DropsInvalid()
    {
        // AC-20: Invalid text causes buffer assignment to fail, so old value remains
        var limit = new PawnCapacityLimit("Sight", 0.5f, null);
        var oldValue = limit.MinValue;

        limit.MinValueBuffer = "invalid";
        // Invalid text is rejected and buffer may not retain the text
        // (behavior depends on implementation - just verify it doesn't change MinValue)
        Assert.Equal(oldValue, limit.MinValue);
    }

    [Fact]
    public void MaxValueBuffer_InvalidText_DropsInvalid()
    {
        // AC-20: Invalid text causes buffer assignment to fail, old value retained
        var limit = new PawnCapacityLimit("Sight", null, 3.0f);
        var oldValue = limit.MaxValue;

        limit.MaxValueBuffer = "invalid";
        // Invalid text is rejected
        Assert.Equal(oldValue, limit.MaxValue);
    }

    [Fact]
    public void Limit_FloatRange_StoresRange()
    {
        // AC-20: Limit field stores the FloatRange
        var limit = new PawnCapacityLimit("Sight");

        limit.MinValue = 1.0f;
        limit.MaxValue = 4.0f;

        Assert.Equal(1.0f, limit.Limit.min);
        Assert.Equal(4.0f, limit.Limit.max);
    }

    [Fact]
    public void LimitCaps_AreCorrect()
    {
        // AC-20: Capacity limit caps are correct
        Assert.Equal(0f, PawnCapacityLimit.LimitMinCap);
        Assert.Equal(5f, PawnCapacityLimit.LimitMaxCap);
    }

    [Fact]
    public void ValueStyle_IsPercentZero()
    {
        // AC-20: Capacity value style is PercentZero
        Assert.Equal(ToStringStyle.PercentZero, PawnCapacityLimit.ValueStyle);
    }

    [Fact]
    public void ExposeData_DoesNotThrow()
    {
        // AC-20: ExposeData does not throw
        var limit = new PawnCapacityLimit("Sight", 0.5f, 1.5f);

        Assert.Null(Record.Exception(() => limit.ExposeData()));
    }

    [Fact]
    public void MultipleInstances_Independent()
    {
        // AC-20: Multiple instances are independent
        var limit1 = new PawnCapacityLimit("Sight", 1.0f, 2.0f);
        var limit2 = new PawnCapacityLimit("Hearing", 0.5f, 1.5f);

        Assert.Equal("Sight", limit1.PawnCapacityDefName);
        Assert.Equal("Hearing", limit2.PawnCapacityDefName);
        Assert.Equal(1.0f, limit1.MinValue);
        Assert.Equal(0.5f, limit2.MinValue);

        limit1.MinValue = 0.0f;
        Assert.Equal(0.5f, limit2.MinValue); // Unchanged
    }
}
