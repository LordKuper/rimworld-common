using System;
using JetBrains.Annotations;

namespace LordKuper.Common.Filters;

/// <summary>
///     Represents the available pawn filter sections for rendering.
///     Supports bitwise combination of multiple sections.
/// </summary>
[PublicAPI]
[Flags]
public enum PawnFilterSections
{
    /// <summary>
    ///     No filter section.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Filter by pawn types.
    /// </summary>
    PawnTypes = 1 << 0,

    /// <summary>
    ///     Filter by pawn health states.
    /// </summary>
    PawnHealthStates = 1 << 1,

    /// <summary>
    ///     Filter by pawn skills.
    /// </summary>
    PawnSkills = 1 << 2,

    /// <summary>
    ///     Filter by work passions.
    /// </summary>
    WorkPassions = 1 << 3,

    /// <summary>
    ///     Filter by work capacities.
    /// </summary>
    WorkCapacities = 1 << 4,

    /// <summary>
    ///     Filter by pawn traits.
    /// </summary>
    PawnTraits = 1 << 5,

    /// <summary>
    ///     Filter by pawn stats.
    /// </summary>
    PawnStats = 1 << 6,

    /// <summary>
    ///     Filter by pawn capacities.
    /// </summary>
    PawnCapacities = 1 << 7,

    /// <summary>
    ///     Filter by pawn primary weapon types.
    /// </summary>
    PawnPrimaryWeaponTypes = 1 << 8,

    /// <summary>
    ///     All filter sections.
    /// </summary>
    All = PawnTypes | PawnHealthStates | PawnSkills | WorkPassions | WorkCapacities | PawnTraits |
          PawnStats | PawnCapacities | PawnPrimaryWeaponTypes
}