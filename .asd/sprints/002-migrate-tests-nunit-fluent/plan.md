---
responsibility:
  owns: task breakdown, dod, task status (checkboxes)
  excludes: requirements, design decisions, code, review findings
  delegates_to: design/ docs (requirements/design), reviews/ (findings)
---

# Plan

<!--
Format rules (parser-critical):
- Overview, Context, Definition of Done — prose only, NO checkboxes
- Checkboxes (- [ ]/- [x]) appear ONLY inside `### Task N:` sections
- Checkboxes in any non-task section break orchestrator task parsing
- A subtask deferred for a manual action stays `- [ ]` and is suffixed ` — BLOCKED: MS-N` (see manual-steps.md)
-->

## Overview

This sprint migrates the `Source/LordKuper.Common.Tests` (net472) suite from xUnit 2.9.3 to NUnit 4.6.1 + NUnit3TestAdapter 6.2.0, and converts all 236 `Assert.*` call sites (12 distinct methods, 11 files) to FluentAssertions 7.2.2 `.Should()` form. It is a behaviour-preserving refactor with opportunistic cleanup: the framework, assertion library, the two custom test-infra seams (RimWorld `AssemblyResolve` and StaticState isolation), and the local AltCover coverage script all move together, while the suite's behaviour, case inventory (142 executed + 3 ignored), and overall coverage (≥ 37.2% testable-core) are preserved. Production code in `Source/LordKuper.Common` is untouched except where strictly required by the migration. No in-repo CI is added — the "CI" scope reduces to `scripts/coverage.ps1` + `.asd/project/commands.yaml`.

The work is decomposed into seven tasks ordered by dependency: the `.csproj` package/usings swap (Task 1) is the foundation; the two infra seams (Tasks 2, 3) and the bulk attribute (Task 4) + assertion (Task 5) rewrites build on it; the coverage script (Task 6) follows; and a final verification gate (Task 7) confirms build, run, and coverage under the new stack. All test-code, infra, and `.csproj` work is owned by `asd-backend-dev` — these are backend-side test sources, and this is unit-test migration only (no integration/e2e scope arises, so `asd-test-engineer` is not engaged).

## Context

- [prd.html](./design/prd.html) — 28 acceptance criteria (AC-1 … AC-28) and the authoritative assertion-mapping reference table.
- [adr.html](./design/adr.html) — ADR-0004 (framework swap), ADR-0005 (FluentAssertions 7.x), ADR-0006 (resolver seam), ADR-0007 (StaticState isolation remap), all proposed.
- [audit.md](./audit.md) — brownfield inventory: per-file test counts, the 236-site `Assert.*` map, the resolver/isolation seams, and the documentation-migration plan.
- Persistent: [adr-0001](../../design/architecture/adr/adr-0001-rimworld-context-isolation-seam.html) (isolation contract this sprint preserves; xUnit vocabulary remapped in design-promote), [adr-0002](../../design/architecture/adr/adr-0002-statranges-adaptive-normalization.html) (why `StatRanges.Ranges` is in the snapshot set), [adr-0003](../../design/architecture/adr/adr-0003-build-governance.html) (`TreatWarningsAsErrors` / `WarningLevel 9999` / `Nullable` / `LangVersion=latest` the migrated stack must satisfy), [stack.html](../../design/architecture/stack.html).
- Project config: `.asd/project/commands.yaml` (`test`, `build`, `lint`, `coverage`, `jb-cleanup`, `jb-inspect`), `.asd/project/custom-coding-rules.md` (§ "Testing (NUnit + FluentAssertions)").
- No related open stubs (`.asd/project/stubs.md` absent; audit "Related open stubs" empty) → no stub tasks.

## Definition of Done

