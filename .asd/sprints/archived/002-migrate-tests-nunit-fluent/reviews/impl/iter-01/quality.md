[REVIEW-impl-quality]: CONCERNS

# Review — quality

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | `RimWorldResolverSetup.cs:20-22,26-27`; `StaticStateFixture.cs` (`<remarks>` indirectly) | Self-contained-code rule (custom-coding-rules "no design-doc references") is violated: comments cite `ADR-0006` by name ("ADR-0006 fallback", "This is the ADR-0006 contingency path"). Code/comments must not reference ASD artifacts; explain the *why* directly. | Replace "ADR-0006 fallback/contingency" with the plain rationale, e.g. "Fallback: register at module-load time because [OneTimeSetUp] runs at execution time, not discovery time, and cannot provably precede discovery-time type loading." Drop the ADR id. |
| 2 | medium | `RimWorldResolverSetup.cs:62-65` | Semantic faithfulness / correctness of resolver seam: env-var lookup falls back to a hard-coded machine-specific absolute path `D:\Games\Steam\steamapps\common\RimWorld`. On any machine/CI without `RIMWORLD_DIR`/`RimWorldDir` this silently resolves to a non-existent dir, `File.Exists` fails, handler returns null, and RimWorld-typed test discovery fails opaquely (NotRunnable / 0 tests) rather than with an actionable error. The build/coverage path already requires `RIMWORLD_DIR`. | Drop the hard-coded fallback and, when neither env var is set, throw/log an actionable message ("Set RIMWORLD_DIR to the RimWorld install dir"), or at minimum surface the missing-dir condition instead of returning null silently. |
| 3 | low | `MathHelperTests.cs:104-105` | Semantic shift on precision conversion (AC-12 site) is faithful only by coincidence. xUnit `Assert.Equal(exp, act, 4)` rounds both operands to 4 decimal places before comparing; `BeApproximately(expected, 1e-4f)` is an absolute `\|a-b\| <= 1e-4` band. These are not equivalent at rounding boundaries. All current `[TestCase]` expectations (0, 0.5, 1) are exact, so the tests pass under either rule and the divergence is latent. | Acceptable as-is (values are exact). Keep the documenting comment. If future cases add non-exact expecteds, re-verify the tolerance reproduces the round-to-N-places intent. No change required this sprint. |
| 4 | low | `StatRangesTests.cs:28,44-45,69-70,87-89,106-108,122` | Weak assertions: six adaptive-normalization tests only assert `(!IsNaN && !IsInfinity).Should().BeTrue()`, so they verify the call does not produce NaN/Inf but not that normalization is correct (any finite garbage passes). Comments state this preserves the original xUnit compound-boolean form, so the migration is faithful; the weakness pre-dates this sprint. | Faithful migration — no fix required for the migration goal. Flag for a future sprint to assert concrete normalized values (e.g. first-value behavior, range expansion results) rather than only finiteness. |
| 5 | low | `StatLimitTests.cs:8-13` (class doc) | Class XML-doc narrates an internal implementation detail of the production type ("calls `Configure(def)` directly... sets `_isConfigured = true`... avoids the recursive initialisation cycle"). This is the latent StatLimit infinite-recursion bug, already logged as a separate out-of-scope task — KNOWN/DEFERRED, not a new finding. Noted only because the test doc encodes the workaround. | No action this sprint. When the parameterless/string-ctor recursion is fixed in its own task, revisit this doc and the `StatDef`-only ctor reliance. |

## Verdict
CONCERNS: 2

## Next action
Route sprint back to `impl` (fix mode). Responsible dev resolves findings #1 and #2 (both medium, autofixable without escalation): strip the `ADR-0006` references from comments to satisfy the self-contained-code rule, and replace the hard-coded RimWorld path fallback with an actionable failure. Findings #3–#5 are low/informational (faithful-migration or known-deferred) and require no code change this sprint. Re-enter impl-review.

## Escalations (optional)
- None. No finding requires user approval (no concept/PRD/contract change, no new abstraction, no scope expansion).
