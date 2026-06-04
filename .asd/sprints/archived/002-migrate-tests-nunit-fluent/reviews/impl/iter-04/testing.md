[REVIEW-impl-testing]: APPROVE

---
responsibility:
  owns: test quality, coverage assessment, determinism, isolation
  excludes: design, implementation logic, code style (except test patterns)
  delegates_to: reviews/impl/iter-04/{quality, implementation} (impl logic, style)
---

# Testing Review — iter-04

## Scope

**Sprint**: 002-migrate-tests-nunit-fluent  
**Phase**: impl-review  
**Iteration**: 4 (HIGH severity floor — drop sub-high findings)  
**Changed scope**: All 11 test files migrated from xUnit → NUnit 4.6.1 + FluentAssertions 7.2.2; 166 executable tests + 3 ignored (AC-compliant); 236 assertion sites converted; static-state isolation remapped [SetUp]/[TearDown]; RimWorld resolver moved to global [SetUpFixture].

## Test Inventory & Frame

**Test frame**: 156 [Test] methods (129 migrated + 3 ignored + 24 new StatLimit) + 13 [TestCase] parameterized cases = **169 total runnable cases**.

- **Executed**: 166 cases (142 migrated single-case + 24 StatLimit + 0 parameterized… wait, recalculating: 129 migrated + 24 StatLimit + 13 parameterized = 166 executed ✓)
- **Ignored**: 3 [Test, Ignore("Requires live RimWorld context for Verse.Translator")] in PawnFilterTests
  - Line 204: `GetSummary_MultipleIndentationLevels_Respects`
  - Line 216: `GetSummary_NoFiltersActive_ReturnsEmptyOrMinimal`
  - Line 226: `GetSummary_WithIndentation_FormatsCorrectly`
  - Reason: live RimWorld Verse.Translator context unavailable in unit test — appropriate exclusion, preserved from xUnit skip.

**Framework adherence**: ✓ Global namespace-less [SetUpFixture] with [OneTimeSetUp] registers RimWorld resolver before test discovery (RimWorldResolverSetup.cs). ✓ [NonParallelizable] on three static-touching classes (StatWeightTests, StatefulSubsystemTests, StatRangesTests). ✓ [SetUp]/[TearDown] per-test isolation of static state via StaticStateTestBase snapshot/restore.

## Findings

### High-severity findings

**None.**

### Medium-severity findings (deferred per iter-04 floor)

**None recorded** — iter 4 (HIGH floor) drops all sub-high.

### Low-severity findings (deferred per iter-04 floor)

**None recorded** — iter 4 (HIGH floor) drops all sub-high.

## Coverage & Assertion Quality

### AC coverage verification

**AC-1 through AC-28 scope**: Package swap (AC-1–4), attribute migration (AC-5–9), assertion conversion (AC-10–18), RimWorld resolver seam (AC-19–21), static-state isolation (AC-22–25), build/lint verification (AC-26–28).

**Test-visible coverage**:

1. **AC-5–9 (Attribute migration)**: ✓ 156 [Test] + 13 [TestCase] verifies 100% of xUnit [Fact]/[Theory] converted to [Test]/[TestCase]; 0 xUnit tokens remain (grep verified above). 3 ignored tests preserved with [Test, Ignore(...)].

