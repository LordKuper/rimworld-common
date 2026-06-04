[REVIEW-impl-external]: CONCERNS

# External Review Report

- **Phase**: impl-review
- **Iteration**: 1
- **Severity floor (this iter)**: low
- **External tool**: Codex CLI 0.136.0 (`codex exec`, stdin prompt + `git diff main...HEAD` payload). The `codex review --base main` path could not be used because this build forbids combining `--base` with a custom prompt; fell back to `codex exec` per the rule's documented fallback. Codex's own sandbox blocked some of its helper file reads but it produced a complete verdict from the supplied diff. Findings below were independently re-verified against the source by this agent before mapping.

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | `Source/LordKuper.Common.Tests/StatLimitTests.cs` (buffer cases: L164 `"12.50"`, L185 expect `"75.00"`, L205 `"-50.25"`, and the Min/Max getter cases) | Buffer round-trip tests hard-code dot-decimal literals as parse inputs and as expected `F2`-formatted outputs. If the production `StatLimit` buffer parse/format uses the current culture, these assertions are culture-dependent and would fail on a comma-decimal locale (e.g. `ru-RU`). The test class is not pinned to a culture, so green-on-dev does not guarantee green on a differently-localized CI agent. Codex finding (id: codex-1, source: `codex exec`). | Pin culture on the affected fixture/tests via NUnit `[SetCulture("en-US")]`, or build the literal inputs and expected strings from `CultureInfo.InvariantCulture`/`CurrentCulture` so the round-trip matches whatever the production code uses. |
| 2 | low | `Source/LordKuper.Common.Tests/Helpers/MathHelperTests.cs:105` (`NormalizeValue_Theory`) | The xUnit→FA migration of `Assert.Equal(expected, result, 4)` to `Should().BeApproximately(expected, 1e-4f)` is not a strict semantic equivalent: xUnit precision-4 rounds both operands to 4 decimal places (~5e-5 effective band) whereas `1e-4` is a wider absolute band. No behavioral divergence on the current inputs (0, 0.5, 1 are exact), and the delta is documented at L94/L104 as the AC-12 semantic-shift site. Codex finding (id: codex-2, source: `codex exec`). | If strict equivalence is desired, tighten the tolerance to `0.00005f` or compare values rounded to 4 decimals. Otherwise confirm the documented AC-12 shift is acceptable as-is. |

## Dropped findings (below severity floor)

None — floor is `low` on iteration 1; all findings kept.

## Dropped findings (nitpick)

None raised by Codex.

## Known / accepted (not counted as new)

- ADR-0006 resolver-registration vs NUnit discovery-time type-load ordering: Codex did not raise it. Would be reported as known-accepted if raised.
- StatLimit parameterless/string-ctor infinite-recursion: Codex did not raise it; tests deliberately use `new StatLimit(StatDef)`. Tracked separately, out of scope.

## Verdict
CONCERNS: 2

## Next action
impl-review routes the sprint back to `impl` (fix mode). The responsible dev resolves the two findings (or, for finding 2, records explicit acceptance of the documented AC-12 tolerance shift), then the sprint re-enters impl-review. Neither finding requires user escalation: both are dev-autofixable and do not touch approved concept, PRD contract, or introduce new abstraction.
