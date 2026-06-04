namespace LordKuper.Common.Tests;

/// <summary>
///     Base class for test classes that mutate global static state
///     (<see cref="DefProvider.Current" />, <see cref="StatRanges" /> caches, etc.).
///     NUnit calls <c>[SetUp]</c> before each <c>[Test]</c> and <c>[TearDown]</c> after each
///     <c>[Test]</c> on the same instance, giving true <em>per-test</em> save/restore — the same
///     isolation granularity as the previous xUnit ctor + <c>Dispose</c> pattern.
/// </summary>
/// <remarks>
///     Test classes that inherit this base must also carry <c>[NonParallelizable]</c>
///     so that the static-touching classes run serially and cannot race each other.
///     NUnit is non-parallel by default; <c>[NonParallelizable]</c> makes the serialization
///     intent explicit and guards against a future accidental opt-in to assembly-level parallelism.
/// </remarks>
public abstract class StaticStateTestBase
{
    private StaticStateFixture? _fixture;

    /// <summary>Saves all static state before each test.</summary>
    [SetUp]
    public void SetUpStaticState()
    {
        _fixture = new StaticStateFixture();
    }

    /// <summary>Restores all static state saved before the test.</summary>
    [TearDown]
    public void TearDownStaticState()
    {
        _fixture?.Dispose();
        _fixture = null;
    }
}