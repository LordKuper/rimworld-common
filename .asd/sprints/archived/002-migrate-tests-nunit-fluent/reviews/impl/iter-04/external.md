[REVIEW-impl-external]: CONCERNS

# External Review Report

- **Phase**: impl-review
- **Iteration**: 04
- **Severity floor (this iter)**: high
- **Codex**: codex-cli 0.136.0 — ran (`codex exec`, stdin prompt + `git diff main...HEAD` for `Source/` and `scripts/coverage.ps1`); `review --json` not available in this version.

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | `Source/LordKuper.Common.Tests/StaticStateTestBase.cs:42` (+48) | `TearDownStaticState` calls `WorkTypeStatMap.Rebuild()` (line 42) *before* nulling `SkillStatMap._map` (line 48). When `_map` is null on entry — which is the steady state, since each test's teardown nulls it at line 48 and `[SetUp]` (lines 26-29) never repopulates it — `WorkTypeStatMap.Rebuild()` reaches `SkillStatMap.Map` (`WorkTypeStatMap.cs:141`), whose getter triggers `SkillStatMap.BuildMap()`. The project's own test `WorkTypeStatMap_Rebuild_ReinitializesWithFakeProvider` (`StatefulSubsystemTests.cs:149-155`) documents this exact path as a Unity-bound hazard (`Logger → Verse.Log.Message → Unity native ECall`, unavailable outside the Unity player) and pre-populates `SkillStatMap._map` with an empty map *before* calling `Rebuild()` to skip `BuildMap()`. The shared teardown base applies the inverse ordering and omits that guard, leaving the documented isolation path unguarded for every `StaticStateTestBase` subclass. Latent: the `Logger` calls inside `BuildMap` are `#if DEBUG`-gated and the provider reads are wrapped in swallowing try/catch, so the current Release/CI suite passes — but the harness contradicts its own documented contract and is a real fragility on a core isolation path. | Mirror the validated pattern from `StatefulSubsystemTests.cs:152-154`: pre-populate `SkillStatMap._map` with an empty `Dictionary<SkillDef, HashSet<StatDef>>` *before* the `WorkTypeStatMap.Rebuild()` call at line 42, then perform the null-reset afterward (or reorder so the `_map` reset and re-seed precede the rebuild). |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | (Codex reported no medium/low findings) | n/a |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | (none) | n/a |

## Verdict
CONCERNS: 1

## Next action
Backend/Test Engineer: align `StaticStateTestBase.TearDownStaticState` with the guarded `WorkTypeStatMap.Rebuild()` pattern already used and documented in `StatefulSubsystemTests.cs:149-155` (pre-seed `SkillStatMap._map` before the rebuild, or reorder the reset to precede the rebuild). This is the only kept finding; AC-28 coverage (41.08%) and the StatLimit ctor-recursion item are known/deferred and were not re-raised. Not a stalemate vs iter-03 (iter-03 = APPROVE; sole open item there, the `[NonParallelizable]` doc-count drift, is fixed and confirmed not re-surfaced).
