[REVIEW-impl-external]: CONCERNS

# External Review Report

- **Phase**: impl-review
- **Iteration**: 1
- **Severity floor (this iter)**: low (all)
- **External tool**: Codex CLI 0.136.0 (`codex exec -`, prompt via stdin)
- **Diff payload**: `git diff main...HEAD` scoped to `Source/` and `Tests/` (6520 lines, 315 KB)

Codex returned `FAIL: 3`. Each finding was verified against current source. Two of the three were
rejected as invalid (Codex itself noted its in-sandbox verification greps were blocked by the
Windows sandbox and could not ground those two findings). One finding survives verification.

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | Tests/StaticStateFixture.cs:23 + consumers (StatefulSubsystemTests.cs:18, StatWeightTests.cs:10, StatRangesTests.cs:12) | `StaticStateFixture` is consumed via `IClassFixture<StaticStateFixture>` and each consuming ctor discards it (`_ = fixture;`). Under `IClassFixture` the fixture ctor runs once before the first fact and `Dispose()` (the save/restore + cache rebuild) runs once after the *last* fact — NOT around each test. Facts mutate `DefProvider.Current` to a fresh `FakeDefProvider` and rebuild static caches, leaking that global state to subsequent facts in the same class. The fixture's own docstring promises "saves and restores global static state around each test … order independence (AC-15/16/19)", which this wiring does not deliver. Facts that read shared state without re-installing a provider (e.g. `RimWorldTime_Constants_AreCorrect`, `PawnFilter_Copy_IsIndependent`) run against leaked state. Custom coding rule requires per-test static save/restore. | Make `StaticStateFixture` per-test: move save/restore into each consuming test class ctor + `IDisposable` (instantiate the fixture in the ctor and dispose it in the class's `Dispose`), or provide a disposable base class that test classes derive from, instead of `IClassFixture`. `IClassFixture` is once-per-class and cannot satisfy per-test isolation. |

## Dropped findings (below severity floor)

_None — floor is `low` (all severities admitted on iteration 1)._

## Dropped findings (rejected — not a real defect)

| # | Codex severity | Location | Codex claim | Reject reason |
|---|---|---|---|---|
| 1 | critical | Tests/AssemblyInitializer.cs:62 | Public `RimWorldContextCollection` implements `ICollectionFixture<AssemblyInitializerFixture>` while the fixture is internal → "breaks compilation". | Invalid. C# does not require generic type arguments of an *implemented interface* to be as accessible as the implementing class (the CS0050 inconsistent-accessibility rule applies to member signatures, not to type arguments of an implemented interface). The interface `ICollectionFixture<T>` is accessible; a public class may implement it with an internal `T`. The test project added this sprint compiles and runs (172+ tests executed for coverage), which is impossible if this were a real compile error. Codex's verification grep was blocked by the sandbox; the claim is ungrounded and contradicted by the build. |
| 2 | high | Source/Helpers/StatHelper.cs:399 | `Rebuild()` does not clear `_statDefsByName`, so stale `StatDef`s from a prior provider remain resolvable after a provider swap. | Invalid. `Rebuild()` → `InitializeUnionStats()` **reassigns** `_statDefsByName = allStatDefsSet.ToDictionary(...)` (StatHelper.cs:354) to a brand-new dictionary built from the current provider's defs; all `_default*`/`_all*` sets are likewise reassigned to fresh instances (lines 291-294, 353-363). Whole-dictionary reassignment is equivalent to clear+repopulate — no stale entries survive. Codex flagged it could not verify (sandbox-blocked grep); verification refutes the premise. Behavior-preservation of the static-ctor→`Rebuild()` extraction holds for the name lookup. |

## Verdict
CONCERNS: 1

Codex's raw `FAIL: 3` is downgraded to `CONCERNS: 1` after adjudication:
- 2 of 3 Codex findings (1 critical, 1 high) rejected as invalid on verification against current source.
- The 1 surviving finding is high-severity but autofixable by the responsible dev without any escalation
  trigger (no change to concept / PRD / API contract, no new abstraction, no scope expansion). Per
  `review-policy.md`, autofixable issues map to `CONCERNS`, not `FAIL`.

Accepted decisions were respected and not re-raised: AC-21 coverage at 38.2%, intentional Limit-test
removal, `EnumHelper` public→internal break (backward_compat=none), StatRanges adaptive (ADR-0002),
IMP-07 weights in code. Codex did not surface defects in the priority focus areas other than the test
fixture: nullable-flow fixes, `IDefProvider`/`DefProvider.Rebuild` behavior preservation, `PawnFilter.Combine`
semantics, `DefProvider.Current` default, and the `RimWorldTime` `F.1`→`F1` format fix were reviewed and
cleared.

## Next action
impl-review routes the sprint back to `impl` (fix mode). The responsible dev converts
`StaticStateFixture` from `IClassFixture` to per-test save/restore (per-test ctor + `IDisposable`, or a
disposable base class) so global static state is restored around each `[Fact]`, then the sprint
re-enters impl-review. No user escalation required.
