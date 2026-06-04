namespace LordKuper.Common.Tests;

/// <summary>
///     Tests for <see cref="RimWorldTime" /> pure paths (arithmetic, comparison, equality, operators).
/// </summary>
public class RimWorldTimeTests
{
    [Test]
    public void CompareTo_EarlierTime_ReturnsNegative()
    {
        var earlier = new RimWorldTime(0, 0, 0f);
        var later = new RimWorldTime(1, 0, 0f);
        // Preserve value-comparison form: CompareTo result < 0
        earlier.CompareTo(later).Should().BeLessThan(0);
    }

    [Test]
    public void CompareTo_LaterTime_ReturnsPositive()
    {
        var earlier = new RimWorldTime(1, 0, 0f);
        var later = new RimWorldTime(2, 0, 0f);
        // Preserve value-comparison form: CompareTo result > 0
        later.CompareTo(earlier).Should().BeGreaterThan(0);
    }

    [Test]
    public void CompareTo_Object_WithNonRimWorldTime_Throws()
    {
        var time = new RimWorldTime(1, 0, 0f);
        var act = () => time.CompareTo("not a rimworld time");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CompareTo_Object_WithNull_ReturnsPositive()
    {
        var time = new RimWorldTime(1, 0, 0f);
        time.CompareTo(null).Should().Be(1);
    }

    [Test]
    public void CompareTo_SameValue_ReturnsZero()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        var time2 = new RimWorldTime(1, 5, 10.5f);
        time1.CompareTo(time2).Should().Be(0);
    }

    [Test]
    public void CompareTo_SameYearDaySameDay_ComparesByHour()
    {
        var hour1 = new RimWorldTime(1, 5, 10f);
        var hour2 = new RimWorldTime(1, 5, 11f);
        // Preserve value-comparison form: CompareTo result < 0
        hour1.CompareTo(hour2).Should().BeLessThan(0);
    }

    [Test]
    public void CompareTo_SameYearDifferentDay_ComparesByDay()
    {
        var day1 = new RimWorldTime(1, 5, 10f);
        var day2 = new RimWorldTime(1, 6, 10f);
        // Preserve value-comparison form: CompareTo result < 0
        day1.CompareTo(day2).Should().BeLessThan(0);
    }

    [Test]
    public void Ctor_FromTotalHours_CalculatesYearDayHour()
    {
        // 1 year = 60 days * 24 hours = 1440 hours
        // 1 day = 24 hours
        // Total: 2 years + 3 days + 5 hours = 2*1440 + 3*24 + 5 = 2880 + 72 + 5 = 2957 hours
        var time = new RimWorldTime(2957f);
        time.Year.Should().Be(2);
        time.Day.Should().Be(3);
        time.Hour.Should().Be(5f);
    }

    [Test]
    public void Ctor_FromTotalHours_NegativeThrows()
    {
        // Negative total hours throws ArgumentOutOfRangeException
        var act = () => new RimWorldTime(-1f);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Ctor_FromTotalHours_Zero_ValidatesAtOrigin()
    {
        var time = new RimWorldTime(0f);
        time.Year.Should().Be(0);
        time.Day.Should().Be(0);
        time.Hour.Should().Be(0f);
    }

    [Test]
    public void Ctor_FromYearDayHour_CalculatesTotalHours()
    {
        // Constructor from year/day/hour components
        var time = new RimWorldTime(1, 5, 12.5f);
        time.Year.Should().Be(1);
        time.Day.Should().Be(5);
        time.Hour.Should().Be(12.5f);
    }

    [Test]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        var time2 = new RimWorldTime(1, 5, 11.5f);
        time1.Equals(time2).Should().BeFalse();
    }

    [Test]
    public void Equals_Object_WithDifferentType_ReturnsFalse()
    {
        var time = new RimWorldTime(1, 5, 10.5f);
        object notATime = "not a time";
        time.Equals(notATime).Should().BeFalse();
    }