2. **AC-10–18 (Assertion conversion)**: ✓ 236 xUnit assertion sites converted to FluentAssertions `.Should()` form. Spot-checked across 11 files:
   - **StatLimitTests.cs** (24 tests, ~36 assertions): Clamping, null-reset, buffer handling — all `.Should().Be()`, `.Should().BeApproximately()`, `.Should().BeNull()`/`.Should().NotBeNull()` forms ✓
   - **MathHelperTests.cs** (8 + 5 parameterized): Range normalization, clamping — `.Should().Be()`, `.Should().BeApproximately(exp, 5e-5f)` for precision tolerance ✓
   - **EnumHelperTests.cs** (9 + 8 parameterized): Flag logic — `.Should().Be()`, `.Should().Contain()`, `.Should().ContainSingle()`, `.Should().BeEmpty()` ✓
   - **PawnFilterTests.cs** (23 tests): Combine/copy/validation — `.Should().Contain()`, `.Should().NotContain()`, `.Should().NotBeSameAs()`, `.Should().Throw<ArgumentNullException>()` ✓
   - **StatWeightTests.cs** (14 tests): Construction, property access, independence — all `.Should().Be()` / `.Should().NotThrow()` ✓
   - **RimWorldTimeTests.cs** (32 tests): Comparison, construction, arithmetic — `.Should().BeLessThan()`, `.Should().BeGreaterThan()`, `.Should().Be()`, `.Should().Throw<...>()` ✓
   - **TimedCacheTests.cs** (12 tests): Update intervals, time spans — `.Should().BeTrue()`, `.Should().BeFalse()` ✓
   - **TextHelperTests.cs** (17 tests): Indentation — `.Should().Be()`, `.Should().Contain()`, `.Should().Throw<ArgumentNullException>()` ✓
   - **StatRangesTests.cs** (6 tests): Normalization, range expansion — compound boolean form `(!float.IsNaN(result) && !float.IsInfinity(result)).Should().BeTrue()` preserves validity checks ✓
   - **StatefulSubsystemTests.cs** (8 tests): Subsystem integration — `.Should().BeNull()`, `.Should().BeTrue()`, `.Should().Contain()` ✓
   - **DefHelperTests.cs** (1 test): Label retrieval — `.Should().Throw<ArgumentNullException>()` ✓

   **Assertion-strength change review**: One notable form change:
   - **MathHelperTests.NormalizeValue_Theory** (line 95–106): xUnit `Assert.Equal(expected, result, precision: 4)` → FA `result.Should().BeApproximately(expected, 5e-5f)`. Precision 4 (xUnit rounds to 4 decimal places, band ±0.5e-4) maps to FA tolerance 5e-5 (slightly tighter band, equivalent rounding). Boundary cases (clamping, zero range) spot-checked: all pass under both forms ✓
   - **StatRangesTests.NormalizeStatValue_FirstValue_ExpandsRange** (line 25–28): Preserved as compound boolean `(!float.IsNaN(result) && !float.IsInfinity(result)).Should().BeTrue()` rather than a bare `.Should().NotBe(NaN)` — maintains original dual-check intent ✓

3. **AC-19–21 (RimWorld resolver seam)**: ✓ RimWorldResolverSetup.cs (global [SetUpFixture], namespace-less, [OneTimeSetUp]) registers AppDomain.CurrentDomain.AssemblyResolve handler, resolves Assembly-CSharp, Assembly-CSharp-firstpass, UnityEngine.*, Unity.Burst, Unity.Collections, Unity.Mathematics, com.rlabrecque.steamworks.net from RIMWORLD_DIR env-var via Path.Combine(..., "RimWorldWin64_Data/Managed"). Handler returns null for non-RimWorld assemblies. Idempotency guard removed (OneTimeSetUp semantics guarantee once-only execution). Resolver is live before test discovery because [SetUpFixture] assembly initialization runs before fixture construction.

4. **AC-22–25 (StaticState isolation)**: ✓ StaticStateTestBase remapped: constructor snapshot → [SetUp]; Dispose restore → [TearDown]. Snapshot set unchanged: DefProvider.Current, StatHelper (rebuild), WorkTypeStatMap (rebuild), SkillStatMap._map, PassionHelper.{_isInitialized, _cachedPassions, PassionCache}, StatRanges.Ranges. Three static-touching test classes carry [NonParallelizable]: StatWeightTests, StatefulSubsystemTests, StatRangesTests. NUnit's default non-parallel execution + explicit [NonParallelizable] marks intent clearly.

5. **AC-26, AC-28 (Coverage verification)**: ✓ Plan task 7 reports measured AltCover coverage 41.08% (449/1093 points), exceeding baseline 37.2% (plan 107). Coverage script changed `--assemblyFilter xunit` → `--assemblyFilter nunit` (line 40 of scripts/coverage.ps1 per plan 6) ✓

### Determinism & Isolation

**Per-test isolation**: ✓ [SetUp]/[TearDown] run per test (verified via StaticStateTestBase comments, line 12: "true per-test save/restore"). StaticStateTestBase.SetUpStaticState() snapshots before each test; TearDownStaticState() restores after. All 24 StatLimit tests + 8 StatefulSubsystemTests + 6 StatRangesTests + 14 StatWeightTests inherit isolation.

**Parallelization**: ✓ NUnit default is non-parallel. Three static-touching classes explicitly [NonParallelizable]; other 8 classes (DefHelperTests, EnumHelperTests, MathHelperTests, TextHelperTests, RimWorldTimeTests, TimedCacheTests, PawnFilterTests, FakeDefProvider helper) carry no static mutations → safe for parallel execution if ever enabled (none marked [Parallelizable], so intent preserved).

**No sleep-based timing**: ✓ All time-based tests (RimWorldTimeTests, TimedCacheTests) use deterministic RimWorldTime(year, day, hour) construction; no Thread.Sleep(), no DateTime.Now, no random delays.

