namespace LordKuper.Common;

/// <summary>
///     Represents the category of a stat.
/// </summary>
public enum StatCategory
{
    /// <summary>
    ///     Stats relevant for pawns.
    /// </summary>
    Pawn,

    /// <summary>
    ///     Stats relevant for apparel.
    /// </summary>
    Apparel,

    /// <summary>
    ///     Stats relevant for all weapons.
    /// </summary>
    Weapon,

    /// <summary>
    ///     Stats relevant specifically for melee weapons.
    /// </summary>
    WeaponMelee,

    /// <summary>
    ///     Stats relevant specifically for ranged weapons.
    /// </summary>
    WeaponRanged,

    /// <summary>
    ///     Stats relevant for tools.
    /// </summary>
    Tool,

    /// <summary>
    ///     Stats relevant for work types.
    /// </summary>
    Work,

    /// <summary>
    ///     All stats, regardless of category.
    /// </summary>
    All
}