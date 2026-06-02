using System;
using System.Collections.Generic;

namespace LordKuper.Common.Helpers;

/// <summary>
///     Provides helper methods for working with enumeration types.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    ///     Identifies the flags of an enumeration that are not present in the specified value.
    /// </summary>
    /// <remarks>
    ///     This method evaluates all flags defined in the enumeration type <typeparamref name="T" /> and
    ///     determines which flags are absent in the provided <paramref name="value" />. The result is a combination of those
    ///     absent flags.
    /// </remarks>
    /// <typeparam name="T">The enumeration type. Must be an enumeration.</typeparam>
    /// <param name="value">The enumeration value to evaluate.</param>
    /// <returns>
    ///     An enumeration value representing the combination of flags that are defined in the enumeration but are not set
    ///     in the specified <paramref name="value" />.
    /// </returns>
    public static T AbsentFlags<T>(T value) where T : Enum
    {
        var valueLong = Convert.ToInt64(value);
        long absentFlags = 0;
        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagLong = Convert.ToInt64(flag);
            if ((valueLong & flagLong) != flagLong && flagLong != 0) absentFlags |= flagLong;
        }
        return (T)Enum.ToObject(typeof(T), absentFlags);
    }

    /// <summary>
    ///     Retrieves the unique flags set in the specified enumeration value, excluding any flags specified in the excluded
    ///     flags.
    /// </summary>
    /// <remarks>
    ///     This method is useful for working with bitwise enumeration types where individual flags are
    ///     represented as distinct bits. Flags with a value of zero are ignored.
    /// </remarks>
    /// <typeparam name="T">The enumeration type. Must be an enumeration.</typeparam>
    /// <param name="value">The enumeration value from which to extract the unique flags.</param>
    /// <param name="excludedFlags">The flags to exclude from the result.</param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> containing the unique flags set in <paramref name="value" /> that are not present
    ///     in <paramref name="excludedFlags" />.
    /// </returns>
    public static IEnumerable<T> GetUniqueFlags<T>(T value, T excludedFlags) where T : Enum
    {
        var valueLong = Convert.ToInt64(value);
        var excludedLong = Convert.ToInt64(excludedFlags);
        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagLong = Convert.ToInt64(flag);
            if ((valueLong & flagLong) == flagLong && (excludedLong & flagLong) != flagLong && flagLong != 0)
                yield return flag;
        }
    }

    /// <summary>
    ///     Retrieves a collection of unique flags that are set in the specified enumeration value.
    /// </summary>
    /// <remarks>
    ///     This method is designed for use with enumerations that have the <see cref="FlagsAttribute" />
    ///     applied. It identifies and returns all individual flags that are set in the provided enumeration
    ///     value.
    /// </remarks>
    /// <typeparam name="T">The enumeration type. Must be an enumeration.</typeparam>
    /// <param name="value">The enumeration value to analyze for set flags.</param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> containing the flags of type <typeparamref name="T" /> that are set in the
    ///     specified <paramref name="value" />. The collection will be empty if no flags are set.
    /// </returns>
    public static IEnumerable<T> GetUniqueFlags<T>(T value) where T : Enum
    {
        var valueLong = Convert.ToInt64(value);
        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagLong = Convert.ToInt64(flag);
            if ((valueLong & flagLong) == flagLong && flagLong != 0)
                yield return flag;
        }
    }

    /// <summary>
    ///     Determines whether the specified value contains all the flags specified in the provided flags parameter.
    /// </summary>
    /// <typeparam name="T">An enumeration type.</typeparam>
    /// <param name="value">The enumeration value to check.</param>
    /// <param name="flags">The flags to verify are present in <paramref name="value" />.</param>
    /// <returns>
    ///     <see langword="true" /> if all the flags in <paramref name="flags" /> are set in <paramref name="value" />  and
    ///     <paramref name="flags" /> is not zero; otherwise, <see langword="false" />.
    /// </returns>
    public static bool HasAllFlags<T>(T value, T flags) where T : Enum
    {
        var valueLong = Convert.ToInt64(value);
        var flagsLong = Convert.ToInt64(flags);
        return (valueLong & flagsLong) == flagsLong && flagsLong != 0;
    }

    /// <summary>
    ///     Determines whether any of the specified flags are set in the given enumeration value.
    /// </summary>
    /// <typeparam name="T">The enumeration type. Must be an <see cref="Enum" />.</typeparam>
    /// <param name="value">The enumeration value to check.</param>
    /// <param name="flags">The flags to test for in the enumeration value.</param>
    /// <returns>
    ///     <see langword="true" /> if any of the specified flags are set in <paramref name="value" />; otherwise,
    ///     <see
    ///         langword="false" />
    ///     .
    /// </returns>
    public static bool HasAnyFlag<T>(T value, T flags) where T : Enum
    {
        var valueLong = Convert.ToInt64(value);
        var flagsLong = Convert.ToInt64(flags);
        return (valueLong & flagsLong) != 0;
    }
}