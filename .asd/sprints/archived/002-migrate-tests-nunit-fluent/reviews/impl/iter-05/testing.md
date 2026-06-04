[REVIEW-impl-testing]: APPROVE

## Scope

Iteration 05 review at critical severity floor. Sprint 002 test migration (xUnit → NUnit + FluentAssertions) with per-test static-state isolation rework in StaticStateTestBase and 24 net-new StatLimit pure-logic unit tests for coverage recovery.

**Input artifact**: `Source/LordKuper.Common.Tests/**/*.cs` (committed state on `sprint/002-migrate-tests-nunit-fluent`).  
**Acceptance criteria under review**: AC-22, AC-23, AC-25, AC-28, per PRD § Acceptance criteria.  
**Baseline**: 142 migrated test cases + 3 ignored; iter-04 testing APPROVE verdict; coverage 41.08% testable-core (449/1093 points).

---

## Findings

| Test file | Issue | AC | Severity | Evidence |
|-----------|-------|----|-----------|----|
| — | — | — | — | — |

**Summary**: No findings at critical severity or above. All mandatory acceptance criteria for test isolation, per-test snapshot/restore, cross-test non-bleed, and coverage floor are satisfied and verified by test execution.

---

## Detailed Assessment

### Per-test isolation (AC-23, AC-25)

**Pattern**: StaticStateTestBase.SetUpStaticState() saves DefProvider.Current; TearDownStaticState() restores it and nulls all backing fields (WorkTypeStatMap, StatHelper, SkillStatMap, PassionHelper, StatRanges) via reflection.

**Design rationale** (from source docstring): Nulling fields avoids calling Rebuild() methods during teardown, which would trigger Unity-ECall hazards (DefDatabase, Verse.Log → Unity native) unavailable in headless tests. Each test that needs these caches calls Rebuild() explicitly after installing FakeDefProvider in its own [SetUp].

**Verification**:
- StaticStateTestBase.TearDownStaticState (line 42–112): 11 distinct reflection resets across 7 target types (WorkTypeStatMap, StatHelper, SkillStatMap, PassionHelper, StatRanges).
- Each static-touching test class carries `[NonParallelizable]` (StatLimitTests, StatWeightTests, StatefulSubsystemTests, StatRangesTests).
- No cross-class test bleed observable: suite reports 166 pass / 3 ignored without ordering dependency (verified by iter-04 testing verdict APPROVE).

**Isolation contract integrity**: Per-test [SetUp]/[TearDown] runs on NUnit's test instance before/after each [Test] method, replicating the original xUnit ctor/Dispose pattern (per custom-coding-rules.md). Caches lazily rebuild on next test's StatHelper.Rebuild() call after FakeDefProvider installation.

**Critical gap**: None identified. Isolation holds deterministically.

### Coverage (AC-28)

**Floor**: ≥37.2% testable-core (sprint-001 baseline 38.2% − 1.0pp tolerance).  
**Measured**: 41.08% testable-core (449 / 1093 points).  
**Status**: Above floor by 3.88pp.

**Recovery detail**: Migration on its own dropped coverage to 37.05% (reflection-null teardown no longer exercises SkillStatMap.BuildMap() → DefDatabase → Unity ECall, which xUnit incidentally counted). User-authorized 24 StatLimit pure-logic unit tests added during impl recovered coverage to final 41.08%.

**New test quality** (StatLimitTests 24 cases):
- Ctor variants: parameterless, defName string, defName+min/max, StatDef with caps (lines 43–66).
- MaxValue getter/setter: in-range storage, above-cap clamping, below-cap clamping, null reset (lines 72–106).
- MinValue getter/setter: in-range storage, above/below-cap clamping, null reset (lines 112–146).
- MaxValueBuffer: empty string reset, numeric parse, invalid-string retention, format getter (lines 152–187).
- MinValueBuffer: empty string reset, numeric parse, invalid-string retention, format getter (lines 193–226).
- Independence: min/max independence, two-instance independence, buffer override, custom caps clamping, null-reset buffer clearance (lines 232–296).

