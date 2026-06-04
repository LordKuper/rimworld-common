---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-simplification]: CONCERNS

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | critical | `Tests/XunitExtensions.cs` (RimWorldTestFramework) · `Tests/AssemblyResolverInitialize.cs` · `Tests/AssemblyInitializer.cs` (AssemblyInitializerFixture) | Three independent implementations of the same RimWorld assembly-resolver register an `AppDomain.AssemblyResolve` handler, each carrying a near-identical copy of `IsRimWorldAssembly` + the resolve handler body. ADR-0001 / decisions-log specified **one** isolation mechanism. Checklist: "Framework wrapping a framework" / redundant scaffolding; code-style §1 (DRY/YAGNI), §3 (scope discipline). `RimWorldTestFramework` (wired via `[assembly: TestFramework]` in `AssemblyInfo.cs`) registers the resolver in its ctor before discovery — that alone satisfies the need. `AssemblyResolverInitialize` (wired via `InitializerTrigger`) is a redundant second resolver on the same event. | Keep exactly one resolver mechanism (the custom `RimWorldTestFramework`, which runs earliest). Delete `AssemblyResolverInitialize.cs` and the `InitializerTrigger` in `AssemblyInfo.cs`. Lift the single `IsRimWorldAssembly` + resolve handler so it exists once. No new abstraction needed — this is deletion, not a layer. |
| 2 | critical | `Tests/AssemblyInitializer.cs:12-65` (AssemblyInitializerFixture, RimWorldContextCollection) | `AssemblyInitializerFixture` + `[CollectionDefinition("RimWorldContext")]` define a third resolver and a collection that **no test class references** (`grep` for `[Collection("RimWorldContext")]` = 0 hits). Dead scaffolding kept "in case we need it" — explicit checklist hit. | Delete `AssemblyInitializer.cs` entirely. The active test collection is `StaticState` (via `StaticStateFixture`), which all stateful tests already use. |
| 3 | critical | `Tests/XunitExtensions.cs:65-66` (`RimWorldTestFrameworkAttribute`) | `RimWorldTestFrameworkAttribute` is declared but never applied anywhere; the framework is selected via the string-form `[assembly: TestFramework("...RimWorldTestFramework", "...")]`. Dead code. | Delete the unused `RimWorldTestFrameworkAttribute` class. |

## Verdict
CONCERNS: 3

Proportionate, NOT flagged (assessed and cleared):
- **`IDefProvider` seam** — 2 real implementers (`VerseDefProvider`, `FakeDefProvider`), 4 narrow members, each with a real production call site documented inline; ADR-0001-approved single seam, no DI container, no balloon. The "interface with one implementer" smell does not apply.
- **`PawnFilter.Combine` → 9 helpers** — 9 independent filter dimensions, each helper picks `main` vs `fallback` by its own `HasValue` flag and copies the related fields. Pure decomposition (no new types, no indirection); the un-split form is one ~70-line method repeating the same shape 9×. Not over-fragmented.
- **`Resources` tooltip collapse** — genuine DRY: 18 hardcoded On/Off/tri-state fields → 9 named accessors over 1 shared `GetFilterTooltip` builder + 2 caches with real callers. The helper adds composition value; simpler than the duplicated original.
- **`StaticStateFixture` + internal `Rebuild()`** — minimum machinery for AC-14/15/16/19; `Rebuild()` is `internal` (not public), reflection used only where no `Rebuild()` exists to avoid adding surface. Proportionate.
- Accepted decisions per scope (NOT raised): IMP-07 weights in code, StatRanges adaptive, LangVersion unpinned, Limit tests removed.

## Next action
impl-review routes the sprint back to `impl` (fix mode). Dev consolidates the three resolver mechanisms into one and deletes the two unused scaffolding types (findings #1–#3 are pure deletions / de-duplication — no escalation required). Sprint re-enters impl-review at iter 02. Findings #1–#3 are `critical` and undroppable on later iterations per review-policy.md.

## Escalations (optional)
- None. All three fixes are deletions / de-duplication; none adds an abstraction, layer, or dependency, so no Complication Approval is required.
