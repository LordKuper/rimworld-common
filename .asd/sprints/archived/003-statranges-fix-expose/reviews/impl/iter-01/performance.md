---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: APPROVE

# Review — performance

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | `Source/LordKuper.Common/StatRanges.cs:64-68` | `NormalizeStatValue` performs a redundant second dictionary hash lookup: `UpdateStatRange` already holds the up-to-date `FloatRange`, then line 67 re-reads `Ranges[stat]`. On the per-scored-def/thing hot path (`GetThingDefScore` / `GetThingScore` loops in `WorkTypeThingRule.cs:200-242`) this is one extra hash + equality compare per stat per item. Not introduced by this change; pre-existing. | Have `UpdateStatRange` return the updated `FloatRange` (or use `ref`-return / `CollectionsMarshal`-style single-access where available on net472). Then `NormalizeValue(value, range)` consumes it directly, eliminating the second lookup. |

## Budget compliance

No performance budgets are defined in `.asd/project/custom-coding-rules.md` (no latency/throughput/memory targets, no regression tolerances). There are no quantitative budgets to enforce for this change. Rubric applied heuristically below.

## Regression assessment

- **`UpdateStatRange` (StatRanges.cs:79-85)**: one `TryGetValue` (O(1)), two struct-field compares, one dict-indexer write. The fix collapses the prior first-observation double-write into a single `Ranges[stat] = range`, so the change is neutral-to-slightly-better. No regression.
- **`NormalizeStatValue` (StatRanges.cs:64-68)**: `UpdateStatRange` + one indexer read + `MathHelper.NormalizeValue`. Same complexity class as before; no regression introduced.
- **`MathHelper.NormalizeValue` (MathHelper.cs:20-32)**: pure arithmetic on a `FloatRange` struct parameter and a pattern-match switch. No loops, no allocation.

## Allocation / boxing

`FloatRange` is a value type; `range` is a stack local; the dictionary value is a struct stored by value. No boxing, no heap allocation on the hot path. Confirmed.

## Anti-patterns

None detected: no n+1, no synchronous IO on the hot path, no unbounded or large-collection allocation, no deep clone, no serialize/parse roundtrip. `Clear()` (StatRanges.cs:38) is an O(n) dictionary clear invoked only for test-isolation teardown — not a runtime hot path.

## Algorithmic complexity

The scoring loops (`WorkTypeThingRule.cs:204-208`, `238-241`) iterate `StatWeights` (bounded by configured stat count, not user-input-sized) and invoke O(1) normalization per entry. No nested loops over user-sized collections, no naive search where a map already exists.

## Verdict
APPROVE

The fix is performance-neutral-to-positive: it removes a redundant first-observation dict write and introduces no new allocations, boxing, or complexity. The single low finding is a pre-existing micro-optimization opportunity, not a regression caused by this change, and does not block approval.

## Next action
None required for approval. Finding #1 is an optional pre-existing improvement the creator may address opportunistically; it is not a gate.
