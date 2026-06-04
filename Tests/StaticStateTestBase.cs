namespace LordKuper.Common.Tests;

/// <summary>
///     Base class for test classes that mutate global static state
///     (<see cref="DefProvider.Current" />, <see cref="StatRanges" /> caches, etc.).
///     xUnit creates a fresh instance of the test class for every <c>[Fact]</c>, so
///     constructing a <see cref="StaticStateFixture" /> here and disposing it in
///     <see cref="Dispose" /> gives true <em>per-test</em> save/restore — matching the
///     requirement for static-state isolation per test.
/// </summary>
/// <remarks>
///     Test classes that inherit this base must also carry
///     <c>[Collection("StaticState")]</c> (which keeps <c>DisableParallelization = true</c>)
///     so that the static-touching classes run serially and cannot race each other.
///     The <see cref="IClassFixture{TFixture}" /> wiring is intentionally <em>not</em> used
///     here: <c>IClassFixture</c> is once-per-class, whereas <c>ctor + Dispose</c> on the
///     test-class instance itself is once-per-test (finding #1, simplification iter-01).
/// </remarks>
public abstract class StaticStateTestBase : IDisposable
{
    private readonly StaticStateFixture _fixture;

    protected StaticStateTestBase()
    {
        _fixture = new StaticStateFixture();
    }

    /// <summary>Restores all static state saved in the constructor.</summary>
    public void Dispose()
    {
        _fixture.Dispose();
        GC.SuppressFinalize(this);
    }
}
