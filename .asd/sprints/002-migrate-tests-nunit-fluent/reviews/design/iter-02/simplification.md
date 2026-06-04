---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-design-simplification]: APPROVE

# Review — simplification

- **Phase**: design-review
- **Iteration**: 02

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | adr.html · ADR-0007 (Serialization remap) | `[NonParallelizable]` is applied to the three static-touching classes (`StatWeightTests`, `StatefulSubsystemTests`, `StatRangesTests`) even though the ADR itself states NUnit is non-parallel by default and the project sets no `[Parallelizable]`. The attribute is self-described as "documentation, not load-bearing wiring" / "belt-and-suspenders." Category: **keep-as-is**. It does not trip a critical checklist item: it is not a caller-toggled config flag, not an abstraction, not dead code, and not defensive code for an impossible case — it guards a real future foot-gun (an accidental `[assembly: Parallelizable]` opt-in would silently race the snapshot/restore isolation). Cost is one zero-runtime attribute per class with explicit intent. The redundancy is acceptable and earns its weight as a regression guard on a load-bearing isolation contract. Noted, not actioned. | None required. Leaving the marker is the correct minimal-viable choice; removing it would trade a self-documenting guard for an implicit dependency on a runner default. |

## Verdict
APPROVE

Checklist sweep (over-engineering, critical/undroppable) — none tripped:

- Interface with one implementer — none introduced. `IDefProvider` is pre-existing (ADR-0001), explicitly out of scope and unchanged.
- Generic with one concrete type param — none.
- Factory for <3 classes — none.
- Plugin system with no plugin — none.
- Abstraction with no second use case — none. ADR-0006 `[SetUpFixture]` and ADR-0007 `[SetUp]`/`[TearDown]` are framework-idiomatic seam replacements for existing seams; resolver/snapshot logic is reused verbatim, not newly abstracted.
- Premature config flag — none. Version pins (NUnit 4.6.1, adapter 6.2.0, FA 7.2.2) are user decisions, not caller-facing flags.
- Defensive code for impossible-by-contract case — the ADR-0006 idempotency guard (`RimWorldResolverInitialized`) is settled per scope (user-accepted ADR-0006). Its defense is now tied to a concrete anticipated second registration path (the `[ModuleInitializer]` contingency), so the once-only guard is justified, not a smell. Not flagged.
- Helper wrapping one stdlib call — none.
- Inheritance depth ≥3 without polymorphic dispatch — `StaticStateTestBase` is a single per-test lifecycle base; depth is shallow and unchanged from ADR-0001.
- Framework wrapping a framework — none; the seam expresses plain NUnit constructs, it does not wrap NUnit.
- Mock of a mock — N/A at design stage; no mock layering proposed.
- Comment restating code — N/A (design artifacts).
- Dead code "in case we need it" — the `[ModuleInitializer]` path is an explicitly anticipated, user-accepted fallback documented as not-yet-built, not dead code carried in the tree.

Complexity-vs-value: the design is minimal-viable. Three deps added / two removed are correctly routed through Complication Approval (already escalated at the design gate per decisions-log 2026-06-04). Attribute and assertion mappings are 1:1 with documented exceptions (the `BeApproximately` tolerance) flagged as reviewable edits. No horizontal scaffolding; the change is a behaviour-preserving vertical swap. Design-principles 1 (Evidence), 2 (KISS), 9 (Evolutionary — reversibility and trigger documented for the resolver seam) are satisfied.

## Next action
Reviewer done. No autofix and no escalation required. Finding #1 is a medium `keep-as-is` note carried for the creator's awareness; it does not block DoD.

## Escalations (optional)
- None.
