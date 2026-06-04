---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-design-simplification]: APPROVE

# Review — simplification

- **Phase**: design-review
- **Iteration**: 03

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above floor (high); no undroppable over-engineering checklist hit | — |

## Assessment notes (no finding raised — recorded for audit trail)

The over-engineering checklist was applied item by item to both ADRs. Three items came near the line and were deliberately NOT raised. Reasoning is recorded so a later iteration with clean context does not re-litigate them as new:

- **ADR-0006 idempotency guard** (`AppDomain.GetData/SetData("RimWorldResolverInitialized")`) — checklist item "Defensive code for impossible-by-contract case" / "Dead code in case we need it". With only the `[OneTimeSetUp]` registration path live, a guard against a *second* concurrent registration protects a case that cannot occur today (the `[ModuleInitializer]` fallback is explicitly "not chosen now"). However: (a) this guard is *pre-existing code preserved verbatim*, not net-new complexity introduced by this design — the migration is contractually behaviour-preserving (AC-20); (b) the resolver seam, including its mechanism, is a settled user-accepted-risk decision per scope instruction and ADR-0006. Removing the guard would be a behaviour change to settled, in-scope-frozen code, not a simplification this reviewer is authorised to demand. Not a finding. The ADR's *narrative justification* leans on a not-yet-existent contingency, but that is documentation framing, not added complexity — and documentation wording is a nitpick-drop category.

- **ADR-0007 `[NonParallelizable]` on the three static-state classes** — checklist item "Premature config flag (no caller chooses non-default)". The ADR is explicit that NUnit is non-parallel by default and this attribute is therefore "documentation, not load-bearing wiring". One redundant-but-inert attribute per class, self-documenting the isolation requirement, with the cons section honestly disclosing it adds nothing behavioural. Net complexity weight is negligible (no abstraction, layer, type, or branch), and it earns its weight as an explicit guard against a future accidental `[assembly: Parallelizable]` opt-in that would silently break the snapshot/restore contract. Earns its weight; not a finding. Below the high floor regardless.

- **ADR-0006 `[ModuleInitializer]` documented contingency** — checklist item "Abstraction with no second use case" / speculative future-proofing. Correctly handled: it is documented as an *anticipated fallback*, explicitly NOT built now, listed under Alternatives. No code, type, or abstraction is added at design time. This is the right way to defer a high-reversal-cost commitment (design-principle 9). Not a finding.

Positive observations (complexity-vs-value): the four ADRs consistently choose the minimal mechanism — 1:1 attribute mapping with no shim layer (ADR-0004), reuse of the existing resolver body behind an idiomatic NUnit hook with "not a new abstraction" stated outright (ADR-0006), verbatim snapshot/restore body with only the invocation vocabulary changed (ADR-0007), and a single authoritative assertion-mapping table in the PRD rather than duplicated rows (ADR-0005, SSoT respected). The three added dependencies (NUnit, NUnit3TestAdapter, FluentAssertions) are correctly routed through Complication Approval as the direct approved sprint scope, not smuggled in. No interface-with-one-implementer, no factory, no plugin system, no generic-with-one-type, no framework-wrapping-framework, no mock-of-a-mock, no inheritance depth ≥3.

## Cross-reviewer guard

No reviewer-proposed fix in scope would itself add an abstraction, layer, or dependency. Should a sibling reviewer propose hardening the AC-20 discovery-time ordering by *building* the `[ModuleInitializer]` fallback now, that would be a new mechanism for a problem that has not manifested — escalate via Complication Approval rather than adopt pre-emptively. Flagged here as a standing guard, not a current finding.

## Verdict
APPROVE

## Next action
Reviewer done. No simplification finding blocks the design-review DoD for this reviewer.

## Escalations (optional)
- none
