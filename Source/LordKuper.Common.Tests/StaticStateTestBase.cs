using System.Collections;
using System.Reflection;
using LordKuper.Common.Helpers;

namespace LordKuper.Common.Tests;

/// <summary>
///     Base class for test classes that mutate global static state
///     (<see cref="DefProvider.Current" />, <see cref="StatRanges" /> caches, etc.).
///     NUnit calls <c>[SetUp]</c> before each <c>[Test]</c> and <c>[TearDown]</c> after each
///     <c>[Test]</c> on the same instance, giving true <em>per-test</em> save/restore — the same
///     isolation granularity as the previous xUnit ctor + <c>Dispose</c> pattern.
/// </summary>
/// <remarks>
///     Test classes that inherit this base must also carry <c>[NonParallelizable]</c>
///     so that the static-touching classes run serially and cannot race each other.
///     NUnit is non-parallel by default; <c>[NonParallelizable]</c> makes the serialization
///     intent explicit and guards against a future accidental opt-in to assembly-level parallelism.
/// </remarks>
public abstract class StaticStateTestBase
{
    private IDefProvider? _originalProvider;

    /// <summary>Saves all static state before each test.</summary>
    [SetUp]
    public void SetUpStaticState()
    {
        _originalProvider = DefProvider.Current;
    }

    /// <summary>Restores all static state saved before the test.</summary>
    /// <remarks>
    ///     All caches are reset by nulling their backing fields rather than calling any Rebuild()
    ///     method. This avoids the Unity-ECall hazard: calling WorkTypeStatMap.Rebuild() or
    ///     StatHelper.Rebuild() during teardown would eventually reach SkillStatMap.BuildMap() or
    ///     DefProvider.Current.AllDefsListForReading(), which hit DefDatabase / Verse native calls
    ///     that are unavailable in a headless test process. Nulling the fields is safe because each
    ///     test that needs these caches already calls StatHelper.Rebuild() / WorkTypeStatMap.Rebuild()
    ///     explicitly in its own setup after installing a FakeDefProvider.
    /// </remarks>
    [TearDown]
    public void TearDownStaticState()
    {
        // Restore the original provider first; all cache resets below must not read it.
        if (_originalProvider != null)
            DefProvider.Current = _originalProvider;
        _originalProvider = null;

        // Reset WorkTypeStatMap backing fields via reflection.
        // Do NOT call WorkTypeStatMap.Rebuild(): it reads SkillStatMap.Map, which triggers
        // SkillStatMap.BuildMap() when _map == null → DefDatabase access → Unity ECall.
        var wtsmType = typeof(WorkTypeStatMap);
        var autoSwitchField =
            wtsmType.GetField("_autoSwitchStatsMap", BindingFlags.NonPublic | BindingFlags.Static);
        var defaultStatsField =
            wtsmType.GetField("_defaultStatsMap", BindingFlags.NonPublic | BindingFlags.Static);
        if (autoSwitchField != null)
            autoSwitchField.SetValue(null, null);
        if (defaultStatsField != null)
            defaultStatsField.SetValue(null, null);

        // Reset StatHelper backing fields via reflection.
        // Do NOT call StatHelper.Rebuild(): it calls DefProvider.Current.AllDefsListForReading()
        // which hits DefDatabase when the restored provider is the production one (null or game).
        // Tests that need StatHelper call StatHelper.Rebuild() themselves after installing
        // a FakeDefProvider, so leaving these null is the correct idle state.
        var shType = typeof(StatHelper);
        var shFields = new[]
        {
            "_allMeleeWeaponStatDefs", "_allRangedWeaponStatDefs", "_allStatDefs",
            "_allToolStatDefs", "_apparelCategories", "_customStatsDefs",
            "_defaultApparelStatDefs", "_defaultPawnStatDefs", "_defaultWeaponStatDefs",
            "_defaultWorkStatDefs", "_pawnCategories", "_statDefsByName",
            "_weaponCategories", "_workCategories"
        };
        foreach (var fieldName in shFields)
        {
            var field = shType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, null);
        }
        // Stats is readonly (Dictionary instance is fixed); only the contents need clearing.
        var statsField = shType.GetField("Stats", BindingFlags.NonPublic | BindingFlags.Static);
        if (statsField?.GetValue(null) is IDictionary statsDict)
            statsDict.Clear();

        // Reset SkillStatMap._map via reflection (lazy BuildMap, no public Rebuild).
        var sksmType = typeof(SkillStatMap);
        var mapField = sksmType.GetField("_map", BindingFlags.NonPublic | BindingFlags.Static);
        if (mapField != null)
            mapField.SetValue(null, null);

        // Reset PassionHelper via reflection (no public Rebuild).
        var phType = typeof(PassionHelper);
        var isInitField =
            phType.GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Static);
        var cachedPassionsField =
            phType.GetField("_cachedPassions", BindingFlags.NonPublic | BindingFlags.Static);
        var passionCacheField =
            phType.GetField("PassionCache", BindingFlags.NonPublic | BindingFlags.Static);
        if (isInitField != null)
            isInitField.SetValue(null, false);
        if (cachedPassionsField != null)
            cachedPassionsField.SetValue(null, null);
        if (passionCacheField?.GetValue(null) is IDictionary cache)
            cache.Clear();

        // Reset StatRanges via public Clear() method.
        StatRanges.Clear();
    }
}
