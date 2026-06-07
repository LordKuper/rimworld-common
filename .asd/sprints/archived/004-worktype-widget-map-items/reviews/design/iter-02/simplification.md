---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-design-simplification]: APPROVE

# Review — simplification

- **Phase**: design-review
- **Iteration**: 2

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above HIGH floor | — |

### Checklist disposition (no trips; recorded for traceability)

| Smell | Verdict | Basis |
|---|---|---|
| New abstraction / options type | keep-as-is | ADR-0009 explicitly rejects render-options object and separate method/overload; uses two nullable params appended to existing `DoWidgetTab`. No new type/layer/interface. Honors Simplicity Default. |
| Generic with one type param | keep-as-is | No new generic introduced; reuses existing `IReadOnlyList<Thing>`. |
| Factory / plugin / framework-wrap | keep-as-is | None proposed. |
| Premature config flag | keep-as-is | No flag added; second list is opt-in by `mapThings` null/empty, a real runtime input the consumer controls — not a speculative toggle. |
| Defensive code for impossible case | keep-as-is | `mapThings` null/empty no-op mirrors the existing `Current.Game == null` guard; the consumer can genuinely pass null (AC-4), so this is a real case, not impossible-by-contract. |
| Helper wrapping one stdlib call | keep-as-is | New private `GetWorkTypeThingTooltip(Thing, rule)` builds a multi-stat tooltip from live instance stats (AC-2), mirroring the existing def helper; adds value beyond a single call. |
| Abstraction with no second use case | keep-as-is | No new abstraction; the dormant `DoThingBox` and public `GetThingScore` are reused as-is. |
| Independent per-list row count | keep-as-is | ADR-0009 explicitly rejects a second `thingIconBoxRowCount` ("adds a parameter with no requirement behind it"); single shared count retained — the simpler choice. |
| Widget-side sort / defensive-copy score path | keep-as-is | ADR-0010 keeps widget presentational; consumer pre-sorts via existing public `GetThingScore`. Rejected "non-mutating defensive copy" correctly deferred as out-of-scope normalizer redesign. |
| `GetBottomPartHeight` complexity | keep-as-is | Side-by-side layout leaves `GetBottomPartHeight` unchanged and non-branching on second-list presence — fewer moving parts than a stacked/growing band. |

### Iter-1 autofix verification (PRD AC-5 / Problem wording, side-by-side alignment)

The iter-1 autofix to PRD AC-5 and the Problem section describes width-split side-by-side layout with constant band height and `GetBottomPartHeight` unchanged. This wording *removes* complexity relative to a stacked layout (which would grow the band and force `GetBottomPartHeight` to branch). It introduces no new abstraction, generic, flag, type, or defensive code, and is consistent with ADR-0009 AC-5. No complexity added.

## Verdict
APPROVE

## Next action
Reviewer done. No simplification findings at or above the HIGH floor; the design actively upholds the ASD Simplicity Default. No creator action required from this reviewer.

## Escalations (optional)
- None. No Complication Approval required; every complication in the design carries explicit justification with simpler alternatives recorded as rejected (ADR-0009 Alternatives, ADR-0010 Alternatives).

REVIEW_DONE: simplification
