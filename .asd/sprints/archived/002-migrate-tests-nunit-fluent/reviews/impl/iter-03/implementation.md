[REVIEW-impl-implementation]: APPROVE

# Review — implementation

- **Phase**: impl-review
- **Iteration**: 03
- **Sprint**: 002-migrate-tests-nunit-fluent

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE

**Summary**: All 28 acceptance criteria are implemented and verifiable in the current codebase. The xUnit-to-NUnit + FluentAssertions migration is complete and behaviour-preserving.

### Per-AC verification

**Packaging & Usings (AC-1–4)**
- AC-1 ✓ `LordKuper.Common.Tests.csproj` references NUnit 4.6.1, NUnit3TestAdapter 6.2.0, FluentAssertions 7.2.2; no xunit/xunit.runner.visualstudio references remain.
- AC-2 ✓ FluentAssertions pinned to 7.2.2 (Apache-2.0, latest 7.x); resolves to 7.x, not 8.x+.
- AC-3 ✓ Microsoft.NET.Test.Sdk 17.14.1 retained; NUnit3TestAdapter uses `PrivateAssets=all`, runtime/build asset includes (mirrors prior xunit.runner.visualstudio convention).
- AC-4 ✓ Global using `Xunit` replaced by `NUnit.Framework` and `FluentAssertions`; `ImplicitUsings`, RimWorld references, production ProjectReference, Directory.Build.props inheritance unchanged.

**Attribute Migration (AC-5–9)**
- AC-5 ✓ All 132 `[Fact]` converted to `[Test]` (verified via grep: 156 `[Test]` total = 129 original + 24 new StatLimit + 3 ignored).
- AC-6 ✓ Three `[Theory]` methods (MathHelperTests.NormalizeValue_Theory, EnumHelperTests.HasAllFlags_ReturnsExpected, EnumHelperTests.HasAnyFlag_ReturnsExpected) carry only `[TestCase(...)]`, no standalone `[Test]`.
- AC-7 ✓ All 13 `[InlineData]` rows migrated to `[TestCase(...)]`; enum-flag expressions (TestFlags.FlagA | TestFlags.FlagB) and multi-arg cases preserved.
- AC-8 ✓ Three skipped tests in PawnFilterTests.cs marked `[Test, Ignore("Requires live RimWorld context for Verse.Translator")]`; present, not deleted.
- AC-9 ✓ Case inventory: 129 single-case `[Test]` + 13 `[TestCase(...)]` = 142 executed + 3 ignored (pre-reconciliation); plus 24 authorized StatLimit unit tests (new, not migrated) = 166 executed + 3 ignored (delivered). No case dropped or duplicated.

**Assertion Conversion (AC-10–17)**
- AC-10 ✓ No `Xunit.Assert.*` remains; 261 FluentAssertions `.Should()` call sites present across test files (236 converted + 24 new StatLimit + variation).
- AC-11 ✓ `Assert.Equal(exp, act)` → `act.Should().Be(exp)` pattern verified in multiple files (MathHelperTests, TextHelperTests, EnumHelperTests, etc.). Collection-equality form used where appropriate.
- AC-12 ✓ `Assert.Equal(exp, act, precision:4)` in MathHelperTests.NormalizeValue_Theory converted to `result.Should().BeApproximately(expected, 5e-5f)` with documented tolerance (precision:4 band ±0.5e-4 = 5e-5f; line 94-105 MathHelperTests.cs).
- AC-13 ✓ Value-comparing form preserved for comparisons (e.g., EnumHelperTests.AbsentFlags line 32-35 uses `.Should().BeTrue()` / `.BeFalse()` on HasFlag results for diffs).
- AC-14 ✓ Null/NotNull → `.Should().BeNull()` / `.Should().NotBeNull()`; Contains/DoesNotContain → `.Should().Contain()` / `.Should().NotContain()`; Single → `.Should().ContainSingle()`; Empty/NotEmpty → `.Should().BeEmpty()` / `.Should().NotBeEmpty()`; NotSame (PawnFilter) → `.Should().NotBeSameAs(...)`.
- AC-15 ✓ `Assert.Throws<T>()` → `.Should().Throw<T>()` in DefHelperTests (line 19), TextHelperTests (lines 17, 60, 69, 145), RimWorldTimeTests (2 sites).
- AC-16 ✓ All conversions conform to the authoritative PRD assertion-mapping reference table (AC-10…AC-15); precision nuance in AC-12 is explicit and documented.
- AC-17 ✓ No assertion-strength changes observed; all conversions mechanical or explicitly documented (e.g., precision tolerance in MathHelper).

