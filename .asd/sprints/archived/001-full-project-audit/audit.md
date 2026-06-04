---
responsibility:
  owns: brownfield findings for sprint scope (existing docs, code, gaps, risks)
  excludes: requirements, decisions, plan, code
  delegates_to: prd.html (requirements), adr.html (decisions), plan.md (tasks)
---

# Audit

## Scope reference
[sprint.md](./sprint.md) — Sprint 001-full-project-audit (shape B: audit + in-scope fixes). Full-project audit of code + docs against all `.asd/rules/*` workflow rules and the three custom rule files.

<!-- ========================================================= -->
<!-- DOCS SIDE (BA-owned). CODE side appended by Architect below. -->
<!-- ========================================================= -->

## Touched areas

Full-project audit — scope is the whole repo. Doc-bearing areas enumerated below:

- `README.md`: root project readme (2 lines, stub).
- `About/`: RimWorld mod metadata (`About.xml`, `Preview.png`, `PublishedFileId.txt`) — Steam Workshop publishing surface.
- `1.6/Languages/**`, `1.5/Languages/**`: keyed localization XML (user-facing UI strings, not design docs).
- `design/product/concept.html`: reverse-engineered project concept (authored this session, status=draft).
- `design/architecture/stack.html` + `design/architecture/tech-reference/*.md`: reverse-engineered tech stack + 6 tech-reference notes (authored this session, status=draft).
- `CLAUDE.md`: ASD workflow integration block (workflow infrastructure — not a product doc).
- `.asd/project/`: custom rule files, `decisions-log.md`, `config.yaml` (workflow infrastructure).
- No `Defs/` XML present in either version folder; no `LICENSE`, `CHANGELOG`, or `CONTRIBUTING`.

## Existing docs found

