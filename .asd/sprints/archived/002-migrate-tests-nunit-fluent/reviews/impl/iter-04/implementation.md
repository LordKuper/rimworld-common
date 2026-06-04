[REVIEW-impl-implementation]: APPROVE

# Review — implementation

- **Phase**: impl-review
- **Iteration**: 04
- **Sprint**: 002-migrate-tests-nunit-fluent

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE

**Summary**: All 28 acceptance criteria are fully implemented and verifiable in the current codebase. The xUnit-to-NUnit + FluentAssertions migration is complete, behaviour-preserving, and coverage-maintained. Fresh-context re-verification confirms prior iter-03 verdict; no regressions detected.

### Per-AC verification (fresh context, iter-04)

**Packaging & Usings (AC-1–4)**
- AC-1 ✓ `LordKuper.Common.Tests.csproj` (lines 14–21) references NUnit 4.6.1, NUnit3TestAdapter 6.2.0, FluentAssertions 7.2.2; no xunit/xunit.runner.visualstudio references remain. Microsoft.NET.Test.Sdk 17.14.1 retained. Transitive Microsoft.Testing.Platform exclusions (lines 28–42) prevent net472 incompatibility.
- AC-2 ✓ FluentAssertions pinned to 7.2.2 (Apache-2.0, latest 7.x); package resolves to 7.x, not 8.x+.
- AC-3 ✓ Microsoft.NET.Test.Sdk 17.14.1 unchanged. NUnit3TestAdapter 6.2.0 configured with `PrivateAssets=all` and `IncludeAssets=runtime; build; native; contentfiles; analyzers; buildtransitive` (csproj lines 17–20), mirroring former xunit.runner.visualstudio convention.
- AC-4 ✓ Global using block (csproj lines 50–51): `<Using Include="NUnit.Framework" />` and `<Using Include="FluentAssertions" />`; replaces former `Xunit`. ImplicitUsings=enable, RimWorld `<Reference>` block (lines 55–71), production `<ProjectReference>`, and Directory.Build.props inheritance unchanged.

**Attribute Migration (AC-5–9)**
- AC-5 ✓ grep count: 156 `[Test]` attributes across 11 test files (RimWorldTimeTests 32, PawnFilterTests 23, StatWeightTests 14, TextHelperTests 17, TimedCacheTests 12, MathHelperTests 12, EnumHelperTests 7, StatefulSubsystemTests 8, DefHelperTests 1, StatRangesTests 6, StatLimitTests 24). Composition: 129 original migrated + 24 new StatLimit + 3 ignored = 156 total. No `[Fact]` attribute remains.
- AC-6 ✓ Three `[Theory]` methods identified and verified:
  - `MathHelperTests.NormalizeValue_Theory` (lines 100–106): 5 `[TestCase(...)]` attributes, no standalone `[Test]`.
  - `EnumHelperTests.HasAllFlags_ReturnsExpected` (lines 89–96): 4 `[TestCase(...)]`, no standalone `[Test]`.
  - `EnumHelperTests.HasAnyFlag_ReturnsExpected` (lines 98–106): 4 `[TestCase(...)]`, no standalone `[Test]`.
  - Total parameterized: 13 `[TestCase]` rows, zero spurious non-runnable cases.
- AC-7 ✓ All 13 `[InlineData(...)]` rows migrated to `[TestCase(...)]` with arguments preserved:
  - MathHelper (5 rows): lines 95–99, including multi-arg clamp/normalization cases and comment annotations (e.g., `// clamped below`).
  - EnumHelper (8 rows): lines 89–92 (HasAllFlags), lines 99–102 (HasAnyFlag), including enum-flag bit-or expressions (`TestFlags.FlagA | TestFlags.FlagB`) as constant arguments.
- AC-8 ✓ Three skipped tests in `Filters/PawnFilterTests.cs` (lines 203–232):
  - `GetSummary_MultipleIndentationLevels_Respects` (line 203): `[Test]` + `[Ignore("Requires live RimWorld context for Verse.Translator")]`.
  - `GetSummary_NoFiltersActive_ReturnsEmptyOrMinimal` (line 216): same pattern.
  - `GetSummary_WithIndentation_FormatsCorrectly` (line 226): same pattern.
  - All three present (not deleted), reported as ignored by NUnit runner.
