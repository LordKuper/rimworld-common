using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;

namespace LordKuper.Common.Tests;

/// <summary>
///     xUnit fixture that saves and restores global static state around each test.
///     This ensures that tests do not leak state to one another and can run in any order.
/// </summary>
/// <remarks>
///     Captured state includes:
///     - <see cref="DefProvider.Current" /> (swapped to a fake for tests)
///     - StatHelper static caches (rebuilt via internal Rebuild())
///     - WorkTypeStatMap static caches (rebuilt via internal Rebuild())
///     - SkillStatMap static cache (rebuilt via internal Rebuild())
///     - PassionHelper static cache (re-initialized via Reflection)
///     - StatRanges.Ranges adaptive cache (restored via Reflection)
///     - DefCache, ThingCache, PassionCache static state (reset via Reflection)
/// </remarks>
[UsedImplicitly]
public class StaticStateFixture : IDisposable
{
    private readonly IDefProvider _originalProvider;

    public StaticStateFixture()
    {
        // Save the original production provider
        _originalProvider = DefProvider.Current;
    }

    /// <summary>
    ///     Restores all static state to its original condition, restoring the original
    ///     <see cref="DefProvider.Current" /> and rebuilding all caches.
    /// </summary>
    public void Dispose()
    {
        // Restore the original provider before rebuilding caches
        DefProvider.Current = _originalProvider;

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

/// <summary>
///     Collection definition for tests that require static-state isolation.
///     Applied via [Collection("StaticState")] on test classes that mutate DefProvider or other shared statics.
/// </summary>
[CollectionDefinition("StaticState", DisableParallelization = true)]
public class StaticStateCollection
{
    // This class is a marker only; it defines the collection.
}