---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-documentation]: CONCERNS

# Review — documentation

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | `design/architecture/adr/adr-0009-worktype-widget-map-things-list.html` (Decision-binds list + Context); `adr-0010-...html` (Context) | Source line-number citations are stale relative to the shipped file. The ADRs cite `WorkTypeThingRuleWidget.cs:179–199` (DoWidgetTab), `:206–210` (GetBottomPartHeight), `:28–44` (DoBottomPart), `:227–246` (GetWorkTypeDefTooltip). After this sprint's edits the actual locations are DoWidgetTab `:241`, GetBottomPartHeight `:269`, DoBottomPart `:38`, GetWorkTypeDefTooltip `:315`. The substantive prose (signature shape, behavior) is correct; only the line anchors drifted. These are pre-change/audit-baseline anchors carried into the promoted ADR — defensible as historical context but no longer resolve to the cited members in the shipped file. | Either drop the precise line numbers (cite by member name only) or update them to the post-sprint locations, so a reader following the anchor lands on the right code. Persistent ADR — owner is the Architect in a future design-promote, not editable in this review. |

<!-- All load-bearing documentation-actuality claims verified against shipped code — see Verdict notes. -->

## Verdict
CONCERNS: 1

Documentation actuality is sound. Every load-bearing claim in the promoted ADR-0009 / ADR-0010 matches the shipped code:

- **12-param signature (AC-6).** ADR-0009 "New (after)" signature table is reproduced verbatim by the shipped `DoWidgetTab` (`WorkTypeThingRuleWidget.cs:241–247`): same param order, names, types, and the `IReadOnlyList<Thing>? mapThings = null` default plus the no-default `ref Vector2 mapThingIconBoxScrollPosition`. The documented 10→12 parameter break is accurate. Old signature is also correctly recorded.
- **Nullable second list + second scroll (AC-1).** Matches code exactly.
- **Side-by-side, half-width split (AC-5).** `DoBottomPart` (`:44–75`) computes `halfWidth = rect.width / 2f`, draws two disjoint header rects and two boxes; the ADR text "side by side… not stacked vertically… disjoint horizontal halves" matches. No stale claim implying vertical stacking exists.
- **Constant band height (AC-5).** `GetBottomPartHeight` (`:269–273`) returns one box height + one section header + one gap and never branches on `mapThings`; matches the ADR's "GetBottomPartHeight is unchanged / band does not grow" claim.
- **Consumer pre-sort, no render-time scoring (AC-3, ADR-0010).** `GetThingScore` is defined only at `WorkTypeThingRule.cs:239` and has no call site in the widget; the widget renders `mapThings!` in given order. Matches ADR-0010's "widget renders in the given order and must not re-sort or call GetThingScore during render."
- **Per-instance tooltip reads live instance (AC-2).** `GetWorkTypeThingTooltip` (`:293–307`) reads `StatHelper.GetStatValue(thing, stat)` directly with no `ThingMaker.MakeThing`; matches the ADR's "reads the live instance directly… not synthesize a temporary Thing."
- **XML-doc pre-sort contract.** The new public `mapThings` param carries the documented pre-sort contract (`:233–239`: "Pre-sort contract (caller responsibility)… renders items in the supplied order and does not re-sort"), satisfying ADR-0010's mitigation ("documenting the pre-sort contract on the public parameter").
- **New strings.** `AvailableItemsOnMapLabel` / `AvailableItemsOnMapTooltip` exist (`Resources.cs:673–681`) as the ADR specified.

**Traceability** PRD AC-1…AC-7 ↔ ADR-0009/0010 ↔ code is intact: PRD defers API-form (AC-1/6) and sort-ownership (AC-3) to the ADR; both promoted ADRs resolve them and the code implements them.

**SSoT** No divergent-home violation. The sprint draft `design/adr.html` and the two promoted `design/architecture/adr/adr-0009/0010` carry identical signature tables and decision text (promotion, not divergence). The sprint draft is the consumed-and-superseded source; the persistent ADRs are the living home.

**Provenance** Both persistent ADRs declare `provenance: original`, `source: ""`, with no provenance badge rendered — correct for original docs.

**Declined `design/api/` doc** Its absence is not a defect (user declined; PRD follow-up explicitly leaves it to design-promote). Not flagged.

## Next action
CONCERNS → finding #1 is a low-severity stale-line-anchor note on a persistent ADR. It does not block DoD and is not editable in impl-review (persistent `design/` is owned by the Architect). PM may carry it as a non-blocking cleanup for a future design-promote, or accept as historical-baseline anchors. No code change required.

REVIEW_DONE: documentation