- AC-9 ✓ Case inventory reconciliation:
  - Migrated (pre-reconciliation): 129 single-case `[Test]` + 13 `[TestCase]` = 142 executed + 3 ignored.
  - Authorized additions (StatLimitTests.cs, 24 new `[Test]` methods, lines 43–240): pure-logic unit tests re-covering code the per-test NUnit isolation legitimately stopped exercising from xUnit's fixture-ctor-based isolation.
  - Final delivered: 129 + 24 = 153 single-case executed `[Test]` + 13 `[TestCase]` = 166 executed + 3 ignored. No case dropped, no case duplicated.

**Assertion Conversion (AC-10–17)**
- AC-10 ✓ Grep confirms 0 `Assert.` call sites in test project. All 236 original xUnit assertions + new FluentAssertions assertions converted to `.Should()` form; verified spot samples: MathHelperTests (lines 16, 24, 34, 44, 55, 63, 71, 82, 90, 105), TextHelperTests (lines 17, 28, 40–42, 51, 60, 69, 80), EnumHelperTests (lines 32–35, 44–48, 56–60, 68–69, 76, 83, 95, 105), DefHelperTests (line 19), PawnFilterTests (lines 26–28, 45, 55–56, 67, 81, 102, 127, 132–137, 145–147), StatWeightTests (lines 17–20, 28–30, 38–40, 45, 52–54, 61–66, 77–79, 89–92).
- AC-11 ✓ Assertion equality form verified across files:
  - `Assert.Equal(exp, act)` → `act.Should().Be(exp)`: MathHelperTests line 16 (`result.Should().Be(1f)`), TextHelperTests line 28 (`sb.ToString().Should().Be(...)`), StatWeightTests line 28 (`weight.StatDefName.Should().Be("TestStat")`).
  - Collection-equality form used: PawnFilterTests line 26 (`.Count.Should().Be(2)`) and line 56 (`.Should().Contain(PawnType.Guest)`).
- AC-12 ✓ Precision float conversion verified in MathHelperTests (lines 93–106):
  - Original xUnit precision parameter = 4 (decimal places), band ±0.5e-4.
  - Converted tolerance = 5e-5f = 10^-4.5 (documented in line 94: `// xUnit precision:4 rounds to 4 decimal places (band ±0.5e-4); faithful equivalent is 5e-5f.`).
  - Method `NormalizeValue_Theory` (line 100) uses `.BeApproximately(expected, 5e-5f)`.
  - Boundary/clamp cases preserved: lines 98–99 (`// clamped below`, `// clamped above`).
- AC-13 ✓ Value-comparing form preserved for comparisons:
  - EnumHelperTests.AbsentFlags (lines 32–35): `absent.HasFlag(...).Should().BeTrue()` (comparison result → boolean check, preserving diff on failure).
  - StatWeightTests (line 20): `weight.Protected.Should().BeFalse()` (plain boolean).
  - MathHelperTests (line 34): `result.Should().Be(-0.5f)` (value comparison → direct equality, strongest diff).
- AC-14 ✓ Null/NotNull/Contains/Empty/NotSame conversions verified:
  - Null/NotNull: StatWeightTests line 18 (`.Should().BeNull()`), MathHelperTests line 90 (`.Should().BeNull()`), StatWeightTests line 145 (`.Should().NotBeNull()`).
  - Contains/DoesNotContain: PawnFilterTests lines 26–27 (`.Count.Should().Be(2)`, `.Should().ContainSingle()`, `.Should().Be(PawnHealthState.Healthy)`), EnumHelperTests line 44 (`.Should().Contain(TestFlags.FlagA)`), line 47 (`.Should().NotContain(TestFlags.FlagB)`), TextHelperTests line 40 (`.Should().Contain("Line 1\r\n")`).
  - Single: PawnFilterTests line 27 (`.Should().ContainSingle()`), EnumHelperTests line 68 (`.Should().ContainSingle()`).
  - Empty/NotEmpty: EnumHelperTests lines 76, 83 (`.Should().BeEmpty()`), line 76 (`.Should().NotContain(...)`).
  - NotSame: PawnFilterTests line 132 (`.Should().NotBeSameAs(original.AllowedPawnTypes)`) — verified as the xUnit NotSame pattern.
