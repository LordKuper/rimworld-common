using LordKuper.Common.Helpers;

namespace LordKuper.Common.Tests.Helpers;

[Flags]
public enum TestFlags
{
    None = 0,
    FlagA = 1 << 0,
    FlagB = 1 << 1,
    FlagC = 1 << 2,
    FlagD = 1 << 3
}

public class EnumHelperTests
{
    [Theory]
    [InlineData(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagA, true)]
    [InlineData(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagA | TestFlags.FlagB, true)]
    [InlineData(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagC, false)]
    [InlineData(TestFlags.FlagA | TestFlags.FlagB, TestFlags.None, false)]
    public void HasAllFlags_ReturnsExpected(TestFlags value, TestFlags flags, bool expected)
    {
        Assert.Equal(expected, EnumHelper.HasAllFlags(value, flags));
    }

    [Theory]
    [InlineData(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagA, true)]
    [InlineData(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagC, false)]
    [InlineData(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagB | TestFlags.FlagC, true)]
    [InlineData(TestFlags.FlagA | TestFlags.FlagB, TestFlags.None, false)]
    public void HasAnyFlag_ReturnsExpected(TestFlags value, TestFlags flags, bool expected)
    {
        Assert.Equal(expected, EnumHelper.HasAnyFlag(value, flags));
    }

    [Fact]
    public void AbsentFlags_AllFlagsPresent_ReturnsNone()
    {
        const TestFlags value = TestFlags.FlagA | TestFlags.FlagB | TestFlags.FlagC | TestFlags.FlagD;
        var absent = EnumHelper.AbsentFlags(value);
        Assert.Equal(TestFlags.None, absent);
    }

    [Fact]
    public void AbsentFlags_ReturnsAbsentFlags()
    {
        const TestFlags value = TestFlags.FlagA | TestFlags.FlagC;
        var absent = EnumHelper.AbsentFlags(value);
        Assert.True(absent.HasFlag(TestFlags.FlagB));
        Assert.True(absent.HasFlag(TestFlags.FlagD));
        Assert.False(absent.HasFlag(TestFlags.FlagA));
        Assert.False(absent.HasFlag(TestFlags.FlagC));
    }

    [Fact]
    public void GetUniqueFlags_ReturnsAllSetFlags()
    {
        const TestFlags value = TestFlags.FlagA | TestFlags.FlagC | TestFlags.FlagD;
        var unique = EnumHelper.GetUniqueFlags(value);
        var result = new HashSet<TestFlags>(unique);
        Assert.Contains(TestFlags.FlagA, result);
        Assert.Contains(TestFlags.FlagC, result);
        Assert.Contains(TestFlags.FlagD, result);
        Assert.DoesNotContain(TestFlags.FlagB, result);
        Assert.DoesNotContain(TestFlags.None, result);
    }

    [Fact]
    public void GetUniqueFlags_ReturnsUniqueFlagsExcludingSpecified()
    {
        const TestFlags value = TestFlags.FlagA | TestFlags.FlagB | TestFlags.FlagC;
        const TestFlags excluded = TestFlags.FlagB;
        var unique = EnumHelper.GetUniqueFlags(value, excluded);
        var result = new HashSet<TestFlags>(unique);
        Assert.Contains(TestFlags.FlagA, result);
        Assert.Contains(TestFlags.FlagC, result);
        Assert.DoesNotContain(TestFlags.FlagB, result);
    }

    [Fact]
    public void GetUniqueFlags_SingleFlag_ReturnsThatFlag()
    {
        var unique = EnumHelper.GetUniqueFlags(TestFlags.FlagB);
        var result = new List<TestFlags>(unique);
        Assert.Single(result);
        Assert.Equal(TestFlags.FlagB, result[0]);
    }

    [Fact]
    public void GetUniqueFlags_ZeroValue_ReturnsEmpty()
    {
        var unique = EnumHelper.GetUniqueFlags(TestFlags.None, TestFlags.None);
        Assert.Empty(unique);
    }

    [Fact]
    public void GetUniqueFlags_ZeroValue_ReturnsEmptyCollection()
    {
        var unique = EnumHelper.GetUniqueFlags(TestFlags.None);
        Assert.Empty(unique);
    }
}