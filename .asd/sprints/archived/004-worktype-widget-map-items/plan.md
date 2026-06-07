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

This sprint restores a second "available items" list in the bottom section of the work-type rule tab of the public widget `WorkTypeThingRuleWidget`. List 1 (items available by `ThingDef`, rendered via `ThingIconBox.DoThingDefBox`) stays unchanged; List 2 renders real on-map `Thing` instances supplied and pre-sorted by the consumer (EquipmentManager), each with a per-instance stat tooltip, drawn side by side with List 1 inside the existing fixed bottom band.

The design is fully locked by ADR-0009 (public API form) and ADR-0010 (score-sort ownership):

- **ADR-0009** — extend the existing public `DoWidgetTab(...)` with two appended opt-in arguments: a nullable `IReadOnlyList<Thing>? mapThings` (default `null`) and a second `ref Vector2 mapThingIconBoxScrollPosition`. No overload, no options object, no separate method. `null`/empty `mapThings` reproduces today's single-list behavior (List 1 full width). The signature grows from 10 to 12 parameters — a documented break (`backward_compat: none`, AC-6). Layout is a horizontal **width** split of the bottom band; the band **height** is constant — `GetBottomPartHeight` is **unchanged** — and both boxes share the single existing `thingIconBoxRowCount`.
- **ADR-0010** — the **consumer pre-sorts** the on-map `Thing` list by descending `WorkTypeThingRule.GetThingScore`; the widget renders in the given order and must **not** call `GetThingScore` (or otherwise re-sort) during render, keeping the render pass free of side effects on the shared `StatRanges` history.

Subsystem decomposition is disabled, so the plan is flat. All production work belongs to **backend-dev**; this is a RimWorld IMGUI widget with no automatable integration test, so the test scope is unit tests for the new pure/isolatable logic plus a documented manual IMGUI verification.

All work is in three production files plus tests and the build output:

- Production: `Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs` (signature, tooltip helper, `DoBottomPart` width split), `Source/LordKuper.Common/UI/Widgets/ThingIconBox.cs` (wire the dormant `DoThingBox`), `Source/LordKuper.Common/Strings*` (`Strings.WorkTypeThingRuleWidget` — two new entries).
- Tests: `Source/LordKuper.Common.Tests/` (new coverage for the testable logic; this area starts from zero coverage).
- Publish target: `1.6/Assemblies/` (net472).

The audit found **no related open stubs** (`.asd/project/stubs.md` does not exist), so there are no stub-resolution tasks.

## Context

- [prd.html](./design/prd.html) — acceptance criteria AC-1..AC-7
- [adr.html](./design/adr.html) — ADR-0009 (public API form: two nullable opt-in params, side-by-side width split, constant height, shared row count) and ADR-0010 (consumer pre-sorts; widget never calls `GetThingScore` at render)
- [audit.md](./audit.md) — file:line references for all touched areas (`DoWidgetTab:179-199`, `DoBottomPart:28-44`, `GetBottomPartHeight:206-210`, `GetWorkTypeDefTooltip:227-246`, `ThingIconBox.DoThingBox:39-79`, `DoThingDefBox:97-136`, `GetThingIconBoxHeight:144-150`, `StatHelper.GetStatValue:131-155`)
- [ADR-0009 / ADR-0010 (persistent)](../../../design/architecture/adr/adr-0009-worktype-widget-map-things-list.html) and [adr-0010-worktype-map-list-consumer-presort.html](../../../design/architecture/adr/adr-0010-worktype-map-list-consumer-presort.html)
- [ADR-0008 (persistent)](../../../design/architecture/adr/adr-0008-statranges-first-observation-fix-public-exposure.html) — exposed the public `GetThingScore` the consumer's pre-sort depends on
- [commands.yaml](../../project/commands.yaml) — `build`, `test`, `lint` and the custom `jb-cleanup` / `jb-inspect` command keys

## Definition of Done

