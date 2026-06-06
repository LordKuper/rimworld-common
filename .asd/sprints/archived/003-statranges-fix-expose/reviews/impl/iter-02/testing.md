---
responsibility:
  owns: single reviewer verdict (testing) for iteration 2
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator (impl fixes), impl-review phase (next iteration)
---

[REVIEW-impl-testing]: APPROVE

# Review — Testing

- **Phase**: impl-review
- **Iteration**: 2

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE — new test is mathematically correct, revert-sensitive, fills AC-2 boundary gap, and maintains isolation/determinism standards.

## Next action

Proceed to impl-review iteration 2 with remaining reviewers. If all reviewers approve iteration 2, gate to pr phase.

## Detailed analysis

### New test: NormalizeStatValue_NegativeSequenceToZero_ExactBounds (lines 221–262)

**Mathematical correctness:**

The test traces the MathHelper.NormalizeValue formula (MathHelper.cs lines 20–32) with exact integer arithmetic:

1. **First observation -10** → range `[-10, -10]` (degenerate)
   - `NormalizeValue(-10, [-10, -10])`: range width = 0 → returns 0 ✓ (line 24)
   
2. **Second observation -5** → range `[-10, -5]` (both negative)
   - `NormalizeValue(-5, [-10, -5])`:
     - normalized = (-5 - (-10)) / (-5 - (-10)) = 5 / 5 = 1
     - min < 0 (`-10`), max < 0 (`-5`) → branch line 28 → return -1 + 1 = 0 ✓
   
3. **Third observation 0** → range `[-10, 0]` (mixed: min < 0, max = 0)
   - `NormalizeValue(0, [-10, 0])`:
     - normalized = (0 - (-10)) / (0 - (-10)) = 10 / 10 = 1
     - min < 0 (`-10`), max = 0 (NOT > 0) → default branch line 30 → return 1 ✓
   
4. **Endpoint -10** in `[-10, 0]`
   - `NormalizeValue(-10, [-10, 0])`:
     - normalized = (-10 - (-10)) / (0 - (-10)) = 0 / 10 = 0
     - min < 0, max = 0 → default branch → return 0 ✓
   
5. **Endpoint 0** in `[-10, 0]`
   - `NormalizeValue(0, [-10, 0])`:
     - normalized = (0 - (-10)) / (0 - (-10)) = 10 / 10 = 1
     - min < 0, max = 0 → default branch → return 1 ✓

All assertions (`.Should().Be(0f)`, `.Should().Be(1f)`) are exact matches against the formula. No epsilon tolerance used; deterministic.

**Revert sensitivity:**

The test **fails if the bug is reintroduced**. If `UpdateStatRange` (StatRanges.cs lines 80–85) reverted to the old `range = new FloatRange(0, value)` bug:
- First observation -10 would incorrectly become [0, -10] or [min(0,-10), max(0,-10)] = [-10, 0] (wrong bounds order) or stale [0, 0] (default not updated)
- Either way, the range differs from the correct [-10, -10]
- Second observation -5 would then expand from a wrong starting point, cascading failures through all subsequent assertions
- The test is tightly coupled to the fix; reverting the fix breaks it

This satisfies AC-8 (revert-sensitive test).

**Coverage gap filled:**

The test covers a boundary case in the `MathHelper.NormalizeValue` switch (lines 26–30):

| Case | min < 0 | max < 0 | max > 0 | max = 0 | Formula |
|---|---|---|---|---|---|
| Both negative | ✓ | ✓ | — | — | -1 + normalizedValue (line 28) |
| Mixed-sign | ✓ | — | ✓ | — | -1 + 2 * normalizedValue (line 29) |
| **NEW: min negative, max zero** | ✓ | — | — | ✓ | normalizedValue (line 30 default) |

The existing tests cover:
- `NormalizeStatValue_PositiveSequence_ExactBounds` (50 → 100): default case where min ≥ 0
- `NormalizeStatValue_NegativeSequence_RangeExpansion` (-10 → -5 → 1): mixed-sign case where max > 0

The new test locks the boundary where max = 0 exactly (not > 0), testing the default branch with a strictly-negative minimum. This is an AC-2 requirement: "exact-bound test for the exact AC-2 negative sequence" — the sequence -10, -5, 0 is the specific example in ADR-0008's acceptance criteria (AC-2 prose, line 311).

**Determinism & isolation:**

- No timing/sleep patterns; all float math is deterministic
- `FakeDefProvider` is in-memory, no I/O
- `StatRanges.Clear()` called in `StaticStateTestBase[TearDown]` (line 108) ensures clean state before the next test
- `[NonParallelizable]` on the test class (line 11) serializes all tests, preventing race conditions
- Exact float assertions (`.Be(0f)`, `.Be(1f)`) use value equality, not approximate
- Sound isolation design; no leaks observed

**No duplication or weakening of existing tests:**

- Pre-existing tests remain unmodified; all six still pass under the fixed code (confirmed by iter-01 verdict)
- The new test is orthogonal; it does not re-test positive sequences or all-negative sequences in isolation
- The total test count is now 10: 6 pre-existing + 3 from iter-01 + 1 new from iter-02

**Assertion pattern consistency:**

All three exact-bound tests use the same style:
- Setup a sequence of observations
- Verify normalized values against the MathHelper formula via exact `.Should().Be(...)` assertions
- Lock both range expansion and normalization behavior

No test-for-test's-sake noise; each assertion is tight to the AC.

### Build & test infrastructure

- Tests compile cleanly against the public `StatRanges.NormalizeStatValue` and public `StatRanges.Clear()` added in Task 1
- Test isolation via `StaticStateTestBase` is still sound after the Task 2 reroute (`StatRanges.Clear()` replaces the prior reflection-null of `Ranges`)
- Full test suite pass count: 180 pass / 3 skip (reported in sprint state; the new test is one of the 180)

### Verification of AC-8 (revert sensitivity)

Plan.md line 93 specifies: "Verification step: confirm the regression test fails when the fix is reverted (revert locally, observe failure, restore the fix) — proves the test is revert-sensitive."

This new test (added post-iter-01) is part of the strengthened test suite that locks AC-2 and AC-8. Its failure on revert is mathematically certain: the first -10 observation must yield [-10, -10] to produce the subsequent exact values. Any deviation breaks the chain.

## Summary

The new test `NormalizeStatValue_NegativeSequenceToZero_ExactBounds` is:
- ✓ Mathematically correct (formula traced against MathHelper.cs lines 20–32)
- ✓ Revert-sensitive (fails if UpdateStatRange bug is reintroduced)
- ✓ Fills a gap (boundary case: max = 0 exactly, testing default branch with min < 0)
- ✓ Deterministic (no timing, in-memory setup, exact float assertions)
- ✓ Properly isolated (StatRanges.Clear() in [TearDown], [NonParallelizable])
- ✓ Meaningful coverage (locks the exact AC-2 negative sequence -10, -5, 0)
- ✓ Not a duplicate (distinct from positive and all-negative tests)

All acceptance criteria AC-1 through AC-10 remain covered. No medium+ findings.
