using System;

namespace LordKuper.Common.Filters;

/// <summary>
///     Represents the health state of a pawn.
/// </summary>
[Flags]
public enum PawnHealthState
{
    /// <summary>
    ///     Represents the absence of a specific value or option.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The pawn is in good health.
    /// </summary>
    Healthy = 1 << 0,

    /// <summary>
    ///     The pawn is resting, recovering from injuries or illnesses.
    /// </summary>
    Resting = 1 << 1,

    /// <summary>
    ///     The pawn requires medical attention or tending.
    /// </summary>
    NeedsTending = 1 << 2,

    /// <summary>
    ///     The pawn is downed and unable to move or act.
    /// </summary>
    Downed = 1 << 3,

    /// <summary>
    ///     The pawn is experiencing a mental break.
    /// </summary>
    Mental = 1 << 4,

    /// <summary>
    ///     The pawn is deceased.
    /// </summary>
    Dead = 1 << 5
}