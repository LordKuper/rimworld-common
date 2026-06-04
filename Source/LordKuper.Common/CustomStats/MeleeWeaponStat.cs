using JetBrains.Annotations;

namespace LordKuper.Common.CustomStats;

/// <summary>
///     Enumerates the custom melee weapon stats available for use.
/// </summary>
[PublicAPI]
public enum MeleeWeaponStat
{
    /// <summary>
    ///     The armor penetration stat.
    /// </summary>
    ArmorPenetration,

    /// <summary>
    ///     The sharp damage per second stat.
    /// </summary>
    DpsSharp,

    /// <summary>
    ///     The blunt damage per second stat.
    /// </summary>
    DpsBlunt,

    /// <summary>
    ///     The tech level stat.
    /// </summary>
    TechLevel
}