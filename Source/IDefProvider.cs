using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Provides access to RimWorld <see cref="Def" /> databases.
///     The default implementation, <see cref="VerseDefProvider" />, delegates to
///     <see cref="DefDatabase{T}" /> and <see cref="WorkTypeDefsUtility" />.
///     Tests install a fake implementation via <see cref="DefProvider.Current" />.
/// </summary>
/// <remarks>
///     This is the single test-isolation seam introduced by ADR-0001.
///     Keep the interface narrow: only add members that have real call sites in the library.
/// </remarks>
[PublicAPI]
public interface IDefProvider
{
    /// <summary>
    ///     Returns all defs of type <typeparamref name="T" /> as a list.
    ///     Corresponds to <c>DefDatabase&lt;T&gt;.AllDefsListForReading</c>.
    /// </summary>
    IReadOnlyList<T> AllDefsListForReading<T>() where T : Def;

    /// <summary>
    ///     Returns all defs of type <typeparamref name="T" /> as an enumerable.
    ///     Corresponds to <c>DefDatabase&lt;T&gt;.AllDefs</c>.
    /// </summary>
    IEnumerable<T> AllDefs<T>() where T : Def;

    /// <summary>
    ///     Looks up a def by <paramref name="defName" />, returning <c>null</c> on miss.
    ///     Corresponds to <c>DefDatabase&lt;T&gt;.GetNamedSilentFail(defName)</c>.
    /// </summary>
    T? GetNamedSilentFail<T>(string? defName) where T : Def;

    /// <summary>
    ///     Returns work type defs in priority order.
    ///     Corresponds to <c>WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder</c>.
    /// </summary>
    IReadOnlyList<WorkTypeDef> WorkTypeDefsInPriorityOrder();
}
