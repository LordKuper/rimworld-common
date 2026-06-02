using System;
using JetBrains.Annotations;
using LordKuper.Common.Cache;
using RimWorld;
using Verse;

namespace LordKuper.Common.Filters.Limits;

/// <summary>
///     Represents a limit for a specific <see cref="TraitDef" />, indicating whether the trait is required or forbidden.
///     Inherits from <see cref="DefCache{T}" /> for caching the <see cref="TraitDef" />.
/// </summary>
[PublicAPI]
public class PawnTraitLimit : DefCache<TraitDef>, IExposable
{
    /// <summary>
    ///     Indicates whether the trait is required (<c>true</c>) or forbidden (<c>false</c>).
    /// </summary>
    public bool Limit;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnTraitLimit" /> class.
    /// </summary>
    public PawnTraitLimit() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnTraitLimit" /> class with the specified trait definition.
    /// </summary>
    /// <param name="def">The trait definition to limit.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="def" /> is null.</exception>
    public PawnTraitLimit([NotNull] TraitDef def) : base(GetDefName(def))
    {
        Limit = true;
    }

    /// <summary>
    ///     Exposes the data for saving and loading.
    /// </summary>
    public new void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref Limit, nameof(Limit));
    }
}