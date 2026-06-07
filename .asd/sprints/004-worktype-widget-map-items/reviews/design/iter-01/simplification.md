[REVIEW-design-simplification]: APPROVE

# Review — simplification

- **Phase**: design-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict
APPROVE

## Next action
Reviewer done. No simplification concerns block design DoD. Architect need not autofix for this reviewer.

## Assessment detail (informational — no findings)

The design was checked against the full over-engineering checklist (`review-policy.md`) and the ASD Simplicity Default. Each axis the scope flagged was verified as the *simpler* choice, with no introduced abstraction:

- **ADR-0009 nullable parameters over options object / separate method.** Confirmed simplest viable form. The decision extends the existing `DoWidgetTab` with one nullable `IReadOnlyList<Thing>? mapThings` (default `null`) + one `ref Vector2` scroll position — no new type, layer, interface, or dependency. The rejected alternatives are exactly the over-engineering traps: an options object would be an *abstraction with one use case* (checklist hit) requiring Complication Approval (not granted); a separate method/overload would *duplicate the bottom-section layout/sizing logic*, creating a second maintenance path and List-1 regression risk. Both are correctly rejected with rationale recorded. The 10→12 parameter growth is honestly logged as a Negative consequence, accepted as the cost of *avoiding* a new abstraction — this is the correct trade under the Simplicity Default, not a smell.
- **Reuse over parallel machinery (audit-confirmed).** Verified against source: `ThingIconBox.DoThingBox(...)` already exists (`ThingIconBox.cs:39-79`) with the `tooltipGetter` parameter the design relies on, and `WorkTypeThingRule.GetThingScore(Thing)` is already `public` (`WorkTypeThingRule.cs:234`). The design wires up the dormant primitive as-is and reuses the existing public score path — no new render primitive, no new score path. The one new private helper (`GetWorkTypeThingTooltip`) is a genuinely-needed per-instance tooltip (AC-2), structurally mirroring the existing `GetWorkTypeDefTooltip`; it adds value (reads the live instance directly, skipping `ThingMaker` synthesis), so it is not a thin stdlib wrapper.
- **ADR-0010 consumer pre-sorts (widget purely presentational).** Confirmed this *removes* complexity rather than pushing it. The widget adds no sort, no normalizer, and no redundant scoring; it renders in given order, symmetric with List 1's existing pre-sorted contract. The rejected "widget sorts a defensive copy" alternative is correctly identified as a non-mutating-normalizer redesign that is out of scope. No defensive code for impossible cases is introduced; the `null`/empty `mapThings` no-op is a real opt-in path (a caller chooses it), not a premature flag.
- **Side-by-side layout, constant height.** Confirmed minimal. `GetBottomPartHeight` is deliberately *unchanged* (band height constant whether one or two lists render), so the two functions cannot mis-size relative to each other — the simplest possible coupling. A single shared `thingIconBoxRowCount` is used; the design explicitly rejects an independent second row count as "a parameter with no requirement behind it," deferring divergent sizing to a future Complication-Approved change. This is the correct application of YAGNI / Evidence-over-Speculation.
- **No dead code, no premature config flag, no speculative generalization.** Two new string entries are concretely consumed by the second header. No "in case we need it" surface is added.

Cross-reviewer guard: no design decision here proposes adding abstraction in response to another concern. If sibling reviewers propose an options object, an in-widget sort, or an independent row count to address their findings, those would each trip the over-engineering checklist and must route through Complication Approval rather than autofix.

## Escalations (optional)
- None.
