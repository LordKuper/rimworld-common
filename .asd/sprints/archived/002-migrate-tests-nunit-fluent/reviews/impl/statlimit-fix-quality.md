[REVIEW-impl-quality]: APPROVE

# Review — Quality

- **Phase**: impl-review
- **Iteration**: PR-gate focused (StatLimit infinite-recursion fix)
- **Scope**: latest commit diff — `Source/LordKuper.Common/Filters/Limits/StatLimit.cs`, `Source/LordKuper.Common.Tests/StatLimitTests.cs`
- **Severity floor**: low (fresh production change — full review)

## Assessment notes

### 1. Correctness of the `_configuring` re-entry guard
The guard is correct and breaks the re-entry cycle without leaving a half-configured object.

Re-entry path: `MinValue.get` → `EnsureConfigured()` → sets `_configuring=true` → `Configure(Def)` → `Def` getter (`DefCache.Def`) → `Initialize()` → overridden `StatLimit.Initialize()` → `base.Initialize()` then `EnsureConfigured()` again. On the nested call `_isConfigured` is still false, but `_configuring` is now true, so the guard short-circuits (`return`) and does nothing. Control unwinds to the outer `Configure(Def)`, which runs to completion and sets `_isConfigured = true` (StatLimit.cs:274). The `finally` then resets `_configuring = false` (StatLimit.cs:295). Post-condition after the outer call: `_isConfigured == true`, caps populated, `_configuring == false`. Fully configured, not half-configured.

Verified for all three relevant ctor paths:
- Parameterless: `_defName` null → `Def` resolves null → `Configure(null)` sets caps ±1000, `ValueStyle = FloatTwo`, initialises `Limit` (StatLimit.cs:249-253, 272-273). Correct.
- String ctor (unresolvable name): `StatHelper.GetStatDef` returns null for empty dict (StatHelper.cs:117-123) → `Configure(null)`, same as above. Correct.
- StatDef ctor: `Configure(def)` runs in the ctor body (StatLimit.cs:77) and sets `_isConfigured = true` before any property access, so `EnsureConfigured` is a no-op and the guard is never engaged. No behaviour change.

The nested early-return is safe specifically because the outer frame owns the in-progress `Configure` call; it is the outer frame that completes configuration, so the short-circuited nested frame does not need to do any work.

### 2. No regression for `new StatLimit(StatDef)`
Confirmed. The StatDef path sets `_isConfigured = true` in the ctor before any getter runs (StatLimit.cs:75-78 → Configure → :274), so `EnsureConfigured` always early-returns on the first branch (`_isConfigured`). The added `_configuring` field/branch is never reached on this path. Tests `Ctor_StatDef_*`, `MaxValue_*`, `MinValue_*`, buffer tests, `CustomCaps_ClampingRespectsDef` all exercise this unchanged behaviour and pass.

### 3. Thread-safety / reentrancy
The `_configuring` flag is a non-volatile instance bool with no locking. This is acceptable and not over-engineered: StatLimit is RimWorld UI/config state, accessed single-threaded on the main thread. Re-entrancy here is single-threaded recursive re-entry on one thread (Configure→Def→Initialize→EnsureConfigured all on the same call stack), which a plain bool handles correctly. Adding locks would be unjustified complication. No finding.

### 4. Regression tests
The 10 new tests (StatLimitTests.cs:314-403) genuinely exercise the bug:
- Parameterless ctor: `MinValue`, `MaxValue`, `MinValueBuffer`, `MaxValueBuffer` accessed without overflow, plus a caps-default assertion.
- String ctor (unresolvable name): same four accessors + caps default, with `FakeDefProvider` + `StatHelper.Rebuild()` correctly restoring the `_statDefsByName` dict that `TearDownStaticState` nulls between tests (StatHelper.cs:354).

Each property access is exactly the call site that previously recursed (property → EnsureConfigured → Configure(Def) → Def → Initialize → EnsureConfigured). Post-conditions are meaningful: `BeNull()` for at-cap values and `BeEmpty()` for empty buffers assert the object reached a coherent configured state, not merely that it failed to crash. The caps-default tests (`Ctor_*_CapsDefaultToConfigureNull`) assert the actual ±1000 caps, confirming `Configure(null)` ran fully. Adequate.

One observation (informational, not a finding): the tests assert the bug is fixed but do not contain a guard that *would have failed* under the pre-fix code with a bounded assertion — a StackOverflowException cannot be caught/asserted in .NET, so "does not overflow" is necessarily expressed as "the call returns and the post-condition holds." This is the correct and only practical way to test this class of bug. The chosen post-condition assertions are the right proxy.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no qualifying findings | — |

## Verdict
APPROVE

## Next action
None required. The fix is correct, the StatDef path is unchanged, and the regression tests meaningfully cover both previously-broken ctor paths. Proceed to PR gate.

## Escalations
None.
