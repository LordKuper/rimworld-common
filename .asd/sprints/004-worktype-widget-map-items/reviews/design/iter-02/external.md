---
responsibility:
  owns: external review aggregation report (kept/dropped accounting for design-review iter 2)
  excludes: codex raw prompt, internal reviewer output
  delegates_to: t_prompt-external-design.md (prompt), t_review-report.md (output)
---

[REVIEW-design-external]: APPROVE

# External Review Report

- **Phase**: design-review
- **Iteration**: 2
- **Severity floor (this iter)**: high (report only high or critical; medium and low dropped)
- **External tool**: Codex CLI (`codex-cli 0.130.0`) — available; review executed via `codex exec --sandbox read-only --output-schema <schema> -o <out> -` (prompt on stdin, JSON-schema output). The installed CLI exposes a prompt + JSON-schema interface rather than the `review --json --input --output` form named in `external-review.md`; the JSON-schema output is the functional equivalent and was parsed and mapped below.
- **Codex raw verdict**: `{"verdict":"APPROVE","iter1_contradiction_resolved":true,"findings":[]}`

## Iteration-1 finding follow-up

| Iter-1 finding | Status | Evidence |
|---|---|---|
| F1 (high) — PRD AC-5 wording contradicted accepted ADR-0009 (implied vertical stacking / growing band height vs. ADR-0009's side-by-side, width-split, constant-height decision) | **Resolved — not re-raised** | PRD AC-5 (`#AC-5`) now states the lists render **side by side**, `DoBottomPart` splits the band **width** into **disjoint horizontal halves**, the bottom-section **height is constant**, `GetBottomPartHeight(...)` is **unchanged** (one box height + one section header + one gap), and List 1 renders **full width** when `mapThings` is null/empty. The Problem section and library-maintainer user story were aligned to the same wording. This matches ADR-0009's "Side-by-side layout, constant height" decision exactly. Codex confirmed `iter1_contradiction_resolved: true`. |

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | none — Codex returned no high/critical findings on the post-fix drafts | — |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | — | none — Codex returned no findings, so nothing was dropped for being below the iter-2 high floor |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | none — Codex returned no nitpick-class findings | — |

## Stalemate check

Not a stalemate. Iter-1 finding set = { F1 (high) }; iter-2 finding set = { } (empty). The two sets differ, so the consecutive-iteration identical-finding condition does not hold. No escalation.

## Verdict
APPROVE

The iter-1 internal-consistency defect (PRD AC-5 vs. ADR-0009) is resolved: AC-5 now matches the accepted side-by-side / constant-height / `GetBottomPartHeight`-unchanged decision, with consistent wording in the Problem section and the library-maintainer user story. No new high- or critical-severity findings were raised against the post-fix prd.html / adr.html drafts. The intentionally-allowed `DoWidgetTab` signature break (documented under AC-6) and the N/A ux-spec/mockups/accessibility items were excluded from scope and correctly produced no findings.

## Next action
External Review APPROVE for design-review iteration 2. Counts as one reviewer toward the design-review DoD (Documentation, UI, Simplification, External Review all APPROVE in the same iteration). No further external-review action required.

REVIEW_DONE: external
