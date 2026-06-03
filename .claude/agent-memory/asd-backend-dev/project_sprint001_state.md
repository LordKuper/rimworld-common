---
name: project_sprint001_state
description: Current state of sprint 001-full-project-audit tasks; which are done and what is next
metadata:
  type: project
---

Sprint `001-full-project-audit` is in progress on branch `sprint/001-full-project-audit`.

**Task 0** (compile-green) — DONE (commit 20b213d).
**Task 1** (build governance ADR-0003) — DONE (commit bd7e2d9). Repo-root `Directory.Build.props` created; `Source/Directory.Build.props` updated with explicit import + fail-fast; `Source/LordKuper.Common.csproj` de-duplicated.
**Task 2** (verify-close packages/) — DONE (commit bd7e2d9). Verified Source/packages/ never existed; PackageReference only.
**Task 3** (IDefProvider seam, ADR-0001) — DONE (commits de59b70, 25475cd, 121592c).
  - New files: `Source/IDefProvider.cs`, `Source/DefProvider.cs` (VerseDefProvider inline).
  - InternalsVisibleTo(LordKuper.Common.Tests) in csproj.
  - Rerouted: StatHelper (×2 DefDatabase), WorkTypeStatMap (WorkTypeDefsUtility + 2× DefDatabase), SkillStatMap (×2 DefDatabase), WorkTypeThingRule (AllDefs + GetNamedSilentFail), DefCache<T> (GetNamedSilentFail).
  - Rebuild() extracted: StatHelper.Rebuild(), WorkTypeStatMap.Rebuild(). Static ctors delegate to them.
  - Logger.LogError context added on provider failure in all static-init paths (AC-24).
  - StatWeight and PassionHelper had no direct DefDatabase calls; no reroute needed.
  - Build: 0 warnings, 0 errors. Tests: 15/15 pass.

**Task 4** (WorkTypeStatMap null-stat logging, AC-8) — DONE (commit e85b3e8). Logger.LogWarning emitted with worktype + stat-name when GetNamedSilentFail<StatDef> returns null. Default weights stay in code.
**Task 5** (StatRanges adaptive XML docs, AC-9) — DONE (commit 09c950f). Remarks blocks added to NormalizeStatValue, GetThingScore, GetThingDefScore documenting ADAPTIVE/order-dependent contract per ADR-0002.
**Task 6** (Collapse Resources tooltip DRY, AC-6) — DONE (commit 498a5fb). 18 backing fields + 9 per-category methods replaced with 2 Dictionary caches + 1 private GetFilterTooltip helper. 9 public signatures unchanged.
**Task 7** (Split PawnFilter.Combine, AC-7) — DONE (commit cabb4b7). 100-line Combine decomposed into 9 private static per-section helpers. HasValue/fallback semantics preserved exactly.

**Why:** Sprint resolves all 27 ACs from the full-project-audit PRD. Task 3 was the critical-path gate for Tasks 8-11 (test harness and coverage).

**How to apply:** Next tasks: Tasks 8-11 (test harness/coverage, depend T3). T12 (IMP-10 docs reconciliation, verify-only). T13 (final cross-cutting gate, depends all).

**IDefProvider member list settled:**
- `IReadOnlyList<T> AllDefsListForReading<T>() where T : Def`
- `IEnumerable<T> AllDefs<T>() where T : Def`
- `T? GetNamedSilentFail<T>(string? defName) where T : Def`
- `IReadOnlyList<WorkTypeDef> WorkTypeDefsInPriorityOrder()`

**Breaking-API note (AC-26):** `DefProvider.Current` is a new public mutable static (additive, not breaking). No previously-public statics were removed or had signatures changed.

**Key structure facts:**
- Root `Directory.Build.props`: TreatWarningsAsErrors, WarningLevel 9999, Nullable=enable (SSoT)
- `Source/Directory.Build.props`: explicit GetPathOfFileAbove import + RimWorld path resolution + fail-fast error target
- `Source/LordKuper.Common.csproj`: LangVersion=latest, InternalsVisibleTo(Tests), no Nullable/TreatWarningsAsErrors (governed by root)
- `Tests/LordKuper.Common.Tests.csproj`: auto-inherits root, no child Directory.Build.props
- `RIMWORLD_DIR` env var is the documented override (D:\Games\Steam\steamapps\common\RimWorld valid on dev machine)