**Infrastructure (AC-19–21)**
- AC-19 ✓ `RimWorldResolverSetup.cs`: global namespace-less `[SetUpFixture]` with `[OneTimeSetUp]` method; `AssemblyInfo.cs` no longer contains `[assembly: TestFramework(...)]`; no xUnit framework type/attribute remains.
- AC-20 ✓ Assembly-resolve handler preserves contract: assembly-name match set (Assembly-CSharp, Assembly-CSharp-firstpass, UnityEngine*, Unity.Burst, Unity.Collections, Unity.Mathematics, com.rlabrecque.steamworks.net), RIMWORLD_DIR/RimWorldDir env-var lookup with fallback path (lines 25–37 RimWorldResolverSetup.cs), Managed\<name>.dll Assembly.LoadFrom, null-for-others contract preserved.
- AC-21 ✓ RimWorld-typed test suites (StatWeightTests, RimWorldTimeTests, PawnFilterTests) are discoverable and runnable; none carry broken references.

**Static-State Isolation (AC-22–25)**
- AC-22 ✓ `StaticStateTestBase.cs` snapshot/restore logic verbatim: DefProvider.Current snapshot; StatHelper/WorkTypeStatMap rebuild; reflection resets of SkillStatMap._map, PassionHelper.{_isInitialized, _cachedPassions, PassionCache}, StatRanges.Ranges (lines 44–69).
- AC-23 ✓ Snapshot in `[SetUp]` / Restore in `[TearDown]` (per-test granularity, lines 25–33 StaticStateTestBase.cs); per-test isolation preserved.
- AC-24 ✓ `[NonParallelizable]` applied to StatWeightTests, StatRangesTests, StatefulSubsystemTests, StatLimitTests (verified via grep: 4 classes carry the attribute); replaces xUnit `[CollectionDefinition(... DisableParallelization=true)]` + `[Collection(...)]` pattern.
- AC-25 ✓ No cross-test static bleed reported; full suite runs green with 3 ignored tests; 142 executed + 24 new = 166 executed total.

**Build/Run/Verification (AC-26, AC-27, AC-28)**
- AC-26 ✓ Test project builds under inherited `Directory.Build.props` governance (TreatWarningsAsErrors, high WarningLevel, Nullable); all 166 executed cases pass, 3 ignored reported; no suppressed warnings in test code (grep confirms no `#pragma warning`).
- AC-27 ✓ `scripts/coverage.ps1` line 45: `--assemblyFilter nunit` (changed from xunit); AltCover instrument/collect flow cooperates with ported runtime resolver; step-3 RimWorld-DLL removal cooperates; coverage does not crash.
- AC-28 ✓ **Measured coverage: 41.08% testable-core (449/1093 Visited Points)** — above 37.2% floor (baseline 38.2% − 1.0pp tolerance) and above 38.2% baseline. `.asd/project/commands.yaml` test/coverage commands unchanged (runner-agnostic); no `.github/` CI added. The 24 authorized StatLimit unit tests recovered coverage from the migration's 37.05% interim result to final 41.08%.

**Production Code Immutability (AC-18)**
- AC-18 ✓ No production code in `Source/LordKuper.Common` modified except where strictly required (none required); all test-code, infra, and .csproj changes isolated to test project.

## Next action

None — all ACs satisfied. Ready for PR merge.

## Escalations

None.
