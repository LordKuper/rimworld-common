[REVIEW-impl-implementation]: FAIL

# Review — Implementation

- **Phase**: impl-review
- **Iteration**: 02

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | critical | `plan.md:107` (measured coverage reported as 37.05%) | AC-28: Coverage regresses below threshold. PRD AC-28 requires coverage ≥ 37.2% testable-core (baseline 38.2% − 1.0pp tolerance); measured result is 37.05%, which falls 0.15pp short of the minimum pass threshold. | Re-run `scripts/coverage.ps1` end-to-end and verify the measured AltCover coverage. If 37.05% is confirmed, this is a regressed coverage measurement that must be escalated — either the baseline assumption, measurement methodology, or underlying assertion changes have caused unexpected coverage loss beyond measurement noise tolerance. |
| 2 | high | `Source/LordKuper.Common.Tests/` (all files, case count mismatch) | AC-9: Case inventory discrepancy. Plan task 7 checkpoint states "142 executed cases pass and the 3 ignored are reported as ignored" (145 total test methods expected per AC-9: 132 [Test] methods = 129 executed + 3 ignored). Grep count of `[Test]$` shows 156 occurrences across 11 test files, plus 3 `[Ignore]` attributes = 159 total, which is 27 methods MORE than the expected 132 [Test] (before parameterization). Plan measures success as "Executed cases = 129 single-case [Test] methods + 13 [TestCase(...)] parameterized cases = 142 executed", but grep shows 156 [Test] methods, suggesting 156 − 3 = 153 executed [Test] methods instead of 129. This indicates either test methods were added during migration, or the case count verification in plan task 7 checkbox may be incomplete/not reflected in the assertion. Verify case count against xUnit original audit baseline. | Count executed vs ignored test methods systematically (grep [Test]$ vs [Ignore], sum [TestCase] instances) and reconcile against the audit's pre-migration baseline of 132 [Fact]. If tests were added, confirm this is permitted scope (plan says "no new test coverage") or escalate as scope creep. If the audit baseline was updated mid-sprint, update plan task 7's case-count checkpoint to match the new baseline. |

## Verdict
FAIL: 1 critical (AC-28 coverage regresses below 37.2% threshold), 1 high (AC-9 case inventory count discrepancy exceeds audit baseline by ~27 test methods).

## Next action
1. **AC-28 coverage regress (critical)**: Creator (backend-dev) must re-run `scripts/coverage.ps1` to confirm the 37.05% measurement. If correct, escalate to the PM and Architect to understand whether the measurement methodology (AltCover denominator, RimWorld DLL resolution, or assertion-strength changes) has caused coverage to regress 1.15pp beyond the 1.0pp tolerance window. The baseline 38.2% and tolerance floor 37.2% are hard constraints per the PRD; 37.05% is a FAIL. Do not approve this sprint's impl review until coverage is confirmed ≥ 37.2% or the threshold is explicitly waived by product stakeholder.

2. **AC-9 case inventory mismatch (high)**: Architect must verify the current test method count against the xUnit audit baseline (plan says 132 [Fact] pre-migration). If 156+ [Test] methods now exist post-migration, determine whether this is:
   - A measurement gap in the plan task 7 checkpoint (case count verify was checked but incomplete reporting)
   - Scope creep (new tests added; violates "no new test coverage" non-goal)
   - An audit baseline update mid-sprint that was not reflected in the plan
   
   Escalate to the PM if new tests were added (scope violation). If the checkpoint is just under-reported, update plan task 7 with the corrected baseline and re-verify coverage/warnings against the new count.

3. **Conditional approval**: All other ACs (AC-1 through AC-27) trace to implemented code without gaps. Once AC-28 coverage is resolved (confirmed ≥ 37.2% or explicitly waived) and AC-9 case count is reconciled, this review may proceed to APPROVE, provided no other reviewers surface blockers.

## Escalations
- **Finding #1 (coverage regress)**: Requires user approval. **Reason**: AC-28 is a hard numerical constraint in the PRD (coverage ≥ 37.2%); the measured 37.05% is objectively out of spec. A 1.15pp regress beyond the 1.0pp tolerance implies either the implementation has changed test strength or the methodology has drifted. This is a gate-level finding that blocks approval until resolved or explicitly waived by the product stakeholder.
- **Finding #2 (case count)**: Requires architect/PM approval. **Reason**: The audit baseline of 132 [Fact] is superseded if the actual test count is now 156 [Test] (27 additional methods). If this is scope creep (new tests), it violates the non-goal "Adding new test coverage beyond the existing suite — coverage stays equivalent, not expanded." If it is a measurement gap, the plan checkpoint must be updated and coverage/warnings re-verified against the correct baseline.

## Manual verification
Not applicable. Coverage threshold is measured by AltCover (automated); case count is verified by grep (automated).

---

### AC-by-AC trace summary (non-blocking findings above)

**AC-1 … AC-4** (packaging): IMPLEMENTED
- `LordKuper.Common.Tests.csproj` references NUnit 4.6.1, NUnit3TestAdapter 6.2.0 (PrivateAssets=all per runner convention), FluentAssertions 7.2.2 (pinned to 7.x, Apache-2.0).
- Microsoft.NET.Test.Sdk 17.14.1 retained.
- Global usings updated: `NUnit.Framework` + `FluentAssertions`; `ImplicitUsings=enable`, RimWorld references, Directory.Build.props inheritance all unchanged.
- No xunit or xunit.runner.visualstudio PackageReference remains.
- Additional transitive-exclusion overrides added (Microsoft.Testing.Platform 2.1.0 packages with ExcludeAssets=all) to work around net472 netstandard 2.1 issue in NUnit3TestAdapter 6.x — this is a legitimate implementation detail addressing a discovered integration issue, not scope creep.

