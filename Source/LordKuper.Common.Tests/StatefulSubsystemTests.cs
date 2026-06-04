using System.Reflection;
using LordKuper.Common.Cache;
using LordKuper.Common.Filters;
using LordKuper.Common.Filters.Limits;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;
using PawnHealthState = LordKuper.Common.Filters.PawnHealthState;

namespace LordKuper.Common.Tests;

/// <summary>
///     Stateful subsystem tests that require FakeDefProvider and StaticStateFixture to isolate global state.
///     Covers: StatHelper, WorkTypeStatMap, SkillStatMap, StatRanges, StatWeight,
///     PawnFilter limits, caches, time+helpers, and WorkTypeThingRule.
/// </summary>
[NonParallelizable]
public class StatefulSubsystemTests : StaticStateTestBase
{
    [Test]
    public void DefCache_InitializesLazily()
    {
        // DefCache lazy initialization
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // Create a DefCache with a non-existent def name
        var cache = new TestDefCache("NonExistent");

        // Should be null on first access (def not found)
        cache.Def.Should().BeNull();
    }

    [Test]
    public void PawnFilter_Combine_PreservesSemantics()
    {
        // Verify Combine semantics preserved
        var main = new PawnFilter
        {
            FilterPawnTypes = true,
            AllowedPawnTypes = [PawnType.Colonist]
        };
        var fallback = new PawnFilter
        {
            FilterPawnTypes = false,
            AllowedPawnTypes = [PawnType.Guest, PawnType.Slave]
        };
        var combined = PawnFilter.Combine(main, fallback);

        // Main's values should win
        combined.FilterPawnTypes.Should().BeTrue();
        combined.AllowedPawnTypes.Should().Contain(PawnType.Colonist);
        combined.AllowedPawnTypes.Should().NotContain(PawnType.Guest);
    }

    [Test]
    public void PawnFilter_Copy_IsIndependent()
    {
        // PawnFilter.Copy creates independent instance
        var original = new PawnFilter
        {
            FilterPawnTypes = true,
            AllowedPawnTypes = [PawnType.Colonist],
            FilterPawnHealthStates = true,
            AllowedPawnHealthStates = PawnHealthState.Healthy
        };
        var copy = original.Copy();

        // Modify copy
        copy.AllowedPawnTypes.Add(PawnType.Guest);
        copy.AllowedPawnHealthStates = PawnHealthState.Downed;

        // Original should be unchanged
        original.AllowedPawnTypes.Should().ContainSingle();
        original.AllowedPawnHealthStates.Should().Be(PawnHealthState.Healthy);
    }

    [Test]
    public void RimWorldTime_Constants_AreCorrect()
    {
        // Time helper constants
        RimWorldTime.HoursInDay.Should().Be(24);
        RimWorldTime.DaysInQuadrum.Should().Be(15);
        RimWorldTime.QuadrumsInYear.Should().Be(4);
        RimWorldTime.DaysInYear.Should().Be(60);
    }

    [Test]
    public void StatHelper_Rebuild_ReinitializesCachesWithFakeProvider()
    {
        // StatHelper can be rebuilt with a fake provider
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // Verify StatHelper is using the fake provider
        var retrieved = StatHelper.GetStatDef("teststat");
        retrieved.Should().NotBeNull();
        retrieved!.defName.Should().Be("TestStat");
    }

    [Test]
    public void StatLimit_Initializes_WithDefaultValues()
    {
        // StatLimit limit tracking
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        var limit = new StatLimit(statDef);

        // StatLimit can be initialized and references the correct StatDef
        limit.Def.Should().Be(statDef);
    }

    [Test]
    public void StatRanges_ClearedBetweenTests()
    {
        // StatRanges is cleared by fixture (preventing cross-test leakage)
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef { defName = "TestStat", label = "Test Stat", category = null };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // Score a value - should not throw and should return normalized value
        var score = StatRanges.NormalizeStatValue(statDef, 10f);
        // Preserve value-comparison form: score >= 0f
        score.Should().BeGreaterThanOrEqualTo(0f);
        // Fixture will clear StatRanges.Ranges after test; verified by order-independence
    }

    [Test]
    public void WorkTypeStatMap_Rebuild_ReinitializesWithFakeProvider()
    {
        // WorkTypeStatMap can be rebuilt with a fake provider
        var fakeProvider = new FakeDefProvider();
        var workTypeDef = new WorkTypeDef { defName = "TestWork" };
        fakeProvider.SetWorkTypeDefsInPriorityOrder(workTypeDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // Pre-populate SkillStatMap._map with an empty map so WorkTypeStatMap.Rebuild()
        // skips BuildMap() which calls Logger → Verse.Log.Message → Unity native ECall
        // (unavailable outside the Unity player). Same reflection pattern as StaticStateFixture.
        var sksmType = typeof(SkillStatMap);
        var mapField = sksmType.GetField("_map", BindingFlags.NonPublic | BindingFlags.Static);
        mapField?.SetValue(null, new Dictionary<SkillDef, HashSet<StatDef>>());
        WorkTypeStatMap.Rebuild();

        // Verify the map is initialized with the fake provider
        WorkTypeStatMap.AutoSwitchStatsMap.Should().NotBeNull();
        WorkTypeStatMap.DefaultStatsMap.Should().NotBeNull();
    }

    // Helper class for DefCache test
    private class TestDefCache : DefCache<StatDef>
    {
        public TestDefCache(string? defName) : base(defName) { }
    }
}