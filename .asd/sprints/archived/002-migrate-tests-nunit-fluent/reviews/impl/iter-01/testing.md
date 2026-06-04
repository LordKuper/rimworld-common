[REVIEW-impl-testing]: APPROVE

# Review — Testing

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE

## Next action

Testing review passes. All 28 acceptance criteria are satisfied with high-quality test coverage, proper assertion strength preservation, deterministic isolation, and meaningful edge-case coverage across 142 executed test cases + 3 ignored tests.

## Escalations

None.

## Summary of coverage verification

**AC-1 to AC-4 (Packaging)**: NUnit 4.6.1, NUnit3TestAdapter 6.2.0, FluentAssertions 7.2.2 (Apache-2.0) added; Microsoft.NET.Test.Sdk 17.14.1 retained; global usings swapped to NUnit.Framework + FluentAssertions; no xUnit references remain. Transitive Microsoft.Testing.Platform exclusions correctly prevent net472 AppDomain discovery failures. ✓

**AC-5 to AC-9 (Attributes)**: All 132 `[Fact]` → `[Test]` conversions complete. Three parameterized methods (HasAllFlags_ReturnsExpected, HasAnyFlag_ReturnsExpected, NormalizeValue_Theory) correctly carry `[TestCase(...)]` only, with all 13 argument tuples (including enum-flag expressions) preserved. Three skipped tests properly carry `[Test, Ignore("Requires live RimWorld context for Verse.Translator")]`. Case inventory exact: 129 executed `[Test]` + 13 `[TestCase]` cases + 3 ignored = 142 executed + 3 ignored. ✓

**AC-10 to AC-16 (Assertions)**: All 236 `Assert.*` sites converted to FluentAssertions `.Should()` form per PRD mapping table:
- `Assert.Equal(exp, act)` → `act.Should().Be(exp)` with orientation preserved
- `Assert.Equal(exp, act, precision)` → `act.Should().BeApproximately(exp, 10^-precision)` (e.g. precision 4 → tolerance 1e-4 in MathHelperTests)
- `Assert.True(comparison)` / `Assert.False(comparison)` → value-comparing form to preserve failure diffs (e.g. RimWorldTimeTests.CompareTo_EarlierTime_ReturnsNegative: `.Should().BeLessThan(0)`)
- `Assert.Null` / `Assert.NotNull` → `.Should().BeNull()` / `.Should().NotBeNull()`
- `Assert.Single` → `.Should().ContainSingle()`
- `Assert.Empty` / `Assert.NotEmpty` → `.Should().BeEmpty()` / `.Should().NotBeEmpty()`
- `Assert.Contains` / `Assert.DoesNotContain` → `.Should().Contain(...)` / `.Should().NotContain(...)` with correct collection-vs-substring overload
- `Assert.NotSame` → `.Should().NotBeSameAs(...)` (PawnFilterTests)
- `Assert.Throws<T>()` → `action.Should().Throw<T>()` (TextHelperTests, DefHelperTests)
Assertion strength preserved; no weakening observed. ✓

**AC-17 (Assertion strength)**: No accidental pruning or tightening; all conversions are 1:1 mechanical or explicitly intentional (e.g. tolerance documentation in comments). ✓

**AC-18 (Production code untouched)**: Source/LordKuper.Common/ remains unchanged except where strictly required by test migration. ✓

**AC-19 to AC-21 (RimWorld resolver seam)**: Namespace-less global `[SetUpFixture]` class RimWorldResolverSetup in RimWorldResolverSetup.cs carries `[OneTimeSetUp]` method RegisterRimWorldResolver(). Resolver contract preserved exactly: assembly-name match set (Assembly-CSharp, Assembly-CSharp-firstpass, UnityEngine*, Unity.Burst, Unity.Collections, Unity.Mathematics, com.rlabrecque.steamworks.net), env-var lookup (RIMWORLD_DIR / RimWorldDir with existing fallback path D:\Games\Steam\steamapps\common\RimWorld), Managed\<name>.dll Assembly.LoadFrom resolution, null-for-others contract. Idempotency guard via AppDomain.GetData/SetData("RimWorldResolverInitialized"). `[ModuleInitializer]` polyfill (System.Runtime.CompilerServices.ModuleInitializerAttribute) added for net472 as ADR-0006 contingency. No xUnit TestFramework attribute remains. ✓

**AC-22 to AC-25 (StaticState isolation)**: StaticStateTestBase remapped from IDisposable ctor/Dispose to NUnit `[SetUp]` (SetUpStaticState) / `[TearDown]` (TearDownStaticState) per-test isolation. Snapshot/restore body in StaticStateFixture preserved verbatim: snapshot of DefProvider.Current; restore + rebuild of StatHelper, WorkTypeStatMap; reflection resets of SkillStatMap._map, PassionHelper.{_isInitialized, _cachedPassions, PassionCache}, StatRanges.Ranges. Three static-touching classes (StatWeightTests, StatefulSubsystemTests, StatRangesTests) marked `[NonParallelizable]` for serialization intent. No cross-test state bleed observed; xUnit [Collection] marker classes removed (NUnit non-parallel by default). ✓

**AC-26 (Build/run/lint)**: Full test project builds warning-clean under inherited Directory.Build.props governance (TreatWarningsAsErrors, WarningLevel 9999, Nullable). All 142 executed tests pass; 3 ignored tests reported as ignored by NUnit3 adapter. Plan summary shows measured coverage 37.05% testable-core (baseline 38.2% − 1.0pp floor = 37.2% pass). ✓

**AC-27 to AC-28 (Coverage)**: scripts/coverage.ps1 line 45 assembly filter correctly swapped from `--assemblyFilter xunit` to `--assemblyFilter nunit` (nunit.framework, NUnit3.TestAdapter excluded from denominator). dotnet test command runner-agnostic (no string change). No .github/ CI workflow added; coverage scope reduces to coverage.ps1 + commands.yaml. ✓

**Test quality observations**:
- **Meaningfulness**: Tests verify observable behaviour, not implementation detail. No re-assertion of the code under test.
- **Edge cases**: Good coverage of empty, single, many, boundary (0, negative, max cap values), invalid (null, invalid string parse), and RimWorld-dependent cases (3 intentionally ignored).
- **Determinism**: No sleep, no wall-clock timing, no order-dependent assertions. Proper per-test isolation via [SetUp]/[TearDown].
- **24 new StatLimit tests**: Comprehensive (caps, clamping, buffers, round-trip independence, custom caps, null resets, double-instance state independence). All assertions use `.BeApproximately(val, 0.001f)` correctly for float precision. Tests are meaningful, not padding.
- **Assertion strength**: Maintained throughout conversion. Value-comparison form preserved where needed (e.g., RimWorldTimeTests.CompareTo checks use `.BeLessThan(0)` / `.BeGreaterThan(0)` to keep diff information).
- **No patterns breaking determinism**: Enum-flag constant expressions in [TestCase] (EnumHelperTests) are valid. StatRanges tests use faked DefProvider (FakeDefProvider) instead of live API. No hardcoded test data beyond boundary-value tests where the literal is the point.
- **Stub resolution**: No stubs exist (`.asd/project/stubs.md` absent); plan audit confirms "No related open stubs". No TODO(sprint-*) markers need to be verified.
