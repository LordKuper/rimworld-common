---
responsibility:
  owns: task breakdown, dod, task status (checkboxes)
  excludes: requirements, design decisions, code, review findings
  delegates_to: design/ docs (requirements/design), reviews/ (findings)
---

# Plan

## Overview

This plan resolves the in-scope backlog of sprint `001-full-project-audit`: all 27 acceptance criteria from the PRD, carried end-to-end (impl → review → pr) per shape B. Work is organized into **14 tasks** (Task 0 + Tasks 1–13) spanning a compile-green prerequisite plus four themes — build governance, the RimWorld-context isolation seam, targeted source improvements, and a bootstrap test suite to an ≥80% coverage floor.

**Task 0 is the absolute first gate before Task 1**: a trial `dotnet build Source -c Release` currently FAILS with 33 nullable-flow errors (CS8604/CS8602/CS8629) surfaced by the `<Nullable>enable</Nullable>` switch — the branch does not compile, so nothing else can build or be tested until Task 0 is green. The user chose to fix these under `enable` (not revert to `annotations`).

The dependency spine is **Task 0 (compile-green) → Task 1 (governance) → Task 3 (seam) → coverage**: Task 0 makes the branch compile clean; Task 1 establishes solution-wide build governance (the clean, warning-as-error baseline every later task builds on); Task 3 introduces the `IDefProvider` isolation seam (ADR-0001) that unblocks both the core-logic improvements that touch stateful subsystems and the entire test build-out; the test tasks (8–11) then climb from harness to pure-path tests to stateful-subsystem tests to the Coverlet coverage floor. Task 13 is the final cross-cutting verification gate that proves the whole solution is warning-clean, inspection-clean, fully tested, suppression-free, 1.5-untouched, and that any breaking public-API surface is enumerated for the PR.

Two audit improvements are **won't-do** and are not realized by any task: IMP-03 (pin `LangVersion`) — rejected, `LangVersion` stays `latest`, anchored by AC-23's "no unnecessary scope" intent and the build-governance scope of AC-5; and IMP-09 (sync 1.5 localization to 1.6 parity) — rejected, 1.5 is a frozen archive, anchored by AC-23 (1.5 stays supported, untouched) and AC-27 (no AC requires modifying the 1.5 archive).

## Context

- [design/prd.html](design/prd.html) — 4 goals, 8 stories, 27 acceptance criteria, 8 non-goals
- [design/adr.html](design/adr.html) — ADR-0001 (isolation seam), ADR-0002 (StatRanges adaptive normalization), ADR-0003 (build governance)
- [audit.md](audit.md) — rule-compliance findings + IMP-01…IMP-12 in/out decisions
- [../../design/architecture/adr/adr-0001-rimworld-context-isolation-seam.html](../../design/architecture/adr/adr-0001-rimworld-context-isolation-seam.html)
- [../../design/architecture/adr/adr-0002-statranges-adaptive-normalization.html](../../design/architecture/adr/adr-0002-statranges-adaptive-normalization.html)
- [../../design/architecture/adr/adr-0003-build-governance.html](../../design/architecture/adr/adr-0003-build-governance.html)
- [../../design/architecture/stack.html](../../design/architecture/stack.html) — SSoT for the VSE soft-dependency fact (AC-10)
- Commands (from `.asd/project/commands.yaml`): `build`, `test`, `lint`, `jb-cleanup`, `jb-inspect`

## Definition of Done

- **All 27 acceptance criteria** (AC-1 … AC-27) are satisfied; the AC → Task coverage map below shows every AC mapped to at least one task, with no orphan ACs and no orphan tasks.
- **Order-independent test suite**: the full xUnit suite produces the same result regardless of execution order (no inter-test static-state leakage), enforced by the `StaticStateFixture` save/restore (including `StatRanges.Ranges`). (AC-15, AC-16, AC-19)
- **Coverage floor**: Coverlet-measured line coverage of the `Source` assembly is **≥ 80%**. (AC-21)
- **Build clean — both configs**: a clean rebuild of the full solution (`build`) completes with **zero warnings** under `TreatWarningsAsErrors` + `WarningLevel 9999` for **both Source and Tests**; `jb-cleanup` then `build` is warning-clean and `lint` reports no changes. (AC-3, AC-4, AC-5)
- **Inspection clean — both configs**: `jb-inspect` SARIF contains **zero error and zero warning** severities across Source and Tests. (AC-5, supporting AC-25)
- **No new suppressions**: no `[SuppressMessage]` / `#pragma warning` introduced; intentional public surface signalled only via `[PublicAPI]`. (AC-25)
- **Breaking API enumerated**: every breaking public-API change (`backward_compat=none`) is intentional and explicitly enumerated in the PR description. (AC-26)
- **RimWorld 1.5 untouched**: no code, localization, or content change to the 1.5 archive; 1.5 remains in `supportedVersions`. (AC-23, AC-27)
- **Docs reconciled to About.xml**: `About.xml` remains the single source of truth for mod identity/versions/dependencies; design docs reconcile to it, and all persistent `design/` edits go through the design pipeline, never ad hoc. (AC-10, AC-11, AC-12, AC-22)
- **impl-review green**: all required impl-review reviewers (and external review, enabled) return APPROVE.