- All PRD acceptance criteria AC-1..AC-7 are satisfied by the tasks below; the AC-to-Task coverage map is complete (see table).
- The public `DoWidgetTab(...)` signature break is documented explicitly: old and new signatures are recorded in `sprint.md` and the decisions-log / `adr.html` (AC-6), and the new public arguments carry XML doc that states the `mapThings` pre-sort contract (ADR-0010 trust boundary).
- Test scope is explicit: **unit tests** cover only the isolatable/pure logic — `GetThingScore` descending-order behavior (the contract the consumer's pre-sort relies on), the new per-instance tooltip helper's output, and the null/empty `mapThings` no-op (List-1-only, no second box, full-width) where reachable headlessly; the IMGUI render path itself is **not** unit-testable and is covered by a documented **manual IMGUI verification** (two lists side by side, no overlap, List 1 unchanged when `mapThings` is null/empty, per-instance tooltips correct). No automatable integration/end-to-end test is in scope.
- The widget never calls `GetThingScore` (or any re-sort) during render (ADR-0010); List 2 renders in the order the consumer supplied.
- `GetBottomPartHeight` is unchanged — the bottom band height is identical for the one-list and two-list cases; the two boxes occupy disjoint horizontal halves with no overlap (AC-5).
- All seven impl-review reviewers (plus external review if enabled) return green; any findings are resolved through the impl⇄impl-review cycle before the sprint advances.
- The solution builds green with **0 warnings** (`build` key), the full test suite (existing + new) passes (`test` key), and `lint` + the `jb-inspect` SARIF are clean.
- The corrected `LordKuper.Common` assembly (net472) is rebuilt from the changed source and republished to `1.6/Assemblies/`.

### AC-to-Task coverage map

| AC | Covered by |
|---|---|
| AC-1 | Task 1 |
| AC-2 | Task 2, Task 3 |
| AC-3 | Task 3, Task 5 |
| AC-4 | Task 3, Task 4 |
| AC-5 | Task 3, Task 4 |
| AC-6 | Task 1 |
| AC-7 | Task 5, Task 6 |

### Task 1: Extend `DoWidgetTab` public signature with nullable `mapThings` + second scroll ref (owner: backend-dev) — AC-1, AC-6

File: `Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs` (`DoWidgetTab`, `:179-199`).

- [x] Append two arguments to `DoWidgetTab(...)` per ADR-0009: `ref Vector2 mapThingIconBoxScrollPosition` and `IReadOnlyList<Thing>? mapThings = null`, after the existing `IReadOnlyList<ThingDef> things` parameter. Signature grows 10 → 12 params. (AC-1)
- [x] Thread the two new arguments from `DoWidgetTab` into `DoBottomPart` symmetrically with the existing `thingIconBoxScrollPosition` / `things` plumbing (no behavioral change yet beyond passing them through). (AC-1)
- [x] Add XML doc on the new public `mapThings` parameter stating the consumer's **pre-sort contract** (descending `GetThingScore` order; the widget renders as-given and does not re-sort — ADR-0010 trust boundary) and that `null`/empty disables the second list. (AC-1, AC-6)
- [x] Record the AC-6 signature break: confirm the old and new `DoWidgetTab` signatures are captured verbatim in `sprint.md` and `adr.html` (already in `adr.html`); ensure the decisions-log carries the break. This is the AC-6 deliverable. (AC-6)

### Task 2: Add per-instance tooltip helper via `StatHelper.GetStatValue` (owner: backend-dev) — AC-2

File: `Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs` (new private helper analogous to `GetWorkTypeDefTooltip`, `:227-246`).

- [x] Add a private helper (e.g. `GetWorkTypeThingTooltip(Thing thing, WorkTypeThingRule rule)`) that builds the instance's label then, for each of the rule's weighted stats, appends the instance's **actual** stat value via `StatHelper.GetStatValue(Thing, StatDef)` (`StatHelper.cs:131-155`). (AC-2)
- [x] Read the **live instance** directly — do **not** synthesize a temporary `Thing` via `ThingMaker.MakeThing` (unlike the def helper); this picks up `equippedStatOffsets`. (AC-2)
- [x] Mirror the existing `Current.Game == null` early-return guard from `GetWorkTypeDefTooltip`. (AC-2)

### Task 3: Wire `ThingIconBox.DoThingBox` into `DoBottomPart` with side-by-side width split + new strings (owner: backend-dev) — AC-2, AC-3, AC-4, AC-5

Files: `Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs` (`DoBottomPart`, `:28-44`), `Source/LordKuper.Common/UI/Widgets/ThingIconBox.cs` (`DoThingBox`, `:39-79`, dormant), `Strings.WorkTypeThingRuleWidget`.

- [x] Add two new entries to `Strings.WorkTypeThingRuleWidget` for List 2's header label + tooltip (on-map / available-now), e.g. `AvailableItemsOnMapLabel` / `AvailableItemsOnMapTooltip`, alongside the existing `AvailableItemsLabel` / `AvailableItemsTooltip`. (AC-2)
- [x] In `DoBottomPart`, when `mapThings` is non-null and non-empty, split the bottom band's **width** into two disjoint halves: List 1 (`DoThingDefBox`, full existing behavior) in one half, List 2 (`DoThingBox`) in the other, each box on the same row with its own section header on a single header row. (AC-5)
- [x] Render List 2 via `ThingIconBox.DoThingBox(rect, ref mapThingIconBoxScrollPosition, mapThings, rightClickAction, def => GetWorkTypeThingTooltip(...))`, passing the second scroll position and the new per-instance tooltip helper from Task 2. (AC-2)
- [x] Render List 2 in the **consumer-supplied order** — do **NOT** call `WorkTypeThingRule.GetThingScore` or otherwise re-sort during render (ADR-0010). (AC-3)
- [x] When `mapThings` is `null` or empty, do **not** split the band: draw only List 1's header + box at **full width**, exactly as today (graceful no-op; mirrors the `Current.Game == null` pattern). (AC-4, AC-5)
- [x] Both boxes use the single shared `thingIconBoxRowCount`; each computes its own width-driven `itemsPerRow` internally — accepted narrower boxes / more internal scroll per ADR-0009. (AC-5)
- [x] Preserve List 1 exactly: same items, `GetWorkTypeDefTooltip` tooltips, and scroll behavior; the early return on `selectedRule == null` is unchanged. (AC-4)

### Task 4: Keep `GetBottomPartHeight` constant — verify no band-height change (owner: backend-dev) — AC-4, AC-5

File: `Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs` (`GetBottomPartHeight`, `:206-210`).

- [x] Leave `GetBottomPartHeight` **unchanged**: it still returns one `GetThingIconBoxHeight(thingIconBoxRowCount)` + one `SectionHeaderHeight` + one `ElementGap`, and does **not** branch on second-list presence (side-by-side layout consumes width, not height). (AC-5)
- [x] Confirm `GetBottomPartHeight` and `DoBottomPart` cannot mis-size the band relative to each other: the reserved band height is identical whether one list or both render, so the two boxes occupy disjoint horizontal regions with no overlap and no spill past the band, and the tab layout above the band is undisturbed. (AC-4, AC-5)

### Task 5: Add unit tests for the new isolatable logic; document manual IMGUI verification (owner: backend-dev) — AC-3, AC-7

Files: `Source/LordKuper.Common.Tests/` (new tests; area starts from zero coverage). UI/IMGUI rendering is not unit-testable; cover only the pure/isolatable logic.

- [x] Add a test asserting `WorkTypeThingRule.GetThingScore`-based descending ordering of a set of `Thing` instances — the contract the consumer's pre-sort (ADR-0010) relies on; relative best-first order holds regardless of observation-order-dependent absolute normalization. (AC-3, AC-7)
- [x] Add a test for the new per-instance tooltip helper's output where it can be exercised headlessly (or, if it requires a live game context, document why it is manual-only and assert the underlying `StatHelper.GetStatValue(Thing, StatDef)` path instead). (AC-7)
- [x] Add a test (where reachable headlessly) that the bottom-section behaves as List-1-only when `mapThings` is `null`/empty — no second box, List 1 full width — guarding AC-4. If the IMGUI path blocks a headless assertion, fold this into the manual verification note. (AC-7)
- [x] Document the **manual IMGUI verification** checklist (no automatable integration test exists for this widget): two lists render side by side with no overlap; List 2 shows on-map instances in the consumer-supplied order with correct per-instance tooltips; List 1 is unchanged; with `mapThings` null/empty List 1 renders full width at the same band height. (AC-7)

### Task 6: Build green (0 warnings), tests pass, republish to `1.6/Assemblies` (owner: backend-dev) — AC-7

- [x] Run `jb-cleanup` (`jb cleanupcode Source\LordKuper.Common.slnx --toolset-path=...`) before build. (AC-7)
- [x] Build the solution green with **0 warnings** using the `build` key (`dotnet build Source\LordKuper.Common.slnx -c Release`). (AC-7)
- [x] Run the full test suite (existing + new) green using the `test` key (`dotnet test Source\LordKuper.Common.Tests\LordKuper.Common.Tests.csproj`); confirm existing tests still pass (no regression). (AC-7)
- [x] Run `lint` (`dotnet format Source\LordKuper.Common.slnx --verify-no-changes --severity warn`) and `jb-inspect` (`jb inspectcode ... -o=.\TestResults\jb-inspect.sarif --build --toolset-path=...`); confirm the SARIF has no error/warning entries. (AC-7)
- [x] Rebuild and republish the corrected `LordKuper.Common` assembly (net472) so the output lands in `1.6/Assemblies/`. (AC-7)

## Risks

- **Public signature break → EquipmentManager call site (high).** The added `ref Vector2` cannot carry a default, so even List-1-only callers must pass a backing field; this breaks the consumer's call site. Permitted under `backward_compat: none`; mitigated by recording old/new signatures (AC-6, Task 1) and XML-documenting the new params. Consumer adaptation is out of scope (sprint Non-goals).
- **Bottom-band layout / overlap regression (medium).** If `DoBottomPart` and `GetBottomPartHeight` drift, the boxes overlap or spill the fixed band. Mitigated by ADR-0009's side-by-side width split keeping `GetBottomPartHeight` unchanged and never branching on second-list presence (Task 3 + Task 4 in lockstep).
- **Accidental render-time scoring (medium).** Calling `GetThingScore` during render would perturb the shared, order-dependent `StatRanges` history every frame. Mitigated by ADR-0010's consumer-pre-sort decision; Task 3 explicitly forbids the render-time call and Task 5 asserts ordering against `GetThingScore` directly instead.
- **Zero starting test coverage + warnings-as-errors (low/medium).** This widget/`ThingIconBox`/scoring area has no tests today, and incomplete XML docs on the new public params or any dead local fails the green build. Mitigated by Task 5 (isolatable-logic tests + documented manual scope) and Task 6 (clean build/lint/inspect gate).
- **`Current.Game == null` / instance availability (low).** Instance lists and per-instance stats need a live game. Mitigated by the tooltip helper's `Current.Game == null` guard (Task 2) and the null/empty `mapThings` no-op (Task 3).

## Dependencies

- Task 2 depends on Task 1 (the threaded `mapThings` / second scroll position must exist before the tooltip helper is wired into `DoBottomPart`).
- Task 3 depends on Task 1 and Task 2 (needs the new parameters threaded through and the per-instance tooltip helper).
- Task 4 depends on Task 3 (the height invariant is verified against the completed side-by-side `DoBottomPart`).
- Task 5 depends on Task 2 and Task 3 (tests target the tooltip helper and the null/empty no-op behavior; the ordering test targets the public `GetThingScore`).
- Task 6 depends on Tasks 1–5 (build + test + republish run against the completed change).

## Out of scope

- Changes inside the EquipmentManager mod — the consumer adapts to the new `DoWidgetTab(...)` signature and owns the descending-`GetThingScore` pre-sort separately (sprint Non-goals).
- The map-item collection logic — gathering the on-map `Thing` instances is supplied by the consumer; the widget renders only the supplied data.
- Introducing independent per-list row counts, an options object, or an overload — rejected in ADR-0009; any such divergence would be a separate Complication-Approved change.
- Creating a persistent `design/api/` widget reference doc — a design-promote decision owned by the architect, not this plan.
