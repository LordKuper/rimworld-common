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
    [TearDown]
    public void TearDownStaticState()
    {
        // Restore the original provider before rebuilding caches
        if (_originalProvider != null)
            DefProvider.Current = _originalProvider;
        _originalProvider = null;

        // Rebuild all dependent static caches to reset them with the production provider
        StatHelper.Rebuild();
        WorkTypeStatMap.Rebuild();

        // Reset SkillStatMap via reflection (it has lazy BuildMap but no public Rebuild)
        var sksmType = typeof(SkillStatMap);
        var mapField = sksmType.GetField("_map", BindingFlags.NonPublic | BindingFlags.Static);
        if (mapField != null)
            mapField.SetValue(null, null);

        // Reset PassionHelper via reflection since there's no public Rebuild
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

        // Reset StatRanges.Ranges via reflection
        var srType = typeof(StatRanges);
        var rangesField = srType.GetField("Ranges", BindingFlags.NonPublic | BindingFlags.Static);
        if (rangesField?.GetValue(null) is IDictionary ranges)
            ranges.Clear();
    }
}
