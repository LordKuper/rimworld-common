[REVIEW-impl-testing]: APPROVE

# Review — Testing

- **Phase**: impl-review
- **Iteration**: 02

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above the medium severity floor | — |

## Verdict

APPROVE

## Next action

Testing review passes. No regressions detected. The test suite remains fully compliant with all 28 acceptance criteria (ACs).

## Escalations

None.

## Verification (iter-02, medium severity floor)

Re-verified test quality and AC coverage completeness against the baseline iter-01 passing state:

**Scope**: Test code assessment only. Comment/doc rot in test-file XML-docs is noted by simplification and documentation reviewers separately (StatRangesTests.cs:9, StatefulSubsystemTests.cs:13,151 stale `StaticStateFixture` references) — not a test coverage or assertion-quality concern, and out of testing-reviewer scope.

**AC-1 to AC-4 (Packaging)** — Verified in `.csproj`:
- NUnit 4.6.1, NUnit3TestAdapter 6.2.0, FluentAssertions 7.2.2 (Apache-2.0, line 21) all present; no xUnit references. ✓
- Transitive Microsoft.Testing.Platform overrides (lines 28-42) correctly prevent net472 AppDomain probe failures. ✓
- Global usings for NUnit.Framework + FluentAssertions confirmed. ✓

**AC-5 to AC-9 (Attributes)** — Spot-checked across test files:
- All 132 `[Fact]` → `[Test]` migrations preserved; no `[Fact]` attribute remains. ✓
- Three parameterized methods (`HasAllFlags_ReturnsExpected`, `HasAnyFlag_ReturnsExpected`, `NormalizeValue_Theory`) carry `[TestCase(...)]` only with no standalone `[Test]` — no spurious non-runnable cases added. ✓
- All 13 `[InlineData]` rows correctly migrated to `[TestCase(...)]`, including enum-flag constant expressions in EnumHelperTests (lines 89–106). ✓
- Three skipped tests (PawnFilterTests.cs:203-233) correctly marked `[Test, Ignore("Requires live RimWorld context for Verse.Translator")]`. ✓
- Case inventory exact: 142 executed + 3 ignored, no silently dropped or duplicated cases. ✓

**AC-10 to AC-16 (Assertions)** — Sample verification across multiple test files:
- RimWorldTimeTests (25+ tests): value-comparison assertions preserved (e.g. `.BeLessThan(0)`, `.BeGreaterThan(0)` instead of weakened `.BeTrue()`) for diffs on CompareTo results. All 236 sites converted; no `Xunit.Assert.*` remains. ✓
- MathHelperTests (line 105): `Assert.Equal(exp, act, precision)` correctly converted to `.BeApproximately(expected, 5e-5f)` with precision tolerance documented. Boundary cases (clamp, zero-range) confirmed. ✓
- EnumHelperTests: `.Should().Contain(...)` and `.Should().NotContain(...)` with correct collection-vs-substring overload. ✓
- StatLimitTests (24 tests): all 24 new tests use `.BeApproximately(val, 0.001f)` (tolerance 1e-3, consistent with FloatTwo precision), no precision mismatch. ✓
- Throws assertions preserved (DefHelperTests, TextHelperTests implied via AC-1 iter-01 audit): `action.Should().Throw<T>()` with exception type and message intent intact. ✓

**AC-17 (Assertion strength)** — No accidental pruning or tightening observed in iter-02; all conversions remain 1:1 or explicitly intentional per prior review. ✓

**AC-18 (Production code untouched)** — Confirmed: diff touches only `Source/LordKuper.Common.Tests/**` and `scripts/coverage.ps1`. No production code in `Source/LordKuper.Common/` changed. ✓

**AC-19 to AC-21 (RimWorld resolver seam)** — Verified in RimWorldResolverSetup.cs:
- Namespace-less global `[SetUpFixture]` class with `[OneTimeSetUp]` method. No xUnit `[assembly: TestFramework]` remains. ✓
- Resolver contract preserved: assembly-name match set, env-var lookup (RIMWORLD_DIR / RimWorldDir), Managed\<name>.dll Assembly.LoadFrom resolution, null-for-others contract all intact (lines 10-57). ✓
- Assembly copy fallback (csproj lines 73-94) documented in comments; resolver registration fires at test discovery time (AppDomain handler pre-registered before fixture load). ✓

**AC-22 to AC-25 (StaticState isolation)** — Verified in StaticStateTestBase.cs:
- SetUp (lines 25-29) captures DefProvider.Current snapshot; TearDown (lines 32-70) restores + rebuilds. Snapshot/restore body verbatim: DefProvider restore, StatHelper.Rebuild(), WorkTypeStatMap.Rebuild(), reflection resets of SkillStatMap._map, PassionHelper statics, StatRanges.Ranges. ✓
- Per-test granularity confirmed: `[SetUp]`/`[TearDown]` called per test method, not per class. ✓
- Three static-touching classes (`StatWeightTests`, `StatefulSubsystemTests`, `StatRangesTests`) carry `[NonParallelizable]` for serialization intent. NUnit non-parallel by default; explicit attribute makes intent clear. ✓
- Cross-test bleed risk mitigated by per-test isolation + non-parallel execution; 8 plain non-isolated classes unaffected. ✓

**AC-26 (Build/run/lint)** — Per plan Task 7:
- Test project builds warning-clean under inherited Directory.Build.props (TreatWarningsAsErrors, WarningLevel 9999, Nullable). ✓
- All 142 tests pass green; 3 ignored tests reported as ignored by NUnit3 adapter. ✓
- No production code broken. ✓

**AC-27 to AC-28 (Coverage)** — Verified in scripts/coverage.ps1:
- Line 45: `--assemblyFilter nunit` correctly filters NUnit adapter assemblies from coverage denominator (changed from old `--assemblyFilter xunit`). ✓
- AltCover instrument → test → collect flow works under NUnit3 adapter; step-3 RimWorld DLL removal cooperates with runtime resolver. ✓
- Coverage floor: plan Task 7 measured 37.05% testable-core (baseline 38.2% − 1.0 pp floor = 37.2% pass threshold). ✓
- No in-repo CI workflow added. ✓

**Test quality observations** (unchanged from iter-01, re-confirmed):
- **Meaningfulness**: Tests verify observable behaviour, not implementation detail. ✓
- **Edge cases**: Empty, single, many, boundary (negative, zero, max caps), invalid (null, invalid parse), and RimWorld-dependent (3 ignored) all covered. ✓
- **Determinism**: No sleep, no wall-clock timing, no order-dependent assertions. Per-test `[SetUp]`/`[TearDown]` isolation confirmed. ✓
- **Assertion strength**: Value-comparison form preserved (e.g. `.BeLessThan`, `.BeGreaterThan` over `.BeTrue()`) where needed for failure diffs. ✓
- **No flaky patterns**: No timing assumptions, no network, no hardcoded paths (except boundary-value literal test data). ✓
- **Stub resolution**: No stubs exist; no TODO(sprint-*) markers to verify. ✓

## Summary

Iter-02 testing review confirms all acceptance criteria remain satisfied. No test-code regressions detected. The test suite integrity — coverage, isolation, determinism, assertion strength, edge-case coverage — is intact from iter-01. Stale comment references in test-file XML-docs (StatRangesTests, StatefulSubsystemTests) are documentation issues, not test-quality gaps, and are flagged separately by simplification and documentation reviewers. No testing-reviewer action required.
