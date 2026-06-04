---
phase: impl-review
iteration: 01
reviewer: performance
verdict: APPROVE
sprint: 002-migrate-tests-nunit-fluent
---

[REVIEW-impl-performance]: APPROVE

## Summary

Performance review of the NUnit + FluentAssertions test migration. The change set is
test-infrastructure only; no production code is edited this sprint (the only production
type gaining coverage, `StatLimit`, receives new tests but no source edits). No production
hot path is altered, so production performance budgets are not at risk.

Note on budget enforcement: `custom-coding-rules.md` defines no quantitative performance
budgets (no latency / memory / throughput numbers, no regression tolerance). Per the
operating contract, there are no numeric budgets to enforce. This review therefore assesses
test-suite execution cost and anti-pattern regressions on the changed test infrastructure
and `scripts/coverage.ps1` on a best-effort basis.

## Findings

### Per-test static-state snapshot/restore cost — acceptable (informational)
`StaticStateTestBase` now runs `[SetUp]`/`[TearDown]` per test instead of per class
(`StaticStateTestBase.cs:21-33`). Every `[TearDown]` invokes `StaticStateFixture.Dispose`
(`StaticStateFixture.cs:37-72`), which runs:
- `StatHelper.Rebuild()` + `WorkTypeStatMap.Rebuild()` — both iterate the swapped
  `DefProvider` collections, which are the tiny `FakeDefProvider` sets used in tests
  (a handful of defs), so the rebuild is cheap;
- a fixed, small number of one-shot reflection lookups (`GetField` on `SkillStatMap`,
  `PassionHelper`, `StatRanges`) — constant per test, not data-scaled.

The snapshot side (`StaticStateFixture` ctor, `StaticStateFixture.cs:27-31`) only reads one
static reference. Across the 5 inheriting classes (`StatWeightTests`, `StatRangesTests`,
`StatefulSubsystemTests`, `StatLimitTests`, `StatStatRanges`-family) the multiplier is the
per-test count, not a quadratic factor. The verified full-suite wall time is ~250 ms, which
confirms the reflection-based restore is not a meaningful cost. No action needed; the
per-test granularity is the correct isolation trade-off and the extra cost is negligible.

### coverage.ps1 — removal of post-instrument RimWorld-DLL deletion — no measurable cost
The script no longer deletes the RimWorld assemblies from the test bin after instrumentation;
they must remain present because NUnit 4.x calls `Assembly.GetTypes()` /
`GetCustomAttributes(true)` at fixture discovery and needs the dependency chain resolvable
from bin (`scripts/coverage.ps1:35-40, 52-55`). Keeping the DLLs in place avoids a
copy-delete-recopy cycle; the only residual cost is disk footprint in the bin directory
during the coverage run, which is not a runtime or suite-execution concern. The restore step
(`coverage.ps1:61-63`) swaps back only the un-instrumented `LordKuper.Common.dll` — a single
file copy, constant cost. No performance regression.

### No anti-pattern regressions in test infra
No n+1 query patterns, no unbounded allocations, no copy-on-large-collection, no deep clones,
and no serialize/parse roundtrips were introduced. Reflection in `StaticStateFixture` operates
on a fixed set of fields with no loops over user-/data-sized collections. The fake-provider
def sets in `StatefulSubsystemTests` are small literals.

### Production performance budgets — not at risk (confirmed)
No production source files are in scope this sprint. Production hot paths are unchanged.

## Verdict

APPROVE — no quantitative perf budgets defined to enforce; the test-infra changes introduce
no execution-cost regression (verified ~250 ms full suite) and no anti-patterns. Production
performance is untouched.

REVIEW_DONE
