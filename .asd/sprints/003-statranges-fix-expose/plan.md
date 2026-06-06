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

This sprint corrects a confirmed first-observation defect in `StatRanges.UpdateStatRange`, exposes the `StatRanges` type as a public, reusable normalization primitive for the downstream EquipmentManager mod, locks the corrected contract in with revert-sensitive tests, and republishes the corrected assembly. The type stays `static` and process-global by design (ADR-0008 D3); it is not converted to an instance class. Decomposition is disabled, so the plan is flat: four tasks covering the production fix + exposure, the test-reset reroute, the strengthened test suite, and the green build + assembly republish.

All work is in two files plus the build output:
- Production: `Source/LordKuper.Common/StatRanges.cs`
- Tests: `Source/LordKuper.Common.Tests/StaticStateTestBase.cs`, `Source/LordKuper.Common.Tests/StatRangesTests.cs`
- Publish target: `1.6/Assemblies/` (net472)

The scope is small and precise; tasks are kept proportional. ADR-0002 / ADR-0007 wording reconciliation is explicitly out of scope here — it is design-promote work already recorded under ADR-0008's Amends note.

## Context

- [prd.html](./design/prd.html) — acceptance criteria AC-1..AC-10
- [adr.html](./design/adr.html) — ADR-0008 (D1 first-observation fix, D2 public exposure + `Clear()`, D3 keep static/process-global, D4 reset via `Clear()`)
- [audit.md](./audit.md) — file:line references for all touched areas
- [ADR-0002 — StatRanges adaptive normalization](../../../design/architecture/adr/adr-0002-statranges-adaptive-normalization.html) — adaptive contract realized by D1 (amended by ADR-0008)
- [ADR-0007 — StaticState isolation NUnit remap](../../../design/architecture/adr/adr-0007-staticstate-isolation-nunit-remap.html) — reset-set membership preserved by D4 (amended by ADR-0008)
- [commands.yaml](../../project/commands.yaml) — build / test / lint / inspect / coverage command keys

## Definition of Done

- All PRD acceptance criteria AC-1..AC-10 are satisfied by the tasks below; the AC-to-Task coverage map is complete (see table).
- Test scope is unit tests only: `StatRanges.cs` is headless library logic exercised through `FakeDefProvider` + `StatHelper.Rebuild()`; no integration, end-to-end, or manual testing is in scope (no live RimWorld game context is required for the changed logic).
- The named regression test (`NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange`) passes on the fix and is verified to fail when the fix is reverted to the old `[0, v]` behavior.
- All seven impl-review reviewers (plus external review if enabled) return green; any findings are resolved through the impl⇄impl-review cycle before this phase closes.
- The solution builds green with **0 warnings** under high warning level + warnings-as-errors (`build` key), and the full test suite (existing + new) passes (`test` key); `lint` and the `jb-inspect` SARIF are clean.
- The corrected Common assembly (net472) is rebuilt from the fixed source and republished to `1.6/Assemblies/`.

### AC-to-Task coverage map

| AC | Covered by |
|---|---|
| AC-1 | Task 1 |
| AC-2 | Task 1, Task 3 |
| AC-3 | Task 1 |
| AC-4 | Task 1 |
| AC-5 | Task 1, Task 2 |
| AC-6 | Task 1 |
| AC-7 | Task 3 |
| AC-8 | Task 3 |
| AC-9 | Task 1, Task 4 |
| AC-10 | Task 4 |

### Task 1: Fix first-observation bug and expose StatRanges (owner: backend-dev) — AC-1, AC-2, AC-3, AC-4, AC-5, AC-6, AC-9

File: `Source/LordKuper.Common/StatRanges.cs`.

- [x] Rewrite `UpdateStatRange` (lines 53-66): on a `TryGetValue` miss, seed a **local** `FloatRange(value, value)`; run both the `min` and `max` comparisons against that seeded local value (never the stale `{0, 0}` `default(FloatRange)`); write the dictionary entry **exactly once**. The first value `v` for any stat yields `[v, v]`. (AC-1, AC-2)
- [x] Ensure the rewrite leaves no unused or dead locals (warnings-as-errors will fail the build otherwise). (AC-9)
- [x] Change `internal static class StatRanges` to `public static class StatRanges`. (AC-3)
- [x] Change `internal static float NormalizeStatValue(StatDef stat, float value)` to `public static`, keeping the signature exactly `NormalizeStatValue(StatDef, float)` (no signature change). (AC-4)
- [x] Add `public static void Clear() => Ranges.Clear();`. (AC-5)
- [x] Keep the class `static` and process-global; do NOT convert it to an instance class and do not alter the `Ranges` cache semantics relied on by `WorkTypeThingRule`. (AC-6)
- [x] Add/adjust XML doc comments for the now-public class, the now-public `NormalizeStatValue`, and the new `Clear()` member, consistent with the ADR-0002 adaptive/order-dependent contract; keep the existing `NormalizeStatValue` `<remarks>` accurate. (AC-9)