- Every acceptance criterion AC-1 … AC-28 from the PRD is satisfied; each is covered by at least one task below (full coverage map in the per-task AC references — no orphan AC).
- Test scope is unit only. No integration or e2e tests are added; `asd-test-engineer` is not engaged; `asd-backend-dev` owns all test-source, infra, and `.csproj` work.
- `LordKuper.Common.Tests.csproj` references NUnit 4.6.1, NUnit3TestAdapter 6.2.0, FluentAssertions 7.2.2, and retains Microsoft.NET.Test.Sdk 17.14.1; no `xunit` / `xunit.runner.visualstudio` reference remains; global usings are `NUnit.Framework` + `FluentAssertions`.
- No `[Fact]` / `[Theory]` / `[InlineData]` / `[Collection]` / `[CollectionDefinition]` / `[assembly: TestFramework]` / `XunitTestFramework` / `Xunit.Assert.*` remains anywhere a NUnit/FluentAssertions equivalent exists.
- The case inventory is preserved exactly: 142 executed (129 single-case `[Test]` + 13 `[TestCase(...)]`) + 3 `[Test, Ignore(...)]`; no case silently dropped or duplicated.
- The full test project builds warning-clean under the inherited `Directory.Build.props` governance (`TreatWarningsAsErrors`, `WarningLevel 9999`, `Nullable`); all tests (including the 3 ignored) are discovered and run green under the NUnit3 adapter via `dotnet test`.
- `scripts/coverage.ps1` runs end-to-end under the NUnit3 adapter; AltCover coverage of testable-core is ≥ 37.2% (sprint-001 baseline 38.2% minus 1.0 pp measurement-noise tolerance; expected to remain ≥ 38.2% since no production code or test case is removed).
- `jb-cleanup` applied before build; `jb-inspect` after lint produces a SARIF with no `error`/`warning` entries (per `.asd/project/custom-coding-rules.md`).
- Production code in `Source/LordKuper.Common` is unchanged except where strictly required by the migration; no test removed to make the suite pass.
- All impl-review reviewers return green (or any findings resolved through the impl⇄impl-review cycle).

### Task 1: Package + global-usings swap in `.csproj`
Owner: asd-backend-dev. Depends on: none (foundation). Satisfies: AC-1, AC-2, AC-3, AC-4.
- [ ] In `Source/LordKuper.Common.Tests/LordKuper.Common.Tests.csproj`, remove the `xunit` (2.9.3) and `xunit.runner.visualstudio` (2.8.2) `PackageReference`s.
- [ ] Add `NUnit` 4.6.1 and `NUnit3TestAdapter` 6.2.0; mirror the former runner convention on the adapter (`PrivateAssets=all`; `IncludeAssets=runtime; build; native; contentfiles; analyzers; buildtransitive`).
- [ ] Add `FluentAssertions` pinned to 7.2.2 (latest 7.x, Apache-2.0); confirm it resolves to 7.x and never floats to 8.x or later.
- [ ] Retain `Microsoft.NET.Test.Sdk` 17.14.1 unchanged (the NUnit3 adapter needs the same VSTest host).
- [ ] Replace the global `<Using Include="Xunit" />` with `<Using Include="NUnit.Framework" />` and add `<Using Include="FluentAssertions" />`.
- [ ] Leave `ImplicitUsings=enable`, the RimWorld `<Reference>` block, the production `<ProjectReference>`, and `Directory.Build.props` inheritance unchanged.
- [ ] Restore packages and confirm the project resolves the new package set (a clean compile of unconverted sources is not expected yet; package resolution is the checkpoint here).

### Task 2: RimWorld `AssemblyResolve` seam → global `[SetUpFixture]`
Owner: asd-backend-dev. Depends on: Task 1. Satisfies: AC-19, AC-20, AC-21.
- [ ] Delete the `[assembly: TestFramework("...RimWorldTestFramework", ...)]` attribute from `Source/LordKuper.Common.Tests/AssemblyInfo.cs`.
- [ ] Remove the `RimWorldTestFramework : XunitTestFramework` type; rename `XunitExtensions.cs` to a non-"Xunit" name (e.g. `RimWorldResolverSetup.cs`) and re-house the resolver in a **namespace-less (global)** `[SetUpFixture]` class with an `[OneTimeSetUp]` method that registers the `AppDomain.CurrentDomain.AssemblyResolve` handler.
- [ ] Preserve the resolution contract verbatim: assembly-name match set (`Assembly-CSharp`, `Assembly-CSharp-firstpass`, `UnityEngine*`, `Unity.Burst`, `Unity.Collections`, `Unity.Mathematics`, `com.rlabrecque.steamworks.net`); env-var lookup (`RIMWORLD_DIR` / `RimWorldDir`) with the existing fallback path; `Managed\<name>.dll` `Assembly.LoadFrom` resolution; null-for-everything-else contract.
- [ ] Keep the `AppDomain.GetData/SetData("RimWorldResolverInitialized")` idempotency guard so registration is once-only across any path (justified by the ADR-0006 `[ModuleInitializer]` fallback contingency).
- [ ] Confirm no xUnit framework attribute or type remains in the project.
- [ ] Verify the resolver is live by running `StatWeightTests`, `RimWorldTimeTests`, `PawnFilterTests` and confirming no `FileNotFoundException` / `TypeLoadException` at discovery or execution. If discovery-time RimWorld type resolution fails, escalate per the ADR-0006 accepted-risk fallback (switch to a `[ModuleInitializer]` with a net472 `IsExternalInit`/attribute polyfill registering the resolver at module load) — raise to the orchestrator before adopting.

