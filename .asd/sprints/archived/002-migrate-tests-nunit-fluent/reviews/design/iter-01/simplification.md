---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-design-simplification]: CONCERNS

# Review — simplification

- **Phase**: design-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | critical | adr.html ADR-0006 decision bullet 2 ("Chosen timing guarantee") + Negative bullet 3 | Defensive code for an impossible-by-contract case. The ADR keeps the `AppDomain.GetData("RimWorldResolverInitialized")` idempotency guard while simultaneously stating the contract makes a second registration impossible: "`[OneTimeSetUp]` runs once per assembly by contract, so the guard is redundant but harmless... We keep the guard defensively". This is exactly the over-engineering checklist item "defensive code for impossible-by-contract case". The justification ("protects against any future second registration path") is the speculative "in case we need it" rationale the checklist rejects — there is no second registration path in scope, and `[OneTimeSetUp]` is the single registration site by design. | Drop the idempotency guard. The single global `[SetUpFixture].[OneTimeSetUp]` is the sole registration path and runs exactly once per assembly by NUnit contract (the same contract this migration relies on for the timing guarantee). If a second registration path is ever introduced, that change carries its own guard decision. Removing the guard also deletes dead `AppDomain.GetData/SetData` state. Category: `simplify`. |
| 2 | low | adr.html ADR-0007 decision bullet 3 ("Serialization remap") + Positive bullet 2 | `[NonParallelizable]` is applied to three classes although NUnit is non-parallel by default and the project sets no `[Parallelizable]`, so the attribute is not load-bearing ("belt-and-suspenders"). Borderline against the premature-config / defensive-code smells. NOT raised as a violation: the ADR is explicit that this is self-documenting intent (making the load-bearing non-parallel assumption visible at the call site), zero-cost, and guards a *possible* future change (an assembly-level parallel opt-in) rather than an impossible-by-contract case. Recorded for transparency only. | No change required. The attribute earns its weight as an explicit statement of an otherwise-implicit, isolation-critical assumption. Category: `keep-as-is`. |

## Verdict
CONCERNS: 1

## Next action
Architect (asd-architect) autofixes finding #1 within the loop: remove the deliberately-retained idempotency guard from the ADR-0006 decision and from the Negative consequence that frames it as kept-for-future, so the seam carries no defensive code for an impossible-by-contract case. Re-enter design-review next iteration. Finding #2 needs no action.

## Notes (cross-reviewer guard, complexity-vs-value)
- Overall the design is minimal-viable for a 1:1 migration. Both infra seams (ADR-0006 resolver, ADR-0007 StaticState) explicitly reuse existing logic verbatim and introduce no new abstraction, layer, generic, factory, or interface. The `[SetUpFixture]` is the NUnit-idiomatic replacement for the removed `[assembly: TestFramework]` seam — a like-for-like swap, correctly argued in ADR-0006 as "not a new abstraction".
- The three added packages (NUnit, NUnit3TestAdapter, FluentAssertions 7.x) are the user-approved sprint scope and are correctly surfaced under Complication Approval (ADR-0004, ADR-0005, decisions-log 2026-06-04). No escalation needed from this reviewer — already on the approved-scope path.
- No interface-with-one-implementer, generic-with-one-type, factory-for-<3, plugin-without-plugin, framework-on-framework, mock-of-mock, or comment-restates-code smell found in either draft.
- Cross-reviewer guard: finding #1's suggested fix (delete the guard) is a removal, so it adds no complexity. No reviewer-proposed complication to escalate.

REVIEW_DONE
