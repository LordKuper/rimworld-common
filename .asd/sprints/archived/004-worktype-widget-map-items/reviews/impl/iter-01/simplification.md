[REVIEW-impl-simplification]: APPROVE

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict
APPROVE

## Next action
Reviewer done. No simplification concerns; no fix routing required from this reviewer.

## Notes (non-findings, recorded for traceability)

The implementation followed the simpler ADR-0009 / ADR-0010 decisions without introducing surprise complexity. Each over-engineering checklist item was checked and cleared:

- **No new abstraction / options-object / overload (keep-as-is).** `DoWidgetTab` was extended in place with two appended opt-in parameters (`ref Vector2 mapThingIconBoxScrollPosition`, `IReadOnlyList<Thing>? mapThings = null`) exactly as ADR-0009 chose over the rejected render-options object and separate-method alternatives. No new type, interface, generic, or factory was introduced. `WorkTypeThingRuleWidget.cs:241-247`.
- **Reuses existing primitives (keep-as-is).** Dormant `ThingIconBox.DoThingBox` is wired up as-is for List 2 (`:74`); `DoThingDefBox` retained for List 1; per-instance stats read directly via `StatHelper.GetStatValue(Thing, StatDef)` (`:305`). No parallel render/scoring machinery built.
- **Tooltip helpers — duplication NOT flagged (keep-as-is).** `GetWorkTypeThingTooltip` (`:293-307`) mirrors `GetWorkTypeDefTooltip` (`:315-332`) per ADR-0009's explicit "private helper analogous to the def helper" instruction. The shared body is ~6 lines and the two diverge on load-bearing behavior (live-instance read incl. `equippedStatOffsets` vs `ThingMaker.MakeThing` synthesis; `LabelCapNoCount` vs `LabelCap`). Extracting a shared helper here would add a parameterized abstraction more complex than the duplication it removes — per the Simplicity Default, NOT recommended.
- **Width-split layout (keep-as-is).** `DoBottomPart` (`:38-92`) splits the band width with plain `Rect` arithmetic (`halfWidth`); no layout-machinery abstraction. Single shared `thingIconBoxRowCount` governs both boxes; ADR-0009's rejected divergent-row-count parameter was correctly NOT added. `GetBottomPartHeight` is unchanged and never branches on second-list presence.
- **No premature config flag.** `mapThings`/`mapThingIconBoxScrollPosition` are caller-chosen capability parameters (consumer opts in by passing a non-empty list), not default-only flags.
- **No defensive-for-impossible code.** `mapThings!` (`:74`) is a justified null-forgiving assertion guarded by `showMapList` and commented; `Current.Game == null` (`:297`,`:319`) and `Where(sw => sw.StatDef != null)` guard real runtime/nullable cases.
- **No render-time re-sort.** Widget renders `mapThings` in given order; never calls `GetThingScore` during render, honoring ADR-0010's side-effect-free render contract.
- **No dead code; no restate-the-code comments.** The change activates a previously-dormant primitive; comments explain intent/why (null-forgiving justification, pre-sort contract), not restatement.
- **Tests — no test-for-test-sake, no over-mocking.** The four `GetThingScore` tests use `FakeDefProvider` (a legitimate injection-seam double, not a mock-of-a-mock) and cover the consumer pre-sort ordering contract (AC-3/AC-7) the widget itself cannot unit-test. The headless `Assert.Ignore` fallback honestly documents the live-context limit rather than asserting against a fake.

## Escalations (optional)
None. No new abstraction, layer, interface, dependency, or complexity increase was introduced; no Complication Approval is required.

REVIEW_DONE: simplification