- AC-15 ✓ Exception assertions verified:
  - `Assert.Throws<T>()` → `.Should().Throw<T>()`: DefHelperTests line 19 (`.Should().Throw<ArgumentNullException>()`), TextHelperTests lines 17, 60, 69 (`.Throw<ArgumentNullException>()`), PawnFilterTests lines 102, 111 (`.Throw<ArgumentNullException>()`), RimWorldTimeTests (verified structure, 2 sites asserting exception type).
  - Lambda wrapping shape preserved (e.g., `var act = () => ...; act.Should().Throw<T>()` pattern).
- AC-16 ✓ All conversions conform to PRD assertion-mapping reference table (Section "Assertion mapping reference", lines 246–265 prd.html). Precision nuance (AC-12) explicitly documented as "Not 1:1" per the reference table line 255.
- AC-17 ✓ No undocumented assertion-strength changes observed. Precision tolerance is the only non-mechanical conversion; it is explicitly called out in code comments (MathHelperTests line 94) and PRD (AC-12).

**Production Code Immutability (AC-18)**
- AC-18 ✓ No production code in `Source/LordKuper.Common` modified except where required by the migration. Spot check: `Source/LordKuper.Common/Helpers/EnumHelper.cs` unchanged (lines 102–140 show public API surface untouched, only `[PublicAPI]` marker present from prior sprint). All test failures are resolved via test-code refactoring, not production changes.

**Infrastructure (AC-19–21)**
- AC-19 ✓ `RimWorldResolverSetup.cs` (lines 1–59): namespace-less global `[SetUpFixture]` class with `[OneTimeSetUp]` static method `RegisterRimWorldResolver()` (lines 20–58). AssemblyInfo.cs (lines 1–5) contains only comment and empty body — no `[assembly: TestFramework(...)]` attribute, no xUnit framework type remains.
- AC-20 ✓ Assembly-resolve handler contract preserved (RimWorldResolverSetup.cs lines 39–58):
  - Assembly-name match set (line 10–17 `IsRimWorldAssembly`): `Assembly-CSharp`, `Assembly-CSharp-firstpass`, `UnityEngine*` (wildcard), `Unity.Burst`, `Unity.Collections`, `Unity.Mathematics`, `com.rlabrecque.steamworks.net`.
  - Env-var lookup (lines 25–31): `RIMWORLD_DIR` or `RimWorldDir` with error handling and default path `RimWorldWin64_Data\Managed`.
  - Assembly.LoadFrom resolution (lines 46–50): load from `Managed\<name>.dll` path.
  - Null-for-others contract (lines 44, 56): return null if not a RimWorld assembly or file not found.
- AC-21 ✓ RimWorld-typed test suites are discoverable and runnable:
  - StatWeightTests (inherits StaticStateTestBase, lines 10): carries `[NonParallelizable]` (line 9).
  - RimWorldTimeTests (lines 1–4): references RimWorld types, discoverable under NUnit.
  - PawnFilterTests (lines 1–6): references RimWorld/Verse types, three tests with `[Ignore(...)]` (not deleted), suite runnable.
  - All three suites listed in plan.md line 63 for resolver verification; no `FileNotFoundException` or `TypeLoadException` reported.

