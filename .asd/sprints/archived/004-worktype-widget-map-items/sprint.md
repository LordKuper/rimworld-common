---
responsibility:
  owns: sprint scope, goal, top-level acceptance criteria
  excludes: task breakdown, design decisions, code, audit findings
  delegates_to: plan.md (tasks), design/ docs (decisions), audit.md (audit)
---

# Sprint 004 — worktype-widget-map-items

## Goal

Restore, in the public widget `WorkTypeThingRuleWidget`
(`Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs`), the ability to show **two**
"available items" lists in the bottom section of the work-type rule tab instead of one.

- **List 1 (exists):** items available by definition (`ThingDef`), rendered via
  `ThingIconBox.DoThingDefBox`, with tooltips showing the calculated stat values for the selected
  rule's stat weights (`GetWorkTypeDefTooltip`). This list must remain functional with no
  regression.
- **List 2 (restore):** real `Thing` instances available on the current map, sorted by descending
  score per the selected rule's stat weights; each tooltip shows the actual stat values of that
  specific instance. The render primitive `ThingIconBox.DoThingBox(...)` already exists. The
  consumer (the EquipmentManager mod) supplies the data; the widget only renders it.
- Enabling the second list is publicly accessible through `WorkTypeThingRuleWidget.DoWidgetTab(...)`.
- The bottom-section height and layout (`GetBottomPartHeight`, `DoBottomPart`) must correctly
  account for the second list, with no overlapping areas.
- Breaking the public signature is acceptable (`backward_compat: none`); if the signature is broken,
  it must be documented explicitly in this sprint.md and in the decisions-log.
- The `LordKuper.Common` assembly is rebuilt and published to `1.6/Assemblies`, green (0 warnings),
  with existing tests passing; tests for the new behavior are added or updated where feasible.

## Acceptance

- **AC1:** The public API lets a consumer enable rendering of the second list (on-map `Thing`
  instances) in the bottom section of the tab.
- **AC2:** List 2 renders real `Thing` instances with a tooltip showing the instance's actual stat
  values.
- **AC3:** List 2 is displayed in descending score order per the selected rule's stat weights.
- **AC4:** List 1 (`ThingDef`) is preserved with no functional regression.
- **AC5:** The bottom-section height and layout account for both lists with no overlap.
- **AC6:** Any break to the public signature is explicitly documented.
- **AC7:** Build is green with 0 warnings; existing tests pass; the new behavior is covered by tests
  where feasible.

## Open design decisions (deferred to design phase)

- **Public API form** — overload vs optional parameter vs separate method vs render-options object.
  Deferred to the design phase; the architect decides during design.
- **Score-sorting owner** — whether the consumer pre-sorts the `Thing` instances or the widget
  performs the descending-score sort per the rule's stat weights. Deferred to the design phase; the
  architect decides during design.

## Out of scope

- Changes inside the EquipmentManager mod (the consumer adapts separately).
- The map-item collection logic (supplied by the consumer).
