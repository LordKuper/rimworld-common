[REVIEW-impl-external]: CONCERNS

# External Review Report

- **Phase**: impl-review
- **Iteration**: 1
- **Severity floor (this iter)**: low (report all findings)
- **External engine**: Codex CLI (codex-cli 0.130.0), `codex exec --sandbox read-only --output-schema … -o … -` (stdin prompt, JSON-schema output; `codex review --json` unsupported by installed CLI)
- **Scope reviewed**: focused sprint-logic diff (`WorkTypeThingRuleWidget.cs`, `Resources.cs`, `WorkTypeThingRule.cs`, `WorkTypeThingRuleTests.cs`); formatting-only cleanup commit excluded.

## Codex positives (confirmed, no finding)

- AC-2 / ADR-0009 tooltip path correct: `GetWorkTypeThingTooltip` reads the live instance via `StatHelper.GetStatValue(thing, stat)`, does not synthesize a temp `Thing` via `ThingMaker`, and mirrors the `Current.Game == null` early-return guard.
- AC-3 / ADR-0010 honored: the widget does not call `GetThingScore` or re-sort during render; `mapThings` is rendered in the supplied (consumer pre-sorted) order. No render-time mutation of shared `StatRanges` history.
- AC-5 height invariant: `GetBottomPartHeight` is unchanged and does not branch on second-list presence; both branches reuse the same band height.

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | `Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs` (`DoBottomPart`, side-by-side branch, halfWidth split) | Codex F1 (category: layout; Codex severity `minor` → ASD `medium`). The split uses two exact half-width rects: List 1 ends at `rect.x + halfWidth`, List 2 starts at the same `rect.x + halfWidth`, with no separating gap. The two halves are mathematically disjoint (so AC-5 "no overlap / disjoint halves" is technically met), but the boxes abut at the seam with no `Layout.ElementGap` between them, so their borders/hover regions touch on the same pixel column — visually inconsistent with the spacing used elsewhere in the layout. | Reserve `Layout.ElementGap` between columns: `var halfWidth = (rect.width - Layout.ElementGap) / 2f;` and start the right column at `rect.x + halfWidth + Layout.ElementGap`, applying the same split to both the header row and the content remainder rects. |

## Dropped findings (below severity floor)

None. Iteration 1 floor is `low`; all findings reported.

## Dropped findings (nitpick)

None raised.

## Verdict
CONCERNS: 1

External Review verdict is CONCERNS. One medium-severity layout finding (seam gap between the two side-by-side boxes). No blocker/critical/high findings: no AC coverage gap, no ADR-0009/ADR-0010 violation, no null-handling bug, no signature/XML-doc drift detected in the logic diff.

## Next action
Per `review-policy.md`, impl-review routes the sprint back to `impl` (fix mode): the responsible dev addresses F1 by inserting `Layout.ElementGap` between the two columns in `DoBottomPart`, then the sprint re-enters impl-review. The finding is below the escalation bar (no scope/contract/abstraction change), so the dev autofixes without user approval.

REVIEW_DONE: external