    [Test]
    public void Equals_Object_WithSameValue_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        object time2 = new RimWorldTime(1, 5, 10.5f);
        time1.Equals(time2).Should().BeTrue();
    }

    [Test]
    public void Equals_SameValue_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        var time2 = new RimWorldTime(1, 5, 10.5f);
        time1.Equals(time2).Should().BeTrue();
    }

    [Test]
    public void GetHashCode_SameValue_SameHash()
    {
        var time1 = new RimWorldTime(1, 5, 10.5f);
        var time2 = new RimWorldTime(1, 5, 10.5f);
        time1.GetHashCode().Should().Be(time2.GetHashCode());
    }

    [Test]
    public void OperatorEqual_DifferentValue_ReturnsFalse()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 11f);
        (time1 == time2).Should().BeFalse();
    }

    [Test]
    public void OperatorEqual_SameValue_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 10f);
        (time1 == time2).Should().BeTrue();
    }

    [Test]
    public void OperatorGreaterThanOrEqual_LaterTime_ReturnsTrue()
    {
        var later = new RimWorldTime(1, 0, 0f);
        var earlier = new RimWorldTime(0, 0, 0f);
        (later >= earlier).Should().BeTrue();
    }

    [Test]
    public void OperatorGreaterThanOrEqual_SameTime_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 10f);
        (time1 >= time2).Should().BeTrue();
    }

    [Test]
    public void OperatorGreaterThan_EarlierTime_ReturnsFalse()
    {
        var earlier = new RimWorldTime(0, 0, 0f);
        var later = new RimWorldTime(1, 0, 0f);
        (earlier > later).Should().BeFalse();
    }

    [Test]
    public void OperatorGreaterThan_LaterTime_ReturnsTrue()
    {
        var later = new RimWorldTime(1, 0, 0f);
        var earlier = new RimWorldTime(0, 0, 0f);
        (later > earlier).Should().BeTrue();
    }

    [Test]
    public void OperatorLessThanOrEqual_EarlierTime_ReturnsTrue()
    {
        var earlier = new RimWorldTime(0, 0, 0f);
        var later = new RimWorldTime(1, 0, 0f);
        (earlier <= later).Should().BeTrue();
    }

    [Test]
    public void OperatorLessThanOrEqual_SameTime_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 10f);
        (time1 <= time2).Should().BeTrue();
    }

    [Test]
    public void OperatorLessThan_EarlierTime_ReturnsTrue()
    {
        var earlier = new RimWorldTime(0, 0, 0f);
        var later = new RimWorldTime(1, 0, 0f);
        (earlier < later).Should().BeTrue();
    }

    [Test]
    public void OperatorLessThan_LaterTime_ReturnsFalse()
    {
        var later = new RimWorldTime(1, 0, 0f);
        var earlier = new RimWorldTime(0, 0, 0f);
        (later < earlier).Should().BeFalse();
    }

    [Test]
    public void OperatorMinus_TwoTimes_ReturnsDifferenceInHours()
    {
        var time1 = new RimWorldTime(1, 0, 10f);
        var time2 = new RimWorldTime(1, 0, 5f);
        var diff = time1 - time2;
        diff.Should().Be(5f);
    }

    [Test]
    public void OperatorNotEqual_DifferentValue_ReturnsTrue()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 11f);
        (time1 != time2).Should().BeTrue();
    }

    [Test]
    public void OperatorNotEqual_SameValue_ReturnsFalse()
    {
        var time1 = new RimWorldTime(1, 5, 10f);
        var time2 = new RimWorldTime(1, 5, 10f);
        (time1 != time2).Should().BeFalse();
    }

    [Test]
    public void OperatorPlus_TimeAndHours_HandlesOverflow()
    {
        var time = new RimWorldTime(1, 0, 20f);
        var result = time + 10f; // 30 hours = 1 day + 6 hours
        result.Day.Should().Be(1);
        result.Hour.Should().Be(6f);
    }

    [Test]
    public void OperatorPlus_TimeAndHours_ReturnsNewTime()
    {
        var time = new RimWorldTime(1, 0, 10f);
        var result = time + 5f;
        result.Hour.Should().Be(15f);
    }

    [Test]
    public void ToString_FormatsCorrectly()
    {
        var time = new RimWorldTime(1, 5, 10.5f);
        var str = time.ToString();
        str.Should().Contain("1"); // year
        str.Should().Contain("5"); // day
        str.Should().Contain("10"); // hour
    }
}