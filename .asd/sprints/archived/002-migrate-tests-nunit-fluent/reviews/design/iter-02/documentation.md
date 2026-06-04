[REVIEW-design-documentation]: APPROVE

# Documentation review — design, iteration 02

Reviewer: Documentation. Severity floor: **medium** (iter 2 — low/nitpick dropped).
Targets: `design/prd.html`, `design/adr.html`. Cross-checked against `sprint.md`, `audit.md`, persistent `design/architecture/` (ADR-0001/0002/0003, stack.html, tech-references).

## Findings

| # | Severity | Location | Finding |
|---|----------|----------|---------|
| — | — | — | No findings at or above the medium floor. |

## Rubric notes (informational — no action required)

- **SSoT**: The `Assert.*` → `.Should()` conversion table has a single canonical home in `prd.html §Assertion mapping reference`. `adr.html` ADR-0005 correctly links to it (`prd.html §Assertion mapping reference`) and explicitly states "this ADR does *not* restate those rows", owning only the conversion *policy* (FA 7.x pin, `BeApproximately` semantic shift, `Assert.True/False` diff rule). No duplication. The same table also appears in `audit.md`, but that is brownfield inventory owned by the audit's responsibility block — a legitimately separate concern, not a downstream copy of the SSoT.
- **Template responsibility blocks**: Both drafts carry well-formed `responsibility` frontmatter (`owns` / `excludes` / `delegates_to`). PRD delegates decisions to `adr.html` and ui/flows to `ux-spec.html`; ADR delegates requirements to `prd.html` and implementation to code. Sections respect declared scope — PRD §UX/UI correctly marks UI "Not applicable" rather than authoring it; ADR defers assertion-mapping rows to the PRD.
- **Provenance**: Both `provenance: original`, `source: ""`. Per `artifact-layout.md` the provenance badge is omitted for `original`, and neither draft renders one. The `.badges .provenance-*` CSS classes exist only for the reverse-engineered/migrated cases and are correctly unused. Correct.
- **Traceability (PRD AC ↔ ADR ↔ audit)**: All 28 ACs trace. AC-1…AC-9 ↔ ADR-0004 (framework/attribute swap); AC-10…AC-17 ↔ ADR-0005 (assertions); AC-19…AC-21 ↔ ADR-0006 (resolver seam); AC-22…AC-25 ↔ ADR-0007 (isolation remap); AC-26…AC-28 ↔ build/coverage consequences across ADR-0004/0005/0006. Inventory figures (132 `[Fact]`, 3 `[Theory]`, 13 `[InlineData]`, 236 `Assert.*`, 12 distinct methods) are consistent across PRD, ADR, and `audit.md`.
- **ADR numbering vs persistent set**: Persistent ADRs are 0001–0003; sprint draft continues 0004–0007 with no collision. ADR-0007's scoped supersession of ADR-0001's *vocabulary* (not its contract) is correctly framed and routed through design-promote, not edited in place.
- **Tech-reference version alignment**: ADR-0004 pins NUnit **4.6.1** + NUnit3TestAdapter **6.2.0**; ADR-0005 pins FluentAssertions **7.2.2**. These match the persistent tech-references exactly (`tech-reference/nunit-4.6.1.md`, `nunit3-testadapter-6.2.0.md`, `fluentassertions-7.2.2.md`), including the `Microsoft.NET.Test.Sdk` 17.14.1 retention. No version drift between the decision prose and the vetted references.
- **Persistent-doc provenance / non-edit discipline**: PRD §Non-goals and ADR-0004/0007 consequences correctly route updates to the superseded xUnit tech-reference, `stack.html`, and approved ADR-0001 through design → design-promote, never editing approved/persistent docs in place. This matches the `audit.md` documentation-migration plan. Cross-reference link paths (`../../../design/architecture/...`) resolve correctly from the sprint `design/` dir to repo-root `design/`.
- **Custom-rules consistency**: Glossary/naming (xUnit, NUnit, FluentAssertions, `net472`, `Directory.Build.props`, AltCover, `scripts/coverage.ps1`) used consistently with `custom-common-rules.md` and the audit. The audit's flag that `custom-coding-rules.md` "Testing (xUnit)" wording and the `custom-common-rules.md` "xUnit, net472" line need retitling is correctly carried as design-promote follow-up, not silently changed here.
- **HTML chrome**: Both drafts are self-contained documents using the same inlined `<head>`/`<style>` chrome as the repo's existing persistent ADRs (`adr-0003-build-governance.html`) — the established repo convention for these design documents. No bare fragments, no duplicated shell conflict. This is a no-UI sprint with no user-facing application HTML; not flagged.

## Verdict

**APPROVE.** SSoT integrity intact (single-homed assertion mapping; correct link-not-copy from ADR-0005 and audit). Responsibility blocks, provenance flags, AC↔ADR↔audit traceability, ADR numbering, and tech-reference version alignment all consistent. Persistent-doc edits correctly deferred to design-promote. No findings at or above the medium severity floor.

## Next action

Proceed to design-promote. Carry the audit's documentation-migration plan (xUnit tech-reference supersession, stack.html test-stack rows, ADR-0001 vocabulary remap, coverlet→AltCover reconciliation, `custom-coding-rules.md`/`custom-common-rules.md` retitling) into that phase — owned by the domain creators, not this reviewer.

## Escalations

None.

REVIEW_DONE
