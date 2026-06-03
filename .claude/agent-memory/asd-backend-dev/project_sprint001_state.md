---
name: project_sprint001_state
description: Sprint 001-full-project-audit — all tasks done; FIX MODE iter-01 resolved; ready for iter-02 review
metadata:
  type: project
---

Sprint `001-full-project-audit` is on branch `sprint/001-full-project-audit`.

All 14 tasks (T0-T13) complete. Sprint entered impl-review iter-01 and was returned to FIX MODE.

## FIX MODE iter-01 resolution (4 commits, 2026-06-04)

| Commit | Finding |
|--------|---------|
| `e6e6db5` | fix(build): consolidate RimWorld path-resolution into repo-root Directory.Build.props |
| `e609f7e` | fix(tests): remove duplicate RimWorld assembly resolvers and dead scaffolding |
| `9d4f672` | fix(tests): enforce per-test static state isolation via StaticStateTestBase |
| `39a10b2` | fix(source): guard SkillStatMap._map lookups with TryGetValue to avoid KeyNotFoundException |

**Gate numbers (Release config):**
- Build: 0 warnings / 0 errors
- Tests: 142 pass / 3 skip (total 145)
- Coverage: Visited Points 416 of 1093 (38.06%)
- SARIF: 0 results

## Key structural changes from FIX MODE

- `Directory.Build.props` (root): now contains BOTH governance props AND RimWorld path-resolution (SSoT). Comment updated.
- `Source/Directory.Build.props` + `Tests/Directory.Build.props`: now thin explicit-import wrappers only (no path-resolution duplication).
- `Tests/LordKuper.Common.Tests.csproj`: removed duplicate `<Nullable>enable</Nullable>` and redundant `RimWorldManagedDir` condition.
- `Tests/StaticStateTestBase.cs`: new abstract base class; `StatefulSubsystemTests`, `StatRangesTests`, `StatWeightTests` now inherit it for per-test isolation.
- `Tests/AssemblyInitializer.cs` + `Tests/AssemblyResolverInitialize.cs`: DELETED.
- `Tests/AssemblyInfo.cs`: stripped to single `[assembly: TestFramework...]` line; `InitializerTrigger` removed.
- `Tests/XunitExtensions.cs`: `RimWorldTestFrameworkAttribute` deleted.
- `Source/SkillStatMap.cs`: `_map[skill]` guarded with `TryGetValue` at lines for skillNeedFactors and skillNeedOffsets.

**Why:** impl-review iter-01 found: IClassFixture was per-class (not per-test); triple resolver duplication; path-resolution SSoT violation; latent SkillStatMap KeyNotFoundException.

**How to apply:** Sprint is ready for impl-review iter-02. No new suppressions introduced. No ADR edits.
