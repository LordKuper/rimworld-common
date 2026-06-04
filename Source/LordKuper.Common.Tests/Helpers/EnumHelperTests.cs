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
    [Test]
    public void AbsentFlags_AllFlagsPresent_ReturnsNone()
    {
        const TestFlags value =
            TestFlags.FlagA | TestFlags.FlagB | TestFlags.FlagC | TestFlags.FlagD;
        var absent = EnumHelper.AbsentFlags(value);
        absent.Should().Be(TestFlags.None);
    }

    [Test]
    public void AbsentFlags_ReturnsAbsentFlags()
    {
        const TestFlags value = TestFlags.FlagA | TestFlags.FlagC;
        var absent = EnumHelper.AbsentFlags(value);
        // Preserve value-comparison form: HasFlag checks as individual .Be() assertions
        absent.HasFlag(TestFlags.FlagB).Should().BeTrue();
        absent.HasFlag(TestFlags.FlagD).Should().BeTrue();
        absent.HasFlag(TestFlags.FlagA).Should().BeFalse();
        absent.HasFlag(TestFlags.FlagC).Should().BeFalse();
    }

    [Test]
    public void GetUniqueFlags_ReturnsAllSetFlags()
    {
        const TestFlags value = TestFlags.FlagA | TestFlags.FlagC | TestFlags.FlagD;
        var unique = EnumHelper.GetUniqueFlags(value);
        var result = new HashSet<TestFlags>(unique);
        result.Should().Contain(TestFlags.FlagA);
        result.Should().Contain(TestFlags.FlagC);
        result.Should().Contain(TestFlags.FlagD);
        result.Should().NotContain(TestFlags.FlagB);
        result.Should().NotContain(TestFlags.None);
    }

    [Test]
    public void GetUniqueFlags_ReturnsUniqueFlagsExcludingSpecified()
    {
        const TestFlags value = TestFlags.FlagA | TestFlags.FlagB | TestFlags.FlagC;
        const TestFlags excluded = TestFlags.FlagB;
        var unique = EnumHelper.GetUniqueFlags(value, excluded);
        var result = new HashSet<TestFlags>(unique);
        result.Should().Contain(TestFlags.FlagA);
        result.Should().Contain(TestFlags.FlagC);
        result.Should().NotContain(TestFlags.FlagB);
    }

    [Test]
    public void GetUniqueFlags_SingleFlag_ReturnsThatFlag()
    {
        var unique = EnumHelper.GetUniqueFlags(TestFlags.FlagB);
        var result = new List<TestFlags>(unique);
        result.Should().ContainSingle();
        result[0].Should().Be(TestFlags.FlagB);
    }

    [Test]
    public void GetUniqueFlags_ZeroValue_ReturnsEmpty()
    {
        var unique = EnumHelper.GetUniqueFlags(TestFlags.None, TestFlags.None);
        unique.Should().BeEmpty();
    }

    [Test]
    public void GetUniqueFlags_ZeroValue_ReturnsEmptyCollection()
    {
        var unique = EnumHelper.GetUniqueFlags(TestFlags.None);
        unique.Should().BeEmpty();
    }

    // [TestCase] only — no standalone [Test] on this parameterized method.
    // Enum-flag expressions like TestFlags.FlagA | TestFlags.FlagB are valid [TestCase] args
    // because they are constant expressions.
    [TestCase(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagA, true)]
    [TestCase(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagA | TestFlags.FlagB, true)]
    [TestCase(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagC, false)]
    [TestCase(TestFlags.FlagA | TestFlags.FlagB, TestFlags.None, false)]
    public void HasAllFlags_ReturnsExpected(TestFlags value, TestFlags flags, bool expected)
    {
        EnumHelper.HasAllFlags(value, flags).Should().Be(expected);
    }

    // [TestCase] only — no standalone [Test] on this parameterized method.
    [TestCase(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagA, true)]
    [TestCase(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagC, false)]
    [TestCase(TestFlags.FlagA | TestFlags.FlagB, TestFlags.FlagB | TestFlags.FlagC, true)]
    [TestCase(TestFlags.FlagA | TestFlags.FlagB, TestFlags.None, false)]
    public void HasAnyFlag_ReturnsExpected(TestFlags value, TestFlags flags, bool expected)
    {
        EnumHelper.HasAnyFlag(value, flags).Should().Be(expected);
    }
}