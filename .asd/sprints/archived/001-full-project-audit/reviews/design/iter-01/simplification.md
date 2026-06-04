[REVIEW-design-simplification]: APPROVE

# Review — Simplification

- **Phase**: design-review
- **Iteration**: 01

## Summary of assessment

Scope is audit-remediation (sprint shape B): no new feature scope, no UX, no new external technology (ADR intro explicitly confirms no new provider/framework/runtime). The design carries exactly three accepted ADRs and 27 ACs, all traced to one of the 12 approved IMPs. I scanned every ADR decision and every AC against the over-engineering checklist (`review-policy.md`) and the Simplicity Default. No critical over-engineering smell trips. A small number of low-severity findings (AC redundancy, one cross-doc consistency gap) are recorded per the iteration-1 severity floor (all severities reported).

### Over-engineering checklist — pass detail (the load-bearing call)

The one item that most plausibly trips the checklist is ADR-0001's `IDefProvider` seam. It does **not** trip, for concrete reasons:

- **"Interface with exactly one implementer" / "Abstraction with no second use case"** — `IDefProvider` has two real implementers (`VerseDefProvider` production pass-through + `FakeDefProvider` for tests) and the second use case (testability of `DefDatabase`-bound code) is the validated, evidence-backed need driving the whole sprint (G4, IMP-11, ≥80% floor). The seam has 6 concrete consumers verified in the audit (`StatHelper`, `WorkTypeStatMap`, `WorkTypeThingRule`, `DefCache`, `StatWeight`, `PassionHelper`), all currently calling `DefDatabase<T>` directly. This is a genuine seam over a real cut line, not speculative indirection.
- **Granularity** — a single `IDefProvider` is the correct granularity. Splitting into per-subsystem providers would be *worse* (more interfaces, no consumer needs the split). Plain func/delegate injection would be *simpler in the abstract*, but ADR-0001 documents (and the code confirms: the statics build caches in static constructors / `[StaticConstructorOnStartup]` with no instance to inject into) that a static `DefProvider.Current` injection point is the minimal seam fitting the existing static design. The two simpler alternatives are explicitly recorded and reasonably rejected in "Alternatives considered".
- **Approval trail** — the new abstraction + `InternalsVisibleTo` grant were taken to the user under Complication Approval (granted 2026-06-03), exactly as `core.md` Simplicity Default requires. No escalation is owed.
- The `Rebuild()` static-ctor extraction, `InternalsVisibleTo`, and `StaticStateFixture` are each the minimum needed to make load-time caches resettable between tests — they are test-isolation machinery for a stated AC (AC-14/15/16/19), not gold-plating.

### Other checklist items — clear

- **Premature config flag** — none added. LangVersion pin was *rejected* (ADR-0003), avoiding a speculative config knob; `RIMWORLD_DIR` override already exists and is documented, not new. Good simplicity discipline.
- **Defensive code for impossible case** — AC-24 (Logger context on static-init failure) and AC-8 (warn on null `StatDef`) target *real, reachable* failures (a renamed/removed vanilla `StatDef` resolving null via `GetNamedSilentFail`), not impossible-by-contract cases.
- **Framework wrapping a framework / new dependency** — none; xUnit already present, no provider library introduced.
- **ADR-0002** correctly adds *zero* machinery — documents the adaptive contract rather than building determinism scaffolding. A genuine simplicity win.
- **ADR-0003** is minimal governance: one root `Directory.Build.props` hoisting three properties, with the duplicate-into-Tests alternative rejected on SSoT grounds. Not over-config.

### Intentional simplicity wins (explicitly NOT flagged as under-engineering)

Per review instructions and confirmed against the artefacts: IMP-07 kept as in-code overridable seed defaults (no Def surface); ADR-0002 keeps adaptive behavior rather than adding determinism machinery; LangVersion pin rejected. All three are correct KISS calls and are not raised.

## Findings

| # | Severity | Location | Description | Suggested fix | Category |
|---|---|---|---|---|---|
| 1 | low | prd.html AC-15 / AC-16 (Cluster F) | AC-16's snapshot target (`StatRanges.Ranges`) is a strict subset of AC-15, which already enumerates `StatRanges` in the static save/restore set. The two ACs partly restate the same requirement, splitting one obligation across two criteria. | Optional: fold AC-16's intent into AC-15 (or keep AC-16 only as the cross-ref note to ADR-0002 that explains *why* `StatRanges` is in the set), so the snapshot set has a single home. Not a structural over-engineering issue — AC-level redundancy only. | simplify |
| 2 | low | adr.html ADR-0001 reroute list vs prd.html AC-15/AC-20 | ADR-0001's `DefProvider.Current` reroute list names `StatHelper, WorkTypeStatMap, WorkTypeThingRule, DefCache, StatWeight, PassionHelper`, but the snapshot set (and AC-15/AC-20) additionally name `SkillStatMap`. Minor consistency gap, not an added abstraction — `SkillStatMap` is either a snapshot-only static or an omitted reroute consumer. | Reconcile the two lists so the seam's consumer set and the snapshot set are stated once and agree (clarify whether `SkillStatMap` reads defs through the seam or is snapshot-only). Consistency fix; no new complexity. | keep-as-is |
| 3 | low | prd.html AC-13, AC-14 | AC-13 ("seam exists") and AC-14 ("save/restore exists") are two near-adjacent existence assertions for the one ADR-0001 mechanism. Defensible as separately testable, but borders on splitting one deliverable into ceremony. | Acceptable as-is (each is independently verifiable). Noted only for the iter-1 all-severities floor; no change required. | keep-as-is |

<!-- No critical/high/medium over-engineering findings. -->

## Verdict
APPROVE

The proposed design is the simplest that satisfies the audit-remediation scope. The single non-trivial abstraction (`IDefProvider`) is proportionate, has a real second implementer and 6 consumers, is the minimal seam fitting the existing static design, and was already user-approved under Complication Approval. No critical/high/medium over-engineering smell is present. The three low-severity findings are AC-level redundancy / cross-doc consistency notes, all autofixable by the BA/Architect without escalation; none introduces or removes an abstraction.

## Next action
APPROVE — no over-engineering blocker. The three low findings are optional polish the BA/Architect may fold in (or defer past the medium floor at iteration 2). No user escalation required; no Complication Approval owed (the one already granted covers `IDefProvider` + `InternalsVisibleTo`).

## Escalations (optional)
- None. No finding requires user approval. The only complication in the design (`IDefProvider` abstraction + `InternalsVisibleTo` grant) was already approved under Complication Approval on 2026-06-03 and is in scope.
