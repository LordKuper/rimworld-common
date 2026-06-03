using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Static injection point for <see cref="IDefProvider" />.
///     Defaults to <see cref="VerseDefProvider" /> (live <see cref="DefDatabase{T}" /> access).
///     Tests swap <see cref="Current" /> to a fake implementation and restore it after each test.
/// </summary>
/// <remarks>
///     This is the single test-isolation seam introduced by ADR-0001.
///     Only one global replacement is supported; no DI container.
/// </remarks>
[PublicAPI]
public static class DefProvider
{
    private static IDefProvider _current = new VerseDefProvider();

    /// <summary>
    ///     Gets or sets the active <see cref="IDefProvider" />.
    ///     Defaults to <see cref="VerseDefProvider" />.
    ///     Test fixtures may replace this and MUST restore it after each test.
    /// </summary>
    public static IDefProvider Current
    {
        get => _current;
        set => _current = value;
    }
}

/// <summary>
///     Production implementation of <see cref="IDefProvider" />: thin pass-through to
///     <see cref="DefDatabase{T}" /> and <see cref="WorkTypeDefsUtility" />.
///     Runtime behavior is identical to the direct calls it replaces.
/// </summary>
internal sealed class VerseDefProvider : IDefProvider
{
    /// <inheritdoc />
    public IReadOnlyList<T> AllDefsListForReading<T>() where T : Def =>
        DefDatabase<T>.AllDefsListForReading;

    /// <inheritdoc />
    public IEnumerable<T> AllDefs<T>() where T : Def =>
        DefDatabase<T>.AllDefs;

    /// <inheritdoc />
    public T? GetNamedSilentFail<T>(string? defName) where T : Def =>
        string.IsNullOrEmpty(defName) ? null : DefDatabase<T>.GetNamedSilentFail(defName);

    /// <inheritdoc />
    public IReadOnlyList<WorkTypeDef> WorkTypeDefsInPriorityOrder() =>
        WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList();
}
