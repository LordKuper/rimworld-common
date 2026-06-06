---
responsibility:
  owns: sprint scope, goal, top-level acceptance criteria
  excludes: task breakdown, design decisions, code, audit findings
  delegates_to: plan.md (tasks), design/ docs (decisions), audit.md (audit)
---

# Sprint 003-statranges-fix-expose

## Goal
Fix a confirmed first-observation bug in `StatRanges.UpdateStatRange` and expose the type for consumption by the downstream EquipmentManager mod, strengthening the test suite to lock in the corrected behavior and re-publishing the assembly.

The current `UpdateStatRange` mishandles the first observation of a stat: on a `TryGetValue` miss it runs the min/max comparisons against the stale `{0,0}` default of the freshly-declared `FloatRange`, so the first value `v` yields `[0, v]` (or `[v, 0]`) instead of the correct degenerate `[v, v]`. EquipmentManager currently maintains its own duplicate range logic (`EquipmentManagerGameComponent_StatRanges`); making `Common.StatRanges` correct and public is the prerequisite for that mod to drop its duplicate and consume the shared implementation. Migrating EquipmentManager itself is a separate, future effort and is motivating context only.

## Acceptance
- **Bug fix** (`Source/LordKuper.Common/StatRanges.cs`): on a `TryGetValue` miss, `UpdateStatRange` seeds `[value, value]` into a local `FloatRange`, runs the two `min`/`max` comparisons against that seeded value (never the stale `{0,0}` default), and writes the dictionary entry exactly once. The first value `v` for any stat yields `[v, v]`.
- **Visibility**: `internal static class StatRanges` -> `public`; `internal static float NormalizeStatValue(...)` -> `public`; add `public static void Clear() => Ranges.Clear();`.
- **Constraint**: the class stays `static` and process-global (intentional, documented adaptive design relied on by `WorkTypeThingRule`); it is NOT converted to an instance class.
- **Tests** (`Source/LordKuper.Common.Tests/StatRangesTests.cs`): add exact-bound assertions that fail on the old code and pass on the fix, using the existing `FakeDefProvider` + `StatHelper.Rebuild()` pattern — first `v=50` -> range `[50,50]` with `NormalizeStatValue` mapping 50->0 and 100->1; sequence `-10, -5, 0` -> `[-10,-5]` -> `[-10,0]` with -10->0 and 0->1; a regression test (e.g. `NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange`) that fails under the old `[0,v]` behavior. If `StaticStateTestBase` resets `StatRanges.Ranges` via reflection, switch it to call the new public `StatRanges.Clear()`.
- **Build & publish**: build green with 0 warnings (high warning level + warnings-as-errors); all existing + new tests pass and demonstrably fail if the fix is reverted; rebuild and publish the Common assembly to `1.6/Assemblies`.

## Out of scope
- Migrating EquipmentManager itself onto `Common.WorkTypeThingRule` / `Common.StatRanges` and deleting its duplicate (`EquipmentManagerGameComponent_StatRanges`) — motivating context only.
