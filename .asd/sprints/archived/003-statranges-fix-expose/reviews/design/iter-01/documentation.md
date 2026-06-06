[REVIEW-design-documentation]: CONCERNS

# Review — documentation

- **Phase**: design-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | `design/adr.html` header badge, line 139 | The header status badge is `<span class="status-draft">proposed</span>` — the CSS class (`status-draft`) disagrees with the meta `status=proposed` (line 18) and the badge text "proposed". The shell contract fills the header badge as `status-{{STATUS}}`; with STATUS=proposed the class must be `status-proposed`. The shell only styles `status-draft/in-review/approved/locked` in `.badges`, so `status-proposed` would render unstyled, but emitting `status-draft` for a `proposed` doc is a wrong-placeholder fill. (The inner `.adr .meta` chip on line 152 is correct: `status-chip status-proposed`.) | Change the header badge to `<span class="status-proposed">proposed</span>` to match the meta + the inner chip; STATUS placeholder must be filled consistently across meta, header badge class, and badge text. |
| 2 | low | `design/prd.html` §Documentation follow-ups, lines 244–252 | The PRD frontmatter `excludes: ... design decisions`, yet this section narrates the ADR-0002/0007 drift and reconciliation routing. It is correctly framed as non-requirements deferred to design-promote (not as decisions), and the same facts are owned by ADR-0008's Amends note and audit.md Gaps — so it is borderline duplication rather than a hard SSoT/responsibility breach. To keep the PRD strictly on requirements, prefer linking to the ADR Amends note rather than restating the drift. | Optional: replace the two restated bullets with a one-line pointer to `adr.html#amends` and `audit.md` Gaps, keeping the PRD's own scope (the XML-doc follow-up note may stay as it is impl guidance, not a decision). |

## Verdict
CONCERNS: 2

Both findings are low severity; neither blocks promotion on its own. Documentation integrity is otherwise sound:

- **SSoT**: No contradictory facts across prd/adr/sprint/audit. The PRD owns AC-1..AC-10 and links sprint.md (1:1 acceptance map, line 212); the ADR resolves AC references back to the PRD (lines 318–321) rather than re-owning them; sprint.md owns high-level acceptance; audit owns the findings. The shared bug narrative recurs at each layer at its own responsibility level (finding → requirement → decision), which is the expected sprint-draft layering, not a duplication defect.
- **Template responsibility-block adherence**: Both drafts carry correct responsibility frontmatter. PRD owns requirements / excludes decisions; ADR owns the single decision / excludes requirements. No requirement text leaks into the ADR and no decision-making leaks into the PRD (see finding #2 for the one borderline section).
- **HTML shell wrapping**: Both drafts are complete shell-conformant documents (matching `<head>` meta block, `.layout` grid, header badges, `<main>`, footer). All required placeholders are filled: DOC_TYPE, SUBSYSTEM, SPRINT_ID, STATUS, UPDATED_AT, RESPONSIBILITY, PROVENANCE, SOURCE, TITLE, STATS, TOC, CONTENT. No bare fragments; no duplicated chrome inside content. PRD STATS counts verified accurate (3 goals, 5 stories, 10 AC, 1 non-goal). Only the STATUS-class mismatch in finding #1.
- **Provenance**: Both `provenance=original`, `source=""` — correct (ADR-0008 is genuinely new; PRD is an original sprint draft). Per artifact-layout.md §Badge omission, the `<span class="provenance-original">` badge is correctly OMITTED in both header badge rows. No misuse of `reverse-engineered`/`migrated`.
- **Traceability**: Every AC traces to both a decision and an acceptance item. AC-1,2→D1; AC-3,4→D2; AC-5→D2+D4; AC-6→D3; AC-7..AC-10→impl/test/build scope (no architectural choice, so the ADR correctly does not invent a decision — it notes it fixes the contract those criteria verify). All four decisions D1–D4 map to ACs; no orphan decisions, no orphan ACs. Each AC maps to a sprint.md acceptance bullet (Bug fix / Visibility / Constraint / Tests / Build & publish).
- **ADR-0002/0007 reconciliation correctly DEFERRED**: Verified against the persistent files — ADR-0002 (line 222) still asserts "no signature or runtime-behavior change to any public member" and describes `NormalizeStatValue` as `internal static` (line 194); ADR-0007 still describes the reflection-null reset of `StatRanges.Ranges` (lines 162–165, 206–214). Neither persistent ADR was edited by these drafts. ADR-0008's Amends note (lines 157–168) explicitly records the superseding clauses while stating "wording reconciliation … is design-promote work, not part of this ADR's decision," and PRD follow-ups + audit Gaps/migration-plan all consistently defer to design-promote. The quoted ADR-0002 clause is accurate. Deferral is correct and clearly flagged.
- **UX-spec / design-system**: Intentionally skipped (headless library, user decision) — not flagged as a defect, per scope.

## Next action
Architect/BA may apply the two low-severity fixes (status-badge class in adr.html; optional de-duplication of the PRD follow-ups section) at their discretion. Neither is a promotion blocker; PM may proceed to design-promote with these noted. No user escalation required.
