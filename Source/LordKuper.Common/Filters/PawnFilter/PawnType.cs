namespace LordKuper.Common.Filters;

/// <summary>
///     Specifies the type of pawn in the game.
/// </summary>
public enum PawnType
{
    /// <summary>
    ///     The pawn type is not defined.
    /// </summary>
    Undefined,

    /// <summary>
    ///     A colonist pawn.
    /// </summary>
    Colonist,

    /// <summary>
    ///     A guest pawn.
    /// </summary>
    Guest,

    /// <summary>
    ///     A prisoner pawn.
    /// </summary>
    Prisoner,

    /// <summary>
    ///     A slave pawn.
    /// </summary>
    Slave,

    /// <summary>
    ///     An animal pawn.
    /// </summary>
    Animal
}