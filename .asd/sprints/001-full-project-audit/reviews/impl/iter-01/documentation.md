[REVIEW-impl-documentation]: CONCERNS

# Impl-review · documentation · sprint 001-full-project-audit · iter-01

Scope: persistent `design/` actuality vs implementation, SSoT integrity, traceability, provenance, custom-rules consistency. Severity floor: all (iteration 1). language.docs=en.

## Findings

| # | Severity | Category | Location | Finding |
|---|----------|----------|----------|---------|
| 1 | high | Doc-vs-code drift | `design/architecture/adr/adr-0003-build-governance.html` (Decision bullet 2; Positive consequence 1; AC-3) vs `Tests/Directory.Build.props` | ADR-0003 asserts in three places that the Tests project has **no** child `Directory.Build.props` and inherits the repo-root file automatically via MSBuild nearest-file auto-import ("The Tests project has **no** child Directory.Build.props... needs no explicit import"; "The Tests project (no child Directory.Build.props) picks up the root file automatically"; AC-3 "auto-inherited by Tests"). In reality `Tests/Directory.Build.props` exists, carries an explicit `GetPathOfFileAbove` import of the root **and** its own RimWorld path-resolution `PropertyGroup` + `CheckRimWorldDir` target (a near-duplicate of `Source/Directory.Build.props`). Governance *outcome* is equivalent (Tests still inherits the root governance props), so this is not a governance regression — but the ADR's described mechanism is factually wrong vs the shipped layout. The ADR must be corrected to describe the actual explicit-import-in-both-children layout. (Reviewer does not edit persistent `design/`; correction is owed by the Architect in design-promote.) |
| 2 | low | SSoT / duplication | `Source/Directory.Build.props` and `Tests/Directory.Build.props` | The RimWorld-path resolution block (RimWorldDir/RimWorldManagedDir property group + `CheckRimWorldDir` error target) is duplicated verbatim across the two child props files. ADR-0003's own Alternatives section rejects "duplicate into the Tests project" as an SSoT violation, yet the path-resolution logic is now duplicated (governance props are correctly centralized; path resolution is not). Either is defensible as code, but it directly contradicts the ADR narrative and the SSoT iron rule. Surface for the Architect together with finding #1. |
| 3 | low | Doc precision | `adr-0002-statranges-adaptive-normalization.html` (Decision bullet 2; AC-9) vs `Source/StatRanges.cs` | ADR-0002 calls `StatRanges.NormalizeStatValue` an "affected public member" and says the adaptive XML doc is added to the "public members". The method is `internal static`, not public. The adaptive XML-doc note IS present and correct (`ADAPTIVE behavior (ADR-0002...)`), so the substantive AC-9 obligation is met; only the "public" characterization is imprecise. `WorkTypeThingRule.GetThingScore`/`GetThingDefScore` ARE public and correctly documented. |

## Verified actual (no drift)

- **ADR-0001 / IDefProvider seam** — `Source/IDefProvider.cs` exposes exactly the seam the ADR describes: `AllDefs<T>`, `AllDefsListForReading<T>`, `GetNamedSilentFail<T>`, `WorkTypeDefsInPriorityOrder`. Default `VerseDefProvider` (nested in `DefProvider.cs`) is a thin pass-through to `DefDatabase<T>`/`WorkTypeDefsUtility`; `DefProvider.Current` is the static injection point. ADR does not mandate a separate file, so the nested impl is fine.
- **ADR-0001 / Rebuild() extraction** — `StatHelper.Rebuild()` and `WorkTypeStatMap.Rebuild()` exist and are invoked from their static ctors (load-time behavior preserved) and from `StaticStateFixture`. `InternalsVisibleTo` for Tests present (`Source/LordKuper.Common.csproj`). `StaticStateFixture` snapshots/restores the declared set incl. `StatRanges.Ranges` (AC-16).
- **ADR-0002 / adaptive XML docs** — present and consistent on `StatRanges.NormalizeStatValue`, `WorkTypeThingRule.GetThingScore`, `WorkTypeThingRule.GetThingDefScore`, each citing ADR-0002 and the set-and-order/observation-history contract. No reproducibility test asserted, as the ADR specifies.
- **ADR-0003 / explicit import (Source)** — `Source/Directory.Build.props` carries the exact `GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../')` import the ADR mandates. Repo-root `Directory.Build.props` hoists `TreatWarningsAsErrors=true`, `WarningLevel=9999`, `Nullable=enable`; no `LangVersion` pin (matches IMP-03 won't-do). Fail-fast: hardcoded `d:\Games\...` default removed; `CheckRimWorldDir` target errors when `RimWorldDir`/`RIMWORLD_DIR` unresolved (AC-2). The explicit-import requirement is satisfied for Source — the drift in #1 is only that the ADR claims Tests does NOT do this when in fact it does.
- **stack.html ↔ About.xml ↔ code** — `supportedVersions` 1.5+1.6, Harmony as sole hard mod dependency (`brrainz.harmony`), VSE (`vanillaexpanded.skills`) as `loadAfter` soft-dep all agree across `About/About.xml`, `stack.html`, and `Source/Compatibility/Vse.cs` (reflection-guarded, all-or-nothing init). No dependency/version SSoT drift.
- **coverage command** — `commands.yaml custom.coverage` (`powershell ... scripts\coverage.ps1`) and `scripts/coverage.ps1` agree on the AltCover approach, the RIMWORLD_DIR requirement, the AltCover global-tool requirement, and the denominator exclusion set (UI, Resources, *WeaponStats/ToolStats, CommonMod, Compatibility.Vse, Logger, PawnHelper, PassionHelper). Consistent.
- **Provenance flags** — ADRs and concept = `original` (no badge, correct). `stack.html` = `reverse-engineered` with `source` set and the warn badge rendered, correct.

## Accepted decisions (not flagged, per dispatch)

- AC-21 coverage at 38.2% — accepted, out of scope for this reviewer.
- XML-doc / comment density following existing code conventions — accepted.
- `StaticStateFixture` resetting SkillStatMap/PassionHelper via reflection rather than a `Rebuild()` (a test-file comment says "rebuilt via internal Rebuild()" while the code uses reflection) — internal test-comment nitpick, not a persistent-doc drift; not raised.

## Verdict

**CONCERNS.** No FAIL-class SSoT violation or missing artefact. One high-severity doc-vs-code drift (#1): ADR-0003's Tests-inheritance mechanism is contradicted by the shipped `Tests/Directory.Build.props`, compounded by duplicated path-resolution logic (#2) that contradicts the ADR's own anti-duplication stance. ADR-0002's "public member" wording is imprecise (#3) but the substantive AC-9 documentation obligation is met. All other ADR/stack/concept/commands facts match the implementation.

## Next action

Architect (design-promote phase) to correct ADR-0003 to describe the actual layout — both `Source/` and `Tests/` carry a child `Directory.Build.props` with an explicit `GetPathOfFileAbove` import plus their own RimWorld path resolution — and reconcile the Alternatives "no duplication" claim with the duplicated path-resolution block, or note the duplication as an accepted bounded exception. Minor: relabel ADR-0002's `NormalizeStatValue` reference from "public member" to "internal member". Reviewer does not edit persistent `design/`.

## Escalations

None requiring user input. No ambiguous SSoT classification.
