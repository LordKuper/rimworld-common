[REVIEW-design-documentation]: CONCERNS

# Review — documentation

- **Phase**: design-review
- **Iteration**: 01

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | `adr.html` ADR-0001/0002/0003 "Acceptance criteria satisfied (by theme)" + the per-ADR `font-size:0.85rem` notes ("AC numbers referenced by cluster/theme to avoid drift, as the BA is renumbering ACs in parallel") | ADR→AC back-traceability is theme-only; no ADR cites a single concrete AC-ID. The dispatch contract requires "every ADR references the ACs it satisfies." The renumbering rationale is now stale — ACs are finalized AC-1…AC-27 in `prd.html`. Forward traceability (PRD→ADR) is sound, but the reverse link the rubric requires is missing in all three ADRs. | Replace the by-theme bullets with explicit AC-ID lists: ADR-0001 → AC-13, AC-14, AC-15, AC-16, AC-18, AC-19 (gates AC-20/AC-21); ADR-0002 → AC-9, AC-16; ADR-0003 → AC-2, AC-3, AC-4, AC-5. Drop the "BA is renumbering" disclaimer. |
| 2 | medium | `prd.html` §IMP traceability table (rows IMP-01…IMP-12) vs Cluster G ACs `AC-24`, `AC-25`, `AC-26` | The table establishes IMP→AC coverage, but the reverse (every AC traces to an origin) is broken for three Cluster-G "cross-cutting" criteria. AC-24 (static-init Logger context), AC-25 (no new suppressions), AC-26 (breaking-API enumeration) appear in no IMP row and trace to no IMP or ADR. AC-22/23/27 are reachable (IMP-10 / IMP-09) but only AC-22 is actually listed in a row; AC-23/AC-27 are referenced only in prose. The PRD's own claim "No AC is invented without a traceable origin" is therefore not demonstrated for these rows. | Add a traceability row (or a Cluster-G origin note) mapping AC-22/23/24/25/26/27 to their source — AC-22/23/27 to IMP-10/IMP-09 respectively, and AC-24/25/26 to their governing audit finding/rule (Fail-Fast-on-Load, suppression policy, `backward_compat=none`) so each has a cited origin rather than free-standing. |
| 3 | low | `prd.html` (problem lede, G2, Cluster C callout, AC-8 cell, IMP-07 traceability row, non-goals item 2, §ADR-decisions ADR-A) and `adr.html` §intro "Considered and not pursued" | The full IMP-07 reframing rationale ("intentional seed defaults overridden by consumer-mod persisted settings via `IExposable` StatWeight/WorkTypeThingRule … reclassified as NOT a violation") is restated near-verbatim in 6+ locations across PRD and again in ADR, rather than stated once and linked. SSoT iron rule: state the fact in one home and link. Low risk today (no divergence), but six copies are six future drift points. | Make one canonical home for the reframing — the Cluster C callout in `prd.html` is the natural anchor — and have the other locations link to `#acceptance-criteria` (or the ADR intro) with a one-line summary instead of repeating the full rationale. |
| 4 | low | `adr.html` §intro "Considered and not pursued" / non-architectural code-fix paragraph; `prd.html` §ADR-decisions `ADR-A` | The dropped ADR-A and the logging-only IMP-07 outcome are documented in both `adr.html` (as "considered and not pursued") and `prd.html` §ADR-decisions (as "ADR-A · RESOLVED — no Def/config surface"). The PRD labels it `ADR-A` while the ADR doc has no such record (it was dropped). A reader cross-referencing `ADR-A` from the PRD finds no matching record in `adr.html`. Naming mismatch (PRD uses provisional ADR-A/B/C; ADR doc uses ADR-0001/0002/0003). | In `prd.html` §ADR-decisions, retire the provisional `ADR-A/B/C` labels in favor of the final IDs: ADR-B → ADR-0002, ADR-C → ADR-0001, and note ADR-A as "dropped, no record (see adr.html intro)." Keeps PRD↔ADR identifiers aligned. |
| 5 | low | `adr.html` ADR-0002 vs `.asd/project/custom-design-rules.md` §Determinism ("No time- or order-dependent behavior in core logic unless explicitly required") | ADR-0002 keeps order-dependent scoring and documents it. This is a legitimate "explicitly required / blessed" outcome and a user-approved decision, so not a violation — but the ADR never cites the custom Determinism rule it is consciously deviating from, so the reviewed exception is implicit rather than recorded against the rule. | Add one line to ADR-0002 Context or Decision noting the custom-design-rules Determinism rule and that this decision is the explicit "unless required" exception it permits, so the deviation is traceable to the rule. |

## Verdict
CONCERNS: 5

## Next action
asd-ba (PRD) and asd-architect (ADR) autofix within the design-review loop — no escalation required, all findings are documentation/traceability fixes within already-approved scope:
- Findings #1, #5 → ADR (asd-architect): add explicit AC-IDs per ADR; cite the Determinism custom rule in ADR-0002.
- Findings #2, #3, #4 → PRD (asd-ba): add Cluster-G AC origins; collapse the duplicated IMP-07 rationale to one linked home; align provisional ADR-A/B/C labels with final ADR-0001/0002/0003 IDs.

Then re-enter design-review (iteration 02).

## Notes (verified clean — not findings)
- HTML shell wrapping: both `prd.html` and `adr.html` are complete shell-wrapped documents (DOCTYPE/html/head/style/script present, all meta placeholders filled, badges + stats + TOC present). No bare fragments, no duplicated chrome. PASS.
- Provenance: both drafts `provenance=original`, `source=""`, and correctly omit the provenance badge. `status=draft` correct for design phase. PASS. (Persistent `concept.html`/`stack.html` carry `reverse-engineered` + populated `source` and DO render the provenance badge — correct.)
- Responsibility frontmatter: both drafts preserve the template `responsibility` block (owns/excludes/delegates_to) and content respects declared scope — PRD carries no decisions (delegates to ADR), ADR carries no requirements (delegates to PRD). PASS.
- SSoT supportedVersions / deps: `1.5 + 1.6` and the Harmony 2.4.2 (compile) / `loadAfter vanillaexpanded.skills` facts are consistent across `About.xml` (SSoT), `stack.html`, `prd.html` (AC-23), and `audit.md`. AC-22 correctly names `About.xml` as the single SSoT with docs reconciling *to* it. No divergence found.
- Intentional decisions confirmed correctly represented and NOT flagged: ux-spec.html absence (no UI scope), IMP-09 won't-do (1.5 frozen, AC-23/AC-27 keep it listed-but-unmodified), IMP-07 logging-only reframe (AC-8), IMP-03 won't-do (LangVersion=latest, ADR-0003 + risk note), adaptive-StatRanges option (b) (ADR-0002 + AC-9/AC-16). All consistent across prd/adr/audit.
- Stats chips accurate: 4 goals / 8 stories (5+3) / 27 AC / 8 non-goals match actual content.