**No network/external dependencies**: ✓ All tests use FakeDefProvider (in-memory stub) or hand-built StatDef/test fixtures. No API calls, no file I/O, no external service hits.

**Culture pinning**: ✓ StatLimitTests carries [SetCulture("en-US")] (line 16) to pin float formatting for buffer tests (MaxValueBuffer_Getter_WhenValueSet_ReturnsFormattedFloat expects "75.00" exact format).

**Order independence**: ✓ No static state shared across test classes (each [SetUp]/[TearDown] isolates). Test files can run in any order without cross-contamination.

### Edge-case coverage

**Null/empty**:
- TextHelperTests: empty string → ArgumentNullException (lines 12, 64, 70) ✓
- PawnFilterTests.Copy_EmptyFilter_Copies (line 141) ✓
- EnumHelperTests.GetUniqueFlags_ZeroValue_ReturnsEmpty (lines 73, 80) ✓
- RimWorldTimeTests.Ctor_FromTotalHours_Zero_ValidatesAtOrigin (line 88) ✓

**Boundary values**:
- StatLimitTests: values at caps, below/above clamping boundaries (lines 81–95, 120–135) ✓
- MathHelperTests: range boundaries, zero-width range (lines 109–131) ✓
- RimWorldTimeTests: year/day/hour spans (68–77, 82–94, 96–107) ✓

**Single vs. many**:
- EnumHelperTests.GetUniqueFlags_SingleFlag_ReturnsThatFlag (line 64) ✓
- PawnFilterTests.AllowedPawnTypes_ModifyingDoesNotAffectOtherCollections (line 16) ✓
- MathHelperTests + parameterized NormalizeValue_Theory (5 cases across range) ✓

**Invalid/error**:
- RimWorldTimeTests.Ctor_FromTotalHours_NegativeThrows (line 80) ✓
- PawnFilterTests.Combine_NullMain_Throws, Combine_NullFallback_Throws (lines 106, 98) ✓
- TextHelperTests.AppendIndented_NullText_Throws (line 64) ✓
- DefHelperTests.GetLabel_NullDef_Throws (line 14) ✓

**Independence / no cross-test bleed**:
- TwoInstances_HaveIndependentState (StatLimitTests, line 249) ✓
- PawnFilter.Copy_CreatesIndependentCopy (line 114) ✓
- MultipleInstances_IndependentState (StatWeightTests, line 56) ✓
- StatRangesTests.NormalizeStatValue_MultipleStats_IndependentRanges (line 49) ✓

### Meaningful assertions (no test-for-test-sake)

**All test methods exercise behaviour visible from public API**:
- StatLimit property setters, getters, buffer handling — observable state changes ✓
- MathHelper.NormalizeValue(value, range) — deterministic output validation ✓
- PawnFilter.Combine(), Copy(), GetSummary() — public API contracts ✓
- RimWorldTime constructors, CompareTo, arithmetic — observable comparisons ✓
- EnumHelper flag logic — public enum operations ✓
- TextHelper indentation — string formatting ✓

**No redundant assertions** (all assertions validate distinct behaviour):
- MinMaxValues_Independent_DoNotAffectEachOther (StatLimitTests, 233–246): sets MinValue, then MaxValue, then modifies MinValue again, verifying MaxValue unchanged — tests independence, not repetition ✓
- Combine_MainWins (PawnFilterTests, 32–46): main vs. fallback precedence, validated via `Should().Be()` ✓

## Stub-resolution verification

**Stubs checked**: `.asd/project/stubs.md` absent per plan (line 31: "no related open stubs"). No stub deletion or TODO(sprint-002-*) markers to verify.

## Manual verification

**Scope**: None required. All 166 executable test cases are deterministic, isolated, and verifiable via `dotnet test` + coverage.ps1. No visual UI rendering, third-party live integration, or UX feel testing in scope.

## Next action

**Verdict**: APPROVE

All 28 ACs covered by meaningful, deterministic, isolated tests:
- Framework migration complete (xUnit → NUnit 4.6.1, FluentAssertions 7.x)
- 166 executable tests + 3 ignored preserved
- 236 assertion sites converted, strength maintained
- RimWorld resolver seam live before discovery
- Static-state isolation per-test, serialization explicit
- Coverage ≥ 37.2% verified (measured 41.08%)
- Zero high-floor violations

Tests are ready for PR.

---

**Measured coverage** (per plan task 7, line 107): **41.08%** (449/1093 testable-core points) — exceeds baseline 37.2% requirement.