**Static-State Isolation (AC-22–25)**
- AC-22 ✓ `StaticStateTestBase.cs` (lines 20–71) snapshot/restore logic verbatim:
  - Snapshot: `[SetUp]` method `SetUpStaticState()` (lines 25–29) saves `DefProvider.Current` to `_originalProvider` field (lines 22, 28).
  - Restore: `[TearDown]` method `TearDownStaticState()` (lines 32–70) restores provider (lines 36–37) and rebuilds caches:
    - `StatHelper.Rebuild()` (line 41).
    - `WorkTypeStatMap.Rebuild()` (line 42).
  - Reflection resets (lines 44–69): `SkillStatMap._map` (lines 45–48), `PassionHelper._isInitialized` (lines 51–59), `PassionHelper._cachedPassions` (lines 51–59), `PassionHelper.PassionCache` (lines 51–59), `StatRanges.Ranges` (lines 66–69). Set of snapshotted/restored statics unchanged from xUnit version.
- AC-23 ✓ Snapshot/restore runs per-test: `[SetUp]` (line 25) and `[TearDown]` (line 32) on instance methods (not static), so NUnit calls them for each test execution. Per-test granularity preserved (not per-class/per-assembly).
- AC-24 ✓ `[NonParallelizable]` applied to static-touching classes (verified via grep across 4 test files):
  - StatWeightTests (line 9 of file): `[NonParallelizable]`.
  - StatRangesTests: `[NonParallelizable]`.
  - StatefulSubsystemTests: `[NonParallelizable]`.
  - StatLimitTests (line 15): `[NonParallelizable]` + `[SetCulture("en-US")]` for deterministic float formatting.
  - Replaces xUnit `[CollectionDefinition("StaticState", DisableParallelization=true)]` / `[Collection("StaticState")]` pattern. No `[assembly: Parallelizable]` applied (NUnit is non-parallel by default).
- AC-25 ✓ No cross-test static bleed reported: full suite (166 executed + 3 ignored) runs deterministically. 8 plain (non-isolated) test classes (TimedCacheTests, PawnFilterTests, MathHelperTests, EnumHelperTests, TextHelperTests, DefHelperTests, RimWorldTimeTests, StatefulSubsystemTests) unaffected by isolation seam.

**Build/Run/Verification (AC-26, AC-27, AC-28)**
- AC-26 ✓ Test project builds warning-clean under inherited `Directory.Build.props` governance (`TreatWarningsAsErrors=true`, `WarningLevel 9999`, `Nullable=enable`). All 166 executed tests pass; 3 `[Test, Ignore(...)]` tests reported as ignored. Implicit assertions: no xunit-related compiler warnings, no FA-related analyzer warnings that would trip `TreatWarningsAsErrors`.
- AC-27 ✓ `scripts/coverage.ps1` (line 45) updated: `--assemblyFilter xunit` changed to `--assemblyFilter nunit`. AltCover instrument/collect flow (lines 44–50, 59) cooperates with ported runtime resolver. Step-3 RimWorld-DLL removal (built into AltCover) does not break resolver. Coverage script runs end-to-end without crash. Dead `--assemblyFilter coverlet` token (pre-existing, line 45 buried in multi-line `altcover` invocation) left as-is per plan tolerance.
- AC-28 ✓ Coverage threshold verified:
  - Measured coverage: 41.08% testable-core (449 / 1093 Visited Points), recorded in commands.yaml line 23 and plan.md line 107.
  - Threshold floor: 37.2% (baseline 38.2% − 1.0pp measurement-noise tolerance). 41.08% > 37.2% ✓.
  - Baseline recovery: framework migration on its own dropped coverage to 37.05% (per-test isolation legitimately stopped exercising SkillStatMap.BuildMap ECall). 24 net-new StatLimit pure-logic unit tests (added under explicit user authorization per AC-9 and plan.md lines 97–100) recovered coverage to final 41.08% >= 38.2% baseline (plus authorization delta).
  - Commands registry: `.asd/project/commands.yaml` (lines 7, 24) `test` and `coverage` command strings runner-agnostic; no change required.
  - CI scope: no `.github/` workflow or pipeline YAML added. Coverage harness remains local (`scripts/coverage.ps1`).

## Next action

None — all 28 acceptance criteria satisfied. Ready for PR phase.

## Escalations

None.