### Task 3: StaticState isolation → `[SetUp]`/`[TearDown]` + `[NonParallelizable]`
Owner: asd-backend-dev. Depends on: Task 1. Satisfies: AC-22, AC-23, AC-24, AC-25.
- [ ] Preserve the `StaticStateFixture` snapshot/restore body verbatim: snapshot of `DefProvider.Current`; restore + rebuild of `StatHelper` and `WorkTypeStatMap`; reflection resets of `SkillStatMap._map`, `PassionHelper.{_isInitialized, _cachedPassions, PassionCache}`, and `StatRanges.Ranges`. The set of snapshotted/restored statics is unchanged.
- [ ] Remap `StaticStateTestBase`: constructor → `[SetUp]` (build fixture / snapshot); `Dispose()` → `[TearDown]` (restore). Drop `IDisposable` from the base class. Preserve per-test granularity (not per-class).
- [ ] Remove the `[CollectionDefinition("StaticState", DisableParallelization = true)]` marker class and all `[Collection("StaticState")]` usages.
- [ ] Apply `[NonParallelizable]` to the three static-touching classes (`StatWeightTests`, `StatefulSubsystemTests`, `StatRangesTests`) as an explicit serialization-intent statement; do NOT add `[assembly: Parallelizable]`.
- [ ] Confirm no cross-test static-state bleed: the full suite yields the same results when run repeatedly and in arbitrary order, and the 8 plain (non-isolated) classes are unaffected.

### Task 4: Attribute migration across the 11 test files
Owner: asd-backend-dev. Depends on: Task 1. Satisfies: AC-5, AC-6, AC-7, AC-8, AC-9.
- [ ] Convert every parameterless `[Fact]` (132 total, including the 3 skipped) to `[Test]`; no `[Fact]` attribute remains.
- [ ] Convert the 3 `[Theory]` methods (`MathHelperTests.NormalizeValue_Theory`, `EnumHelperTests.HasAllFlags_ReturnsExpected`, `EnumHelperTests.HasAnyFlag_ReturnsExpected`): each `[InlineData(...)]` row becomes one `[TestCase(...)]`, with NO standalone `[Test]` on the parameterized method (a bare `[Test]` would create a spurious non-runnable case).
- [ ] Migrate all 13 `[InlineData(...)]` rows (5 in MathHelper, 8 in EnumHelper) to `[TestCase(...)]`, preserving every argument tuple — including the EnumHelper enum-flag expressions (`TestFlags.FlagA | TestFlags.FlagB`, valid as constant `[TestCase]` args) and the MathHelper multi-arg / commented cases.
- [ ] Convert the 3 skipped tests in `Filters/PawnFilterTests.cs` (`GetSummary_MultipleIndentationLevels_Respects`, `GetSummary_NoFiltersActive_ReturnsEmptyOrMinimal`, `GetSummary_WithIndentation_FormatsCorrectly`) to `[Test, Ignore("Requires live RimWorld context for Verse.Translator")]` — kept present, reported as ignored, not deleted.
- [ ] Verify the case inventory matches exactly: 132 `[Fact]` → 132 `[Test]` (129 executed + 3 ignored) plus 13 `[TestCase]` cases = 142 executed + 3 ignored; no case dropped or duplicated.

