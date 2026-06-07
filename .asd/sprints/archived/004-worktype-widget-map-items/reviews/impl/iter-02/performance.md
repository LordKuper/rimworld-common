---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: APPROVE

# Review — performance

- **Phase**: impl-review
- **Iteration**: 2

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict
APPROVE

Reviewed at HIGH severity floor (iteration 2): only high/critical findings reportable. None found.

Confirmations against the focus items:

- **Tooltip is genuinely lazy.** `ThingIconBox.cs:88-90` (`DoThingBox`) and `:156-158` (`DoThingDefBox`) construct the tooltip via `new TipSignal(() => tooltipGetter(capturedItem), capturedItem.GetHashCode() ^ salt)`. The `Func<string>` overload defers the expensive string construction (StringBuilder + LINQ + `StatHelper.GetStatValue`) until RimWorld's `TooltipHandler` invokes the delegate on hover. Grep over the file confirms `tooltipGetter` is invoked only inside the two lambdas — there is no eager `tooltipGetter(thing)` call anywhere in the per-frame loop. The iter-01 medium finding (F1: eager per-frame tooltip build) is resolved.

- **No render-time scoring/sort (ADR-0010).** No `GetThingScore`/`GetThingDefScore`/`SortBy*` reference exists in `ThingIconBox.cs`; both box methods iterate the supplied `IReadOnlyList` and render in given order. The render path remains side-effect-free and does not perturb the shared `StatRanges` observation history, consistent with the consumer-pre-sorts contract.

- **Per-item closure / `TipSignal` allocation is acceptable.** Each hovered-or-not item allocates one small closure (capturing `capturedItem` + `tooltipGetter`) and one `TipSignal` struct per frame. This is the standard RimWorld tooltip-region pattern and is orders of magnitude cheaper than the prior eager string build (which ran StringBuilder/LINQ/stat lookups for every item every frame). It does not constitute a HIGH/CRITICAL regression; below the iteration-2 floor it is not reportable.

The hash `^ salt` ID derivation is constant-cost and the two distinct salts (`ThingBoxTooltipSalt` / `ThingDefBoxTooltipSalt`) prevent cross-box ID collisions without adding per-frame cost.

Note: `.asd/project/custom-coding-rules.md` defines no quantitative perf budgets (no latency/memory/throughput thresholds); the applicable performance constraint is ADR-0010's render-time no-scoring rule, which is satisfied.

## Next action
None. No performance blockers. Creator may proceed; sibling reviewers' verdicts govern overall gate.

REVIEW_DONE: performance
