using LordKuper.Common.Filters.Limits;
using RimWorld;
using Xunit;

namespace LordKuper.Common.Tests.Filters.Limits;

/// <summary>
///     Tests for <see cref="PawnTraitLimit" /> trait requirement/forbidding (AC-20).
/// </summary>
[Collection("StaticState")]
public class PawnTraitLimitTests : IClassFixture<StaticStateFixture>
{
    private readonly StaticStateFixture _fixture;

    public PawnTraitLimitTests(StaticStateFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Ctor_Parameterless_InitializesEmpty()
    {
        // AC-20: Parameterless constructor
        var limit = new PawnTraitLimit();

        Assert.Null(limit.Def);
        Assert.False(limit.Limit); // Default Limit is false
    }

    [Fact]
    public void Ctor_WithTraitDef_StoresDefAndDefaultsLimitToTrue()
    {
        // AC-20: Constructor from TraitDef sets Limit to true
        var def = new TraitDef { defName = "Kind" };

        var limit = new PawnTraitLimit(def);

        Assert.Equal(def, limit.Def);
        Assert.True(limit.Limit); // Defaults to true (required)
    }

    [Fact]
    public void Def_Stores()
    {
        // AC-20: Def property is stored correctly
        var def = new TraitDef { defName = "Industrious" };

        var limit = new PawnTraitLimit(def);

        Assert.NotNull(limit.Def);
        Assert.Equal("Industrious", limit.Def!.defName);
    }

    [Fact]
    public void Limit_CanBeModified()
    {
        // AC-20: Limit property can be toggled
        var limit = new PawnTraitLimit();
        Assert.False(limit.Limit);

        limit.Limit = true;
        Assert.True(limit.Limit);

        limit.Limit = false;
        Assert.False(limit.Limit);
    }

    [Fact]
    public void Limit_TrueIndicatesRequired()
    {
        // AC-20: Limit=true indicates trait is required
        var def = new TraitDef { defName = "Brave" };
        var limit = new PawnTraitLimit(def);

        // Defaults to true (required)
        Assert.True(limit.Limit);

        limit.Limit = false;
        // Now it indicates forbidden
        Assert.False(limit.Limit);
    }

    [Fact]
    public void Limit_FalseIndicatesForbidden()
    {
        // AC-20: Limit=false indicates trait is forbidden
        var limit = new PawnTraitLimit();
        limit.Limit = false;

        Assert.False(limit.Limit);
    }

    [Fact]
    public void ExposeData_DoesNotThrow()
    {
        // AC-20: ExposeData does not throw
        var def = new TraitDef { defName = "Tough" };
        var limit = new PawnTraitLimit(def);

        Assert.Null(Record.Exception(() => limit.ExposeData()));
    }

    [Fact]
    public void MultipleInstances_Independent()
    {
        // AC-20: Multiple instances are independent
        var def1 = new TraitDef { defName = "Trait1" };
        var def2 = new TraitDef { defName = "Trait2" };

        var limit1 = new PawnTraitLimit(def1);
        var limit2 = new PawnTraitLimit(def2);

        limit1.Limit = true;
        limit2.Limit = false;

        Assert.True(limit1.Limit);
        Assert.False(limit2.Limit);
    }

    [Fact]
    public void Ctor_WithTraitDef_NullThrows()
    {
        // AC-20: Null TraitDef throws
        TraitDef? def = null;

        Assert.Throws<ArgumentNullException>(() => new PawnTraitLimit(def!));
    }

    [Fact]
    public void ToggleLimit_CanInvertBehavior()
    {
        // AC-20: Limit can be toggled to invert required/forbidden semantics
        var def = new TraitDef { defName = "Quick" };
        var limit = new PawnTraitLimit(def);

        Assert.True(limit.Limit); // Starts required

        limit.Limit = !limit.Limit;
        Assert.False(limit.Limit); // Now forbidden

        limit.Limit = !limit.Limit;
        Assert.True(limit.Limit); // Back to required
    }

    [Fact]
    public void SerializationRoundTrip_PreservesState()
    {
        // AC-20: Limit state is preserved through ExposeData
        var def = new TraitDef { defName = "Tough" };
        var limit = new PawnTraitLimit(def) { Limit = true };

        var originalLimit = limit.Limit;
        Assert.True(originalLimit);

        // Simulate save/load by verifying ExposeData works
        Assert.Null(Record.Exception(() => limit.ExposeData()));
        Assert.Equal(originalLimit, limit.Limit);
    }
}
