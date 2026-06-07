---
responsibility:
  owns: brownfield findings for sprint scope (existing docs, code, gaps, risks)
  excludes: requirements, decisions, plan, code
  delegates_to: prd.html (requirements), adr.html (decisions), plan.md (tasks)
---

# Audit

## Scope reference
[sprint.md](./sprint.md)

## Touched areas (docs side)
- `design/product/`: product-level docs (`concept.html`). The concept mentions a "consistent IMGUI widget toolkit" as a project pillar but never describes individual widgets. A small additive change to one widget does not alter the product concept; no concept update is expected from this sprint.
- `design/architecture/`: architecture docs (`stack.html`, ADRs, tech-reference). `stack.html` references the IMGUI toolkit broadly under `Source/UI/**`; no widget-level API doc exists. No existing ADR concerns `WorkTypeThingRuleWidget` or its public surface.
- Missing area — per-widget / public-API reference docs: there is no `design/api/` tree and no document describing `WorkTypeThingRuleWidget`, `ThingIconBox`, `DoWidgetTab`, or the bottom-section layout. If this sprint breaks the public signature of `DoWidgetTab(...)` (allowed by scope, `backward_compat: none`), there is currently no persistent doc that records the widget's public contract to update — the break is captured only in `sprint.md` and the decisions-log.

## Existing docs found
- None found.
  - No documentation anywhere in the repo (`design/`, `.asd/project/`, repo root README, or elsewhere) describes `WorkTypeThingRuleWidget`, `ThingIconBox`, `DoThingBox` / `DoThingDefBox`, `DoWidgetTab`, `GetBottomPartHeight` / `DoBottomPart`, the work-type rule tab, the "available items" lists, or stat-scoring of things.
  - Closest adjacent references (do NOT describe the widget or its API; listed for traceability only):
    - [stack.html](../../../design/architecture/stack.html) (line 183): single generic line — Unity IMGUI "underpinning the widget toolkit in `Source/UI/**`". No per-widget detail.
    - [concept.html](../../../design/product/concept.html): product pillar "a consistent IMGUI widget toolkit" and value-prop "a unified widget set (buttons, fields, sliders, selectors, tabs, scroll views, pawn boxes)". Generic; no work-type-rule widget, no two-list behavior.
    - [README.md](../../../README.md): one line ("Common library for my Rimworld mods"). No API surface.
  - There is no `design/api/`, `design/product/requirements/`, or any reverse-engineered/migrated PRD for this widget. No external/user-provided documentation URLs were supplied.

## Documentation migration plan

No pre-existing out-of-format/out-of-location documentation describes this widget, so there is nothing to migrate into `design/` from a non-ASD source. The items below are persistent-doc *creation/update* candidates the architect and design phase should weigh; they flow through design → design-promote and are recorded here for visibility (the existing-docs migration table follows and is empty).

