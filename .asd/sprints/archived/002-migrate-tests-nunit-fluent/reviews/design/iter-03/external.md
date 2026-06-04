[REVIEW-design-external]: CONCERNS

# External Review Report

- **Phase**: design-review
- **Iteration**: 3
- **Severity floor (this iter)**: high (drop low + medium; count high + critical)
- **External tool**: Codex CLI (codex-cli 0.136.0, `codex exec`, model gpt-5.5) — ran successfully
- **Source mapping**: Codex `severity=high` → ASD `high`

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | prd.html · §Acceptance criteria · AC-28 | AC-28 (priority `must`) is not objectively verifiable: it requires the AltCover "Visited Points N of M" figure to stay "within the expected band," but defines no numeric pre-migration baseline, tolerance, or pass/fail threshold. As written the criterion cannot be deterministically passed or failed, violating the atomic/unambiguous AC requirement. | State the pre-migration baseline (e.g. recorded `N of M`) and an explicit acceptable delta — exact N/M equality, or a defined percentage / visited-points tolerance allowed after the NUnit migration. Cross-reference the recorded audit figure if one exists. |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | (none reported) | — |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | (none reported) | — |

## Stalemate check

No stalemate. The single kept finding (AC-28 coverage-band testability) is **new** — it does not appear in the supplied prior finding set:
- iter-01 CRITICAL resolver-timing (ADR-0006): user-accepted risk with `[ModuleInitializer]` fallback — NOT re-raised by Codex this iteration.
- iter-01 HIGH AC-9 arithmetic: resolved — not re-raised.
- iter-02 HIGH `[Theory]`→NUnit mapping ambiguity: resolved (AC-6/AC-9 + ADR-0004 now state `[TestCase(...)]`-only, 142 executed + 3 ignored) — not re-raised.

No prior finding recurred; no identical-set repetition across two iterations.

## Verdict
CONCERNS: 1

## Next action
PM/BA to revise PRD AC-28 to make coverage equivalence objectively verifiable: pin the pre-migration AltCover baseline (`Visited Points N of M`) and an explicit tolerance/threshold (exact equality or a stated delta). Re-run external review on the next iteration once AC-28 carries a measurable acceptance condition.