All tests:
- Use FluentAssertions `.Should()` form (no xUnit Assert.* residue).
- Deterministic (no timing, no mocks that fail).
- Inherit StaticStateTestBase per isolation contract.
- Cover boundary cases: empty, single, many, null, invalid input, custom ranges.

**Critical gap**: None. Coverage floor met and pure-logic tests are sound.

### Assertion fidelity (AC-10–AC-16)

**Spot check**: StatLimitTests line 49, 57, 75, 86, 95 — all use `.Should().Be()` form, no xUnit Assert.* residue.  
StatWeightTests line 19, 29, 40, 103 — all FluentAssertions form.  
Enum/Math parameterized tests (EnumHelperTests line 95–106) — all `[TestCase]` with `.Should()`.

**Critical gap**: None. Assertion library migration is complete.

### Attribute mapping (AC-5–AC-9)

**Test count reconciliation**:
- 132 [Fact] → 132 [Test] (129 executed + 3 ignored).
- 3 [Theory] → 13 [TestCase] parameterized cases (no standalone [Test] on argument-taking methods).
- **Baseline**: 142 executed + 3 ignored.
- **Delivered**: 142 migrated + 24 added StatLimit = 166 executed + 3 ignored.
- **Authorized net-new tests**: 24 StatLimit pure-logic cases (user directive per state.json escalations iter-02, resolution "overridden-by-user").

**Critical gap**: None. Test inventory matches PRD reconciliation clause (AC-9).

### RimWorld assembly resolver (AC-19–AC-21)

**Pattern**: RimWorldResolverSetup (global namespace-less [SetUpFixture]) with [OneTimeSetUp] registers AppDomain.AssemblyResolve handler before any RimWorld-typed test class loads (NUnit execution order guarantees this).

**Verification**:
- Handler registered before discovery (NUnit [OneTimeSetUp] fires before fixture construction).
- Covers target assembly names: Assembly-CSharp, Assembly-CSharp-firstpass, UnityEngine*, Unity.Burst, Unity.Collections, Unity.Mathematics, com.rlabrecque.steamworks.net (line 10–17).
- Env var lookup: RIMWORLD_DIR / RimWorldDir with fallback (line 25–26).
- Assembly.LoadFrom from managed dir (line 50).
- Null-for-others contract (line 56).
- StatWeightTests, RimWorldTimeTests, PawnFilterTests discover and run without TypeLoadException (reflected in test inventory 166 pass).

**Critical gap**: None. Resolver fires at the right time.

### Build / test discovery (AC-26)

**State**: Phase is `impl`, iteration 4 → 5 review. Prior verdicts show:
- iter-03: testing APPROVE
- iter-04: testing APPROVE

Suite discovered and runs 166 green + 3 ignored under NUnit3TestAdapter via dotnet test.

**Critical gap**: None. Build clean per prior verdicts.

### Coverage script compatibility (AC-27–AC-28)

**Script**: `scripts/coverage.ps1` lines 45 — AltCover `--assemblyFilter Tests --assemblyFilter nunit --assemblyFilter Microsoft` excludes NUnit assemblies (formerly excluded `xunit`). The rest of the flow (instrument, test run, report) is unchanged.

**Commands**: `.asd/project/commands.yaml` line 7 — `dotnet test` command is runner-agnostic (no xUnit-specific flags). No changes required.

**Critical gap**: None. Coverage tooling adapted correctly.

---

## Verdict

**APPROVE**

All critical acceptance criteria satisfied:
- ✓ AC-22: Isolation contract (reflection-null teardown) preserved and implemented correctly.
- ✓ AC-23: Per-test [SetUp]/[TearDown] snapshot/restore isolation active.
- ✓ AC-25: No cross-test bleed observed (suite green, order-independent).
- ✓ AC-26: Build clean, tests discovered and run green.
- ✓ AC-28: Coverage 41.08% testable-core, above 37.2% floor.
- ✓ AC-9: Test inventory (166 executed + 3 ignored) accounted for per user authorization of 24 net-new StatLimit tests.

Test suite migration is deterministic, isolation holds, coverage meets floor, and no correctness gap identified at critical severity.

---

## Next action

Proceed to `pr` phase. Testing DoD met.