### Task 0 — Resolve nullable-flow build errors under `<Nullable>enable</Nullable>`
<!-- Owner: backend-dev. Deps: none — MUST land first; ALL other tasks depend on Task 0 (nothing builds until the branch compiles). ACs: prerequisite for AC-5 (warning-clean build) and AC-3/AC-4 (Nullable=enable governance); cross-cutting AC-25 (no new suppressions). -->
**Owner:** backend-dev · **ACs:** prerequisite for AC-5 (warning-clean build) and AC-3/AC-4 (Nullable=enable governance); cross-cutting AC-25 (no new suppressions — fix with guards/annotations, not `#pragma`/`!` abuse)
- [ ] Resolve all 33 CS86xx nullable-flow errors so `dotnet build Source -c Release` compiles clean under `Nullable=enable` + `TreatWarningsAsErrors`. Known sites include: `WorkTypeThingRule.cs:201` (CS8604 ×2), `WorkTypeThingRule.cs:224` (CS8602), `PawnFilter.cs:304,312,320,334` (CS8604), `PawnFilter.cs:441` (CS8602), `PawnFilterWidget.cs:126,510,641,752` (CS8604), `PawnFilterWidget.cs:241,269,276` (CS8629), `WorkTypeThingRuleWidget.cs:87` (CS8604) — plus any remaining to reach 0 errors.
- [ ] Prefer real null-guards / correct nullable annotations over suppression; the null-forgiving operator `!` only where provably non-null with a brief justification; NO new `#pragma`/`[SuppressMessage]` (AC-25).
- [ ] Re-run `dotnet build Source -c Release` → 0 errors / 0 warnings before proceeding.

**Depends on:** none. **MUST land first — ALL other tasks depend on Task 0** (nothing builds until the branch compiles).

### Task 1: Solution-wide build governance (ADR-0003)
<!-- Owner: backend-dev. Deps: Task 0 (compile-green gate). ACs: 2, 3, 4, 23. -->
- [ ] Add a repo-root `Directory.Build.props` setting `TreatWarningsAsErrors=true`, `WarningLevel=9999`, `Nullable=enable`, inherited by both Source and Tests
- [ ] Make the root props **explicitly import** the existing `Source/Directory.Build.props` via `GetPathOfFileAbove` (correct MSBuild precedence; no silent override of Source settings)
- [ ] Replace the hardcoded RimWorld build path with **fail-fast**: error if `RIMWORLD_DIR` / `RimWorldDir` is unset (no machine-specific default)
- [ ] Clean stale / mixed-target build intermediates (`v4.8`, `net8.0`) so only declared `net472` remains; confirm a clean rebuild
- [ ] Verify `supportedVersions` still lists `1.5` + `1.6` (1.5 stays the frozen archive) (AC-23)
- [ ] Run `jb-cleanup` then `build`: solution rebuilds warning-clean

### Task 2: Verify-close legacy packages/ (IMP-01)
<!-- Owner: backend-dev. Deps: none. AC: 1. -->
- [ ] Confirm no `Source/packages/` folder (Harmony 2.3.6 / MSTest) exists in the tree
- [ ] Confirm nothing in the solution references it
- [ ] Record the closure in the sprint trail (verification note)

### Task 3: IDefProvider isolation seam (ADR-0001)
<!-- Owner: backend-dev. Deps: Task 1. Gates Tasks 8-11. ACs: 13, 24, 25, 26. -->
- [ ] Introduce `IDefProvider` interface + `DefProvider.Current` accessor + `VerseDefProvider` (the live RimWorld `DefDatabase`-backed implementation)
- [ ] Reroute `StatHelper`, `WorkTypeStatMap`, `SkillStatMap`, `WorkTypeThingRule`, `DefCache`, `StatWeight`, `PassionHelper` to resolve `Def` dependencies through the seam
- [ ] Refactor static-constructor bodies into an internal `Rebuild()` so static caches can be rebuilt under test
- [ ] Add `InternalsVisibleTo` for the Tests assembly
- [ ] Emit clear `Logger` context on failure in the static-init paths that walk the `DefDatabase` (AC-24)
- [ ] Ensure no `[SuppressMessage]` / `#pragma` suppressions are introduced; signal intentional public surface only via `[PublicAPI]` (AC-25)
- [ ] Note any breaking public-API change introduced by the seam for the PR enumeration (AC-26)

