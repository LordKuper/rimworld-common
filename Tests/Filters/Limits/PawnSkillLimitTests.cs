using LordKuper.Common.Filters.Limits;
using RimWorld;
using Verse;
using Xunit;

namespace LordKuper.Common.Tests.Filters.Limits;

/// <summary>
///     Tests for <see cref="PawnSkillLimit" /> skill level range constraints (AC-20).
/// </summary>
[Collection("StaticState")]
public class PawnSkillLimitTests : IClassFixture<StaticStateFixture>
{
    private readonly StaticStateFixture _fixture;

    public PawnSkillLimitTests(StaticStateFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Ctor_Parameterless_InitializesEmpty()
    {
        // AC-20: Parameterless constructor
        var limit = new PawnSkillLimit();

        Assert.Null(limit.SkillDef);
        Assert.Null(limit.SkillDefName);
    }

    [Fact]
    public void Ctor_WithDefName_StoresName()
    {
        // AC-20: Constructor with def name
        var limit = new PawnSkillLimit("Shooting");

        Assert.Equal("Shooting", limit.SkillDefName);
    }

    [Fact]
    public void Ctor_WithDefNameAndValues_StoresAll()
    {
        // AC-20: Constructor with def name and bounds
        var limit = new PawnSkillLimit("Shooting", 5f, 15f);

        Assert.Equal("Shooting", limit.SkillDefName);
        Assert.Equal(5, limit.MinValue);
        Assert.Equal(15, limit.MaxValue);
    }

    [Fact]
    public void MinValue_SetAndGet_ClampsToBounds()
    {
        // AC-20: MinValue is clamped between 0 and 20
        var limit = new PawnSkillLimit("Shooting");

        limit.MinValue = 10;
        Assert.Equal(10, limit.MinValue);

        // Below min cap
        limit.MinValue = -5;
        Assert.Equal(0, limit.MinValue); // Clamped to LimitMinCap (0)

        // Above max cap
        limit.MinValue = 100;
        Assert.Equal(20, limit.MinValue); // Clamped to LimitMaxCap (20)
    }

    [Fact]
    public void MaxValue_SetAndGet_ClampsToBounds()
    {
        // AC-20: MaxValue is clamped between 0 and 20
        var limit = new PawnSkillLimit("Shooting");

        limit.MaxValue = 15;
        Assert.Equal(15, limit.MaxValue);

        // Below min cap
        limit.MaxValue = -5;
        Assert.Equal(0, limit.MaxValue); // Clamped to LimitMinCap

        // Above max cap
        limit.MaxValue = 100;
        Assert.Equal(20, limit.MaxValue); // Clamped to LimitMaxCap (20)
    }

    [Fact]
    public void MinValue_Null_ResetsToMinCap()
    {
        // AC-20: Setting MinValue to null resets it to LimitMinCap (0)
        var limit = new PawnSkillLimit("Shooting");

        limit.MinValue = 10;
        Assert.Equal(10, limit.MinValue);

        limit.MinValue = null;
        Assert.Null(limit.MinValue); // Null when at LimitMinCap
    }

    [Fact]
    public void MaxValue_Null_ResetsToMaxCap()
    {
        // AC-20: Setting MaxValue to null resets it to LimitMaxCap (20)
        var limit = new PawnSkillLimit("Shooting");

        limit.MaxValue = 15;
        Assert.Equal(15, limit.MaxValue);

        limit.MaxValue = null;
        Assert.Null(limit.MaxValue); // Null when at LimitMaxCap
    }

    [Fact]
    public void MinValueBuffer_ParsesValidInt()
    {
        // AC-20: MinValueBuffer parses valid integer
        var limit = new PawnSkillLimit("Shooting");

        limit.MinValueBuffer = "8";
        Assert.Equal(8, limit.MinValue);
    }

    [Fact]
    public void MaxValueBuffer_ParsesValidInt()
    {
        // AC-20: MaxValueBuffer parses valid integer
        var limit = new PawnSkillLimit("Shooting");

        limit.MaxValueBuffer = "12";
        Assert.Equal(12, limit.MaxValue);
    }

    [Fact]
    public void MinValueBuffer_RoundsFloatInput()
    {
        // AC-20: MinValueBuffer rounds float input to int
        var limit = new PawnSkillLimit("Shooting");

        limit.MinValueBuffer = "7.6";
        // Should round to 8
        Assert.Equal(8, limit.MinValue);
    }

    [Fact]
    public void MaxValueBuffer_RoundsFloatInput()
    {
        // AC-20: MaxValueBuffer rounds float input to int
        var limit = new PawnSkillLimit("Shooting");

        limit.MaxValueBuffer = "14.3";
        // Should round to 14
        Assert.Equal(14, limit.MaxValue);
    }

    [Fact]
    public void Limit_IntRange_Stores()
    {
        // AC-20: The Limit field stores the IntRange
        var limit = new PawnSkillLimit("Shooting");
        limit.MinValue = 5;
        limit.MaxValue = 18;

        Assert.Equal(5, limit.Limit.min);
        Assert.Equal(18, limit.Limit.max);
    }

    [Fact]
    public void LimitCaps_AreCorrect()
    {
        // AC-20: Skill limit caps are correct (0-20)
        Assert.Equal(0, PawnSkillLimit.LimitMinCap);
        Assert.Equal(20, PawnSkillLimit.LimitMaxCap);
    }

    [Fact]
    public void ValueStep_IsOne()
    {
        // AC-20: Skill value step is 1
        Assert.Equal(1, PawnSkillLimit.ValueStep);
    }

    [Fact]
    public void ExposeData_DoesNotThrow()
    {
        // AC-20: ExposeData does not throw
        var limit = new PawnSkillLimit("Shooting", 5, 15);

        Assert.Null(Record.Exception(() => limit.ExposeData()));
    }

    [Fact]
    public void MultipleInstances_Independent()
    {
        // AC-20: Multiple instances are independent
        var limit1 = new PawnSkillLimit("Shooting", 5, 10);
        var limit2 = new PawnSkillLimit("Melee", 8, 15);

        Assert.Equal("Shooting", limit1.SkillDefName);
        Assert.Equal("Melee", limit2.SkillDefName);
        Assert.Equal(5, limit1.MinValue);
        Assert.Equal(8, limit2.MinValue);

        limit1.MinValue = 0;
        Assert.Equal(8, limit2.MinValue); // Unchanged
    }

    [Fact]
    public void MinMaxBoundary_MinEqualsMax()
    {
        // AC-20: Min and max can be equal
        var limit = new PawnSkillLimit("Shooting", 12, 12);

        Assert.Equal(12, limit.MinValue);
        Assert.Equal(12, limit.MaxValue);
    }

    [Fact]
    public void DefaultLimit_IsFullRange()
    {
        // AC-20: Default Limit is [0, 20]
        var limit = new PawnSkillLimit("Shooting");

        Assert.Equal(0, limit.Limit.min);
        Assert.Equal(20, limit.Limit.max);
    }
}
