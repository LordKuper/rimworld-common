using JetBrains.Annotations;

namespace LordKuper.Common.CustomStats;

/// <summary>
///     Enumerates all custom ranged weapon statistics.
/// </summary>
[UsedImplicitly]
public enum RangedWeaponStat
{
    /// <summary>
    ///     Accuracy-accounted damage per second.
    /// </summary>
    Dpsa,

    /// <summary>
    ///     Accuracy-accounted damage per second at close range.
    /// </summary>
    DpsaClose,

    /// <summary>
    ///     Accuracy-accounted damage per second at short range.
    /// </summary>
    DpsaShort,

    /// <summary>
    ///     Accuracy-accounted damage per second at medium range.
    /// </summary>
    DpsaMedium,

    /// <summary>
    ///     Accuracy-accounted damage per second at long range.
    /// </summary>
    DpsaLong,

    /// <summary>
    ///     The maximum range of the weapon.
    /// </summary>
    Range,

    /// <summary>
    ///     The time required to warm up the weapon before firing.
    /// </summary>
    Warmup,

    /// <summary>
    ///     The number of shots fired per burst.
    /// </summary>
    BurstShotCount,

    /// <summary>
    ///     The number of ticks between shots in a burst.
    /// </summary>
    TicksBetweenBurstShots,

    /// <summary>
    ///     The armor penetration value of the weapon.
    /// </summary>
    ArmorPenetration,

    /// <summary>
    ///     The stopping power of the weapon.
    /// </summary>
    StoppingPower,

    /// <summary>
    ///     The damage dealt by the weapon.
    /// </summary>
    Damage,

    /// <summary>
    ///     The technological level of the weapon.
    /// </summary>
    TechLevel
}