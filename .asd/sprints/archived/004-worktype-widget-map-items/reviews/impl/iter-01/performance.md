---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: CONCERNS

# Review — performance

- **Phase**: impl-review
- **Iteration**: 1

## Scope note

This is a RimWorld mod library with no formal performance budget defined in
`.asd/project/custom-coding-rules.md`. Per the operating contract, findings are assessed against
the stated baseline: "no per-frame regression vs the single-list baseline" and ADR-0010's
render-time invariant (no `GetThingScore`/sort during render).

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | `ThingIconBox.cs:66`, `:123` (consumed by `WorkTypeThingRuleWidget.cs:74-75`, `:71-72`, `:89-90`) | **Eager tooltip construction every frame for every item.** `DoThingBox`/`DoThingDefBox` invoke `tooltipGetter(thing)` *unconditionally* inside the per-item render loop — the delegate result is computed before being passed to `TooltipHandler.TipRegion`, so it is NOT lazy/hover-gated. For List 2 this means `GetWorkTypeThingTooltip` runs a `new StringBuilder()`, `rule.StatWeights.Where(...).Select(...).ToHashSet()` (allocates a closure + HashSet), and one `StatHelper.GetStatValue(thing, stat)` (live RimWorld stat-worker pipeline call at `StatHelper.cs:147` `thing.GetStatValue`) per stat — for every on-map `Thing`, every IMGUI frame, even with no pointer over the box. With List 2 active this roughly doubles the number of items on this path versus the single-list baseline. | Make the tooltip lazy so the string is built only on hover. `TooltipHandler.TipRegion` has an overload taking `TipSignal` with a `Func<string>`; pass `() => tooltipGetter(thing)` instead of the eagerly-evaluated `tooltipGetter(thing)` in `ThingIconBox.cs:66` and `:123`. This eliminates the per-frame string/HashSet/GetStatValue cost for non-hovered items in both lists. |
| 2 | low | `WorkTypeThingRuleWidget.cs:299-300`, `:321-322` | **Per-frame LINQ allocation in tooltip builders.** `rule.StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef!).ToHashSet()` allocates a closure, two enumerator state machines, and a `HashSet<StatDef>` on every invocation. Because of finding #1 this currently runs every frame per item; even after #1 is fixed it runs on every hover frame. The `StatWeights` set is small, so absolute cost is minor, but the `HashSet` is unnecessary — the source weights are already distinct by stat. | Iterate `rule.StatWeights` directly with a null-guard inside the `foreach`, dropping the intermediate `ToHashSet()`. Lower priority than #1; fixing #1 already removes the per-frame multiplier. |
| 3 | low | `WorkTypeThingRuleWidget.cs:325-327` (`GetWorkTypeDefTooltip`, List 1 only) | **Pre-existing: `ThingMaker.MakeThing` per item.** List 1's tooltip synthesizes a fresh `Thing` via `ThingMaker.MakeThing` on every invocation — a heavyweight allocation. Combined with finding #1 (eager per-frame) this is the single most expensive per-frame item operation in the bottom band. Note this is the *existing* List 1 path, unchanged by this sprint; the new List 2 path (`GetWorkTypeThingTooltip`) correctly avoids it by reading the live instance (per ADR-0009/ADR-0010). Flagged for completeness because finding #1's fix (lazy tooltips) also resolves the per-frame cost here, making the `MakeThing` call hover-only. | Covered by finding #1: making tooltips lazy confines `MakeThing` to hover frames. No separate change needed if #1 is applied. |

## Verdict
CONCERNS: 3

## Confirmed (no findings)

- **ADR-0010 render-time invariant holds.** `DoBottomPart` (`WorkTypeThingRuleWidget.cs:38-92`) does
  NOT call `GetThingScore` or sort `mapThings` during render. The XML doc (`:229-239`, `:35`) and the
  comment at `:73` document the consumer-pre-sort contract; the widget renders `mapThings` in the given
  order via `DoThingBox`. No render-driven mutation of the shared `StatRanges` history. This is the main
  regression risk for the sprint and it is correctly avoided.
- **No render-time sort/`GetThingScore` anywhere on the hot path** — verified across `DoBottomPart`,
  `DoThingBox`, and the new tooltip helper.
- **`DoBottomPart` rect math** (`:48-91`) is a handful of struct `Rect` computations per frame — negligible,
  no heap allocation, no per-item work beyond what each box already does.
- **No n+1 / nested loops over map-sized collections** introduced. Each box loops its own list once
  (`ThingIconBox.cs:54`, `:113`); the inner `equippedStatOffsets`/`statBases` scans in `StatHelper`
  iterate small per-def modifier lists, not map-sized collections.
- **Closure capture in `DoBottomPart`** (`def => GetWorkTypeDefTooltip(...)`, `thing => GetWorkTypeThingTooltip(...)`
  at `:72`, `:75`, `:90`) allocates two/one delegate objects per frame — standard IMGUI idiom, matches the
  pre-existing single-list path, negligible and not a regression.

## Next action

Backend Dev: address finding #1 (make tooltip construction lazy/hover-gated in `ThingIconBox.DoThingBox`
and `DoThingDefBox`) to remove the per-frame tooltip cost that List 2 amplifies versus the single-list
baseline. Findings #2 and #3 are subsumed by the #1 fix and need no separate work. Note that #1 touches a
shared, pre-existing primitive (`ThingIconBox`) whose eager-tooltip behavior predates this sprint; if the
team prefers to scope this sprint strictly to the additive List 2 change and defer the `ThingIconBox`
lazy-tooltip fix, that is a defensible call — but it should be a recorded decision, since List 2 measurably
increases the per-frame cost the primitive incurs.

REVIEW_DONE: performance
