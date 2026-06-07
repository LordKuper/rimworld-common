---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-simplification]: APPROVE

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 2

## Scope

Severity floor = HIGH (iter 2 of impl-review; only high/critical reported). Reviewed the fix commits since `46288c4` (ignoring style `1604b2a` + republish `5d767cd`):

1. Lazy `TipSignal` tooltips in `ThingIconBox.cs` (`DoThingBox` / `DoThingDefBox`).
2. Tooltip-ID salt constants in `ThingIconBox.cs`.
3. `ElementGap` band-split math in `WorkTypeThingRuleWidget.DoBottomPart`.
4. Refactored equal-score / descending-order tests in `WorkTypeThingRuleTests.cs`.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above HIGH floor | — |

## Assessment against over-engineering checklist

- **Lazy tooltips** (`ThingIconBox.cs:81-91`, `:149-159`) — uses RimWorld's existing `TipSignal(Func<string>, int)` constructor directly. No new abstraction, type, or wrapper. The `Func<Thing,string>?` / `Func<ThingDef,string>?` parameters are plain delegates, each supplied by a single concrete caller in `DoBottomPart`. Comments state *why* (lazy evaluation, ID-collision avoidance), not what — not a restating comment. No smell.
- **Salt constants** (`ThingIconBox.cs:25`, `:31`) — two plain `const int`; each has a distinct, real purpose (collision-free unique IDs across the two box renderers that may share an item hash). Not a config flag, not an abstraction. Minimal and justified. No smell.
- **`ElementGap` band split** (`WorkTypeThingRuleWidget.cs:50-52`) — plain rect arithmetic reusing the existing `Layout.ElementGap` constant (spacing SSoT). No helper, no abstraction. No smell.
- **Test refactor** (`WorkTypeThingRuleTests.cs:80-153`) — both tests call the existing public `StatRanges.NormalizeStatValue` directly and reuse the existing `FakeDefProvider` double once each; the `weight * norm` arithmetic mirrors the production score formula without abstracting it. No mock-of-a-mock, no new harness. No smell.

No fix introduces a new abstraction, layer, interface, dependency, config flag, or generalization. Nothing trips Complication Approval. Consistent with ADR-0009 / ADR-0010 (Simplicity Default, smallest viable change) and `design-principles.md` KISS.

## Verdict
APPROVE

## Next action
Reviewer done. No fixes required from this reviewer.

## Escalations (optional)
- none

REVIEW_DONE: simplification