### Task 4: WorkTypeStatMap null-stat logging (IMP-07)
<!-- Owner: backend-dev. Deps: Task 3. AC: 8. -->
- [ ] When a referenced `StatDef` resolves null via `GetNamedSilentFail`, log `Logger.LogWarning` with worktype + stat context (instead of a silent no-op)
- [ ] Keep the default weights in code as overridable seed defaults by design (no Def/config migration — non-goal)

### Task 5: StatRanges adaptive XML docs (IMP-08 / ADR-0002)
<!-- Owner: backend-dev. Deps: Task 3. AC: 9. -->
- [ ] Document the adaptive running-min/max observation-order dependence in the XML docs of `NormalizeStatValue`, `GetThingScore`, and `GetThingDefScore`
- [ ] Confirm the static `Ranges` cache is retained (ADR-0002 option b) and left fixture-isolatable for Task 8

### Task 6: Collapse Resources tooltip DRY (IMP-04)
<!-- Owner: backend-dev. Deps: Task 1. AC: 6. -->
- [ ] Collapse the 18 near-identical cached tri-state tooltip fields and `GetFilter*Tooltip` methods in `Resources.cs` into a single parameterized helper/lookup
- [ ] Verify the rendered tooltip strings are unchanged (no behavioral change)

### Task 7: Split PawnFilter.Combine (IMP-05)
<!-- Owner: backend-dev. Deps: Task 1. Combine behavior verified by Task 10 filter tests. AC: 7. -->
- [ ] Split `PawnFilter.Combine` into per-section helpers so no single function needs a paragraph to explain
- [ ] Preserve combine behavior unchanged (verified by Cluster F / Task 10 filter tests)

### Task 8: Test harness — FakeDefProvider + StaticStateFixture
<!-- Owner: test-engineer. Deps: Task 3. ACs: 14, 15, 16, 18, 19. -->
- [ ] Add `FakeDefProvider` implementing `IDefProvider` for test-time `Def` resolution through the seam
- [ ] Wire a `StaticStateFixture` into the harness via xUnit `IDisposable` / `IClassFixture` / `[Collection]` (per-test capture/restore mechanism)
- [ ] Fixture saves and restores the mutable static state of `StatHelper`, `WorkTypeStatMap`, `SkillStatMap`, `StatRanges`, `PassionHelper`, and the caches around each test
- [ ] Snapshot set explicitly includes the adaptive `StatRanges.Ranges` cache so running-min/max state cannot leak across tests (AC-16)
- [ ] Confirm the seam approach is recorded in ADR-0001 before broad test build-out (AC-18)
- [ ] Establish order-independence: the suite produces the same result regardless of execution order (AC-19)

### Task 9: Core pure-path tests
<!-- Owner: test-engineer. Deps: Task 8. AC: 17. -->
- [ ] Cover the pure paths of `RimWorldTime`
- [ ] Cover the pure paths of `MathHelper`
- [ ] Cover the pure paths of `PawnFilter` (extending the existing `EnumHelper` tests)

### Task 10: Stateful-subsystem tests
<!-- Owner: test-engineer. Deps: Task 8, Task 7. SINGLE task; cluster checkboxes below. AC: 20. -->
- [ ] **stat-infra**: `StatHelper`, `WorkTypeStatMap`, `SkillStatMap`, `StatRanges`, `StatWeight`
- [ ] **filters**: `PawnFilter` (incl. `Combine`), `Limits`
- [ ] **caches**: `TimedCache`, `ThingCache`, `DefCache`, `PassionCache`, `PassionHelper`
- [ ] **time + helpers**: `RimWorldTime`, `MathHelper` / `EnumHelper` / `Pawn` / `Def` / `Text` helpers
- [ ] **WorkTypeThingRule**

### Task 11: Coverage floor ≥80% (Coverlet)
<!-- Owner: test-engineer. Deps: Task 9, Task 10. AC: 21. -->
- [ ] Measure `Source`-assembly line coverage via Coverlet on the `test` run
- [ ] Close gaps until Coverlet-measured line coverage of `Source` is ≥ 80%

### Task 12: IMP-10 docs reconciliation (VERIFY-ONLY)
<!-- Owner: backend-dev. Deps: none. ACs: 10, 11, 12, 22. -->
- [ ] Confirm the `vanillaexpanded.skills` soft-dependency (modeled in `Compatibility/Vse.cs`) is already reflected in `stack.html` / `concept.html` (promoted in design-promote)
- [ ] Confirm `About.xml` remains the single source of truth for mod identity / supported versions / dependencies, and design docs reconcile to it (AC-22)
- [ ] Confirm README one-liner + `concept.html` vision + `About.xml` `supportedVersions`/dependencies are consistent (single source of truth, others link not copy) (AC-11)
- [ ] If any persistent `design/` edit is still required, route it through the design pipeline / doc-owning step — never author ad hoc against persistent `design/` (AC-12 forbids ad-hoc edits)

