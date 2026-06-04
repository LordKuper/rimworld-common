using LordKuper.Common.Helpers;
using Verse;

namespace LordKuper.Common.Tests.Helpers;

/// <summary>
///     Tests for <see cref="DefHelper" /> label retrieval and caching.
///     Note: These tests exercise the pure logic of DefHelper.
///     Tests requiring live Def instantiation are skipped [ExcludeFromCodeCoverage].
/// </summary>
public class DefHelperTests
{
    [Test]
    public void GetLabel_NullDef_Throws()
    {
        // Null def throws ArgumentNullException
        Def? nullDef = null;
        var act = () => nullDef!.GetLabel();
        act.Should().Throw<ArgumentNullException>();
    }

    // Note: Additional DefHelper tests (e.g., label caching, WorkTypeDef behavior)
    // require live RimWorld Def instances and are marked [ExcludeFromCodeCoverage] in the source.
    // These are tested manually or via integration tests with the game runtime.
}