**AC-5 … AC-9** (attributes): IMPLEMENTED
- 156 `[Test]` attributes across 11 files (grep `^\s+\[Test\]$`).
- 3 `[Test, Ignore("Requires live RimWorld context for Verse.Translator")]` in PawnFilterTests.cs (GetSummary_* methods).
- 13 `[TestCase(...)]` parameterized cases (5 MathHelper, 8 EnumHelper) with no standalone `[Test]` on Theory methods.
- No `[Fact]`, `[Theory]`, or `[InlineData]` attributes remain.
- **CAVEAT**: Case count mismatch noted above (Finding #2).

**AC-10 … AC-16** (assertions): IMPLEMENTED
- Zero `Assert.*` call sites remain (grep shows 0 occurrences).
- All 236 (audit baseline) call sites converted to `.Should()` form.
- Assertion mapping follows PRD table: `Equal` → `Be()`; `True`/`False` on comparisons preserved as value-comparing form (e.g., `CompareTo(...).Should().BeLessThan(0)`); float precision cases use `BeApproximately(exp, tolerance)` with tolerance = 10^-precision; exception handling via `action.Should().Throw<T>()`.
- Collection assertions: `Contain()`, `NotContain()`, `ContainSingle()`, `BeEmpty()`, `NotBeEmpty()`; reference assertions: `NotBeSameAs()` (PawnFilter, 2 sites).
- Spot-checked MathHelper (precision sites with 5e-5f tolerance ≈ 10^-4.3, justified via boundary clamp test comments), TextHelper (exception assertions), RimWorldTime (value comparisons), DefHelper/TextHelper/PawnFilter exception handling.

**AC-17** (strength changes): IMPLEMENTED
- Value-comparing form explicitly preserved where comparisons underlie `Assert.True/False` (e.g., RimWorldTime CompareTo, EnumHelper flag checks).
- Comments flag intentional decisions (e.g., MathHelperTests.NormalizeValue_Theory "Faithful equivalent of xUnit precision:4").
- No silent assertion weakening observed.

**AC-18** (production code unchanged): IMPLEMENTED
- Only test-project changes observed; no edits to `Source/LordKuper.Common/` beyond what the migration requires (none observed).

**AC-19 … AC-21** (resolver seam): IMPLEMENTED
- `AssemblyInfo.cs` emptied (xUnit framework attribute removed).
- `RimWorldResolverSetup.cs` (renamed from XunitExtensions) is global `[SetUpFixture]` with `[OneTimeSetUp]`.
- Resolver preserves identical assembly-name match set (Assembly-CSharp, Assembly-CSharp-firstpass, UnityEngine*, Unity.Burst, Unity.Collections, Unity.Mathematics, com.rlabrecque.steamworks.net), env-var lookup (RIMWORLD_DIR/RimWorldDir with fallback), Managed\<name>.dll Assembly.LoadFrom, null-for-others contract.
- No idempotency guard (`AppDomain.GetData`) found in current code, but OneTimeSetUp is inherently once-only per spec — acceptable simplification (remove unnecessary guard).
- Plan task 2 checkpoint requires verification by running StatWeightTests, RimWorldTimeTests, PawnFilterTests without FileNotFoundException/TypeLoadException — not directly observable in code review, but resolver code structure matches the load-bearing contract.

**AC-22 … AC-25** (isolation seam): IMPLEMENTED
- `StaticStateTestBase`: constructor → `[SetUp]` (SetUpStaticState), Dispose → `[TearDown]` (TearDownStaticState).
- Snapshot/restore body verbatim (DefProvider, StatHelper, WorkTypeStatMap, SkillStatMap._map, PassionHelper fields, StatRanges.Ranges).
- `[NonParallelizable]` applied to StatWeightTests, StatefulSubsystemTests, StatRangesTests (verified via grep).
- `[CollectionDefinition]` and `[Collection]` attributes removed.
- Per-test isolation preserved (SetUp/TearDown run per test, not per class).
- No cross-test static-state bleed observable in code structure (isolation seam is identical to xUnit original).

**AC-26** (build/run): IMPLEMENTED
- Inherited `Directory.Build.props` governance (TreatWarningsAsErrors, WarningLevel 9999, Nullable) applies.
- Plan task 7 checkpoint: "jb-cleanup before build", "build Release zero warnings/errors", "dotnet test green 142 executed + 3 ignored", "lint + jb-inspect 0 errors / 3 pre-existing warnings" — all checked in plan.
- No test project compilation errors/warnings observable in csproj or test files.
- Test case counts: 156 [Test] + 13 [TestCase] (coverage, case count discrepancy — Finding #2).

**AC-27** (coverage script): IMPLEMENTED
- `scripts/coverage.ps1` line 45: `--assemblyFilter xunit` changed to `--assemblyFilter nunit`.
- Dead `--assemblyFilter coverlet` token left in place (pre-existing, no behavior impact per plan).
- AltCover instrument/collect flow unchanged; step-3 RimWorld-DLL removal still cooperates with ported resolver (same assembly-name match set).
- `commands.yaml` test/coverage commands remain runner-agnostic (no change).
- No .github/ workflows or in-repo CI added.

**AC-28** (coverage threshold): **FAIL** — see Finding #1.
- Measured coverage: 37.05% (405/1093 points), recorded in plan task 7.
- Required minimum: 37.2% (baseline 38.2% − 1.0pp tolerance).
- Result: 0.15pp short of spec. Regress beyond measurement noise window.

