---
responsibility:
  owns: brownfield findings for sprint scope (existing docs, code, gaps, risks)
  excludes: requirements, decisions, plan, code
  delegates_to: prd.html (requirements), adr.html (decisions), plan.md (tasks)
---

# Audit

## Scope reference
[sprint.md](./sprint.md)

<!-- ========================================================================= -->
<!-- DOCS SIDE (BA) — filled below.                                            -->
<!-- CODE SIDE (Architect) — placeholders only; Architect appends findings.    -->
<!-- ========================================================================= -->

## Touched areas

> Docs-side only. Architect appends source-code areas.

- `design/architecture/tech-reference/xunit-2.9.3.md`: project-vetted reference for the entire xUnit test stack (xunit 2.9.3 + Microsoft.NET.Test.Sdk 17.14.1 + xunit.runner.visualstudio 2.8.2). Scope removes/replaces this stack, so the reference is fully superseded by an NUnit + NUnit3TestAdapter + FluentAssertions reference.
- `design/architecture/tech-reference/coverlet-collector-6.0.4.md`: reference for coverlet.collector 6.0.4. Coverage is in fact driven by AltCover via `scripts/coverage.ps1`, not the coverlet collector path this doc describes; scope touches the coverage runner. Reference is stale relative to actual tooling and the migration.
- `design/architecture/stack.html`: lists xunit, Microsoft.NET.Test.Sdk, xunit.runner.visualstudio, and coverlet.collector across the Frameworks, Version-pinning, and Tooling tables, and tags the test language chip as "Tests/ (xUnit)". Every test-stack row changes when xUnit→NUnit + FluentAssertions lands.
- `design/architecture/adr/adr-0001-rimworld-context-isolation-seam.html`: approved ADR whose Decision/Consequences/Acceptance sections describe the static-isolation seam in xUnit terms (`StaticStateFixture` via `IClassFixture` + `[Collection]` serialization, `IDisposable` snapshot/restore). The mechanism remaps to NUnit (`[SetUpFixture]`/`[NonParallelizable]` + `[SetUp]`/`[TearDown]`) under this sprint; the ADR's xUnit-specific prose becomes inaccurate.
- `design/architecture/adr/adr-0003-build-governance.html`: approved ADR governing Source + Tests via repo-root `Directory.Build.props`. Does not name xUnit but governs the Tests project whose package set changes; cross-check that no governance assumptions break when test packages swap.
- `.asd/project/custom-coding-rules.md` (section "Testing (xUnit)", lines 41-45): coding rule titled and worded around xUnit (`IClassFixture`/`ICollectionFixture`, constructor + `IDisposable` isolation). Title and mechanism wording become wrong post-migration. NOTE: `.asd/project/` is project rules, not workflow infrastructure under `.asd/rules/`; flagged for update but BA does not edit it directly.
- `.asd/project/custom-common-rules.md` (line 14): names the Tests project as "xUnit, net472". One-line stack mention to update to NUnit.
- `.asd/project/commands.yaml` (lines 18-23): documents the AltCover `coverage` command and the `test` command. Machine-only SSoT; comments reference the AltCover flow. The runner-facing wording is touched when the suite moves to the NUnit3 adapter.
- `scripts/coverage.ps1`: coverage harness. Header comment names `coverlet.collector` as the failing alternative and the `altcover` invocation carries `--assemblyFilter xunit` (line 40-41). The xunit assembly filter and any xUnit-coupled assumptions must change with the framework swap. (Script is also code-side; surfaced here because its embedded documentation/comments are doc-relevant.)

## Existing docs found

> Docs relevant to the test suite, test framework, and coverage/CI tooling. Quotes rendered from the source; paths cite the original.

