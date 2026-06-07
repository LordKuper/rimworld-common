[REVIEW-impl-quality]: APPROVE

# Review — quality

- **Phase**: impl-review
- **Iteration**: 2

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no high/critical findings | — |

### Below-floor notes (informational only — do not affect verdict)

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| n1 | low | `ThingIconBox.cs:90,158` | Tooltip unique id is `GetHashCode() ^ salt`. `GetHashCode()` is not guaranteed collision-free across distinct items in one box; two colliding items could share a TooltipHandler cache key and show the wrong cached text. In practice RimWorld `Thing.GetHashCode()` (thingIDNumber) and `ThingDef.GetHashCode()` (shortHash/index) are effectively unique, and the per-item salt only protects against cross-box collisions — so impact is negligible. Pre-existing id-derivation pattern, not introduced by this fix. | If ever observed, key on a per-box monotonic index instead of hash. |
| n2 | low | `WorkTypeThingRuleWidget.cs:50` | `halfWidth = (rect.width - Layout.ElementGap) / 2f` goes negative if `rect.width < ElementGap` (pathologically narrow band). Only reachable in the side-by-side branch; the pre-existing single-list path has no such guard either, and the bottom band is a fixed, comfortably-wide region at runtime. | No action needed; clamp only if a future caller can pass sub-gap widths. |

## Verdict
APPROVE

The three iter-01 fix commits are correct and introduce no HIGH/CRITICAL regression:

- **Lazy `TipSignal` (767ea56).** Closures capture a fresh per-iteration local (`capturedThing` / `capturedDef`) declared inside the loop body, not the shared iteration variable — no loop-variable capture bug. The text getter `() => tooltipGetter(x)` invokes the same `tooltipGetter` as before, so tooltip content is unchanged; it now runs only on hover, eliminating the per-frame StringBuilder/LINQ/`GetStatValue` cost. The `tooltipGetter != null` guard is preserved. Unique ids use distinct box salts (`0x54426F78` / `0x44426F78`) so the two boxes cannot collide, and the id is stable per item within the process.
- **ElementGap width math (23085b5).** `list1` right edge = `x + halfWidth`; `list2` left edge = `x + halfWidth + ElementGap` → exactly one `ElementGap` of separation, no overlap. `list2` right edge = `x + 2*halfWidth + ElementGap = x + rect.width` → fills the band with no spill. The split is applied consistently: both header rects and both content boxes derive from `list1HeaderRect` / `list2HeaderRect`.
- **Refactored test (e2be4c8).** `GetThingScore_SameWeights_ZeroDeviations_ProducesEqualScores` now asserts headlessly via two identical `StatRanges.NormalizeStatValue(statDef, 0f)` calls on the same range — deterministic, always runs (no skip), isolated via `FakeDefProvider` + `StatHelper.Rebuild()` on the static-state base. It asserts the real ordering-stability invariant; the companion `DescendingOrder` test covers higher-deviation-scores-first. The documented headless deferral of the full `GetThingScore`→`thing.GetStatValue` round-trip is acceptable and unchanged.
- **ADR-0010 honored.** `DoBottomPart` renders `mapThings` in supplied order with `null` re-sort; no `GetThingScore` call at render time. The List 2 tooltip getter calls `StatHelper.GetStatValue` (not `NormalizeStatValue`/`GetThingScore`), so it never perturbs the shared `StatRanges` history — the lazy change further ensures it runs only on hover. Render pass stays side-effect-free.

## Next action
None required. No qualifying (high/critical) findings at the iteration-2 severity floor. PM may proceed to close the impl-review loop pending sibling reviewer verdicts.

REVIEW_DONE: quality
