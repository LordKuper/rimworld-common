---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-simplification]: CONCERNS

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 02

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | StatRangesTests.cs:9 | Doc comment references `StaticStateFixture`, a type that no longer exists (deleted; logic inlined into `StaticStateTestBase`). Rotted comment pointing at dead code — violates code-style rule 7 (no comment referencing removed code). Category: **simplify**. | Replace `StaticStateFixture` with `StaticStateTestBase` (the actual base now cleared between tests), or drop the sentence — the `[NonParallelizable]` + base class already convey isolation. |
| 2 | medium | StatefulSubsystemTests.cs:13 | Class-summary comment says fixtures "require FakeDefProvider and StaticStateFixture"; `StaticStateFixture` is deleted. Stale reference to removed type. Category: **simplify**. | Change `StaticStateFixture` to `StaticStateTestBase`. |
| 3 | medium | StatefulSubsystemTests.cs:151 | Inline comment "Same reflection pattern as StaticStateFixture" points at a deleted type. Category: **simplify**. | Point to `StaticStateTestBase.TearDownStaticState` (where the reflection-reset pattern now lives) or drop the cross-reference. |

## Verdict
CONCERNS: 3

## Next action
Route back to `impl` (fix mode): the responsible dev updates the three doc/inline comments to reference `StaticStateTestBase` instead of the deleted `StaticStateFixture`, then re-enter impl-review. No escalation needed — pure comment-rot cleanup, no abstraction added or removed.

## Notes (verified clean — no findings)
- `StaticStateFixture.cs` deleted; duplication collapsed into single-level `StaticStateTestBase` (inheritance depth 1, real polymorphic SetUp/TearDown dispatch across 2+ fixtures — not an over-engineering hit).
- Resolver is `[SetUpFixture]`/`[OneTimeSetUp]`-only with fail-fast on missing `RIMWORLD_DIR`; no ModuleInitializer, no polyfill, no idempotency guard. Single registration path, no needless layering.
- No `XunitExtensions` tombstone; no stale coverlet token (coverage.ps1 is AltCover-only with a justified why-comment).
- `FakeDefProvider : IDefProvider` is a legitimate test double of a production interface, not a mock-of-mock and not a single-implementer interface introduced by the tests.
- No dead code "in case", no premature config flags, no factories, no framework-wrapping-framework. Over-engineering checklist otherwise clean.
