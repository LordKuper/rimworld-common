---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-simplification]: CONCERNS

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | critical | `StaticStateFixture.cs` (whole file) + `StaticStateTestBase.cs:18,24,31` | `StaticStateFixture : IDisposable` holds the entire save/restore body but has exactly one consumer: `StaticStateTestBase`, which does `new StaticStateFixture()` in `[SetUp]` and `_fixture.Dispose()` in `[TearDown]`. Two types model one responsibility with no second use case — over-engineering checklist: "Abstraction with no second use case." The `IDisposable` shape is an xUnit-era artifact (ctor/`Dispose` fixture); the NUnit-idiomatic form puts save/restore directly in `[SetUp]`/`[TearDown]`. The migration carried the wrapper forward instead of collapsing it. | Inline the `StaticStateFixture` ctor body into `StaticStateTestBase.SetUpStaticState()` and the `Dispose()` body into `TearDownStaticState()`; hold `_originalProvider` as a field on the base class; delete `StaticStateFixture.cs`. Net reduction: one type, one file, the `IDisposable` plumbing, and the nullable `_fixture` field. No behavior change (per-test save/restore granularity is identical). This is a simplification, not a complication — no escalation. |
| 2 | low | `scripts/coverage.ps1:45` | `--assemblyFilter coverlet` remains in the AltCover invocation after coverlet was removed from the project (no coverlet package reference exists in the test csproj). Dead config left behind — checklist: "Dead code left 'in case we need it'." Harmless but a migration leftover. | Drop the `--assemblyFilter coverlet` token. (Below the iteration-1 floor of `low` only by virtue of being `low`; retained because it is a concrete leftover, not a style opinion.) |

## Verdict
CONCERNS: 2

## Next action
impl-review routes the sprint back to `impl` (fix mode). Test Engineer collapses `StaticStateFixture` into `StaticStateTestBase` (finding #1) and removes the stale `coverlet` filter token (finding #2). Both fixes reduce surface area; neither needs Complication Approval. Sprint re-enters impl-review for the next iteration.

## Escalations (optional)
- None. Both findings are simplifications (surface reduction); no new abstraction, layer, dependency, or contract change. No user approval required.

## Notes (scope-bounded, not findings)
- `RimWorldResolverSetup` dual registration (`[ModuleInitializer]` + `[OneTimeSetUp]`) and its idempotency guard: settled by design rationale; the guard prevents double-registration when both paths fire in one process. Not flagged.
- `ModuleInitializerAttribute` polyfill: net472 has no built-in; compile-only, internal, invisible to callers. Justified, not flagged.
- `[NonParallelizable]` on the four static-touching fixtures (`StatefulSubsystemTests`, `StatRangesTests`, `StatWeightTests`, `StatLimitTests`): explicit serialization intent per the static-isolation rule; `RimWorldTimeTests`/`TimedCacheTests`/helper tests correctly omit it (no static state touched). Not redundant — not flagged.
- `FakeDefProvider` fluent `AddDef`/`SetWorkTypeDefsInPriorityOrder`: a hand-built fake, not a mock-of-a-mock; each method carries real indexing/storage logic. Not a thin wrapper. Not flagged.
- Self-contained-code violations (in-code references to `AC-6/AC-7`, `AC-12`, `ADR-0006` in test/setup comments) are real but belong to the Documentation/coding-rules reviewer, not over-engineering. Out of this reviewer's scope.

REVIEW_DONE