### Task 5: Assertion conversion (236 sites) to FluentAssertions
Owner: asd-backend-dev. Depends on: Task 1; coordinate with Task 4 (same 11 files). Satisfies: AC-10, AC-11, AC-12, AC-13, AC-14, AC-15, AC-16, AC-17, AC-18.
- [ ] Convert all 236 `Assert.*` call sites (12 methods: `Equal`, `True`, `False`, `Null`, `NotNull`, `Throws`, `Contains`, `DoesNotContain`, `Single`, `NotSame`, `Empty`, `NotEmpty`) per the PRD's authoritative assertion-mapping reference table; no `Xunit.Assert.*` site remains where a FluentAssertions equivalent exists.
- [ ] `Assert.Equal(exp, act)` → `act.Should().Be(exp)`; collection-equality sites → `act.Should().Equal(exp)`; preserve expected/actual orientation.
- [ ] `Assert.Equal(exp, act, precision)` (e.g. MathHelper precision `4`) → `act.Should().BeApproximately(exp, 10^-precision)` (precision 4 → tolerance `1e-4`); document the chosen tolerance per site and spot-check the boundary/clamp (MathHelper normalization) cases for equivalent pass/fail.
- [ ] `Assert.True`/`Assert.False` over a value comparison → the value-comparing FA form (`x.Should().Be(y)`) to preserve the diff; plain boolean checks → `.Should().BeTrue()` / `.Should().BeFalse()`.
- [ ] `Null`/`NotNull` → `.Should().BeNull()`/`.Should().NotBeNull()`; `Contains`/`DoesNotContain` → `.Should().Contain(...)`/`.Should().NotContain(...)` (choose collection vs substring overload per site); `Single` → `.Should().ContainSingle()`; `Empty`/`NotEmpty` → `.Should().BeEmpty()`/`.Should().NotBeEmpty()`; `NotSame` (2 in PawnFilter) → `.Should().NotBeSameAs(...)`.
- [ ] `Assert.Throws<T>(() => ...)` (in DefHelper, TextHelper, RimWorldTime, PawnFilter) → `action.Should().Throw<T>()`, preserving the asserted exception type and any message/inner-exception assertion at the site.
- [ ] Treat any assertion-strength change (tightening or pruning a weak/redundant assertion, permitted by refactor+cleanup mode) and any non-1:1 site (e.g. the precision nuance) as an explicit, individually reviewable edit — never an accidental side effect of mechanical conversion.
- [ ] Make no change to production code in `Source/LordKuper.Common` except where strictly required by the migration; remove no test to make the suite pass.

### Task 6: Coverage script + command registry under NUnit
Owner: asd-backend-dev. Depends on: Task 1, Task 2 (resolver must be live for the step-3 lazy-resolve path). Satisfies: AC-27, AC-28 (coverage-tooling side).
- [ ] In `scripts/coverage.ps1` (line 40), change the AltCover `--assemblyFilter xunit` to `--assemblyFilter nunit` so the NUnit adapter assemblies (`nunit.framework`, `NUnit3.TestAdapter`) are excluded from the coverage denominator and not counted.
- [ ] Leave the dead `--assemblyFilter coverlet` token as-is (coverlet is not referenced; pre-existing, no behaviour impact) unless its removal is trivially clean; do not otherwise alter the AltCover instrument/collect flow or the step-3 RimWorld-DLL removal.
- [ ] Verify `.asd/project/commands.yaml` `test` and `coverage` command strings still resolve and run against the migrated suite (the wrappers are runner-agnostic; expect no string change).
- [ ] Confirm no `.github/` workflow or other in-repo CI config is added (scope reduces to the coverage script + command registry).
- [ ] Run `scripts/coverage.ps1` end-to-end and confirm the AltCover instrument → step-3 DLL-removal → lazy runtime-resolve → collect path cooperates without crashing under the NUnit3 adapter.

