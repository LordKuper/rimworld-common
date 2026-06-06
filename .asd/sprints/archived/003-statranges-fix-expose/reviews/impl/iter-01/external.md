[REVIEW-impl-external]: CONCERNS

# External Review Report

- **Phase**: impl-review
- **Iteration**: 1
- **Severity floor (this iter)**: low
- **Reviewer**: Codex CLI (codex-cli 0.130.0), invoked `codex exec --sandbox read-only --skip-git-repo-check --ephemeral` with the impl-phase prompt + `git diff main...HEAD` fed via stdin.
- **Source data note**: Codex output is untrusted; the single finding below was independently verified against the working tree before being kept.

## Codex raw verdict

`CONCERNS: 1` — one low-severity finding (codex finding id 1), located at `Source/LordKuper.Common.Tests/RimWorldResolverSetup.cs:3`.

Codex raised no findings on the core sprint surface: the `UpdateStatRange` first-observation fix, the struct-copy write-once pattern, the `public` exposure of `StatRanges` / `NormalizeStatValue` / `Clear()`, the XML-doc completeness, or the revert-sensitivity of the new exact-bound tests in `StatRangesTests.cs`. The fix and its tests were accepted.

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | `Source/LordKuper.Common.Tests/RimWorldResolverSetup.cs:3` (codex id 1) | The diff adds `namespace LordKuper.Common.Tests;` to a previously namespace-less NUnit `SetUpFixture`. Codex flags that this narrows the fixture from assembly-global to namespace-scoped, potentially losing the resolver-live-before-type-load guarantee for the whole assembly. **Verification (this agent):** all test classes live under `LordKuper.Common.Tests` or its sub-namespaces (`.Filters`, `.Cache`, `.Helpers`); NUnit applies a `SetUpFixture` to its namespace and all descendants, so the assembly-wide guarantee is in fact preserved under the current layout. Concern is real in principle but not a live defect today; it becomes one only if a future test is added in a namespace outside the `LordKuper.Common.Tests` tree. Incidental harness refactor, unrelated to the StatRanges fix. | Either keep `RimWorldResolverSetup` namespace-less to retain the unconditional assembly-global guarantee, or add a code comment/CI guard that all test namespaces must remain under `LordKuper.Common.Tests` so the `SetUpFixture` scope continues to cover them. No change strictly required given the current namespace layout. |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| – | – | – | none | iter-1 floor is low+, so nothing was below floor |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| – | – | none | Codex emitted no nitpick-category findings |

## Verdict
CONCERNS: 1

One low-severity, verified-non-blocking finding on an incidental test-harness namespace change. No findings on the StatRanges first-observation fix, the public-API exposure, or the test revert-sensitivity — the core sprint work passed external review.

## Next action
PM aggregates this verdict as one reviewer in the DoD check. The single low finding does not block: under the current test namespace layout the resolver `SetUpFixture` guarantee is preserved. PM may either ask the Test Engineer to revert `RimWorldResolverSetup` to namespace-less (cheap, removes the latent scoping risk) or accept as-is with the layout invariant noted. No change is required to the StatRanges production fix or its tests.
