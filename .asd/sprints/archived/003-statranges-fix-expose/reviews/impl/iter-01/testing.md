---
responsibility:
  owns: single reviewer verdict (testing) for iteration 1
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator (impl fixes), impl-review phase (next iteration)
---

[REVIEW-impl-testing]: APPROVE

# Review — Testing

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE — all acceptance criteria meaningfully covered by revert-sensitive tests; edge cases on changed code are present; test isolation is deterministic and sound.

## Next action

Proceed to impl-review iteration 1 with remaining reviewers (implementation, quality, documentation, performance, simplification, external review).

## Detailed rationale

### Test coverage & AC traceability

**AC-1 & AC-2 (first-observation fix, exact-bound normalization):**
- `NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange` (StatRangesTests.cs:126–149): directly locks the contract by asserting `NormalizeStatValue(stat, 50) == 0` on first observation (degenerate range [50, 50]), then expanding to [50, 100] and verifying `NormalizeStatValue(stat, 100) == 1`. This test **fails if the fix is reverted** to the old `range = new FloatRange(0, value)` behavior, because the initial call would return 0.5 (buggy [0, 50] normalized) instead of 0.
- `NormalizeStatValue_NegativeSequence_RangeExpansion` (StatRangesTests.cs:152–187): covers AC-2's exact bound sequence -10, -5, 1 with mixed-sign edge case. Verifies: first observation -10 → [−10, −10] → normalizes to 0; second -5 → [−10, −5] → normalizes to 0 (both-negative case per MathHelper line 28); third 1 → [−10, 1] → normalizes to 1 (mixed-sign case per line 29). Also verifies -10 in the final range maps to -1. The comment at line 174–177 explicitly traces the MathHelper formula for mixed-sign ranges, confirming the implementation understanding. **This test fails if `UpdateStatRange` doesn't seed the initial range to [value, value]**, because -10 would become [0, -10] (invalid min/max order) or [0, 0] (if the stale default was used).
- `NormalizeStatValue_PositiveSequence_ExactBounds` (StatRangesTests.cs:190–218): complements the first test with an independent positive sequence (50 → 100) and verifies stable re-observation (`50 → 50` stays 0 after range expands to [50, 100]). Revert-sensitive: initial call would yield 0.5 instead of 0 under the bug.

**AC-3, AC-4 (public visibility):**
- Tests call `StatRanges.NormalizeStatValue(statDef, value)` and `StatRanges.Clear()` as public members without casting or reflection, implicitly verifying public access. The fact that all nine tests in StatRangesTests.cs compile and pass confirms the type and members are public.

**AC-5 (Clear() and test isolation):**
- `StaticStateTestBase` (line 108) calls `StatRanges.Clear();` directly, replacing prior reflection. All StatRangesTests inherit from StaticStateTestBase with `[NonParallelizable]`, so `[SetUp]/[TearDown]` is called per test. This is verified by the six pre-existing tests (lines 14–123) which maintain the `!IsNaN && !IsInfinity` assertions and pass, implying the Clear() reset is working (tests would fail if ranges accumulated across tests). The three new tests each start with an independent range and pass, further confirming isolation.

**AC-6 (remain static, process-global):**
- StatRanges.cs line 24 remains `public static class StatRanges`. No instance members added. The four-line UpdateStatRange rewrite (lines 80–85) keeps the static `Ranges` dictionary unchanged in semantics (only the initialization bug is fixed). WorkTypeThingRule consumer continues using the shared cache as intended.

**AC-7 & AC-8 (exact bounds, revert sensitivity):**
- Tests include exact-value assertions (`.Should().Be(0f)`, `.Should().Be(1f)`, `.Should().Be(-1f)`) rather than range checks. These are revert-sensitive: the old [0, v] behavior would produce different normalized values (0.5 vs 0 for the first observation case, and wrong mixed-sign results). The regression test is explicitly named to signal revert-sensitivity.

### Edge cases on changed code

**UpdateStatRange rewrite (StatRanges.cs lines 80–85):**
1. **First positive observation** (v=50): seeded as [50, 50], both comparisons run against the seeded value. ✓ Tested by NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange.
2. **First negative observation** (v=−10): seeded as [−10, −10], comparisons preserve sign. ✓ Tested by NormalizeStatValue_NegativeSequence_RangeExpansion.
3. **First zero** (v=0): seeded as [0, 0]. ✓ Existing test NormalizeStatValue_ZeroValue (line 112) covers this.
4. **Degenerate range (second call with value inside [min, max])**. ✓ NormalizeStatValue_PositiveSequence_ExactBounds verifies 50 stays at 0 after range expands to [50, 100].
5. **Range expansion below min**: second call with -5 after observing -10. ✓ NormalizeStatValue_NegativeSequence_RangeExpansion line 167–172.
6. **Range expansion above max**: second call with 100 after observing 50. ✓ NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange line 144–148 and NormalizeStatValue_PositiveSequence_ExactBounds line 207–208.
7. **Multiple independent stats**: each maintains separate ranges. ✓ NormalizeStatValue_MultipleStats_IndependentRanges (line 49) already tests this.

### Determinism & flaky patterns

- **No sleep/timing patterns**: all tests use synchronous FakeDefProvider and StatHelper.Rebuild(); no threading or time-based assertions.
- **Static state isolation**: StaticStateTestBase saves/restores global state via [SetUp]/[TearDown]. StatRanges.Clear() is called in TearDown (line 108), zeroing the cache. [NonParallelizable] on all test classes prevents parallel execution. Sound design; no isolation leaks observed.
- **No network or I/O**: all assertions use in-memory FakeDefProvider and hand-built StatDef objects. Deterministic.

### jb-warning cleanups verification

The following test files had jb-warning removals:
- **StatWeightTests.cs**: removed redundant `Ctor_Parameterless_InitializesEmpty()` argument label; assertions remain unchanged. Spot-check confirms the `.Should()` chain at line 17–20 still tests all four properties.
- **StatLimitTests.cs**: assertions in capping tests (e.g., line 77–79 `MaxValue.Should().BeApproximately(500f, 0.001f)`) are intact.
- **StatefulSubsystemTests.cs**: test logic at line 41–50 (PawnFilter combine test) unchanged; the [Test] method structure is preserved.
- **RimWorldResolverSetup.cs**: one-time setup logic at line 42–59 unchanged; only stringly-typed field references in other files were cleaned of redundant syntax.

No assertion intent was weakened. Null-forgiving operators were removed only where no longer needed (e.g., post-cleanup of non-null checks in test setup).

### Stub resolution verification

The plan (plan.md line 62–95) identifies Task 1–Task 4. No stub entries from `.asd/project/stubs.md` are referenced in the diff, and no sprint-specific TODO markers would be present (this is implementation, not a feature stub). No relevant stubs to verify.

## Summary

The three new tests (NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange, NormalizeStatValue_NegativeSequence_RangeExpansion, NormalizeStatValue_PositiveSequence_ExactBounds) directly assert the corrected [v, v] degenerate-range behavior with exact value checks, making them revert-sensitive to the old [0, v] bug. Edge cases (first positive, first negative, zero, degenerate, expansion below/above) are covered across the new tests and pre-existing tests. Test isolation via StatRanges.Clear() in StaticStateTestBase is deterministic and sound. No flaky patterns, no weakened assertions from jb-warning cleanup. All ACs 1–10 are addressed with meaningful test coverage.
