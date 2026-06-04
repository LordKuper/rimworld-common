---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-documentation]: CONCERNS

# Review — documentation

- **Phase**: impl-review
- **Iteration**: 3

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | `design/architecture/adr/adr-0007-staticstate-isolation-nunit-remap.html` (Context §, Decision §, Consequences neg #3, AC-24) + `design/architecture/tech-reference/nunit-4.6.1.md:37` | Persistent docs say the static-isolation seam applies to **three** classes (`StatWeightTests`, `StatefulSubsystemTests`, `StatRangesTests`). As-built there are **four**: `StatLimitTests` also `: StaticStateTestBase` and carries `[NonParallelizable]` (`StatLimitTests.cs:15,17`). The fourth fixture was added with the user-authorized +24 StatLimit coverage-recovery tests this sprint (per state.json) and the docs were not updated in lockstep. Concrete, falsifiable count drift in two persistent docs covering the very isolation contract ADR-0007 stresses must be maintained in lockstep; a maintainer reading "three" mis-identifies the protected set. | Update ADR-0007 (Context "Three test classes…", Decision "…to the three classes", Consequences negative, AC-24) and nunit-4.6.1.md:37 to enumerate all four static-touching classes incl. `StatLimitTests`. Domain owner = Architect (ADR) in a doc-reconciliation pass; reviewer does not edit persistent design/. |

## Verdict
CONCERNS: 1

## Next action
Architect updates ADR-0007 and the NUnit tech-reference to reflect four static-touching fixtures (add `StatLimitTests`). All other reconciled items verified accurate against code (see below) — no further action there.

## Verified accurate (no findings)
- **ADR-0006** matches as-built verbatim: `CopyRimWorldTestDeps` `AfterTargets="Build"` target in `LordKuper.Common.Tests.csproj:79-94` copies the documented DLL set incl. `netstandard.dll` 2.1 (PRIMARY discovery mechanism); namespace-less `[SetUpFixture] RimWorldResolverSetup` + `[OneTimeSetUp]` in `RimWorldResolverSetup.cs` is the FALLBACK runtime resolver; resolution contract (name-match set, `RIMWORLD_DIR`/`RimWorldDir`, `Managed\<name>.dll` `LoadFrom`, null-for-others) preserved; no `AppDomain.GetData/SetData` idempotency guard and no `[ModuleInitializer]` exist in code (correctly stated as absent); `[ModuleInitializer]` recorded as considered-and-rejected. No phantom delete-DLL step — `coverage.ps1` keeps RimWorld DLLs in bin through instrument + run; step 5 only restores the un-instrumented `LordKuper.Common.dll`.
- **ADR-0007** mechanism matches: snapshot/restore inlined in `StaticStateTestBase.cs` `[SetUp]`/`[TearDown]` (no `StaticStateFixture` type exists in code); captured set (`DefProvider.Current`, `StatHelper`/`WorkTypeStatMap` rebuild, reflection resets of `SkillStatMap._map`, `PassionHelper._isInitialized/_cachedPassions/PassionCache`, `StatRanges.Ranges`) matches the source exactly. Only the class-count enumeration is stale (finding #1).
- **stack.html** accurate: NUnit 4.6.1, FluentAssertions 7.2.2 (Apache-7.x rationale), NUnit3TestAdapter 6.2.0, AltCover via `scripts/coverage.ps1`, coverlet.collector abandoned-with-reason — all match csproj + script.
- **commands.yaml** coverage entry states 41.08% measured at sprint cut, consistent with state.json adjudication; floor >=37.2% recorded.
- **HTML shell / provenance**: both ADRs carry correct responsibility frontmatter, `provenance: original`, no provenance badge rendered (correct for original), required meta placeholders filled.

## Escalations
- none
