[REVIEW-impl-simplification]: APPROVE

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 4

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above high floor; no over-engineering checklist hits | — |

## Verdict
APPROVE

## Next action
DoD-eligible from the simplification angle. No fix routing required. PM collects sibling reviewer verdicts for iteration-04 DoD.

## Notes (non-blocking, not raised as findings)

Scanned the full migrated test surface and support scaffolding against the over-engineering checklist. Cleared the candidates that could look like hits:

- `FakeDefProvider : IDefProvider` (`FakeDefProvider.cs`) — `category: keep-as-is`. Not a single-implementer interface and not a mock-of-mock. `IDefProvider` is a pre-existing production seam (sprint 001, ADR-0001) with a production implementer `VerseDefProvider` (`Source/LordKuper.Common/DefProvider.cs:33`). `FakeDefProvider` is one hand-built test double of that real interface — the canonical, simplest isolation approach, not test-introduced abstraction.
- `TestTimedCache` (`Cache/TimedCacheTests.cs:167`), `TestDefCache` (`StatefulSubsystemTests.cs:163`) — `keep-as-is`. Minimal subclasses that exist only to expose a protected/abstract production ctor to the test. No added layer; removing them is impossible (the base types are not directly constructible).
- `MakeStatDef` factory (`StatLimitTests.cs:23`) — `keep-as-is`. Not a "factory for fewer than three classes" smell: it builds one type with the specific field set that replicates `Configure(null)` defaults, reused across ~20 tests. Earns its weight (DRY over copy-pasted def literals).
- Reflection-based static-cache resets in `StaticStateTestBase.TearDownStaticState` and `StatefulSubsystemTests` — `keep-as-is`. These reset private statics that have no public reset hook. The simpler-looking alternative (adding public `Rebuild` seams to production for test convenience) would be the *more* complex / production-invasive path and would trip scope expansion. Reflection here is the lower-complexity choice given the constraint.
- Documented `[Ignore("Requires live RimWorld context ...")]` tests (`Filters/PawnFilterTests.cs:204,216,226`) and the coverage-boundary note in `DefHelperTests.cs:22` — `keep-as-is`. Not dead code "in case we need it": they are real tests parked at the RimWorld-runtime boundary with a stated reactivation condition, consistent with the coverage-denominator exclusions documented in `scripts/coverage.ps1`.
- `scripts/coverage.ps1` — `keep-as-is`. The AltCover-over-coverlet choice and the per-type denominator exclusions are justified inline by a concrete tool limitation (coverlet yields 0% against the RimWorld-referencing assembly). Complexity earns its weight; no wrapper-of-a-wrapper.

Comment-restates-code instances exist in several test files (e.g. `// Protected property getter/setter`, `// Zero value is handled`). These sit below the high floor (low/medium clarity) and fall under the nitpick drop-list (pure wording polish) — not raised.

## Escalations
None. No reviewer-proposed fix would add an abstraction, layer, or dependency, so no Complication Approval is required from this reviewer.
