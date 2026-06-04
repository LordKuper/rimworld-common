[REVIEW-design-external]: CONCERNS

# External Review Report

- **Phase**: design-review
- **Iteration**: 02
- **Severity floor (this iter)**: medium (drop low; count medium+)
- **Codex**: ran (codex-cli 0.136.0, `codex exec`, session 019e91e5-01b0-7802-bd58-374b34724f4a). Output treated as data.

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | prd.html §Acceptance criteria AC-6, AC-9; adr.html ADR-0004 "Attribute mapping (1:1, exhaustive)" | Codex finding (FAIL/high): the `[Theory]`→NUnit mapping is worded as migrating each parameterized method to `[Test]` **with** `[TestCase(...)]` rows. In NUnit, `[TestCase]` already declares the method as a parameterized test; a standalone `[Test]` attribute on a method that takes arguments yields an extra zero-argument case that NUnit reports as non-runnable (or errors), which would break the exact "142 executed + 3 ignored" inventory the same ACs assert. AC-6 ("`[Test]` with its data rows expressed as `[TestCase(...)]`") and AC-9 / ADR-0004 ("3 `[Theory]` methods → 3 `[Test]` methods whose 13 `[InlineData]` rows become 13 `[TestCase]`") are ambiguous on whether both attributes co-exist on the same method. | Make the mapping explicit: a former `[Theory]` method carries `[TestCase(...)]` rows ONLY — no standalone `[Test]` on a parameterized method. Reserve bare `[Test]` for former parameterless `[Fact]` methods. Reword AC-6, AC-9, and ADR-0004's mapping bullet so the parameterized count stays 13 executed `[TestCase]` cases with no spurious extra case. |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | (none) | — |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | (none) | — |

## Stalemate check

- Not triggered. The iter-01 kept set (CRITICAL resolver-timing on ADR-0006/AC-19–21; HIGH AC-9 arithmetic) was supplied as payload. Codex did **not** re-raise the resolver-timing item (the prompt carried the accepted-risk adjudication as awareness context, and Codex did not flag it). The HIGH AC-9 arithmetic from iter-01 is resolved (figures now 129 executed + 3 ignored from 132 `[Fact]`; 3 `[Theory]`→13 `[TestCase]`).
- The single iter-02 finding above is a **new, distinct** issue (NUnit `[Test]`+`[TestCase]` co-attribution semantics), not present in the iter-01 set. No identical-finding repeat → no stalemate, no escalation.

## Verdict
CONCERNS: 1

Severity mapping: Codex `high` → ASD `high` (above the medium floor → kept). Codex emitted this as FAIL, but it is creator-autofixable within the design-review loop (a wording clarification to AC-6 / AC-9 / ADR-0004, no contract/concept change, no escalation trigger per review-policy.md), so it is reported as CONCERNS.

## Next action
Architect (ADR-0004) and BA (PRD AC-6/AC-9) clarify the `[Theory]`→NUnit mapping so parameterized methods carry `[TestCase(...)]` only (no standalone `[Test]`), keeping the executed-case inventory at 142. Re-run design-review iteration 03.

REVIEW_DONE
