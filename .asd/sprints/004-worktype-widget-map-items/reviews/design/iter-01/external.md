---
responsibility:
  owns: external review aggregation report (kept/dropped accounting for design-review iter 1)
  excludes: codex raw prompt, internal reviewer output
  delegates_to: t_prompt-external-design.md (prompt), t_review-report.md (output)
---

[REVIEW-design-external]: CONCERNS

# External Review Report

- **Phase**: design-review
- **Iteration**: 1
- **Severity floor (this iter)**: low (report all findings)
- **External tool**: Codex CLI (`codex-cli 0.130.0`) — available; review executed via `codex exec --sandbox read-only --output-schema` (the installed CLI exposes a prompt+JSON-schema interface rather than the `review --json --input --output` form named in `external-review.md`; the JSON-schema output is the functional equivalent and was parsed and mapped below).

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | prd.html:AC-5 (`#AC-5`) | Codex F1. PRD AC-5 states `GetBottomPartHeight(...)` and `DoBottomPart(...)` must "account for both lists (each list's section header + `GetThingIconBoxHeight(...)` + gaps)". That phrasing reads as the bottom band's **height** summing two header+box+gap groups (a vertical stack / taller band), which contradicts the accepted **ADR-0009** decision that the lists are drawn **side by side**, `GetBottomPartHeight` is **unchanged**, and `DoBottomPart` splits the band **width** (band height identical for one-list and two-list cases). An AC that is ambiguous/contradictory with its own ADR resolution risks driving a wrong implementation. | Reword PRD AC-5 to match ADR-0009 explicitly: bottom band height stays constant (one box height + one header + one gap, unchanged from today); when `mapThings` is present the band **width** is split between two side-by-side boxes (each with its own header on a single shared header row); when `mapThings` is null/empty List 1 renders full width at the same band height. State "no overlap" as disjoint **horizontal** regions, not vertical stacking. |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | — | none — iteration 1 floor is `low`, so no finding was dropped for being below floor |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | none — Codex returned no nitpick-class findings | — |

## Verdict
CONCERNS: 1

One `high`-severity internal-consistency defect between prd.html (AC-5) and adr.html (ADR-0009). Autofixable by the creator (asd-ba) within the design-review loop — it is a wording/traceability correction to align an acceptance criterion with the already-accepted ADR, not a change to the approved concept, contract, or scope, so no escalation is required.

The intentionally-allowed `DoWidgetTab` signature break, the absent ux-spec/mockups, and missing UI/accessibility docs were excluded from review scope (N/A for an IMGUI library widget) and correctly produced no findings.

## Next action
asd-ba reworks prd.html AC-5 to match ADR-0009's constant-height / width-split / side-by-side wording (and align AC-5's "no overlap" language to disjoint horizontal regions). Re-run design-review iteration 2. CONCERNS does not block — the creator autofixes and the loop advances.