### Task 7: Verification gate (build, run, coverage, lint)
Owner: asd-backend-dev. Depends on: Tasks 1-6. Satisfies: AC-26, AC-28 (coverage-threshold side); cross-cutting confirmation of AC-1 … AC-25, AC-27.
- [ ] Run `jb-cleanup` (solution code-cleanup profile) before build, per `.asd/project/custom-coding-rules.md`.
- [ ] Build the test project under the inherited `Directory.Build.props` governance (`TreatWarningsAsErrors`, `WarningLevel 9999`, `Nullable`) in Release and confirm zero warnings/errors — resolve any NUnit/FluentAssertions analyzer or obsolete-API warning at the source (no blanket suppression).
- [ ] Run `dotnet test` under the NUnit3 adapter and confirm all tests are discovered and green: 142 executed cases pass and the 3 `[Test, Ignore(...)]` are reported as ignored.
- [ ] Run `lint` (`dotnet format --verify-no-changes`), then `jb-inspect`, and verify `TestResults/jb-inspect.sarif` has no `error` or `warning` severity entries.
- [ ] Confirm no `Assert.*` remains where an FA equivalent exists, and no residual `[Fact]`/`[Theory]`/`[InlineData]`/`[Collection]`/`XunitTestFramework`/`[assembly: TestFramework]` tokens remain (grep sweep).
- [ ] Run `scripts/coverage.ps1` and confirm testable-core coverage ≥ 37.2% (baseline 38.2% − 1.0 pp tolerance); expected ≥ 38.2% since no production code or test case is removed. Record the measured figure for the impl-review verdict.
- [ ] Confirm production code in `Source/LordKuper.Common` is unchanged except where strictly required by the migration.

## Risks
- Assembly-resolver timing (highest risk): a global `[SetUpFixture]`/`[OneTimeSetUp]` registers the resolver at execution time and does not provably precede discovery-time type loading (ADR-0006 accepted risk). If RimWorld-typed tests JIT-fail at discovery, the symptom is mass test failure, not an obvious wiring bug. Mitigation: namespace-less placement is a hard requirement; verify by running the RimWorld-typed suites; escalate to the `[ModuleInitializer]` (net472 polyfill) fallback if discovery-time resolution actually fails (Task 2).
- `BeApproximately` tolerance: xUnit precision is decimal-place rounding, FA tolerance is an absolute band — a wrong tolerance silently loosens/tightens a float assertion. Mitigation: `10^-precision` rule + boundary spot-checks, each precision site flagged as a reviewable edit (Task 5).
- Warnings-as-errors gate: any NUnit/FluentAssertions analyzer or obsolete-API warning fails the build under `WarningLevel 9999`. Mitigation: build Release and resolve warnings at source before impl-review (Task 7).
- Refactor+cleanup scope creep: opportunistic assertion tightening could silently change test strength. Mitigation: every strength change is an explicit, reviewable edit, never an incidental conversion side effect (Task 5).
- Coverage-harness coupling: `coverage.ps1` step 3 deletes the copied RimWorld DLLs and relies on the ported runtime resolver; a stale `--assemblyFilter` or a resolver regression makes coverage crash or drift. Mitigation: swap the filter and re-run end-to-end in the same change (Tasks 2, 6).

## Dependencies
- Task 2, Task 3, Task 4, Task 5 each depend on Task 1 (package/usings swap).
- Task 4 and Task 5 touch the same 11 test files and must be coordinated (sequence or careful merge) to avoid edit conflicts.
- Task 6 depends on Task 1 and Task 2 (the resolver must be live for the coverage step-3 lazy-resolve path).
- Task 7 depends on Tasks 1-6 (it is the integrated verification gate).

## Out of scope
- Adding new test coverage beyond the existing suite — coverage stays equivalent, not expanded.
- Changing production code in `Source/LordKuper.Common` except where strictly required by the migration.
- Adding any in-repo CI workflow (`.github/workflows/`, pipeline YAML); CI scope reduces to `scripts/coverage.ps1` + `.asd/project/commands.yaml`.
- Migrating to FluentAssertions 8.x or any commercially-licensed assertion library — pinned to 7.x.
- Deleting or permanently disabling the 3 RimWorld-context-dependent tests — they remain as `[Test, Ignore(...)]`.
- Changing build governance (`Directory.Build.props`), the RimWorld reference set, or the production project reference.
- Editing persistent design docs (xUnit/coverlet tech-references, `stack.html`, ADR-0001) or project rules in place — those route through design / design-promote, tracked in the audit's documentation-migration plan, not this plan.
