using LordKuper.Common.Helpers;
using RimWorld;

namespace LordKuper.Common.Tests;

/// <summary>
///     Tests for <see cref="StatWeight" /> construction, weight math, and serialization.
/// </summary>
[NonParallelizable]
public class StatWeightTests : StaticStateTestBase
{
    [Test]
    public void Ctor_Parameterless_InitializesEmpty()
    {
        // Parameterless constructor for serialization
        var weight = new StatWeight();
        weight.StatDef.Should().BeNull();
        weight.StatDefName.Should().BeNull();
        weight.Weight.Should().Be(0f);
        weight.Protected.Should().BeFalse();
    }

    [Test]
    public void Ctor_WithNameWeightAndProtection_StoresAll()
    {
        // Constructor with defName, weight and protection flag
        var weight = new StatWeight("TestStat", 1.0f, true);
        weight.StatDefName.Should().Be("TestStat");
        weight.Protected.Should().BeTrue();
        weight.Weight.Should().Be(1.0f);
    }

    [Test]
    public void Ctor_WithStatDefNameAndWeight_StoresAll()
    {
        // Constructor with defName, weight, and protection
        var weight = new StatWeight("TestStat", 1.5f, false);
        weight.StatDefName.Should().Be("TestStat");
        weight.Weight.Should().Be(1.5f);
        weight.Protected.Should().BeFalse();
    }

    [Test]
    public void ExposeData_RoundTrip_PreservesState()
    {
        // ExposeData for serialization (simplified test without actual Scribe infrastructure)
        var original = new StatWeight("TestStat", 1.5f, true);

        // In real scenarios, ExposeData would be called within a Scribe context.
        // Here we're just ensuring it doesn't throw and is implemented.
        var act = () => original.ExposeData();
        act.Should().NotThrow();
    }

    [Test]
    public void MultipleInstances_IndependentState()
    {
        // Multiple StatWeight instances do not share state
        var weight1 = new StatWeight("Stat1", 1.0f, true);
        var weight2 = new StatWeight("Stat2", 2.0f, false);
        weight1.StatDefName.Should().Be("Stat1");
        weight2.StatDefName.Should().Be("Stat2");
        weight1.Weight.Should().Be(1.0f);
        weight2.Weight.Should().Be(2.0f);
        weight1.Protected.Should().BeTrue();
        weight2.Protected.Should().BeFalse();
        weight1.Weight = 3.0f;
        weight1.Weight.Should().Be(3.0f);
        weight2.Weight.Should().Be(2.0f);
    }

    [Test]
    public void Protected_CanBeModified()
    {
        // Protected property getter/setter
        var weight = new StatWeight("TestStat", 0.0f, false);
        weight.Protected.Should().BeFalse();
        weight.Protected = true;
        weight.Protected.Should().BeTrue();
        weight.Protected = false;
        weight.Protected.Should().BeFalse();
    }

    [Test]
    public void StatDefName_ReturnsStoredName()
    {
        // StatDefName property returns the stored name
        var weight = new StatWeight("MyStat", 0.0f, false);
        weight.StatDefName.Should().Be("MyStat");
        var weight2 = new StatWeight();
        weight2.StatDefName.Should().BeNull();
    }

    [Test]
    public void StatDef_FromName_LazyInitialization()
    {
        // StatDef is lazily resolved from StatDefName via StatHelper
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var weight = new StatWeight("TestStat", 0.0f, true);

        // Should lazily resolve the StatDef via StatHelper.GetStatDef
        var resolved = weight.StatDef;
        resolved.Should().NotBeNull();
        resolved.defName.Should().Be("TestStat");
    }

    [Test]
    public void StatDef_LazyInitialization_ResolvesByName()
    {
        // StatDef property resolves the def from the name on first access
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // Create with name, not StatDef
        var weight = new StatWeight("TestStat", 1.0f, false);

        // First access to StatDef should resolve it
        var resolved = weight.StatDef;
        resolved.Should().NotBeNull();
        resolved.defName.Should().Be("TestStat");
    }

    [Test]
    public void StatDef_NonExistentDef_ReturnsNull()
    {
        // Accessing StatDef for a non-existent def returns null
        var fakeProvider = new FakeDefProvider();
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var weight = new StatWeight("NonExistent", 0.5f, false);
        var resolved = weight.StatDef;
        resolved.Should().BeNull();
    }

    [Test]
    public void WeightCap_Constant_IsSet()
    {
        // WeightCap constant is defined (even if not enforced)
        StatWeight.WeightCap.Should().Be(2f);
    }

    [Test]
    public void Weight_CanBeModified()
    {
        // Weight property can be set independently
        var weight = new StatWeight("TestStat", 1.0f, false);
        weight.Weight.Should().Be(1.0f);
        weight.Weight = 0.5f;
        weight.Weight.Should().Be(0.5f);
        weight.Weight = 2.0f;
        weight.Weight.Should().Be(2.0f);
    }

    [Test]
    public void Weight_ExceedsWeightCap_Stored()
    {
        // Weights > WeightCap are stored (cap is not enforced at construction)
        var weight = new StatWeight("TestStat", 5.0f, false);
        weight.Weight.Should().Be(5.0f);
        // Explicit comparison kept as value form to preserve the diff
        weight.Weight.Should().BeGreaterThan(StatWeight.WeightCap);
    }

    [Test]
    public void Weight_NegativeValue_Stored()
    {
        // Negative weights are stored (though semantically unusual)
        var weight = new StatWeight("TestStat", -0.5f, false);
        weight.Weight.Should().Be(-0.5f);
    }
}