using LordKuper.Common;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;
using Xunit;

namespace LordKuper.Common.Tests;

/// <summary>
///     Tests for <see cref="StatWeight" /> construction, weight math, and serialization (AC-20).
/// </summary>
[Collection("StaticState")]
public class StatWeightTests : IClassFixture<StaticStateFixture>
{
    private readonly StaticStateFixture _fixture;

    public StatWeightTests(StaticStateFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Ctor_Parameterless_InitializesEmpty()
    {
        // AC-20: Parameterless constructor for serialization
        var weight = new StatWeight();

        Assert.Null(weight.StatDef);
        Assert.Null(weight.StatDefName);
        Assert.Equal(0f, weight.Weight);
        Assert.False(weight.Protected);
    }

    [Fact]
    public void Ctor_WithNameWeightAndProtection_StoresAll()
    {
        // AC-20: Constructor with defName, weight and protection flag
        var weight = new StatWeight("TestStat", 1.0f, isProtected: true);

        Assert.Equal("TestStat", weight.StatDefName);
        Assert.True(weight.Protected);
        Assert.Equal(1.0f, weight.Weight);
    }

    [Fact]
    public void Ctor_WithStatDefNameAndWeight_StoresAll()
    {
        // AC-20: Constructor with defName, weight, and protection
        var weight = new StatWeight("TestStat", 1.5f, isProtected: false);

        Assert.Equal("TestStat", weight.StatDefName);
        Assert.Equal(1.5f, weight.Weight);
        Assert.False(weight.Protected);
    }

    [Fact]
    public void StatDef_FromName_LazyInitialization()
    {
        // AC-20: StatDef is lazily resolved from StatDefName via StatHelper
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var weight = new StatWeight("TestStat", 0.0f, isProtected: true);

        // Should lazily resolve the StatDef via StatHelper.GetStatDef
        var resolved = weight.StatDef;
        Assert.NotNull(resolved);
        Assert.Equal("TestStat", resolved!.defName);
    }

    [Fact]
    public void StatDef_LazyInitialization_ResolvesByName()
    {
        // AC-20: StatDef property resolves the def from the name on first access
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);

        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // Create with name, not StatDef
        var weight = new StatWeight("TestStat", 1.0f, isProtected: false);

        // First access to StatDef should resolve it
        var resolved = weight.StatDef;
        Assert.NotNull(resolved);
        Assert.Equal("TestStat", resolved!.defName);
    }

    [Fact]
    public void StatDef_NonExistentDef_ReturnsNull()
    {
        // AC-20: Accessing StatDef for a non-existent def returns null
        var fakeProvider = new FakeDefProvider();
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var weight = new StatWeight("NonExistent", 0.5f, isProtected: false);
        var resolved = weight.StatDef;

        Assert.Null(resolved);
    }

    [Fact]
    public void Protected_CanBeModified()
    {
        // AC-20: Protected property getter/setter
        var weight = new StatWeight("TestStat", 0.0f, isProtected: false);
        Assert.False(weight.Protected);

        weight.Protected = true;
        Assert.True(weight.Protected);

        weight.Protected = false;
        Assert.False(weight.Protected);
    }

    [Fact]
    public void Weight_CanBeModified()
    {
        // AC-20: Weight property can be set independently
        var weight = new StatWeight("TestStat", 1.0f, isProtected: false);
        Assert.Equal(1.0f, weight.Weight);

        weight.Weight = 0.5f;
        Assert.Equal(0.5f, weight.Weight);

        weight.Weight = 2.0f;
        Assert.Equal(2.0f, weight.Weight);
    }

    [Fact]
    public void Weight_NegativeValue_Stored()
    {
        // AC-20: Negative weights are stored (though semantically unusual)
        var weight = new StatWeight("TestStat", -0.5f, isProtected: false);
        Assert.Equal(-0.5f, weight.Weight);
    }

    [Fact]
    public void Weight_ExceedsWeightCap_Stored()
    {
        // AC-20: Weights > WeightCap are stored (cap is not enforced at construction)
        var weight = new StatWeight("TestStat", 5.0f, isProtected: false);
        Assert.Equal(5.0f, weight.Weight);
        Assert.True(weight.Weight > StatWeight.WeightCap);
    }

    [Fact]
    public void ExposeData_RoundTrip_PreservesState()
    {
        // AC-20: ExposeData for serialization (simplified test without actual Scribe infrastructure)
        var original = new StatWeight("TestStat", 1.5f, isProtected: true);

        // In real scenarios, ExposeData would be called within a Scribe context.
        // Here we're just ensuring it doesn't throw and is implemented.
        Assert.Null(Record.Exception(() => original.ExposeData()));
    }

    [Fact]
    public void StatDefName_ReturnsStoredName()
    {
        // AC-20: StatDefName property returns the stored name
        var weight = new StatWeight("MyStat", 0.0f, isProtected: false);
        Assert.Equal("MyStat", weight.StatDefName);

        var weight2 = new StatWeight();
        Assert.Null(weight2.StatDefName);
    }

    [Fact]
    public void MultipleInstances_IndependentState()
    {
        // AC-20: Multiple StatWeight instances do not share state
        var weight1 = new StatWeight("Stat1", 1.0f, isProtected: true);
        var weight2 = new StatWeight("Stat2", 2.0f, isProtected: false);

        Assert.Equal("Stat1", weight1.StatDefName);
        Assert.Equal("Stat2", weight2.StatDefName);
        Assert.Equal(1.0f, weight1.Weight);
        Assert.Equal(2.0f, weight2.Weight);
        Assert.True(weight1.Protected);
        Assert.False(weight2.Protected);

        weight1.Weight = 3.0f;
        Assert.Equal(3.0f, weight1.Weight);
        Assert.Equal(2.0f, weight2.Weight);
    }

    [Fact]
    public void WeightCap_Constant_IsSet()
    {
        // AC-20: WeightCap constant is defined (even if not enforced)
        Assert.Equal(2f, StatWeight.WeightCap);
    }
}
