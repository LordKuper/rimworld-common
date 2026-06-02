using System;
using JetBrains.Annotations;
using LordKuper.Common.Filters;
using RimWorld;
using Verse;
using PawnHealthState = LordKuper.Common.Filters.PawnHealthState;

namespace LordKuper.Common.Helpers;

/// <summary>
///     Provides utility methods for analyzing and categorizing pawns in the game.
/// </summary>
/// <remarks>
///     The <see cref="PawnHelper" /> class includes methods for determining various attributes of pawns,
///     such as their health state, type, primary weapon type, and work passion. These methods are designed to simplify
///     common operations related to pawns and provide consistent results based on game logic.
/// </remarks>
[PublicAPI]
public static class PawnHelper
{
    /// <summary>
    ///     Determines the health state of the specified pawn.
    /// </summary>
    /// <param name="pawn">The pawn whose health state is to be determined.</param>
    /// <returns>The <see cref="PawnHealthState" /> of the pawn.</returns>
    public static PawnHealthState GetPawnHealthState([NotNull] Pawn pawn)
    {
        if (pawn.Dead)
            return PawnHealthState.Dead;
        var state = PawnHealthState.None;
        if (pawn.Downed)
            state |= PawnHealthState.Downed;
        if (pawn.InMentalState)
            state |= PawnHealthState.Mental;
        var health = pawn.health;
        if (health.HasHediffsNeedingTend() || health.hediffSet.HasTendableHediff())
            state |= PawnHealthState.NeedsTending;
        if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
            state |= PawnHealthState.Resting;
        return state == PawnHealthState.None ? PawnHealthState.Healthy : state;
    }

    /// <summary>
    ///     Determines the type of the specified pawn.
    /// </summary>
    /// <param name="pawn">The pawn whose type is to be determined.</param>
    /// <returns>The <see cref="PawnType" /> of the pawn.</returns>
    public static PawnType GetPawnType([NotNull] Pawn pawn)
    {
        if (pawn.IsFreeNonSlaveColonist) return PawnType.Colonist;
        if (pawn.IsSlaveOfColony) return PawnType.Slave;
        if (pawn.IsPrisonerOfColony) return PawnType.Prisoner;
        if (pawn is { IsColonist: true, GuestStatus: GuestStatus.Guest } || pawn.HasExtraHomeFaction() ||
            pawn.HasExtraMiniFaction())
            return PawnType.Guest;
        if (pawn.IsAnimal && pawn.Faction == Faction.OfPlayer) return PawnType.Animal;
        return PawnType.Undefined;
    }

    /// <summary>
    ///     Determines the primary weapon type of the specified pawn.
    /// </summary>
    /// <param name="pawn">The pawn whose primary weapon type is to be determined. Cannot be <see langword="null" />.</param>
    /// <returns>
    ///     A <see cref="PawnPrimaryWeaponType" /> value indicating the type of the pawn's primary weapon:
    ///     <see
    ///         cref="PawnPrimaryWeaponType.Melee" />
    ///     for melee weapons,  <see cref="PawnPrimaryWeaponType.Ranged" /> for ranged
    ///     weapons,  <see cref="PawnPrimaryWeaponType.None" /> if the pawn has no primary weapon,  or
    ///     <see
    ///         cref="PawnPrimaryWeaponType.Undefined" />
    ///     if the weapon type cannot be determined.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pawn" /> is <see langword="null" />.</exception>
    public static PawnPrimaryWeaponType GetPrimaryWeaponType([NotNull] Pawn pawn)
    {
        if (pawn == null) throw new ArgumentNullException(nameof(pawn));
        var weapon = pawn.equipment?.Primary;
        if (weapon == null)
            return PawnPrimaryWeaponType.None;
        var def = weapon.def;
        if (def.IsMeleeWeapon)
            return PawnPrimaryWeaponType.Melee;
        if (def.IsRangedWeapon)
            return PawnPrimaryWeaponType.Ranged;
        return PawnPrimaryWeaponType.Undefined;
    }

    /// <summary>
    ///     Gets the highest work passion for the specified pawn and work type.
    /// </summary>
    /// <param name="pawn">The pawn whose work passion is to be determined.</param>
    /// <param name="workType">The work type to check for passion.</param>
    /// <returns>The highest <see cref="Passion" /> for the given work type, or <see cref="Passion.None" /> if unavailable.</returns>
    public static Passion GetWorkPassion([NotNull] Pawn pawn, [NotNull] WorkTypeDef workType)
    {
        if (pawn == null) throw new ArgumentNullException(nameof(pawn));
        if (workType == null) throw new ArgumentNullException(nameof(workType));
        return pawn.skills?.MaxPassionOfRelevantSkillsFor(workType) ?? Passion.None;
    }
}