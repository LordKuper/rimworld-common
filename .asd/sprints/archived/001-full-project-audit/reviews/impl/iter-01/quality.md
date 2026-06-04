[REVIEW-impl-quality]: CONCERNS

# Review — quality

- **Phase**: impl-review
- **Iteration**: 1

## Scope reviewed

- `Source/IDefProvider.cs`, `Source/DefProvider.cs` (isolation seam, ADR-0001)
- `Source/Helpers/StatHelper.cs` (static-ctor → `Rebuild()`)
- `Source/WorkTypeStatMap.cs` (`Rebuild()`, null-stat `LogWarning`)
- `Source/SkillStatMap.cs`, `Source/Cache/DefCache.cs` (DefProvider reroute)
- `Source/WorkTypeThingRule.cs` (nullable `!` guards, ADR-0002 docs)
- `Source/Filters/PawnFilter/PawnFilter.cs` (`Combine` split, AC-7)
- `Source/Resources.cs` (tooltip DRY collapse, AC-6)
- `Source/RimWorldTime.cs` (`F1` format)
- `Source/StatRanges.cs` (ADR-0002 adaptive contract)
- `Tests/FakeDefProvider.cs`, `Tests/StaticStateFixture.cs`, and stateful/pure-path test classes
- `Directory.Build.props` (ADR-0003)

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | `Tests/StaticStateFixture.cs:23` + `Tests/StatefulSubsystemTests.cs:18`, `Tests/StatRangesTests.cs:12` | The fixture is wired as `IClassFixture<StaticStateFixture>`, so its ctor (save `DefProvider.Current`) and `Dispose` (restore + rebuild caches) run **once per class**, not per test. Tests within a class that swap `DefProvider.Current` / mutate static caches leak that state to sibling tests in the same class (restore happens only at class teardown). It is mitigated today only because every in-class test re-sets the provider and calls `StatHelper.Rebuild()`/`WorkTypeStatMap.Rebuild()` at its start, but the isolation is by-convention, not enforced by the harness. Custom rule (`custom-coding-rules.md` §Testing) asks for save/restore via test constructor + `IDisposable` (i.e. per-test). | Either (a) make `StaticStateFixture` per-test by having each stateful test class itself implement `IDisposable` and instantiate/dispose the fixture in its own ctor/`Dispose`, or (b) document explicitly that every test in these classes MUST re-prime the provider (and add an assert/helper that does so) so the convention is not silently breakable. |
| 2 | low | `Source/StatRanges.cs:54-67` | `UpdateStatRange` reads a stale local: on a new stat, `TryGetValue` misses, `range` stays `default` `(0,0)` while `Ranges[stat]` is set to `(value,value)`; the following `range.min > value` / `range.max < value` checks run against `(0,0)`, so the stored range becomes `(min(0,value), max(0,value))` — 0 is injected into every stat range on first observation. **Pre-existing**; ADR-0002 explicitly retains StatRanges behavior unchanged (AC-9: "no behavioral change, only XML docs"). Reported for record, not as a sprint regression. | Out of scope for this sprint (ADR-0002). If ever revisited: re-read `Ranges[stat]` after the insert, or `return new FloatRange(value,value)` early on the miss path. |
| 3 | low | `Source/SkillStatMap.cs:77,88` | `_map[needFactor.skill]` / `_map[needOffset.skill]` index without a guard; if a `StatDef.skillNeedFactors`/`skillNeedOffsets` references a `SkillDef` not present in `AllDefsListForReading<SkillDef>()`, this throws `KeyNotFoundException`. **Pre-existing** behavior preserved by the static-ctor→`BuildMap` refactor (the reroute through `DefProvider.Current` did not change it). Real game data keeps these consistent, so it is not reachable in practice. | Optional: `if (!_map.TryGetValue(needFactor.skill, out var stats)) continue;`. Not required this iteration. |
| 4 | low | `Source/WorkTypeStatMap.cs:141` / `SkillStatMap.Map` | `Rebuild()` reads `SkillStatMap.Map`, which lazily builds against `DefProvider.Current` and then caches. The fixture (`StaticStateFixture.Dispose`) resets `SkillStatMap._map` to null via reflection, but within a class a test that calls `WorkTypeStatMap.Rebuild()` after a prior test already populated `SkillStatMap.Map` with a different fake provider will reuse the stale skill map (no `SkillStatMap.Rebuild()` is invoked between in-class tests). Same root cause as finding #1 (per-class vs per-test). | Covered by the fix for #1; alternatively add an internal `SkillStatMap.Rebuild()` and call it from the stateful tests' priming step. |

## Verdict
CONCERNS: 1 medium, 3 low

## Assessment of the specific risks raised in the dispatch

- **Nullable `!` operators / guards** (`WorkTypeThingRule.GetThingDefScore/GetThingScore`, `PawnFilter.Copy`, `StatHelper.GetStatDef`): every `!` is preceded by a real runtime guard (`Where(... != null)`, `IsNullOrEmpty` check, or `def == null ? null` filter). No wrong-behavior risk found.
- **`Rebuild()` extraction behavior-preserving**: `StatHelper.Rebuild()` runs the same init sequence the static ctor did and clears `Stats`; `WorkTypeStatMap.Rebuild()` reproduces the original `BuildMap` ordering and dictionary population. `PostInit`/label reinit remains separate. Behavior-preserving — confirmed.
- **`PawnFilter.Combine` split (AC-7)**: each per-section helper selects source by `main.FilterX.HasValue ? main : fallback`, copies the matching collections via new containers (shallow element copy, matching original container-level semantics), then `Validate()` runs with `TriStateMode = false`. Semantics preserved — confirmed; covered by `PawnFilterTests`/`StatefulSubsystemTests`.
- **`DefProvider.Current` thread-safety / default**: plain `{ get; set; }` defaulting to `new VerseDefProvider()`. No locking, but all callers run on RimWorld's main thread and tests are non-parallel (`DisableParallelization`). ADR-0001 documents the single-replacement, no-DI design. Not a defect.
- **`RimWorldTime` `F1` fix**: `ToString()` uses `{Hour:F1}` correctly; the year/day round-trip through `GetTotalHours` + single-float ctor is consistent (`HoursInYear` = `HoursInDay * DaysInYear` = 1440). Verified against `RimWorldTimeTests` (e.g. `Ctor_FromYearDayHour_CalculatesTotalHours`, `2957f` decomposition). Correct.
- **Resource leaks / off-by-one / contract drift vs ADRs**: none found. Tooltip collapse (AC-6) preserves string composition; `using System.Collections.Concurrent` is used (no dead-using warning). `Directory.Build.props` matches ADR-0003. No security-relevant surface (mod library; no I/O, secrets, injection, or crypto).

## Next action
Sprint routes back to `impl` (fix mode). Responsible dev addresses finding #1 (the only at-floor-or-above concern: per-class vs per-test static-state isolation) — preferred fix is per-test fixture disposal or an enforced re-prime step. Findings #2–#4 are low-severity / pre-existing-by-ADR and may be deferred or acknowledged without change. None require escalation (no concept/PRD/API/contract change, no new abstraction).

## Escalations (optional)
- None.