### Task 2: Route test-isolation reset through Clear() (owner: test-engineer) — AC-5

File: `Source/LordKuper.Common.Tests/StaticStateTestBase.cs` (lines 107-111).

- [x] Replace the reflection-null reset of the `StatRanges.Ranges` backing field with a direct `StatRanges.Clear();` call, removing the stringly-typed `"Ranges"` field lookup. Preserve the reset-set membership contract (the `StatRanges` entry stays reset on `[TearDown]`); only the mechanism changes. (AC-5)

### Task 3: Strengthen StatRanges tests with exact-bound and regression assertions (owner: test-engineer) — AC-2, AC-7, AC-8

File: `Source/LordKuper.Common.Tests/StatRangesTests.cs`, using the existing `FakeDefProvider` + `StatHelper.Rebuild()` setup pattern.

- [x] Add an exact-bound test: first `v = 50` yields range `[50, 50]`, `NormalizeStatValue(stat, 50)` returns `0`, then `NormalizeStatValue(stat, 100)` returns `1`. (AC-2, AC-7)
- [x] Add an exact-bound test for the sequence `-10, -5, 1`: range becomes `[-10, -5]` then `[-10, 1]`, with exact normalized values verified including mixed-sign behavior. (AC-2, AC-7)
- [x] Add a named regression test `NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange` that passes on the fix and demonstrably fails if the fix is reverted to the old `[0, v]` behavior. (AC-8)
- [x] Keep the six existing tests; all still pass under the fixed code. (AC-7)

### Task 4: Build green and republish corrected assembly (owner: backend-dev) — AC-9, AC-10

- [ ] Build the solution with 0 warnings under high warning level + warnings-as-errors using the `build` command key (`dotnet build Source\LordKuper.Common.slnx -c Release`). (AC-9)
- [ ] Run the full test suite (existing + new) green using the `test` command key (`dotnet test Source\LordKuper.Common.Tests\LordKuper.Common.Tests.csproj`). (AC-9)
- [ ] Verification step: confirm the regression test fails when the fix is reverted (revert locally, observe failure, restore the fix) — proves the test is revert-sensitive. (AC-8 support)
- [ ] Run `lint` (`dotnet format ... --verify-no-changes --severity warn`) and `jb-inspect`; confirm the SARIF has no error/warning entries. (AC-9)
- [ ] Rebuild and republish the corrected Common assembly (net472) so the output lands in `1.6/Assemblies/`. (AC-10)

## Risks

- Public-API commitment: promoting `StatRanges` + `NormalizeStatValue` + `Clear()` to `public` makes them a consumer-facing surface. Mitigation: keep the `NormalizeStatValue(StatDef, float)` signature unchanged and document the adaptive/order-dependent contract on the public members (`backward_compat: none` permits future breaks, but the surface is adopted deliberately).
- Behavior shift for existing consumers: `WorkTypeThingRule` scores change for first-observed stats (`[v, v] → 0` instead of buggy `[0, v] → nonzero`). This is the intended correction; the new exact-bound tests lock the corrected values.
- Warnings-as-errors strictness: incomplete XML docs on new public members or any dead local in the rewrite fails the build. Mitigation handled in Task 1 subtasks.
- Stale assembly: shipping corrected source against a stale `1.6/Assemblies/` artifact is a silent regression. Mitigation: Task 4 treats republish as a hard gate after green tests.

## Dependencies

- Task 2 depends on Task 1 (`StatRanges.Clear()` must exist before `StaticStateTestBase` can call it).
- Task 3 depends on Task 1 (the corrected behavior and public `NormalizeStatValue`/`Clear()` must exist for the exact-bound and regression assertions to pass as specified).
- Task 4 depends on Tasks 1, 2, and 3 (build + test + republish run against the completed fix, reroute, and tests).

## Out of scope

- Migrating the EquipmentManager mod itself onto `Common.StatRanges` / `Common.WorkTypeThingRule` and deleting its duplicate range logic (`EquipmentManagerGameComponent_StatRanges`). Motivating context only — a separate, future effort.
- Wording reconciliation of ADR-0002 and ADR-0007. Owned by design-promote (recorded in ADR-0008's Amends note); not part of this plan's implementation.