- [xUnit @ 2.9.3 tech-reference](../../../design/architecture/tech-reference/xunit-2.9.3.md): "This reference covers the whole xUnit-based test stack as one unit, since the three packages are version-coupled" — xunit 2.9.3, Microsoft.NET.Test.Sdk 17.14.1, xunit.runner.visualstudio 2.8.2. Documents `[Fact]`/`[Theory]` + `[InlineData]`, `Xunit.Assert.*`, the global `Using Include="Xunit"`, and the `PrivateAssets=all` runner convention. Entirely about the stack being removed.
- [coverlet.collector @ 6.0.4 tech-reference](../../../design/architecture/tech-reference/coverlet-collector-6.0.4.md): "NuGet PackageReference in Tests/LordKuper.Common.Tests.csproj, version 6.0.4 ... VSTest data collector form (`--collect:"XPlat Code Coverage"`)". Describes the coverlet collector path. Conflicts with actual coverage tooling (AltCover, see below) — coverage in practice runs through `scripts/coverage.ps1`, not this collector.
- [Stack — rimworld-common](../../../design/architecture/stack.html): reverse-engineered stack doc (status: draft, updated 2026-06-03). Lists the full xUnit + coverlet test tooling in three tables and a "Tests/ (xUnit)" language chip. Test-stack rows are all migration-affected.
- [ADR-0001 · RimWorld context isolation seam](../../../design/architecture/adr/adr-0001-rimworld-context-isolation-seam.html): approved. Decision adds "an xUnit `StaticStateFixture` (used via `IClassFixture` plus a `[Collection]` to serialize across classes that touch shared statics)"; Consequences note "Cross-class test serialization via `[Collection]` reduces test parallelism". The isolation contract is correct; only the xUnit vocabulary is migration-affected.
- [ADR-0002 · StatRanges adaptive normalization](../../../design/architecture/adr/adr-0002-statranges-adaptive-normalization.html): approved. Referenced by ADR-0001 for the `StatRanges.Ranges` cache included in the snapshot set; relevant insofar as the snapshot/restore fixture it informs is being reimplemented in NUnit. No xUnit-specific wording of its own found — verify on update.
- [ADR-0003 · Solution-wide build governance](../../../design/architecture/adr/adr-0003-build-governance.html): approved. Repo-root `Directory.Build.props` is the SSoT for `TreatWarningsAsErrors`/`WarningLevel 9999`/`Nullable` over both Source and Tests. No xUnit naming, but governs the Tests project's build; relevant as a constraint the migrated test project must keep satisfying (e.g. NUnit/FluentAssertions usings must compile warning-clean).
- [custom-coding-rules.md — "Testing (xUnit)"](../../../.asd/project/custom-coding-rules.md): "tests mutating global/cached/static state MUST save/restore via the test constructor + `IDisposable` (or `IClassFixture`/`ICollectionFixture` for shared setup)". Section header and isolation mechanism are stated in xUnit terms.
- [custom-common-rules.md](../../../.asd/project/custom-common-rules.md): "**Tests**: `Source/LordKuper.Common.Tests/` ... xUnit, `net472`." Single stack-naming line.
- [commands.yaml](../../../.asd/project/commands.yaml): machine SSoT for `test` / `coverage` commands. `coverage` runs `scripts/coverage.ps1`; comment block documents the AltCover flow and the "Visited Points N of M (P%)" coverage proxy.
- [scripts/coverage.ps1](../../../scripts/coverage.ps1): the actual coverage harness. Header documents why AltCover (Cecil static instrumentation) is used instead of `coverlet.collector` (which "silently yields 0%" against the RimWorld-referencing assembly). Carries an `--assemblyFilter xunit`.
- [README.md](../../../README.md): "Common library for my Rimworld mods" — two lines, no testing/build content. No migration impact; noted for completeness.

### Coverage-tooling reality check (memory verification)

Agent memory recorded that coverage uses AltCover via `scripts/coverage.ps1` (not coverlet). VERIFIED against current state: `scripts/coverage.ps1` invokes the `altcover` global tool for instrument/collect, and `.asd/project/commands.yaml` maps `coverage` → `scripts/coverage.ps1`. The persistent `coverlet-collector-6.0.4.md` tech-reference and the `stack.html` coverlet rows are therefore already stale (describe a coverage path not actually in use) independent of this sprint — flagged below.

### No CI configuration found

No `.github/workflows/`, no `azure-pipelines.yml`, no `.gitlab-ci.yml`, no `appveyor.yml` or other CI config exists in the repository. The sprint scope's "update CI config" item has no current target file: CI is not configured in-repo. Coverage/test running is local-only via `commands.yaml` + `scripts/coverage.ps1`. (Flagged as a gap for the Architect/PM to confirm scope: either CI is external/undocumented, or the "CI config" scope line resolves to the local command SSoT only.)

<!-- ========================================================================= -->
<!-- CODE SIDE — Architect appends below. Do not fill (BA scope = docs only).   -->
<!-- ========================================================================= -->

## Touched areas (code side)

> Source-code areas the migration rewrites. Merge with the docs-side list above.