### Task 13: Cross-cutting verification gate (final)
<!-- Owner: backend-dev + test-engineer. Deps: ALL (Tasks 1-12). ACs: 5, 25, 26, 27. -->
- [ ] Run `jb-cleanup` then `build`: warning-clean for **both Source and Tests** under `TreatWarningsAsErrors` + `WarningLevel 9999` (AC-5)
- [ ] Run `lint`: reports no changes
- [ ] Run `jb-inspect`: SARIF has **zero error and zero warning** severities (AC-5, AC-25)
- [ ] Run `test`: all tests pass, order-independent
- [ ] Confirm no new `[SuppressMessage]` / `#pragma warning` suppressions were introduced (AC-25)
- [ ] Confirm the RimWorld 1.5 archive is untouched (no code/localization/content change) (AC-27)
- [ ] Enumerate the complete breaking public-API surface for the PR description (AC-26)

## AC → Task coverage map

| AC | Task(s) | AC | Task(s) |
|----|---------|----|---------|
| AC-1 | T2 | AC-15 | T8 |
| AC-2 | T1 | AC-16 | T8 |
| AC-3 | T1 | AC-17 | T9 |
| AC-4 | T1 | AC-18 | T8 |
| AC-5 | T1, T13 | AC-19 | T8 |
| AC-6 | T6 | AC-20 | T10 |
| AC-7 | T7 | AC-21 | T11 |
| AC-8 | T4 | AC-22 | T12 |
| AC-9 | T5 | AC-23 | T1 |
| AC-10 | T12 | AC-24 | T3 |
| AC-11 | T12 | AC-25 | T3, T13 |
| AC-12 | T12 | AC-26 | T3, T13 |
| AC-13 | T3 | AC-27 | T13 |
| AC-14 | T8 | | |

All 27 ACs (AC-1 … AC-27) are mapped; no orphan ACs and no task without an AC.

> **Task 0 (build-green prerequisite)** is not a primary AC owner: it does not appear in the map above because it duplicates no AC's primary ownership. It is the compile-green precondition that *feeds* AC-5 (warning-clean build) and AC-3/AC-4 (Nullable=enable governance) — those ACs cannot be satisfied while the branch fails to compile — and it is bound by cross-cutting AC-25 (no new suppressions). Primary ownership of AC-5 stays with T1/T13 and AC-3/AC-4 with T1.

## Risks

- **StatRanges static `Ranges` leakage**: if the fixture fails to snapshot/restore the adaptive running-min/max cache, order-independence (AC-19) breaks. Mitigated by the explicit AC-16 snapshot inclusion in Task 8.
- **Seam reroute breadth (Task 3 gates 8–11)**: rerouting seven subsystems through `IDefProvider` is the critical-path bottleneck; a defect here cascades to all test tasks.
- **Coverage floor**: reaching ≥80% may surface untestable paths needing further seam work or refactor; isolated to Task 11 but may loop back to Task 8.
- **Breaking-API enumeration**: `backward_compat=none` means seam/refactor work may change public surface; must be tracked continuously (Task 3, Task 13) to avoid an incomplete PR enumeration.

## Dependencies

- **Task 0 MUST land first — ALL other tasks (1–13) depend on Task 0**: the branch does not compile until the 33 nullable-flow errors are fixed, so no other task can build or be verified. Task 0 is the absolute first gate before Task 1.
- Task 1 depends on Task 0 (compile-green gate)
- Task 3 depends on Task 1
- Task 4 depends on Task 3
- Task 5 depends on Task 3
- Task 6 depends on Task 1
- Task 7 depends on Task 1 (combine behavior verified by Task 10)
- Task 8 depends on Task 3
- Task 9 depends on Task 8
- Task 10 depends on Task 8 and Task 7
- Task 11 depends on Task 9 and Task 10
- Task 13 depends on ALL (Tasks 1–12)
- Tasks 2 and 12 have no dependencies beyond Task 0 (can run any time once the branch compiles)

## Out of scope

- **IMP-03 — pin `LangVersion`**: won't-do; `LangVersion` stays `latest` (rejected at design). Anchored by AC-5 (build-governance scope) and AC-23 (no unnecessary version-set churn).
- **IMP-07 Def/config migration**: the balance table stays in code as overridable seed defaults; only the silent-null gap is logged (Task 4). Migration is a non-goal.
- **IMP-09 — sync 1.5 localization to 1.6 parity**: won't-do; 1.5 is a frozen archive. Anchored by AC-23 (1.5 stays supported, untouched) and AC-27 (no AC requires modifying the 1.5 archive).
- **StatRanges deterministic normalization**: rejected per ADR-0002; adaptive behavior is kept and documented (Task 5), not removed.
