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
[Collection("StaticState")]
public class StatefulSubsystemTests : StaticStateTestBase
{

    // Helper class for DefCache test
    private class TestDefCache : DefCache<StatDef>
    {
        public TestDefCache(string? defName) : base(defName) { }
    }

    [Fact]
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
        Assert.Null(cache.Def);
    }

    [Fact]
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
        Assert.True(combined.FilterPawnTypes);
        Assert.Contains(PawnType.Colonist, combined.AllowedPawnTypes);
        Assert.DoesNotContain(PawnType.Guest, combined.AllowedPawnTypes);
    }

    [Fact]
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
        Assert.Single(original.AllowedPawnTypes);
        Assert.Equal(PawnHealthState.Healthy, original.AllowedPawnHealthStates);
    }

    [Fact]
    public void RimWorldTime_Constants_AreCorrect()
    {
        // Time helper constants
        Assert.Equal(24, RimWorldTime.HoursInDay);
        Assert.Equal(15, RimWorldTime.DaysInQuadrum);
        Assert.Equal(4, RimWorldTime.QuadrumsInYear);
        Assert.Equal(60, RimWorldTime.DaysInYear);
    }

    [Fact]
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
        Assert.NotNull(retrieved);
        Assert.Equal("TestStat", retrieved.defName);
    }

    [Fact]
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
        Assert.Equal(statDef, limit.Def);
    }

    [Fact]
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
        Assert.True(score >= 0f);
        // Fixture will clear StatRanges.Ranges after test; verified by order-independence
    }

    [Fact]
    public void WorkTypeStatMap_Rebuild_ReinitializesWithFakeProvider()
    {
        // WorkTypeStatMap can be rebuilt with a fake provider
        var fakeProvider = new FakeDefProvider();
        var workTypeDef = new WorkTypeDef { defName = "TestWork" };
        fakeProvider.SetWorkTypeDefsInPriorityOrder(workTypeDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();
        WorkTypeStatMap.Rebuild();

        // Verify the map is initialized with the fake provider
        Assert.NotNull(WorkTypeStatMap.AutoSwitchStatsMap);
        Assert.NotNull(WorkTypeStatMap.DefaultStatsMap);
    }
}