- `Source/LordKuper.Common.Tests/LordKuper.Common.Tests.csproj`: package refs (`Microsoft.NET.Test.Sdk` 17.14.1, `xunit` 2.9.3, `xunit.runner.visualstudio` 2.8.2) and the global `<Using Include="Xunit" />`. xunit + runner removed; NUnit + NUnit3TestAdapter + FluentAssertions 7.x added; `<Using>` swapped to `NUnit.Framework` (+ `FluentAssertions`). `ImplicitUsings` is `enable`. SDK pins (`Microsoft.NET.Test.Sdk`) stay — required by the NUnit3 adapter too.
- `Source/LordKuper.Common.Tests/AssemblyInfo.cs`: holds the entire xUnit framework seam (`[assembly: TestFramework("...RimWorldTestFramework", "LordKuper.Common.Tests")]`). Deleted/replaced — NUnit has no `TestFramework` assembly attribute; the resolver moves to a `[SetUpFixture]`.
- `Source/LordKuper.Common.Tests/XunitExtensions.cs`: `RimWorldTestFramework : XunitTestFramework` + the `AssemblyResolve` handler. Rewritten as an assembly-level NUnit `[SetUpFixture]` with `[OneTimeSetUp]`. File should be renamed (no longer "Xunit").
- `Source/LordKuper.Common.Tests/StaticStateFixture.cs`: `StaticStateFixture : IDisposable` (snapshot/restore) + `[CollectionDefinition("StaticState", DisableParallelization = true)] StaticStateCollection`. The `IDisposable` snapshot logic is framework-agnostic and is reused; the `CollectionDefinition` marker class is removed (NUnit uses `[NonParallelizable]`, not collections).
- `Source/LordKuper.Common.Tests/StaticStateTestBase.cs`: `abstract StaticStateTestBase : IDisposable` driving per-test ctor/Dispose save/restore. Remapped to NUnit `[SetUp]`/`[TearDown]` (per-test in NUnit too) on the base class.
- 11 test files (`Cache/TimedCacheTests.cs`, `Filters/PawnFilterTests.cs`, `Helpers/{DefHelper,EnumHelper,MathHelper,TextHelper}Tests.cs`, `RimWorldTimeTests.cs`, `StatRangesTests.cs`, `StatWeightTests.cs`, `StatefulSubsystemTests.cs`): attribute rewrites + 236 `Assert.*` → `.Should()` conversions.
- `Source/LordKuper.Common.Tests/FakeDefProvider.cs`: framework-agnostic test fake (`IDefProvider`). No xUnit coupling; untouched by the migration.
- `scripts/coverage.ps1`: AltCover harness. `--assemblyFilter xunit` (line 40) excludes the xunit assemblies from the coverage denominator; must become `--assemblyFilter nunit` (NUnit3 adapter assemblies are `nunit.framework`, `NUnit3.TestAdapter`). The `dotnet test ... --no-build` invocation (line 52) is runner-agnostic and works unchanged once the adapter swaps. `--assemblyFilter coverlet` is dead (coverlet not referenced) — pre-existing, leave or drop.
- `.asd/project/commands.yaml`: `test`/`coverage` commands are `dotnet test` / `coverage.ps1` wrappers — runner-agnostic; no command-string change needed, only the AltCover filter inside the script.

## Existing implementation found

> What the current xUnit suite already does that the NUnit port must preserve. Concrete inventory below.

### Test-framework packages (current, from `.csproj`)
- `Microsoft.NET.Test.Sdk` 17.14.1 — VSTest host; **kept** (NUnit3TestAdapter also needs it).
- `xunit` 2.9.3 — core + assert; **removed** (→ `NUnit` 4.x or 3.x; pick at design).
- `xunit.runner.visualstudio` 2.8.2 (`PrivateAssets=all`, `IncludeAssets=runtime;build;...`) — adapter; **removed** (→ `NUnit3TestAdapter`, same PrivateAssets convention).
- `FluentAssertions` — **not currently referenced**; added at 7.x (Apache-2.0, last free-license major).
- `<Using Include="Xunit" />` global using + `ImplicitUsings=enable`; production ref `..\LordKuper.Common\LordKuper.Common.csproj`; RimWorld refs (`Assembly-CSharp`, `UnityEngine.{CoreModule,IMGUIModule,TextRenderingModule}`) with `<Private>False</Private>`. Build governance (Nullable, `TreatWarningsAsErrors`, WarningLevel) inherited from `Source/Directory.Build.props` — NUnit/FluentAssertions usings must compile warning-clean under that.

