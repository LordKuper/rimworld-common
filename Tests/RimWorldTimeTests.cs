using LordKuper.Common;

namespace LordKuper.Common.Tests;

/// <summary>
///     Tests for <see cref="RimWorldTime" /> pure paths (arithmetic, comparison, equality, operators).
///     AC-17: pure-path coverage for RimWorldTime.
/// </summary>
public class RimWorldTimeTests
{
    [Fact]
    public void Ctor_FromYearDayHour_CalculatesTotalHours()
    {
        // AC-17: constructor from year/day/hour components
        var time = new RimWorldTime(1, 5, 12.5f);
        Assert.Equal(1, time.Year);
        Assert.Equal(5, time.Day);
        Assert.Equal(12.5f, time.Hour);
    }

    [Fact]
    public void Ctor_FromTotalHours_NegativeThrows()
    {
        // AC-17: negative total hours throws ArgumentOutOfRangeException
        Assert.Throws<ArgumentOutOfRangeException>(() => new RimWorldTime(-1f));
    }

    [Fact]
    public void Ctor_FromTotalHours_Zero_ValidatesAtOrigin()
    {
        var time = new RimWorldTime(0f);
        Assert.Equal(0, time.Year);
        Assert.Equal(0, time.Day);
        Assert.Equal(0f, time.Hour);
    }

    [Fact]
    public void Ctor_FromTotalHours_CalculatesYearDayHour()
    {
        // 1 year = 60 days * 24 hours = 1440 hours
        // 1 day = 24 hours
        // Total: 2 years + 3 days + 5 hours = 2*1440 + 3*24 + 5 = 2880 + 72 + 5 = 2957 hours
        var time = new RimWorldTime(2957f);
        Assert.Equal(2, time.Year);
        Assert.Equal(3, time.Day);
        Assert.Equal(5f, time.Hour);
    }

