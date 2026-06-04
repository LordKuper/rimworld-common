[REVIEW-impl-external]: APPROVE

# External Review Report

- **Phase**: impl-review
- **Iteration**: 02
- **Severity floor (this iter)**: high
- **External engine**: Codex CLI 0.136.0 (`codex exec -`, model gpt-5.5, read-only sandbox)
- **Diff reviewed**: `git diff 2270762..HEAD` (commits e6e6db5, e609f7e, 9d4f672, 39a10b2, 1dc4f5b)

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | None. Codex returned APPROVE: no new high/critical defects introduced by the fix diff. | — |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | None reported. | — |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | None reported. | — |

## Iter-01 finding verification

| Finding | Status | Evidence |
|---|---|---|
| #1 — StaticStateFixture wired as `IClassFixture` gave per-class (not per-test) isolation; `DefProvider.Current` leaked between tests in the same class | RESOLVED | New `Tests/StaticStateTestBase.cs` constructs `StaticStateFixture` in its ctor and disposes it in `Dispose()`. xUnit constructs the test-class instance per `[Fact]`, so ctor+Dispose is per-test save/restore. The three stateful classes (`StatRangesTests`, `StatWeightTests`, `StatefulSubsystemTests`) now inherit the base and dropped the `IClassFixture` wiring; `[Collection("StaticState")]` (DisableParallelization=true) retained. `StaticStateFixture` saves `DefProvider.Current` in ctor and restores it in Dispose — confirmed in `Tests/StaticStateFixture.cs`. Leak resolved. |

## Other fix-round changes assessed (no new high/critical issues)

- Assembly-resolver consolidation: `AssemblyInitializer.cs` + `AssemblyResolverInitialize.cs` deleted; resolver now registered once by `RimWorldTestFramework` ctor (runs before discovery). `InitializerTrigger`/`RimWorldTestFrameworkAttribute` dead code removed. No regression — single registration path retained.
- `Directory.Build.props` consolidation: root is the single SSoT (governance + RimWorld path resolution + `CheckRimWorldDir` fail-fast); `Source/` and `Tests/` children are thin explicit-import wrappers, neither shadows the root.
- `SkillStatMap.BuildMap`: indexer `_map[needFactor.skill]` / `_map[needOffset.skill]` replaced with `TryGetValue` guards — eliminates `KeyNotFoundException` for mod-added SkillDefs absent from the provider. Defensive change with no behavioral regression for the in-map path.
- ADR-0002 / ADR-0003 HTML edits are doc-only wording/date corrections.

## Accepted decisions respected (not re-raised)

AC-21 coverage 38.2%; Limit tests removed; EnumHelper public→internal; StatRanges adaptive (ADR-0002); IMP-07 weights in code; LangVersion not pinned. Independently confirmed state (build 0/0, 142 tests pass, coverage ~38%, jb-inspect SARIF 0) not re-verified per instruction.

## Verdict
APPROVE

## Next action
PM aggregates this external verdict with internal reviewer output for the iter-02 impl-review gate. No fixes required from this external pass.