### Custom test-framework seam (the load-bearing part)
- `AssemblyInfo.cs`: `[assembly: TestFramework("LordKuper.Common.Tests.RimWorldTestFramework", "LordKuper.Common.Tests")]` — xUnit's hook to swap the framework before discovery.
- `XunitExtensions.cs`: `RimWorldTestFramework : XunitTestFramework`; its **constructor** runs before any discovery/execution and calls `InitializeRimWorldResolver()`, which (once, guarded by `AppDomain.GetData("RimWorldResolverInitialized")`) registers an `AppDomain.CurrentDomain.AssemblyResolve` handler. The handler resolves RimWorld assemblies (`Assembly-CSharp`, `Assembly-CSharp-firstpass`, `UnityEngine*`, `Unity.Burst`, `Unity.Collections`, `Unity.Mathematics`, `com.rlabrecque.steamworks.net`) from `%RIMWORLD_DIR%` / `%RimWorldDir%` (fallback `D:\Games\Steam\steamapps\common\RimWorld`) `\RimWorldWin64_Data\Managed\<name>.dll` via `Assembly.LoadFrom`, returning `null` for everything else.
- **What the NUnit equivalent must preserve**: register the same `AssemblyResolve` handler **once, before any test type loads** (RimWorld-typed test classes JIT-fail otherwise). NUnit mapping = assembly-level `[SetUpFixture]` (namespace-less / global) with `[OneTimeSetUp]`. The `AppDomain.GetData` idempotency guard can stay or be dropped (OneTimeSetUp runs once anyway). This also interlocks with `coverage.ps1` step 3, which deletes the copied RimWorld DLLs precisely so this runtime resolver handles them lazily — the resolver must keep working post-migration or coverage runs break.