    [Fact]
    public void CompareTo_SameValue_ReturnsZero()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        var time2 = new RimWorldTime(1, 5, 10.5f);
        Assert.Equal(0, time1.CompareTo(time2));
    }

    [Fact]
    public void CompareTo_EarlierTime_ReturnsNegative()
    {
        var earlier = new RimWorldTime(0, 0, 0f);
        var later = new RimWorldTime(1, 0, 0f);
        Assert.True(earlier.CompareTo(later) < 0);
    }

    [Fact]
    public void CompareTo_LaterTime_ReturnsPositive()
    {
        var earlier = new RimWorldTime(1, 0, 0f);
        var later = new RimWorldTime(2, 0, 0f);
        Assert.True(later.CompareTo(earlier) > 0);
    }

    [Fact]
    public void CompareTo_SameYearDifferentDay_ComparesByDay()
    {
        var day1 = new RimWorldTime(1, 5, 10f);
        var day2 = new RimWorldTime(1, 6, 10f);
        Assert.True(day1.CompareTo(day2) < 0);
    }

    [Fact]
    public void CompareTo_SameYearDaySameDay_ComparesByHour()
    {
        var hour1 = new RimWorldTime(1, 5, 10f);
        var hour2 = new RimWorldTime(1, 5, 11f);
        Assert.True(hour1.CompareTo(hour2) < 0);
    }

    [Fact]
    public void CompareTo_Object_WithNull_ReturnsPositive()
    {
        var time = new RimWorldTime(1, 0, 0f);
        Assert.Equal(1, time.CompareTo((object?)null));
    }

    [Fact]
    public void CompareTo_Object_WithNonRimWorldTime_Throws()
    {
        var time = new RimWorldTime(1, 0, 0f);
        Assert.Throws<ArgumentException>(() => time.CompareTo("not a rimworld time"));
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        var time2 = new RimWorldTime(1, 5, 10.5f);
        Assert.True(time1.Equals(time2));
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        var time2 = new RimWorldTime(1, 5, 11.5f);
        Assert.False(time1.Equals(time2));
    }

    [Fact]
    public void Equals_Object_WithSameValue_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        object time2 = new RimWorldTime(1, 5, 10.5f);
        Assert.True(time1.Equals(time2));
    }

    [Fact]
    public void Equals_Object_WithDifferentType_ReturnsFalse()
    {
        var time = new RimWorldTime(1, 5, 10.5f);
        Assert.False(time.Equals("not a time"));
    }

    [Fact]
    public void GetHashCode_SameValue_SameHash()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        var time2 = new RimWorldTime(1, 5, 10.5f);
        Assert.Equal(time1.GetHashCode(), time2.GetHashCode());
    }

    [Fact]
    public void OperatorLessThan_EarlierTime_ReturnsTrue()
    {
        var earlier = new RimWorldTime(0, 0, 0f);
        var later = new RimWorldTime(1, 0, 0f);
        Assert.True(earlier < later);
    }

    [Fact]
    public void OperatorLessThan_LaterTime_ReturnsFalse()
    {
        var later = new RimWorldTime(1, 0, 0f);
        var earlier = new RimWorldTime(0, 0, 0f);
        Assert.False(later < earlier);
    }

    [Fact]
    public void OperatorGreaterThan_LaterTime_ReturnsTrue()
    {
        var later = new RimWorldTime(1, 0, 0f);
        var earlier = new RimWorldTime(0, 0, 0f);
        Assert.True(later > earlier);
    }

    [Fact]
    public void OperatorGreaterThan_EarlierTime_ReturnsFalse()
    {
        var earlier = new RimWorldTime(0, 0, 0f);
        var later = new RimWorldTime(1, 0, 0f);
        Assert.False(earlier > later);
    }

    [Fact]
    public void OperatorLessThanOrEqual_EarlierTime_ReturnsTrue()
    {
        var earlier = new RimWorldTime(0, 0, 0f);
        var later = new RimWorldTime(1, 0, 0f);
        Assert.True(earlier <= later);
    }

    [Fact]
    public void OperatorLessThanOrEqual_SameTime_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 10f);
        Assert.True(time1 <= time2);
    }

    [Fact]
    public void OperatorGreaterThanOrEqual_LaterTime_ReturnsTrue()
    {
        var later = new RimWorldTime(1, 0, 0f);
        var earlier = new RimWorldTime(0, 0, 0f);
        Assert.True(later >= earlier);
    }

    [Fact]
    public void OperatorGreaterThanOrEqual_SameTime_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 10f);
        Assert.True(time1 >= time2);
    }

    [Fact]
    public void OperatorEqual_SameValue_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 10f);
        Assert.True(time1 == time2);
    }

    [Fact]
    public void OperatorEqual_DifferentValue_ReturnsFalse()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 11f);
        Assert.False(time1 == time2);
    }

    [Fact]
    public void OperatorNotEqual_SameValue_ReturnsFalse()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 10f);
        Assert.False(time1 != time2);
    }

    [Fact]
    public void OperatorNotEqual_DifferentValue_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 11f);
        Assert.True(time1 != time2);
    }

    [Fact]
    public void OperatorMinus_TwoTimes_ReturnsDifferenceInHours()
    {
        var time1 = new RimWorldTime(1, 0, 10f);
        var time2 = new RimWorldTime(1, 0, 5f);
        var diff = time1 - time2;
        Assert.Equal(5f, diff);
    }

    [Fact]
    public void OperatorPlus_TimeAndHours_ReturnsNewTime()
    {
        var time = new RimWorldTime(1, 0, 10f);
        var result = time + 5f;
        Assert.Equal(15f, result.Hour);
    }

    [Fact]
    public void OperatorPlus_TimeAndHours_HandlesOverflow()
    {
        var time = new RimWorldTime(1, 0, 20f);
        var result = time + 10f; // 30 hours = 1 day + 6 hours
        Assert.Equal(1, result.Day);
        Assert.Equal(6f, result.Hour);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var time = new RimWorldTime(1, 5, 10.5f);
        var str = time.ToString();
        Assert.Contains("1", str); // year
        Assert.Contains("5", str); // day
        Assert.Contains("10", str); // hour
    }
}
