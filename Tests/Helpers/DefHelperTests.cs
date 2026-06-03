using LordKuper.Common.Helpers;
using Xunit;

namespace LordKuper.Common.Tests.Helpers;

/// <summary>
///     Tests for <see cref="DefHelper" /> label retrieval and caching (AC-17, AC-20).
///     Note: These tests exercise the pure logic of DefHelper.
///     Tests requiring live Def instantiation are skipped [ExcludeFromCodeCoverage].
/// </summary>
public class DefHelperTests
{
    [Fact]
    public void GetLabel_NullDef_Throws()
    {
        // AC-17: Null def throws ArgumentNullException
        Verse.Def? nullDef = null;

        Assert.Throws<ArgumentNullException>(() => nullDef!.GetLabel());
    }

    // Note: Additional DefHelper tests (e.g., label caching, WorkTypeDef behavior)
    // require live RimWorld Def instances and are marked [ExcludeFromCodeCoverage] in the source.
    // These are tested manually or via integration tests with the game runtime.
}