### StaticState isolation
- `StaticStateFixture : IDisposable` — ctor snapshots `DefProvider.Current`; `Dispose()` restores it, then rebuilds dependent static caches: `StatHelper.Rebuild()`, `WorkTypeStatMap.Rebuild()`, and via reflection resets `SkillStatMap._map`, `PassionHelper.{_isInitialized,_cachedPassions,PassionCache}`, `StatRanges.Ranges`. This reflection-driven snapshot/restore set is the actual isolation contract (matches ADR-0001) and is framework-agnostic — port verbatim.
- `[CollectionDefinition("StaticState", DisableParallelization = true)] StaticStateCollection` — xUnit serialization marker; → NUnit `[NonParallelizable]` (assembly default is already non-parallel in NUnit unless `[Parallelizable]` is set, so this is belt-and-suspenders).
- `StaticStateTestBase : IDisposable` — ctor builds a `StaticStateFixture`, `Dispose()` disposes it. Comment explicitly notes the design chose **ctor+Dispose (once per test)** over `IClassFixture` (once per class) for true per-test isolation (finding #1, simplification iter-01). NUnit `[SetUp]`/`[TearDown]` are also per-test, so the semantics map cleanly: `[SetUp]` → `new StaticStateFixture()`, `[TearDown]` → `.Dispose()`.
- Classes using the seam (carry `[Collection("StaticState")]` + inherit `StaticStateTestBase`): `StatWeightTests`, `StatefulSubsystemTests`, `StatRangesTests`. The other 8 test classes are plain (no static-state isolation).

### Attribute inventory
- `[Fact]`: 132 (incl. the 3 skipped). (Raw grep shows 133; one hit is a `<c>[Fact]</c>` doc-comment in `StaticStateTestBase.cs`, not an attribute.) → `[Test]`.
- `[Theory]`: 3 (`MathHelperTests.NormalizeValue_Theory`, `EnumHelperTests.HasAllFlags_ReturnsExpected`, `EnumHelperTests.HasAnyFlag_ReturnsExpected`). → `[Test]` + `[TestCase(...)]`.
- `[InlineData]`: 13 (5 in MathHelper, 8 in EnumHelper). → `[TestCase(...)]`. Note MathHelper InlineData carries inline `// comment` trailers and 4-arg tuples; EnumHelper InlineData passes enum-flag expressions (`TestFlags.FlagA | TestFlags.FlagB`) — valid in `[TestCase]` since enums are constant expressions.
- `[Fact(Skip="...")]`: 3, all in `Filters/PawnFilterTests.cs`, all reason `"Requires live RimWorld context for Verse.Translator"`: `GetSummary_MultipleIndentationLevels_Respects` (line 201), `GetSummary_NoFiltersActive_ReturnsEmptyOrMinimal` (line 212), `GetSummary_WithIndentation_FormatsCorrectly` (line 221). → `[Test, Ignore("Requires live RimWorld context for Verse.Translator")]`.
- Test method declarations: 135 across 11 files (132 `[Fact]` + 3 `[Theory]`); executed-case count ~146 once the 13 `[InlineData]` rows expand (matches sprint.md). No `[MemberData]`/`[ClassData]`/`[Trait]`/`[Fact(Timeout=)]` in use.

### Per-file test method counts (`[Fact]`+`[Theory]`)
- `RimWorldTimeTests.cs`: 32 (plain, no isolation)
- `Filters/PawnFilterTests.cs`: 23 (plain; holds all 3 skipped)
- `Helpers/TextHelperTests.cs`: 17 (plain)
- `StatWeightTests.cs`: 14 (StaticState)
- `Helpers/MathHelperTests.cs`: 12+1 Theory = 13 decls (plain)
- `Cache/TimedCacheTests.cs`: 12 (plain)
- `Helpers/EnumHelperTests.cs`: 7+2 Theory = 9 decls (plain)
- `StatefulSubsystemTests.cs`: 8 (StaticState)
- `StatRangesTests.cs`: 6 (StaticState)
- `Helpers/DefHelperTests.cs`: 1 (plain; single `Assert.Throws`)
- (infra: `StaticStateFixture.cs`, `StaticStateTestBase.cs`, `XunitExtensions.cs`, `AssemblyInfo.cs`, `FakeDefProvider.cs` — no test methods)

### `Assert.*` inventory (236 call sites, 12 distinct methods) → FluentAssertions map
| xUnit Assert | count | FluentAssertions 7.x equivalent |
|---|---|---|
| `Assert.Equal(exp, act)` | ~108 | `act.Should().Be(exp)` |
| `Assert.Equal(exp, act, precision)` | (subset, e.g. MathHelper `,4`) | `act.Should().BeApproximately(exp, tolerance)` — **mapping nuance**: xUnit precision = decimal places, FA = absolute tolerance; not a 1:1 swap |
| `Assert.True(x)` / `Assert.False(x)` | many (PawnFilter/RimWorldTime/TimedCache/StatRanges-heavy) | `x.Should().BeTrue()` / `.BeFalse()` |
| `Assert.Null` / `Assert.NotNull` | several | `.Should().BeNull()` / `.Should().NotBeNull()` |
| `Assert.Contains` / `Assert.DoesNotContain` | several | `.Should().Contain(...)` / `.Should().NotContain(...)` (collection vs substring overloads both exist in FA) |
| `Assert.Single` | several | `.Should().ContainSingle()` (or `.Should().HaveCount(1)`) |
| `Assert.Empty` / `Assert.NotEmpty` | several | `.Should().BeEmpty()` / `.Should().NotBeEmpty()` |
| `Assert.NotSame` | 2 (PawnFilter) | `.Should().NotBeSameAs(...)` |
| `Assert.Throws<T>(() => ...)` | several (DefHelper, TextHelper, RimWorldTime, PawnFilter) | `action.Should().Throw<T>()` — lambda wrapping shape changes |

Distinct methods in use, exhaustively: `Equal, True, False, Null, NotNull, Throws, Contains, DoesNotContain, Single, NotSame, Empty, NotEmpty`. No `Assert.Same`, `Assert.IsType`, `Assert.Collection`, `Assert.Raises`, or `Assert.Multiple` present.

## Gaps

<!-- Architect: code-side missing pieces. -->
Code-side gaps (pieces scope needs that do not exist in current code):
- **No NUnit assembly-level setup mechanism exists.** xUnit's `[assembly: TestFramework]` hook (the only place the resolver is wired) has no direct NUnit analog; a global (namespace-less) `[SetUpFixture]` with `[OneTimeSetUp]` must be authored. This is the highest-risk new piece — if it runs too late or in the wrong scope, RimWorld-typed test classes fail to JIT.
- **FluentAssertions is not referenced at all.** New PackageReference (7.x) + global using to add; nothing to migrate, purely additive.
- **No `BeApproximately` tolerance decided** for the float assertions currently using xUnit's `Assert.Equal(exp, act, precision)` (precision = decimal digits). FA has no decimal-digits overload; a tolerance value must be chosen per call. Behaviour-preserving only if tolerance ≈ 10^-precision.
- **NUnit major version not yet pinned** (3.x vs 4.x). NUnit 4 drops the legacy `Assert.That`-classic API and bumps the min adapter; on net472 both work but the adapter/SDK pin differs. Decide at design.
- **No NUnit/FluentAssertions tech-reference doc exists** (BA flag) — devs/test-engineer will refuse to implement against un-referenced tech; must be authored in design-promote before impl.
- **No in-repo CI** (BA flag, confirmed code-side): no `.github/`, no pipeline YAML anywhere in the repo. The scope's "update CI config" line has **no target file**; it reduces entirely to `.asd/project/commands.yaml` (runner-agnostic, no change) + `scripts/coverage.ps1` (`--assemblyFilter xunit` → `nunit`). Confirm with PM that CI is out-of-repo or scope collapses to the local command SSoT.

<!-- Docs-side gaps surfaced by BA:
     - No persistent NUnit/FluentAssertions tech-reference exists yet (the xUnit one is superseded, not replaced).
     - coverlet-collector tech-reference + stack.html coverlet rows already describe a coverage path not in use (AltCover) — pre-existing doc drift, in this sprint's blast radius.
     - No in-repo CI config exists; the "update CI config" scope line has no target. Confirm scope.
-->

## Risks

<!-- Architect appends code-side risks. Docs-side risks surfaced by BA: -->
- Approved-ADR drift: ADR-0001 (approved) describes the isolation seam in xUnit terms (`StaticStateFixture`/`IClassFixture`/`[Collection]`). impact=approved persistent design doc becomes inaccurate once the seam is reimplemented in NUnit; mitigation=route the ADR vocabulary update through design → design-promote (do not silently edit an approved ADR); keep the isolation *contract* (snapshot/restore set) unchanged so only the mechanism wording changes.
- Stale coverage reference compounds: `coverlet-collector-6.0.4.md` + `stack.html` already misrepresent coverage as coverlet when it is AltCover. impact=migration may "fix" the wrong doc or carry the coverlet framing forward; mitigation=correct the coverage framing to AltCover at the same time, not just the framework swap.
- CI-scope ambiguity: scope names "CI config" but none exists in-repo. impact=untargetable scope line; mitigation=PM/Architect confirm whether CI is out-of-repo or the scope reduces to `commands.yaml`/`coverage.ps1` only.

Code-side risks:
- Assembly-resolver timing regression: the RimWorld `AssemblyResolve` handler MUST be registered before any RimWorld-typed test class loads. impact=if the NUnit `[SetUpFixture]` runs after type-load, RimWorld-referencing tests (most of the suite) throw `FileNotFoundException`/`TypeLoadException` at discovery or first touch — looks like mass test failure, not a wiring bug. mitigation=use a global (no-namespace) `[SetUpFixture]` with `[OneTimeSetUp]`; verify the resolver is live by running the RimWorld-typed suites (`StatWeightTests`, `RimWorldTimeTests`, `PawnFilterTests`) first; keep the env-var lookup + fallback path identical.
- Coverage-harness coupling: `coverage.ps1` step 3 deletes the copied RimWorld DLLs and relies on the runtime resolver to reload them lazily, and step 1/2 instrument against the test `bin`. impact=if the resolver port regresses or `--assemblyFilter xunit` is left stale, coverage either crashes or counts NUnit adapter assemblies into the denominator (coverage number drifts, not an obvious break). mitigation=swap `--assemblyFilter xunit` → `--assemblyFilter nunit` in the same change; re-run `scripts/coverage.ps1` end-to-end and confirm "Visited Points" stays in the expected band; keep AltCover/Cecil flow otherwise untouched.
- `Assert.Equal(exp, act, precision)` → `BeApproximately` semantic shift: xUnit precision is decimal-place rounding; FA tolerance is absolute. impact=silently looser/tighter float assertions; a wrong tolerance can mask or invent failures (MathHelper normalization tests). mitigation=convert each precision-N site to tolerance 10^-N (or the test author's intended epsilon); spot-check the boundary/clamp cases.
- Warnings-as-errors gate: `Directory.Build.props` sets `TreatWarningsAsErrors` + high WarningLevel + Nullable over Tests (ADR-0003). impact=NUnit/FluentAssertions usings, obsolete-API warnings (e.g. NUnit4 classic-Assert deprecations, FA license/obsolete notices), or nullable-flow changes in rewritten asserts will fail the build, not just warn. mitigation=pin a warning-clean NUnit major; build Release after migration to surface analyzer errors before impl-review.
- Behaviour-preserving-refactor scope creep: scope permits tightening/pruning "weak/redundant" assertions during the swap. impact=mechanical `.Should()` conversion silently changing test strength (e.g. `Assert.True(x == y)` → `.Should().BeTrue()` loses the diff that `.Should().Be(y)` would give). mitigation=prefer the value-comparing FA form over boolean form when converting `Assert.True/False` over comparisons; treat any assertion-strength change as an explicit, reviewable edit, not a side effect.

## Subsystems map (optional, decomposition enabled)

<!-- N/A — project.subsystem_decomposition: disabled. -->

## Dependencies (optional)

<!-- Architect: package-level dependency deltas (xunit/runner/coverlet out; NUnit/adapter/FluentAssertions in). -->
Package deltas in `Source/LordKuper.Common.Tests/LordKuper.Common.Tests.csproj`:

| Action | Package | Current | Target | Notes |
|---|---|---|---|---|
| keep | `Microsoft.NET.Test.Sdk` | 17.14.1 | 17.14.1 | VSTest host; required by NUnit3TestAdapter too |
| remove | `xunit` | 2.9.3 | — | core + `Xunit.Assert` |
| remove | `xunit.runner.visualstudio` | 2.8.2 | — | `PrivateAssets=all`; VS/`dotnet test` adapter |
| add | `NUnit` | — | 3.x or 4.x (TBD design) | core + `NUnit.Framework.Assert` |
| add | `NUnit3TestAdapter` | — | latest compatible | mirror `PrivateAssets=all` runner convention; name stays `NUnit3TestAdapter` even for NUnit 4 |
| add | `FluentAssertions` | — | 7.x | Apache-2.0; 8.x is commercial — pin to 7.x |
| n/a | `coverlet.collector` | not referenced | — | csproj does not reference it; the persistent coverlet tech-reference + stack rows are stale (AltCover is the real path). Nothing to remove from the csproj. |

Global using delta (`<ItemGroup><Using ...></ItemGroup>`): `Xunit` → `NUnit.Framework` (+ `FluentAssertions`). `ImplicitUsings=enable` unchanged. No change to the RimWorld `<Reference>` block, the production `<ProjectReference>`, or `Directory.Build.props` inheritance.

## Migration notes (optional)

Code-side migration mechanics (from → to):
- Framework seam: `AssemblyInfo.cs` `[assembly: TestFramework(...)]` + `XunitExtensions.cs` `RimWorldTestFramework : XunitTestFramework` (resolver in ctor) → global `[SetUpFixture]` class with `[OneTimeSetUp]` registering the same `AppDomain.AssemblyResolve` handler. Delete `AssemblyInfo.cs`'s attribute; rename `XunitExtensions.cs`. Preserve: RimWorld assembly-name match set, env-var lookup (`RIMWORLD_DIR`/`RimWorldDir` + fallback), `Managed\<name>.dll` `Assembly.LoadFrom`, null-for-others contract.
- Isolation: `StaticStateFixture.Dispose()` snapshot/restore body → reused verbatim, called from base-class `[SetUp]`(snapshot)/`[TearDown]`(restore). `[CollectionDefinition("StaticState", DisableParallelization=true)]` + `[Collection("StaticState")]` → `[NonParallelizable]` on the three static-touching classes (or assembly-level, since NUnit is non-parallel by default). `StaticStateTestBase` ctor/Dispose → `[SetUp]`/`[TearDown]`.
- Attributes: `[Fact]` → `[Test]`; `[Theory]`+`[InlineData(...)]` → `[Test]`+`[TestCase(...)]`; `[Fact(Skip="r")]` → `[Test, Ignore("r")]` (3 sites in `PawnFilterTests.cs`).
- Assertions: 236 `Assert.*` → `.Should()` per the map table above; special-case `Assert.Equal(exp, act, precision)` → `.Should().BeApproximately(exp, 10^-precision)` and `Assert.Throws<T>(lambda)` → `lambda.Should().Throw<T>()`.
- Coverage: `scripts/coverage.ps1` line 40 `--assemblyFilter xunit` → `--assemblyFilter nunit`; verify AltCover instrument/collect + the runtime resolver still cooperate (step-3 DLL removal relies on the ported resolver). `commands.yaml` `test`/`coverage` strings unchanged (runner-agnostic).
- CI: no in-repo CI to migrate; "CI config" scope item resolves to `commands.yaml` (no change) + `coverage.ps1` (filter swap) only — pending PM confirmation.

<!-- Docs-side migration framing by BA:
     - xUnit tech-reference → NUnit + FluentAssertions tech-reference (supersede, not edit-in-place).
     - coverlet-collector tech-reference → reconcile with AltCover reality (or retire).
     - stack.html test-stack rows: xunit/runner/coverlet → NUnit/NUnit3TestAdapter/FluentAssertions; coverage framing → AltCover.
     - ADR-0001 isolation-seam vocabulary: xUnit constructs → NUnit constructs (contract preserved).
     - custom-coding-rules "Testing (xUnit)" + custom-common-rules stack line: xUnit → NUnit wording.
-->

## Related open stubs (optional)

<!-- Architect confirmed: .asd/project/stubs.md does not exist in the repo (verified 2026-06-04). No stub registry → no related open stubs. -->
| Sprint of origin | File:Line | Reason | Owner |
|---|---|---|---|
| — | — | no related open stubs | — |

## Documentation migration plan

> Items found outside ASD format/location that should become or update persistent docs, paired with the framework swap. Persistent `design/` docs (tech-references, stack.html, ADRs) are already in ASD format/location, so the "migrate into design/" rows are framed as updates routed through the normal design → design-promote flow rather than new migrations. Code-side items (csproj, coverage.ps1) are Architect's; listed here only where doc/comment content is the migration target.

| # | Source (path/URL) | Format | Proposed target in `design/` | Type | Notes |
|---|---|---|---|---|---|
| 1 | `design/architecture/tech-reference/xunit-2.9.3.md` | md | `design/architecture/tech-reference/nunit-*.md` (+ FluentAssertions) | reverse-engineered (new), supersede old | xUnit stack reference is fully replaced; author an NUnit + NUnit3TestAdapter + FluentAssertions 7.x reference and retire/supersede the xUnit one. Already in `design/`; routes through design-promote, not a fresh migration. |
| 2 | `design/architecture/tech-reference/coverlet-collector-6.0.4.md` | md | reconcile or retire under `design/architecture/tech-reference/` | reverse-engineered (correct) | Describes a coverage path (coverlet collector) not actually used; reality is AltCover. Pre-existing drift in this sprint's blast radius — correct to AltCover framing or retire alongside the migration. |
| 3 | `design/architecture/stack.html` | html | same path (update) | reverse-engineered (update) | Swap test-stack rows xunit/runner/coverlet → NUnit/adapter/FluentAssertions across Frameworks + Version-pinning + Tooling tables; fix "Tests/ (xUnit)" chip; correct coverage framing to AltCover. |
| 4 | `design/architecture/adr/adr-0001-rimworld-context-isolation-seam.html` | html | same path (update, approved doc) | original (update via design-promote) | Remap xUnit isolation vocabulary (`StaticStateFixture`/`IClassFixture`/`[Collection]`/`IDisposable`) to NUnit (`[SetUpFixture]`/`[NonParallelizable]`/`[SetUp]`/`[TearDown]`). Approved ADR — must not be silently edited; route the wording update through design-promote. Isolation contract unchanged. |
| 5 | `.asd/project/custom-coding-rules.md` ("Testing (xUnit)") | md | n/a (project rule, not `design/`) | flag-for-update | Section title + isolation-mechanism wording is xUnit-specific. Not a `design/` migration; flagged so the rule is retitled/reworded to NUnit. BA does not edit project rules directly. |
| 6 | `.asd/project/custom-common-rules.md` (Tests line) | md | n/a (project rule) | flag-for-update | "xUnit, net472" → "NUnit, net472". One-line stack mention; flag only. |
| 7 | `.asd/project/commands.yaml` (coverage/test comments) | yaml | n/a (machine SSoT) | flag-for-update | Comment/wording references the runner; reconfirm `test`/`coverage` against the NUnit3 adapter. Machine file — Architect/dev owns the edit. |