- New widget public-API reference (candidate): the sprint adds/changes the public entry point that enables the second list and may break `DoWidgetTab(...)`'s signature (`backward_compat: none`). There is currently no persistent record of this widget's public contract. Recommend the design phase produce a public-API doc fragment (e.g. `design/api/worktype-thing-rule-widget.html`, or the project's chosen api-doc target) capturing the post-change public surface, the enable-second-list parameter form (chosen during design), and the score-sort ownership decision. Whether this is created now or deferred is a design-promote decision.
- ADR (candidate, conditional): if the public signature of `DoWidgetTab(...)` is broken, or if the score-sort responsibility (consumer pre-sorts vs widget sorts) is decided as a non-obvious tradeoff, an ADR may be warranted to record the decision and its rationale per the project's decisions-log convention. The architect decides in the design phase whether the change rises to ADR level; this is flagged, not mandated.
- Concept / stack (no change expected): an additive second list in one widget does not change the product vision or the tech stack. No update to `concept.html` or `stack.html` is anticipated.

Items found outside ASD format/location that should become persistent docs in `design/`:

| # | Source (path/URL) | Format | Proposed target in `design/` | Type | Notes |
|---|---|---|---|---|---|
| — | — | — | — | — | no migrations: no pre-existing out-of-format docs describe this widget |

<!-- Code-side sections appended by the architect (asd-architect). -->

## Touched areas (code side)

- `Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs` — public entry `DoWidgetTab(...)` (`:179-199`); private `DoBottomPart(...)` (`:28-44`); private `GetBottomPartHeight(int)` (`:206-210`); private `GetWorkTypeDefTooltip(ThingDef, WorkTypeThingRule)` (`:227-246`). This is the file scope targets.
- `Source/LordKuper.Common/UI/Widgets/ThingIconBox.cs` — render primitives `DoThingDefBox(...)` (`:97-136`, List 1, in use) and `DoThingBox(...)` (`:39-79`, List 2, exists but currently unused); height helper `GetThingIconBoxHeight(int)` (`:144-150`).
- `Source/LordKuper.Common/WorkTypeThingRule.cs` — scoring API `GetThingDefScore(ThingDef)` (`:200-209`) and `GetThingScore(Thing)` (`:234-242`); `StatWeights` (`:117-124`).
- `Source/LordKuper.Common/Helpers/StatHelper.cs` — `GetStatValue(Thing, StatDef)` (`:131-155`) and `GetStatValueDeviation(Thing, StatDef)` (`:216-`) used by per-instance tooltip + score paths.
- `Source/LordKuper.Common/UI/Tabs.cs` — `DoTab(...)` (`:45-58`) composes the three-region (top / scrollable / bottom) tab; bottom region gets a single fixed height from `GetBottomPartHeight`.

## Existing implementation found

- **Current `DoWidgetTab` public signature (verbatim, `WorkTypeThingRuleWidget.cs:179-184`):**
  ```csharp
  public static void DoWidgetTab(Rect rect, ref float scrollableContentHeight,
      ref Vector2 scrollPosition, int thingIconBoxRowCount,
      IReadOnlyCollection<WorkTypeThingRule> workTypeRules,
      WorkTypeThingRule? selectedWorkTypeRule, Action<WorkTypeThingRule> selectRuleAction,
      Action updateThingsAction, ref Vector2 thingIconBoxScrollPosition,
      IReadOnlyList<ThingDef> things)
  ```
  It carries exactly **one** items collection (`IReadOnlyList<ThingDef> things`) and **one** thing-box scroll position (`ref Vector2 thingIconBoxScrollPosition`). There is no parameter for on-map `Thing` instances and no second scroll position.

- **Bottom-part rendering — single list only (`DoBottomPart`, `:28-44`):** draws one section header (`AvailableItemsLabel`/`AvailableItemsTooltip`), a refresh button in the right quarter of the header row, a vertical gap, then **one** `ThingIconBox.DoThingDefBox(remRect, ref thingIconBoxScrollPosition, things, null, def => GetWorkTypeDefTooltip(def, selectedRule))`. Returns early when `selectedRule == null`. No second header, no second box, no second scroll state.

- **Bottom-part height — sized for one box (`GetBottomPartHeight`, `:206-210`):**
  ```csharp
  var thingIconBoxHeight = ThingIconBox.GetThingIconBoxHeight(thingIconBoxRowCount);
  return thingIconBoxHeight + Labels.SectionHeaderHeight + Layout.ElementGap;
  ```
  Reserves exactly one box + one section header + one gap. The tab's bottom region (`Tabs.DoTab`, `Tabs.cs:45-58`) is a fixed band sized by this single return value; the bottom content is **not** scrollable as a whole (only each `ThingIconBox` scrolls internally). A second list will need its own additional reserved height here or the two boxes will overlap inside the fixed band.

- **`ThingIconBox.DoThingBox(...)` — render primitive for real instances EXISTS (`:39-79`):**
  ```csharp
  public static void DoThingBox(Rect rect, ref Vector2 scrollPosition,
      IReadOnlyList<Thing> things, Action<Thing>? rightClickAction,
      Func<Thing, string>? tooltipGetter)
  ```
  Structurally parallel to `DoThingDefBox`. Per-instance tooltip is supported via `tooltipGetter` (`:66`, `TooltipHandler.TipRegion(thingRect, tooltipGetter(thing))`). Left-click opens `Dialog_InfoCard(thing)` (`:71`), right-click invokes `rightClickAction`. Icon resolution handles styled/instance graphics (`:59-62`). This method has no current caller in the production assembly — it is dormant and ready to wire up.

- **`DoThingDefBox(...)` (`:97-136`)** is the List-1 primitive (signature mirrors above but typed on `ThingDef`). Both boxes scroll internally and compute `itemsPerRow`/`rowCount` from `rect.width`.

- **Tooltip helpers:**
  - List 1 (def): `GetWorkTypeDefTooltip(ThingDef def, WorkTypeThingRule rule)` (`:227-246`) — builds `def.LabelCap`, then for each weighted stat appends `- {stat.LabelCap} = {StatHelper.GetStatValue(thing, stat):N2}`. Note it **constructs a temporary `Thing`** from the def (`ThingMaker.MakeThing`, `:237-239`) to compute stat values, and returns early if `Current.Game == null` (`:231`).
  - List 2 (instance): **no `GetWorkTypeThingTooltip(Thing, rule)` helper exists.** The building blocks are present: `StatHelper.GetStatValue(Thing, StatDef)` (`StatHelper.cs:131-155`) returns the actual stat value of a specific instance (including `equippedStatOffsets`). A per-instance tooltip would be an additive private helper analogous to `GetWorkTypeDefTooltip`, but it would read the real instance's stats directly (no `ThingMaker` synthesis needed).

- **Scoring — both def AND instance score paths already exist (`WorkTypeThingRule.cs`):**
  - `GetThingDefScore(ThingDef def)` (`:200-209`) — sums `NormalizeStatValue(statDef, GetStatValueDeviation(def, statDef)) * weight` over the rule's stat weights.
  - `GetThingScore(Thing thing)` (`:234-242`) — the **instance** equivalent, normalized `[-1..1]`, summing over `_statWeights.Values` using `StatHelper.GetStatValueDeviation(thing, statDef)`. This is the exact score-for-instance path List 2's descending sort needs; it does **not** need to be added.
  - Reference sort precedent: `GetGloballyAvailableItems()` (`:168-176`) already does `items.SortByDescending(GetThingDefScore)` for the def list. The same pattern with `GetThingScore` would sort the instance list.
  - **Caveat (documented in source, `:181-194` / `:215-228`):** both score methods are intentionally **observation-order-dependent** — `StatRanges.NormalizeStatValue` maintains a running, expanding per-stat min/max, so a thing's normalized score depends on the set/order of all items scored in the process. Design must decide where the sort happens (consumer vs widget) with this non-stability in mind; sorting the two lists against a shared range history will produce different normalization than sorting them in isolation.

- **Strings:** only `AvailableItemsLabel` / `AvailableItemsTooltip` exist for the bottom section (`DoBottomPart:38-39`). A second list with its own header label/tooltip would need new entries in `Strings.WorkTypeThingRuleWidget`.

## Gaps

- **No public parameter to pass on-map `Thing` instances.** `DoWidgetTab` accepts only `IReadOnlyList<ThingDef> things`. AC1 requires a public way to supply/enable the instance list — the signature must change (allowed; `backward_compat: none`). Exact form (overload / optional param / options object) is **deferred to design** per sprint.md.
- **No second scroll position.** Only `ref Vector2 thingIconBoxScrollPosition` exists; List 2 needs its own scroll state threaded through `DoWidgetTab` → `DoBottomPart`, or a shared/different scroll model decided in design.
- **`DoBottomPart` renders one box.** Needs a second header + second `DoThingBox` call (and the layout split between the two boxes) added.
- **`GetBottomPartHeight` reserves height for one box only.** Must account for a second `GetThingIconBoxHeight(...)` + its header + gap, or the two boxes overlap inside the fixed bottom band (AC5). Whether both lists share `thingIconBoxRowCount` or take independent row counts is an open layout decision.
- **No per-instance tooltip helper.** AC2 needs a `Thing`-based tooltip showing the instance's actual stat values. Not present; must be added (building blocks exist via `StatHelper.GetStatValue(Thing, StatDef)`).
- **No wiring of `GetThingScore` into a sort for the rendered list.** The score method exists, but nothing currently sorts an on-map `Thing` list for display. AC3 sort must be applied — by the widget or the consumer (ownership **deferred to design**).
- **No tests** exist for `WorkTypeThingRuleWidget`, `ThingIconBox`, or `WorkTypeThingRule` scoring (no test file references them; the lone `StatefulSubsystemTests` hit is incidental). AC7 ("covered by tests where feasible") starts from zero coverage for this area; UI/IMGUI rendering is hard to unit-test, but `GetThingScore` ordering and any new tooltip/sort helper are testable in isolation.

## Risks

- **Public signature break → EquipmentManager consumer (`risk: high call-site breakage`)**: impact — `DoWidgetTab`'s current 10-parameter signature is the consumer's integration point; changing it (new params for the instance list / second scroll position) breaks the EquipmentManager call site. Mitigation — break is explicitly permitted and must be documented in `sprint.md` + decisions-log (AC6); consumer adapts separately (out of scope per sprint.md). Design should minimize churn by choosing the parameter form deliberately (single options object vs many positional params).
- **Bottom-band layout/overlap regression (`risk: medium`)**: impact — the bottom region is a **fixed-height, non-scrolling band**; if `GetBottomPartHeight` and `DoBottomPart` are not updated in lockstep, the second box overlaps the first or spills past the band (AC5, AC4 regression on List 1). Mitigation — update height calc and render split together; verify each box's reserved rect = header + `GetThingIconBoxHeight(rows)` + gaps; consider independent row counts.
- **Score correctness / order-dependence (`risk: medium`)**: impact — `GetThingScore`/`GetThingDefScore` normalization is observation-order-dependent by design (`WorkTypeThingRule.cs:181-194`). Rendering two lists may perturb the shared `StatRanges` history, shifting both lists' apparent ordering/normalized values vs. before. Mitigation — design must explicitly decide sort ownership (consumer vs widget) and account for shared range state; AC3 ("descending score order") is satisfied by relative order within a list, which `SortByDescending(GetThingScore)` preserves regardless of absolute normalization.
- **`Current.Game == null` / instance availability (`risk: low`)**: impact — instance lists only exist with a live game; tooltip/score paths reference live game state. Mitigation — consumer supplies the on-map things; widget should no-op gracefully when the instance list is null/empty (mirror the def-tooltip `Current.Game == null` guard pattern).

## Dependencies

- RimWorld `Assembly-CSharp` + Unity IMGUI (already referenced via `$(RimWorldManagedDir)`): `Thing`, `ThingDef`, `StatDef`, `Verse.Widgets`, `TooltipHandler`, `Dialog_InfoCard`, `MouseoverSounds`. No new external dependency — all primitives (`DoThingBox`, `GetThingScore`, `GetStatValue(Thing, StatDef)`) are already in the assembly. **No new tech introduced → no tech-reference doc required.**

## Migration notes

- Not applicable. This is an in-assembly additive change to an existing widget; no data migration, no doc migration on the code side. The only "migration" is the EquipmentManager consumer adapting to the new `DoWidgetTab` signature, which is out of scope for this sprint (consumer-side).

## Related open stubs

`.asd/project/stubs.md` does not exist in this repository (no stubs registry present). No open stubs reference the touched widget files or owners.

| Sprint of origin | File:Line | Reason | Owner |
|---|---|---|---|
| — | — | no related open stubs | — |
