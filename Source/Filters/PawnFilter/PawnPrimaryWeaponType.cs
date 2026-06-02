namespace LordKuper.Common.Filters;

/// <summary>
///     Specifies the primary weapon type of pawn.
/// </summary>
public enum PawnPrimaryWeaponType
{
    /// <summary>
    ///     Indicates that the pawn has no primary weapon.
    /// </summary>
    None,

    /// <summary>
    ///     Indicates that the pawn's primary weapon is a melee weapon.
    /// </summary>
    Melee,

    /// <summary>
    ///     Indicates that the pawn's primary weapon is a ranged weapon.
    /// </summary>
    Ranged,

    /// <summary>
    ///     Indicates that the pawn's primary weapon type is undefined.
    /// </summary>
    Undefined
}