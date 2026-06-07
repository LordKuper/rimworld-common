[REVIEW-design-documentation]: CONCERNS

# Review — documentation

- **Phase**: design-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | prd.html `#acceptance-criteria` AC-5 (line 226) vs adr.html `#a-decision` / `#acceptance` AC-5 (lines 250-259, 467) | SSoT contradiction on a decided fact. PRD AC-5 states `GetBottomPartHeight(...)` must "account for both lists (each list's section header + `GetThingIconBoxHeight(...)` + gaps)" — a per-list (plural headers + box heights) reservation that describes the **vertical-stacking, growing-band** model. The ADR decided the **opposite**: side-by-side layout, `GetBottomPartHeight` **unchanged**, returning exactly one box height + one section header + one gap, band height identical for one-list and two-list cases. AC-5 is the testable contract; two homes state mutually exclusive sizing behavior, so the implementer/test author cannot tell which height model is authoritative. | Revise PRD AC-5 to match the accepted ADR-0009 side-by-side decision: the band height is constant (one `GetThingIconBoxHeight` + one header + one gap), `GetBottomPartHeight` does not grow for the second list, and `DoBottomPart` splits the band **width** between two boxes (each on a single shared header row) with no overlap. Keep PRD pointing to the ADR as the home for the layout decision rather than restating a divergent height formula. |
| 2 | low | prd.html `#problem` (line 165) | The Problem narrative frames the second list as requiring "matching height accounting" so the new list does not "overlap the first" — phrasing consistent with the rejected vertical-growth model and inconsistent with the side-by-side / constant-height decision now in ADR-0009. As a problem statement (pre-decision) this is defensible, but combined with finding #1 it reinforces a stale mental model a reader may carry into AC-5. | Optionally soften to neutral framing (e.g. the second list must be laid out without overlapping the first) so the Problem section does not imply band-height growth that the ADR rejects. Lower priority than #1; fixing #1 largely resolves the residual ambiguity. |

## Verdict
CONCERNS: 2

Notes on items verified clean (no finding raised):

- **Old/new `DoWidgetTab` signature.** ADR `#a-signature` "Old (before)" block (lines 314-319) reproduces the audit's verbatim current signature (`WorkTypeThingRuleWidget.cs:179-184`, audit lines 54-61) exactly — same 10 parameters, same `ref`/types/order, ending `IReadOnlyList<ThingDef> things)`. New signature appends `ref Vector2 mapThingIconBoxScrollPosition` + `IReadOnlyList<Thing>? mapThings = null`, consistent with the ADR decision text and the AC-6 record. No drift.
- **Traceability.** sprint.md AC1–AC7 ↔ PRD AC-1–AC-7 ↔ ADR `#acceptance` mapping all align 1:1; PRD `#open-decisions` correctly defers Public API form and Score-sort ownership to the ADR (ADR-0009 / ADR-0010), and the ADR resolves both. Open decisions are not double-decided in the PRD.
- **Responsibility blocks.** sprint.md, audit.md, prd.html, adr.html each stay within their `responsibility` frontmatter (sprint=scope/AC, audit=brownfield findings, prd=requirements, adr=decisions). The PRD `#follow-ups` correctly defers the candidate `design/api/` doc to design-promote rather than deciding it.
- **Provenance.** Both prd.html and adr.html declare `provenance: original`, `source: ""`; badge correctly omitted (no provenance badge rendered, matching the `original` rule). ADR status `accepted`, PRD status `draft` — appropriate for design-review stage.
- **Missing ux-spec.html** is expected for this IMGUI library sprint (precedent 003) and PRD `#ui-ux` documents the N/A rationale; not a defect.
- **Score-sort ownership** (ADR-0010, consumer pre-sorts) is consistently stated across PRD AC-3 and the ADR; the order-dependence caveat is sourced to the audit and ADR-0002 without contradiction.

## Next action
asd-architect (and/or asd-ba) autofix finding #1 within the loop: align PRD AC-5 with the accepted ADR-0009 side-by-side / constant-`GetBottomPartHeight` decision so the height model has one authoritative home. Optionally address #2. No escalation required — this is a doc-alignment fix to make the artefacts agree on an already-accepted decision, not a concept/contract change. Re-enter design-review next iteration.