| Path | Type | Current state | Relevance |
|---|---|---|---|
| [README.md](../../../README.md) | Markdown readme | Stub — title + one-line description ("Common library for my Rimworld mods"). No usage, build, install, or API content. | Public-facing entry point. Thin; superseded in substance by `concept.html` + `stack.html`. |
| [About/About.xml](../../../About/About.xml) | RimWorld mod metadata (XML) | Complete & current. `name`, `author`, `packageId=LordKuper.Common`, `supportedVersions` 1.5/1.6, Harmony dependency, `loadAfter` (harmony, vanillaexpanded.skills), `description`. | SSoT for mod identity, supported versions, dependencies as seen by RimWorld/Steam. Authoritative — design docs must reconcile *to* it. |
| [About/PublishedFileId.txt](../../../About/PublishedFileId.txt) | Steam Workshop ID | Single ID `3531352422`. | Publishing identity. Machine data, not prose. |
| About/Preview.png | Workshop preview image | Present (binary). | User-facing Workshop asset. Out of doc-audit scope (image). |
| [1.6/Languages/English/Keyed/Common_Keyed.xml](../../../1.6/Languages/English/Keyed/Common_Keyed.xml) | Localization (keyed XML) | Complete — 116 keys (Stats, PawnType, PawnHealthState, PawnPrimaryWeaponType, Passions, Actions, WorkTypeThingRuleWidget, PawnFilter). | User-facing text asset (default/fallback language). Not a design doc. |
| [1.6/Languages/Russian/Keyed/Common_Keyed.xml](../../../1.6/Languages/Russian/Keyed/Common_Keyed.xml) | Localization (keyed XML) | Complete — full parity with 1.6 English. | User-facing text asset. Not a design doc. |
| [1.5/Languages/English/Keyed/Common_Keyed.xml](../../../1.5/Languages/English/Keyed/Common_Keyed.xml) | Localization (keyed XML) | **Stale subset** — 47 keys only. Missing all PawnType, PawnHealthState, PawnPrimaryWeaponType, Passions, PawnFilter keys present in 1.6. Has one key 1.6 lacks (`WorkTypeThingRuleWidget.SelectWorkType`). | User-facing text asset. Divergence vs 1.6 is a content finding (see code side / migration plan note). |
| 1.5/Languages — Russian | (absent) | No Russian keyed file under 1.5. | Localization gap: 1.6 ships RU, 1.5 does not. |
| [design/product/concept.html](../../../design/product/concept.html) | ASD Concept doc | Draft, `provenance=reverse-engineered`, `source=Source/ + csproj + README.md`. 6 sections. | Persistent product SSoT (in `design/`, pre-existing this session). In review pipeline. |
| [design/architecture/stack.html](../../../design/architecture/stack.html) | ASD Stack doc | Draft, `provenance=reverse-engineered`. | Persistent tech SSoT. In review pipeline. |
| [design/architecture/tech-reference/*.md](../../../design/architecture/tech-reference/) | Tech-reference notes (6 files) | Authored this session (harmony 2.4.2, rimworld-verse 1.6, unity-imgui, dotnet-framework 4.7.2, xunit 2.9.3, coverlet 6.0.4). | Supporting reference for stack. Persistent. |
| [CLAUDE.md](../../../CLAUDE.md) | Workflow integration | Generated ASD block. | Workflow infrastructure (English-mandated). Not a product doc; out of migration scope. |

## Documentation migration plan

Items found outside ASD format/location that should become (or reconcile with) persistent docs in `design/`. Concept and stack already live in `design/` and flow through the design pipeline, so they are NOT relisted here.

| # | Source (path/URL) | Format | Proposed target in `design/` | Type | Notes |
|---|---|---|---|---|---|
| 1 | `README.md` | Markdown | `design/product/concept.html` (already authored) | reverse-engineered | README content (title + one-liner) is fully subsumed by `concept.html`. **SSoT concern**: keep README as the GitHub/Workshop landing stub but make it point at / stay consistent with `concept.html`; do not let two prose sources of "what is this" drift. No new design doc needed — reconcile only. |
| 2 | `About/About.xml` | RimWorld metadata XML | (no migration — remains SSoT) | — | About.xml is the authoritative source for mod identity, `supportedVersions` (1.5/1.6), and dependencies. **SSoT concern**: `concept.html` / `stack.html` must reconcile *to* About.xml, not the reverse. Flag any drift: e.g. `loadAfter vanillaexpanded.skills` is a soft integration fact not yet reflected in design docs — candidate to note in stack/concept. |
| 3 | `1.5` + `1.6/Languages/**` keyed XML | Localization XML | (no migration — user-facing text assets) | — | Not design docs; do not promote into `design/`. Logged here only to flag the **1.5↔1.6 divergence** (1.5 EN is a 47-key stale subset; 1.5 has no RU) for the code side to triage as a content/parity finding, not a docs-migration action. |

<!-- No prose docs require net-new promotion into design/. concept.html + stack.html already cover the brownfield prose. -->

## DOCS-side rule compliance

Pass/fail per applicable doc-facing rule. Code/test rule compliance is assessed by the Architect on the code side.

### `.asd/rules/language-policy.md`

| Rule | Applies to | Verdict | Evidence |
|---|---|---|---|
| User-facing artifacts (PRD/ADR/design/* docs) in `language.docs` (=en) | `design/**`, `audit.md`, `sprint.md` | **PASS** | `concept.html`, `stack.html`, tech-reference `*.md`, this `audit.md`, `sprint.md` are all English. |
| Machine-readable files in English, dense | `About/PublishedFileId.txt`, `config.yaml`, `decisions-log.md` | **PASS** | English / numeric. |
| Workflow infrastructure (CLAUDE.md, rules) in English | `CLAUDE.md`, `.asd/rules/**`, `.asd/project/custom-*.md` | **PASS** | All English imperative prose. |
| File names / branches / commit subjects in English | repo-wide | **PASS** | All doc paths and recent commit subjects are English. |
| Localization assets — out of language-policy scope | `1.x/Languages/**` | **N/A** | Keyed XML are user-facing *game* strings (EN default + RU translation), governed by RimWorld localization, not the ASD language matrix. Russian content here is correct, not a violation. |

Language-policy verdict: **PASS** across all in-scope doc artifacts.

### `.asd/project/custom-design-rules.md` (doc-facing aspects)

| Rule | Verdict | Evidence / note |
|---|---|---|
| Modding & patchability — public surface documented as an integration contract | **PARTIAL** | `concept.html` names "dependent RimWorld mods" as the consumer and frames the library as an integration contract, but there is no per-subsystem requirements/API doc enumerating the public surface. Acceptable at audit baseline (no feature scope); flag as a future doc opportunity, not a sprint blocker. |
| Data-driven over hardcoded — design docs specify Def/config surface, not literals | **PASS (vacuous)** | No PRD/ADR introducing tunables exists this sprint; nothing to violate. Stats are surfaced as RimWorld `StatDef`-style keyed entries, consistent with the rule's spirit. |
| Determinism — documented where relevant | **N/A** | No design doc makes determinism claims requiring doc-level assertion this sprint. Belongs to code-side assessment. |

### `.asd/project/custom-common-rules.md` (doc-facing aspects)

| Rule | Verdict | Evidence / note |
|---|---|---|
| Project layout (Source=net472 prod, Tests=xUnit) accurately reflected in docs | **PASS** | `stack.html` + tech-reference docs match the stated layout (net472, Harmony 2.4.2 compile-only, xUnit). |

### `.asd/rules/artifact-layout.md` (SSoT / placement, doc-facing)

| Rule | Verdict | Evidence / note |
|---|---|---|
| Persistent design docs live under `design/`; drafts under `<sprint>/design/` | **PASS** | `concept.html`/`stack.html` are in persistent `design/` (authored directly this session as brownfield extraction, per decisions-log). No stray design prose elsewhere. |
| Single source of truth per fact | **CONCERN** | Two reconciliation points: (a) README one-liner vs `concept.html` vision; (b) `About.xml` `supportedVersions`/deps vs `stack.html`/`concept.html`. Neither currently contradicts, but both must be kept in sync. Tracked in Documentation migration plan #1 and #2. |

### Localization content findings (informational, for code-side triage)

- **1.5 keyed XML is stale** vs 1.6: 1.5/English has 47 keys; 1.6/English has 116. Missing in 1.5: all `PawnType.*`, `PawnHealthState.*`, `PawnPrimaryWeaponType.*`, `Passions.*`, `Actions.{Edit,Delete,Reset,Select}`, and the entire `PawnFilter.*` block. If 1.5 code references those keys at runtime, players on 1.5 will see raw key strings. Conversely 1.5 has `WorkTypeThingRuleWidget.SelectWorkType` which 1.6 lacks.
- **No Russian translation for 1.5**: 1.6 ships `Russian/Keyed`; 1.5 ships English only.
- These are user-facing-text/content findings, not ASD design-doc violations. Surfaced for the code side to decide in-scope/deferred.

<!-- ========================================================= -->
<!-- CODE SIDE (Architect-owned). Fill the sections below.       -->
<!-- ========================================================= -->

## Touched areas (code side)

Merges with the docs-side enumeration above. Full-project scope; code-bearing areas:

- `Source/` (~40 prod `.cs` files) under namespaces: root (`LordKuper.Common`), `Cache`, `Filters`/`Filters.Limits`/`Filters.PawnFilter`, `CustomStats`, `Helpers`, `UI`/`UI.Widgets`, `Compatibility`.
- `Source/LordKuper.Common.csproj`, `Source/Directory.Build.props`, `Source/LordKuper.Common.sln` — build surface.
- `Tests/` — single test file `Tests/Helpers/EnumHelperTests.cs`; `Tests/LordKuper.Common.Tests.csproj`.
- `.asd/project/commands.yaml`, `.asd/project/config.yaml` — build/lint/test command SSoT + workflow config.

## Existing implementation found

Concise subsystem map (decomposition disabled — informal grouping):

- **Mod entry / infra** — `CommonMod.cs` (`Mod` subclass; ctor runs `Harmony.PatchAll`, `Vse.Initialize`), `Logger.cs` (static wrapper over Verse `Log`), `Resources.cs` (translated `Strings` + `[StaticConstructorOnStartup]` `Textures`).
- **Time** — `RimWorldTime.cs`: `readonly struct`, total-hours arithmetic, comparison/equality operators, `GetMapTime`/`GetHomeTime`.
- **Cache** — `Cache/TimedCache.cs` (abstract interval gate), `Cache/DefCache.cs` (`IExposable`, lazy `Def` resolution by name, uses C# `field` keyword), `Cache/PassionCache.cs` (per-passion data record), `ThingCache.cs` (per-`Thing` stat-value dictionary with timed invalidation).
- **Stat infrastructure** — `Helpers/StatHelper.cs` (static ctor builds category→`StatDef` sets; `PostInit` nested `[StaticConstructorOnStartup]` retranslates custom-stat labels), `StatCategory.cs`, `StatRanges.cs` (running min/max normalization), `StatWeight.cs`, `SkillStatMap.cs` (skill→stats), `WorkTypeStatMap.cs` (worktype→weighted stats; **hardcoded default-weight table**), `WorkTypeThingRule.cs` (rule scoring + `IExposable`).
- **Custom stats** — `CustomStats/` (`MeleeWeaponStats`/`RangedWeaponStats`/`ToolStats` synthesize `StatDef`s from enums at type-init; `*Stat.cs` enums).
- **Filters** — `Filters/PawnFilter/` (`PawnFilter` filter model + `Combine`/`Copy`/`GetSummary`/`SatisfiesFilter`/`Validate`/`ExposeData`; `PawnType`/`PawnHealthState`/`PawnPrimaryWeaponType` enums), `Filters/Limits/` (`StatLimit`, `PawnSkillLimit`, `PawnCapacityLimit`, `PawnTraitLimit`).
- **Helpers** — `DefHelper`, `EnumHelper` (flag set ops; the only unit-tested type), `MathHelper` (range normalization), `PassionHelper` (passion cache + VSE-aware icon), `PawnHelper` (health-state/type/weapon/passion classification), `TextHelper`, `StatDefCategoryComparer`.
- **UI (IMGUI)** — `UI/` partial `Fields`/`Buttons`/`Checkboxes`/`Labels`/`Layout`/`ScrollView`/`Sections`/`Tabs`/`Windows`/`Icons` + `UI/Widgets/` (`PawnBox`, `PawnFilterWidget`, `ThingIconBox`, `WorkTypeThingRuleWidget`). Immediate-mode draw helpers over `Verse.Widgets`.
- **Compatibility** — `Compatibility/Vse.cs`: reflection/Harmony `AccessTools` bridge to Vanilla Skills Expanded (`vanillaexpanded.skills`); fully guarded, all-or-nothing init with try/catch.

## Rule-compliance findings (code side)

Per category: PASS / PARTIAL / FAIL with `file:line` evidence. Verdicts are static-analysis judgments — the build/lint was not executed this phase (no live RimWorld managed dir / `jb` CLI available here), so build-dependent verdicts are marked *(static, unverified by build)*.

### custom-coding-rules · Nullability (`<Nullable>enable</Nullable>`)

**PASS (static, unverified by build).** `Source/LordKuper.Common.csproj:8` sets `enable`. Reference types are annotated consistently across the tree: `string?`/`T?` fields and nullable returns are used deliberately — e.g. `Cache/DefCache.cs:20,25,49,61` (`T? _def`, `string? _defName`, `T? Def`), `Compatibility/Vse.cs:24,29,44` (nullable `FieldInfo?`/`PropertyInfo?`/delegate), `WorkTypeThingRule.cs:20,35,40`, `RimWorldTime.cs:70,125` (`object? obj`). Boundary null-guards throw `ArgumentNullException` rather than relying on flow analysis (`StatHelper.cs:161-162`, `PawnHelper.cs:77`, `PawnFilter.cs:173-174`). No `#nullable disable` anywhere. Residual risk: RimWorld/Unity reference assemblies are oblivious-null, so a clean build under `enable` depends on annotation discipline at every Verse interop point — only a real compile confirms zero CS86xx warnings.

### custom-coding-rules · Zero warnings (`TreatWarningsAsErrors` + `WarningLevel 9999`)

**PARTIAL / AT-RISK (static, unverified by build).** Flags set for both configs (`csproj:24-32`). Likely warning sources to verify before claiming green:
- **Language-version / `field` keyword**: `LangVersion=latest` (`csproj:7`) with `<TargetFramework>net472</TargetFramework>`. Code uses the C# 13 contextual `field` keyword in property accessors — `Cache/DefCache.cs:66`, `Cache/PassionCache.cs:44`. Under a current SDK this compiles, but `field` emits a known analyzer warning in some toolsets when a member named `field` could be ambiguous; with `WarningLevel 9999` + warnings-as-errors this is the single highest build-break risk. Confirm the pinned SDK treats `field` cleanly.
- **`obj/` framework drift**: `Source/obj/.../v4.8.AssemblyAttributes.cs` and `Tests/obj/.../net8.0` + `net472` artifacts coexist (stale intermediate output vs declared `net472`). Not a source warning per se, but signals the project has been built under mixed SDKs/targets — a clean rebuild is needed to trust any zero-warning claim.
- **`Tests` project is NOT covered by the zero-warnings policy**: `Directory.Build.props` lives under `Source/` only (`Source/Directory.Build.props`), so `Tests/LordKuper.Common.Tests.csproj` inherits neither `TreatWarningsAsErrors` nor `WarningLevel 9999`. Test code can warn freely. This is a policy gap, not a code defect.

### custom-coding-rules / code-style §7 · XML docs on public members (`GenerateDocumentationFile=True`)

**PASS.** `csproj:17` enables doc generation. Public-API doc coverage is thorough and consistent across spot-checks: every public type/member in `Logger.cs`, `RimWorldTime.cs`, `Cache/*`, `Helpers/EnumHelper.cs`, `Helpers/StatHelper.cs`, `Helpers/PawnHelper.cs`, `WorkTypeThingRule.cs`, `Filters/PawnFilter/PawnFilter.cs`, `UI/Fields_Sliders.cs`, `Compatibility/Vse.cs`, and `Resources.cs` carries `<summary>` plus `<param>`/`<returns>`/`<exception>` where applicable. With `GenerateDocumentationFile=True` + warnings-as-errors, any missing doc on a public member would already break the build, which corroborates the high coverage. Minor: `Compatibility/Vse.cs:14` (`public static class Vse`) and `Helpers/EnumHelper.cs:9` carry summaries but the class-level docs are thin relative to behavior — acceptable.

### custom-coding-rules · Analyzer / suppression policy

**PASS.** Zero comment-pragma suppressions and zero `[SuppressMessage]` across `Source/` (grep for `#pragma warning` / `SuppressMessage` / `ReSharper disable` → no matches). Suppression is expressed only through the *preferred* attribute route per the rule: `[PublicAPI]` (JetBrains.Annotations) on intentionally-public-but-unreferenced API surface (e.g. `Logger.cs:11`, `RimWorldTime.cs:11`, `WorkTypeStatMap.cs:14`, throughout). For a library whose consumers are external mods, `[PublicAPI]` is a legitimate "used implicitly" signal, not an unjustified silencing. No bare suppressions needing justification exist.

### custom-coding-rules · Logging (project `Logger`)

**PASS.** Verse `Log.*` is called in exactly one place — inside the `Logger` wrapper itself (`Logger.cs:33,53,73`). No `Console.*`/`Debug.Log`/direct `Log.*` anywhere else in `Source/` (grep confirmed). All subsystems route through `Logger.LogError/LogWarning/LogMessage` (e.g. `StatHelper.cs:178,217`, `PassionHelper.cs:59`, `Vse.cs:173,210`, `WorkTypeThingRule.cs:85`, and `#if DEBUG`-gated trace in `PawnFilter.cs`/`SkillStatMap.cs`). Debug-only spam is correctly compiled out of Release via `#if DEBUG`. Levels are meaningful (error for failures, warning for recoverable stat-eval misses, message/debug for state).

### custom-coding-rules · xUnit static-state isolation

**PASS (vacuously) / GAP.** The only test file, `Tests/Helpers/EnumHelperTests.cs`, tests `EnumHelper` — pure static functions over a local `[Flags] TestFlags` enum with **no global/cache mutation**, so the save/restore rule is not triggered and is not violated. However this is a **coverage gap, not compliance**: the genuinely stateful, isolation-sensitive subsystems (`StatHelper` static ctor + `PostInit`, `WorkTypeStatMap`/`SkillStatMap`/`StatRanges`/`PassionHelper` lazy static caches, `TimedCache`/`ThingCache`) have **zero tests**. The moment any of those gets a test it will need `IDisposable`/`IClassFixture` save-restore — none exists yet. Also: no `[Collection]`/fixture scaffolding present, and Test project lacks the zero-warning policy (see above).

### custom-design-rules · Modding & patchability

**PASS (with one note).** Methods are small and stable; public entry points are clear; the library is explicitly designed as an integration contract (`[PublicAPI]` pervasive). No mod extension points are sealed unnecessarily — caches expose `virtual Update`/`ExposeData` (`TimedCache.cs:47`, `ThingCache.cs:54`, `DefCache.cs:71`) and base types are `abstract`/non-sealed. Static constructors **do** run side-effecting work: `StatHelper` static ctor (`StatHelper.cs:105-114`) walks the entire `StatDef` database, and `[StaticConstructorOnStartup]` blocks (`StatHelper.PostInit`, `Resources.Textures`, `MeleeWeaponStats`/etc. type-init synthesizing `StatDef`s) do real work at game load. This is **idiomatic RimWorld** (that is how mods hook startup) and is guarded/lazy, so it does not violate "no heavy static ctors" in spirit — but it is the main patchability-risk surface and worth a design note: failures there happen at load with no try/catch around the DefDatabase walks (cf. Fail-Fast-on-Load, which this actually satisfies).

### custom-design-rules · Data-driven over hardcoded

**FAIL (one concrete violation).** `WorkTypeStatMap.cs:32-47` hardcodes a balance/tuning table — `DefaultWorkTypeStats` literally maps `"Cooking"`/`"Hunting"`/`"Doctor"` defNames to stat defNames and weight multipliers (`FoodPoisonChance→2f`, `ButcheryFleshEfficiency→1.5f`, `HuntingStealth→2f`, `MedicalPotency→2f`, etc.). The rule states "Stat/balance/tuning values come from RimWorld `Def`s or config, never hardcoded literals in code." These are exactly such tuning constants embedded in code. Secondary literals of the same family: per-recipe default weights `0.8f`/`0.5f` (`WorkTypeStatMap.cs:121,126,129,131`) and `1f` skill default (`:110`). Note: the UI step/threshold literals (`Fields_Sliders.cs:314-317`, `MathHelper.cs:24`) are presentation/epsilon constants, not game balance, and are out of this rule's scope. → see IMP-07.

> **Reclassified 2026-06-03 (user):** the default weights are intentional seed defaults overridden by consuming mods via persisted settings (`IExposable` StatWeight/WorkTypeThingRule) — NOT a prohibited hardcoded-balance violation. Retained action: log via Logger.LogWarning on null StatDef resolution (was silent). No Def extraction. The original FAIL above is preserved for the audit record; the live verdict for this finding is withdrawn to logging-only (see reframed IMP-07).

### custom-design-rules / code-style §11 · Determinism

**PASS (with a documented caveat).** Core stat math, filtering, flag ops, and time arithmetic are pure functions of inputs (`MathHelper`, `EnumHelper`, `RimWorldTime`, `PawnFilter.SatisfiesFilter`, `WorkTypeThingRule.GetThingScore`). Caveat — **order/observation dependence in `StatRanges`** (`StatRanges.cs:25-49`): `NormalizeStatValue` mutates a running global min/max per `StatDef` as values are observed, so the normalized score of a given `Thing` depends on what was scored before it (and persists for process lifetime). `WorkTypeThingRule.GetThingScore/GetThingDefScore` (`WorkTypeThingRule.cs:180-203`) therefore are **not** referentially transparent across calls. This is an intentional adaptive-normalization design, but it is incidental order-dependence in core scoring logic and is undocumented as such — flag as a design note / determinism caveat (IMP-08), not necessarily a fix.

### code-style + design-principles · Simplicity Default / over-engineering smells

**PASS — low over-engineering.** No interface-with-one-impl, no speculative generics, no factory-for-<3, no plugin-without-plugin. `DefCache<T>` generic has multiple real `T` (`StatDef` + others via `DefDatabase<T>`). The `Vse` reflection bridge is justified by a real external mod. Two mild smells worth noting (low severity): (a) `Resources.Strings.PawnFilter` hand-rolls 18 cached tri-state-tooltip fields + near-identical `GetFilter*Tooltip` methods (`Resources.cs:99-544`) — heavy repetition that a small table/helper could collapse (IMP-04); (b) `PawnFilter.Combine` (`PawnFilter.cs:171-275`) is a ~100-line method of repeated `if main.HasValue … else fallback …` blocks — a paragraph-long function the style rule says to split (IMP-05).

## Gaps

- **Test coverage far below the 80% floor** (code-style §17). One test file exercises only `EnumHelper`. Stateful core (`StatHelper`, `WorkTypeStatMap`, `SkillStatMap`, `StatRanges`, `PassionHelper`, caches, `PawnFilter`, `WorkTypeThingRule`, `RimWorldTime`) is untested. Most of it needs RimWorld `DefDatabase`/game context, so reaching 80% requires an abstraction/isolation seam that does not yet exist (cf. custom-coding-rules "RimWorld APIs … must be abstracted or guarded").
- **Tests project not under zero-warnings / nullability-strict governance** — `Directory.Build.props` is `Source/`-scoped only; no root or `Tests/` equivalent.
- **No determinism/contract doc** for the adaptive `StatRanges` normalization behavior (consumers may assume score stability).
- **Tech-reference coverage**: chosen tech is harmony, rimworld-verse, unity-imgui, dotnet-framework, xunit, coverlet — all six `design/architecture/tech-reference/*.md` notes were authored this session, so **no tech-reference gap**. (One nuance to reconcile: `stack.html`/tech-ref state Harmony 2.4.2 = the *compile* version in `csproj:35`; the seeded "Harmony 2.3.6" refers to a legacy `packages/` folder that no longer exists — see IMP-01.)

## Risks

- **Build may not be warning-clean under `latest`+net472**: impact=`TreatWarningsAsErrors` turns any `field`-keyword or interop-null warning into a build failure; mitigation=run `jb-cleanup`→`build`→`jb-inspect` on the pinned SDK before trusting green; pin `LangVersion` to an explicit version rather than `latest` so SDK upgrades cannot silently change `field` semantics.
- **Hardcoded balance table drift** (`WorkTypeStatMap`): impact=stat/weight tuning lives in code, so balance changes need a recompile and can desync from Def reality (a renamed/removed vanilla `StatDef` silently no-ops via `GetNamedSilentFail`); mitigation=move to a Def/config surface (IMP-07).
- **Order-dependent scoring** (`StatRanges`): impact=non-reproducible `Thing` scores across sessions / call orders; mitigation=document as adaptive-by-design or seed ranges from `StatDef` min/max (IMP-08).
- **Static-init failure at load**: impact=an exception inside a `[StaticConstructorOnStartup]`/static ctor (DefDatabase walks in `StatHelper`/`WorkTypeStatMap`) aborts mod load; mitigation=acceptable per Fail-Fast-on-Load, but ensure `Logger` context is emitted (currently these walks are unguarded).
- **Localization parity** (BA-surfaced, code-relevant): impact=1.5 keyed XML has 47 keys vs 1.6's 116; runtime `.Translate()` calls in `Resources.cs` for `PawnType`/`PawnHealthState`/`PawnPrimaryWeaponType`/`Passions`/`PawnFilter` keys will render raw key strings for 1.5 players, and 1.5 ships no Russian; mitigation=sync 1.5 keys or confirm 1.5 build does not surface those widgets (IMP-09).

## Improvement opportunities

User decides in/out at the audit gate; the call below is a recommendation only.

| ID | Description | File:line | Category | Risk | Recommended |
|---|---|---|---|---|---|
| IMP-01 | **Seeded**: legacy unused `Source/packages/` folder (Harmony 2.3.6, MSTest 3.10.2) cleanup. **Verified absent** — no `packages/` dir on disk or in git; `csproj` uses PackageReference (Harmony 2.4.2 compile-only). Action reduces to confirming nothing references it and recording closure. | `Source/` (folder not present) | cleanup | low | **In-scope (approved by user)** |
| IMP-02 | **Seeded**: hardcoded RimWorld path fallback `d:\Games\SteamLibrary\...\RimWorld` in build props. Already overridable via `RIMWORLD_DIR` env / `RimWorldDir` prop, so functional — but the literal default ties the build to one machine and risks per-dev edits. Consider documenting `RIMWORLD_DIR` as required (drop the hardcoded fallback or make absence fail-fast with a clear message). | `Source/Directory.Build.props:4` | maintainability | low | **In-scope (approved by user)** |
| IMP-03 | Pin `LangVersion` to an explicit version (e.g. the C# rev the `field` keyword needs) instead of `latest`, so an SDK bump can't silently change `field`/`collection-expr` semantics under warnings-as-errors. | `Source/LordKuper.Common.csproj:7` | correctness | low | **Won't-do (user 2026-06-03): LangVersion stays `latest`; SDK-drift risk accepted.** |
| IMP-04 | Collapse the 18 near-identical cached tri-state tooltip fields + `GetFilter*Tooltip` methods into a single parameterized helper / lookup. Large DRY win. | `Source/Resources.cs:99-544` | simplify | med | **In-scope (approved by user)** |
| IMP-05 | Split `PawnFilter.Combine` (~100 lines of repeated HasValue/else blocks) into a per-section helper to satisfy the "function needing a paragraph" rule. | `Source/Filters/PawnFilter/PawnFilter.cs:171-275` | maintainability | med | **In-scope (approved by user)** |
| IMP-06 | Establish zero-warnings + `Nullable=enable` governance for the **Tests** project (add a root or `Tests/`-scoped `Directory.Build.props`, or a `Directory.Build.props` at repo root). | `Tests/LordKuper.Common.Tests.csproj` | maintainability | low | **In-scope (approved by user)** |
| IMP-07 | Move the hardcoded `DefaultWorkTypeStats` balance table (and the `0.8f`/`0.5f`/`1f`/`1.5f`/`2f` weight literals) to a Def/config surface per data-driven rule. | `Source/WorkTypeStatMap.cs:32-47,110,121-131` | correctness | high | **Reframed → logging-only (no Def surface); weights stay as overridable seed defaults.** |
| IMP-08 | Document (or make deterministic) the adaptive `StatRanges` running-min/max normalization so consumers know scores are observation-order-dependent; option: seed ranges from `StatDef` bounds. | `Source/StatRanges.cs:25-49` | correctness | med | **In-scope (approved by user)** |
| IMP-09 | **BA-folded**: sync stale 1.5 keyed localization (47 keys) up to 1.6 parity (116 keys) incl. missing Russian, OR confirm 1.5 never surfaces those widgets. Runtime `.Translate()` in `Resources.cs` would show raw keys to 1.5 players. | `1.5/Languages/**`, consumed by `Source/Resources.cs` | correctness | med | **Won't-do (user 2026-06-03): RimWorld 1.5 is a conserved frozen archive — no localization/parity changes.** |
| IMP-10 | **BA-folded**: reflect `loadAfter`/soft-dep `vanillaexpanded.skills` (modelled in `Compatibility/Vse.cs`) in `stack.html`/`concept.html` as an integration fact, reconciling design docs to `About.xml`. | `Source/Compatibility/Vse.cs:169`; `About/About.xml`; `design/architecture/stack.html` | maintainability | low | **In-scope (approved by user)** |
| IMP-11 | Bootstrap real test coverage toward the 80% floor: add an isolation seam for RimWorld `DefDatabase`/game context and first tests for `RimWorldTime`, `MathHelper`, `PawnFilter` pure paths; wire xUnit `IClassFixture` save/restore for the static caches. | `Tests/`; seams in `Helpers/StatHelper.cs`, `WorkTypeStatMap.cs` | maintainability | high | **Delivered at achieved level (user-accepted 2026-06-04): AltCover coverage measurement + 142 passing tests; 38.2% testable-core Visited Points. Full 80% not pursued (harness scope).** |
| IMP-12 | Clean stale build intermediates / mixed-target `obj/` artifacts (`v4.8`, `net8.0` alongside declared `net472`) and confirm a clean rebuild; prevents misleading warning/coverage runs. | `Source/obj/**`, `Tests/obj/**` | cleanup | low | **In-scope (approved by user)** |

> User decision 2026-06-03: all 12 improvement opportunities approved IN-SCOPE for sprint 001 (shape B). Sequencing/risk handled in plan phase. Notable: IMP-07 requires a Def-schema ADR; IMP-11 (test-coverage seam) is large; IMP-01 reduces to verify-and-close (legacy packages/ confirmed absent).

## Related open stubs

`.asd/project/stubs.md` is absent — **no related open stubs (stubs.md absent)**.

<!-- | — | — | no related open stubs | — | -->
