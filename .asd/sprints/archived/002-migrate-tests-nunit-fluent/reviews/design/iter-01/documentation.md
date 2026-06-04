---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-design-documentation]: CONCERNS

# Review — documentation

- **Phase**: design-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | adr.html `#adr-0005` Decision (ll. 322-333) vs prd.html `#assertion-mapping` (ll. 246-265) | The assertion conversion table is declared the PRD's authoritative SSoT (AC-16, PRD ll. 229,246). ADR-0005 then re-lists ~9 of the per-method direct maps verbatim in prose (`Assert.Equal → .Should().Be`, `Null/NotNull`, `Contains/DoesNotContain`, `Single`, `Empty/NotEmpty`, `NotSame`, `Throws<T>`). This duplicates the SSoT-owned conversion data rather than linking to it. The ADR correctly cites and links the PRD table, so the intent is right, but the copied rows are a SSoT tension under the iron rule. | In ADR-0005 Decision, replace the inline list of direct maps with a single link to PRD `#assertion-mapping` and retain in the ADR only the *policy* facts the ADR genuinely owns: the 7.x-vs-8.x pin, the `BeApproximately` precision-nuance treatment, and the diff-preservation/AC-13/AC-17 policy. The mechanical per-method maps should live only in the PRD. |
| 2 | low | adr.html shell meta `subsystem=sprint` (l. 16) vs STATS chip `subsystem: project` (l. 138) and each `.adr .meta` chip `subsystem: project` (ll. 152,287,401,517) | Internal inconsistency in the same document on the SUBSYSTEM value. Per `artifact-layout.md` the shell `{{SUBSYSTEM}}` for a sprint draft is `sprint` (meta tag is correct); the per-ADR chips and STATS chip assert `project`. The two are defensible separately (ADR content promotes to project-wide `design/architecture/adr/`) but should not disagree within one file. | Pick one rendering and apply it consistently. Recommended: keep shell meta `subsystem=sprint` for the draft, and label the per-ADR chips `project` only if accompanied by an explicit note that it reflects the post-promote home; otherwise align all to `sprint` for the draft and let design-promote set `project`. |
| 3 | low | prd.html `#user-stories` (ll. 186,201) | AC-26 (a `build/run` criterion: warning-clean build + green under the NUnit3 adapter) is cited under both the "behaviour-preserving refactor" maintainer story and the "coverage" maintainer story. The dual mapping blurs which story owns the criterion. | Cite AC-26 under the build/run-oriented story only; the coverage story should reference AC-27…AC-28 (coverage script + AltCover band) which are its actual criteria. |

## Verdict
CONCERNS: 3

## Next action
Architect (ADR owner) and BA (PRD owner) autofix findings 1-3 within the loop; sprint re-enters the next design-review iteration. No escalation required — all three are documentation-internal consistency fixes that do not change any approved concept, requirement, API contract, or scope.

## Notes (not findings)
- Provenance: both drafts `provenance: original` with `source` empty; provenance badge correctly omitted from both headers per `artifact-layout.md`. Correct.
- Responsibility frontmatter present and preserved in both drafts; section content respects the declared `owns`/`excludes` (PRD delegates decisions to ADR; ADR delegates requirements to PRD).
- Traceability complete: all 28 ACs map to goals/stories and are claimed by ADR-0004 (AC-1…AC-9), ADR-0005 (AC-10…AC-17), ADR-0006 (AC-19…AC-21,AC-27), ADR-0007 (AC-22…AC-25); AC-18/AC-26/AC-28 covered. All cross-doc links to persistent `design/architecture/adr/adr-0001/0002/0003-*.html` and the xUnit tech-reference resolve to existing files; flat (decomposition-disabled) ADR pathing is correct per config.
- The 3 new persistent tech-references (`nunit-4.6.1.md`, `nunit3-testadapter-6.2.0.md`, `fluentassertions-7.2.2.md`) exist (Architect-owned, not under review this phase); the audit's documentation-migration plan correctly routes stack.html/ADR-0001/coverlet-reference updates through design-promote and explicitly forbids in-place edits of approved persistent docs (SSoT-respecting).
- Custom-rules consistency (custom-common-rules.md / custom-design-rules.md): drafts use consistent project terminology; the xUnit→NUnit rule/wording updates in `custom-coding-rules.md` and `custom-common-rules.md` are correctly deferred to design-promote (non-goals l. 281), not done in these drafts.
- No-UX is expected for this test-migration sprint (PRD `#ux` "Not applicable") — not a finding.
