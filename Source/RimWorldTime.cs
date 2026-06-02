using System;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Represents a point in RimWorld time, including year, day, and hour.
/// </summary>
[PublicAPI]
public readonly struct RimWorldTime : IEquatable<RimWorldTime>, IComparable<RimWorldTime>, IComparable
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RimWorldTime" /> from year, day, and hour.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="day">The day of the year.</param>
    /// <param name="hour">The hour of the day.</param>
    public RimWorldTime(int year, int day, float hour) : this(GetTotalHours(year, day, hour)) { }

    /// <summary>
    ///     Calculates the total hours from year, day, and hour.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="day">The day of the year.</param>
    /// <param name="hour">The hour of the day.</param>
    /// <returns>Total hours as a float.</returns>
    private static float GetTotalHours(int year, int day, float hour)
    {
        return year * HoursInYear + day * HoursInDay + hour;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="RimWorldTime" /> from total hours.
    /// </summary>
    /// <param name="hours">Total hours since year 0, day 0, hour 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if hours is negative.</exception>
    public RimWorldTime(float hours)
    {
        if (hours < 0) throw new ArgumentOutOfRangeException(nameof(hours));
        Hour = hours % HoursInDay;
        Year = Math.DivRem((int)Math.Truncate(hours / HoursInDay), DaysInYear, out var day);
        Day = day;
    }

    /// <summary>
    ///     Compares this instance to another <see cref="RimWorldTime" />.
    /// </summary>
    /// <param name="other">The other <see cref="RimWorldTime" />.</param>
    /// <returns>
    ///     Less than zero if this is earlier, zero if equal, greater than zero if later.
    /// </returns>
    public int CompareTo(RimWorldTime other)
    {
        var yearComparison = Year.CompareTo(other.Year);
        if (yearComparison != 0) return yearComparison;
        var dayComparison = Day.CompareTo(other.Day);
        return dayComparison != 0 ? dayComparison : Hour.CompareTo(other.Hour);
    }

    /// <summary>
    ///     Compares this instance to another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>
    ///     Less than zero if this is earlier, zero if equal, greater than zero if later.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if object is not a <see cref="RimWorldTime" />.</exception>
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        return obj is RimWorldTime other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(RimWorldTime)}");
    }

    /// <summary>
    ///     Determines if one <see cref="RimWorldTime" /> is earlier than another.
    /// </summary>
    public static bool operator <(RimWorldTime left, RimWorldTime right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Determines if one <see cref="RimWorldTime" /> is later than another.
    /// </summary>
    public static bool operator >(RimWorldTime left, RimWorldTime right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Determines if one <see cref="RimWorldTime" /> is earlier than or equal to another.
    /// </summary>
    public static bool operator <=(RimWorldTime left, RimWorldTime right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Determines if one <see cref="RimWorldTime" /> is later than or equal to another.
    /// </summary>
    public static bool operator >=(RimWorldTime left, RimWorldTime right)
    {
        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    ///     Checks equality with another <see cref="RimWorldTime" />.
    /// </summary>
    /// <param name="other">The other <see cref="RimWorldTime" />.</param>
    /// <returns>True if equal, otherwise false.</returns>
    public bool Equals(RimWorldTime other)
    {
        return Year == other.Year && Day == other.Day && Hour.Equals(other.Hour);
    }

    /// <summary>
    ///     Checks equality with another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if equal, otherwise false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is RimWorldTime other && Equals(other);
    }

    /// <summary>
    ///     Gets the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Year;
            hashCode = (hashCode * 397) ^ Day;
            hashCode = (hashCode * 397) ^ Hour.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    ///     Determines if two <see cref="RimWorldTime" /> instances are equal.
    /// </summary>
    public static bool operator ==(RimWorldTime left, RimWorldTime right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Determines if two <see cref="RimWorldTime" /> instances are not equal.
    /// </summary>
    public static bool operator !=(RimWorldTime left, RimWorldTime right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     The day of the year.
    /// </summary>
    public readonly int Day;

    /// <summary>
    ///     The hour of the day.
    /// </summary>
    public readonly float Hour;

    /// <summary>
    ///     Gets the total hours represented by this instance.
    /// </summary>
    private float TotalHours => GetTotalHours(Year, Day, Hour);

    /// <summary>
    ///     The year.
    /// </summary>
    public readonly int Year;

    /// <summary>
    ///     Gets the current map time as a <see cref="RimWorldTime" />.
    /// </summary>
    /// <param name="map">The map to get time from.</param>
    /// <returns>The current <see cref="RimWorldTime" /> for the map.</returns>
    public static RimWorldTime GetMapTime(Map? map)
    {
        return map == null
            ? new RimWorldTime(0, 0, 0)
            : new RimWorldTime(GenLocalDate.Year(map), GenLocalDate.DayOfYear(map), GenLocalDate.HourFloat(map));
    }

    /// <summary>
    ///     Gets the home map time as a <see cref="RimWorldTime" />.
    /// </summary>
    /// <returns>
    ///     The current <see cref="RimWorldTime" /> for the player's home map.
    ///     If there is no home map, returns a default <see cref="RimWorldTime" /> (year 0, day 0, hour 0).
    /// </returns>
    public static RimWorldTime GetHomeTime()
    {
        return GetMapTime(Find.AnyPlayerHomeMap);
    }

    /// <summary>
    ///     Gets the difference in hours between two <see cref="RimWorldTime" /> instances.
    /// </summary>
    /// <param name="a">The first time.</param>
    /// <param name="b">The second time.</param>
    /// <returns>The difference in hours.</returns>
    public static float operator -(RimWorldTime a, RimWorldTime b)
    {
        return a.TotalHours - b.TotalHours;
    }

    /// <summary>
    ///     The number of days in a year.
    /// </summary>
    public const byte DaysInYear = QuadrumsInYear * DaysInQuadrum;

    /// <summary>
    ///     The number of quadrums in a year.
    /// </summary>
    public const byte QuadrumsInYear = 4;

    /// <summary>
    ///     The number of days in a quadrum.
    /// </summary>
    public const byte DaysInQuadrum = 15;

    /// <summary>
    ///     The number of hours in a day.
    /// </summary>
    public const byte HoursInDay = 24;

    /// <summary>
    ///     The number of hours in a year.
    /// </summary>
    public const uint HoursInYear = HoursInQuadrum * QuadrumsInYear;

    /// <summary>
    ///     The number of hours in a quadrum.
    /// </summary>
    public const uint HoursInQuadrum = HoursInDay * DaysInQuadrum;

    /// <summary>
    ///     Adds a number of hours to a <see cref="RimWorldTime" /> instance.
    /// </summary>
    /// <param name="a">The time.</param>
    /// <param name="b">The number of hours to add.</param>
    /// <returns>A new <see cref="RimWorldTime" /> with the added hours.</returns>
    public static RimWorldTime operator +(RimWorldTime a, float b)
    {
        return new RimWorldTime(a.TotalHours + b);
    }

    /// <summary>
    ///     Returns a string representation of the time span in years, days, and hours.
    /// </summary>
    /// <returns>
    ///     A string formatted as "{Year}y {Day}d {Hour}h", where the year and day are displayed as integers and the hour
    ///     with one decimal place.
    /// </returns>
    public override string ToString()
    {
        return $"{Year:N0}y {Day:N0}d {Hour:F.1}h";
    }
}