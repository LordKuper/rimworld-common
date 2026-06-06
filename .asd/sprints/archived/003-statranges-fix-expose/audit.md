---
responsibility:
  owns: brownfield findings for sprint scope (existing docs, code, gaps, risks)
  excludes: requirements, decisions, plan, code
  delegates_to: prd.html (requirements), adr.html (decisions), plan.md (tasks)
---

# Audit

## Scope reference
[sprint.md](./sprint.md)

## Touched areas (docs side)
- `design/architecture/adr/adr-0002-statranges-adaptive-normalization.html`: governs the `StatRanges` normalization contract; the adaptive XML-doc obligation and the snapshot/restore obligation it records both intersect this sprint's bug-fix, public-exposure, and `Clear()` changes.
- `design/architecture/adr/adr-0001-rimworld-context-isolation-seam.html`: mandates that the static `StatRanges.Ranges` cache be snapshot/restored by the test isolation seam; the proposed `Clear()` and the `StaticStateTestBase` reset path touch this contract.
- `design/architecture/adr/adr-0007-staticstate-isolation-nunit-remap.html`: documents the NUnit static-state isolation remap (`StaticStateTestBase` reflection-reset of caches including `StatRanges.Ranges`); the scope's "switch the reset to call public `StatRanges.Clear()`" item lands here.
- `Source/LordKuper.Common/StatRanges.cs` (XML doc comments): the `NormalizeStatValue` member carries the adaptive-behavior XML docs blessed by ADR-0002; visibility change (internal->public) and the new `Clear()` member need doc-comment coverage consistent with that contract.
- `design/architecture/stack.html`: records the version-specific `1.6/Assemblies/` build/publish layout referenced by the rebuild-and-publish acceptance item (informational; no doc change anticipated).
- `design/product/concept.html`: states the value proposition (shared primitives consumed by dependent mods); the public-exposure change for the downstream EquipmentManager mod is consistent with it (informational; no doc change anticipated).

## Existing docs found
- [ADR-0002 - StatRanges adaptive normalization](../../../design/architecture/adr/adr-0002-statranges-adaptive-normalization.html): approved decision keeping `NormalizeStatValue` adaptive (running min/max per `StatDef`); records the order-dependence as the intended contract and requires it to be documented in XML docs. Directly governs this sprint's type. Note: ADR text describes `NormalizeStatValue` as `internal static` and asserts "no signature or runtime-behavior change to any public member" — this sprint changes both visibility (internal->public) and first-observation runtime behavior, so the ADR will read as out-of-date once the fix lands.
- [ADR-0001 - RimWorld-context isolation seam](../../../design/architecture/adr/adr-0001-rimworld-context-isolation-seam.html): mandates the static `StatRanges.Ranges` cache be in the test fixture's snapshot/restore set so suites stay order-independent. The proposed public `Clear()` is a candidate replacement for the current reflection-based reset.
- [ADR-0007 - StaticState isolation NUnit remap](../../../design/architecture/adr/adr-0007-staticstate-isolation-nunit-remap.html): documents `StaticStateTestBase` resetting static caches (including `StatRanges.Ranges`) by reflection-nulling backing fields. The scope item to route the reset through the new public `StatRanges.Clear()` would alter the as-built reset mechanism this ADR describes.
- [Concept - rimworld-common](../../../design/product/concept.html): project vision/value proposition (shared, versioned primitives consumed by dependent mods). Frames why exposing `StatRanges` to the downstream EquipmentManager mod fits the library's purpose. No requirement-level detail on `StatRanges`.
- [Tech stack](../../../design/architecture/stack.html): records net472 target and the per-RimWorld-version folder convention (`1.6/Assemblies/`) used by the rebuild-and-publish step. Informational only.
- `Source/LordKuper.Common/StatRanges.cs` (in-source XML doc comments, lines 8-46): class summary plus the ADR-0002 adaptive-behavior `<remarks>` on `NormalizeStatValue`. These are the authoritative member-level docs for the type and must stay consistent with the visibility change and new `Clear()` member.

Note: no standalone README, changelog, wiki, or `.md`/`.txt`/`.rst`/`.html` design doc dedicated to `StatRanges`, `WorkTypeThingRule`, stat normalization, the public-API surface, or the `1.6/Assemblies` publish process exists beyond the items above. The repo `README.md` is a one-line description only.

## Touched areas (code side)
- `Source/LordKuper.Common/StatRanges.cs:11`: `internal static class StatRanges` -> `public`; the bug fix lands in `UpdateStatRange` (lines 53-66); visibility flip of `NormalizeStatValue` (line 42, `internal` -> `public`); new `public static void Clear() => Ranges.Clear();`. Class stays static/process-global.
- `Source/LordKuper.Common/StatRanges.cs:42-46` (`NormalizeStatValue`) and `:53-66` (`UpdateStatRange`): the two methods carrying the corrected first-observation logic and the public-surface change.
- `Source/LordKuper.Common/WorkTypeThingRule.cs:200-209` (`GetThingDefScore`), `:234-242` (`GetThingScore`): the only production consumers of `StatRanges.NormalizeStatValue`; they rely on the static process-global range cache to accumulate min/max across all scored defs/things. Not edited by this sprint, but their adaptive contract is the reason the class must stay static (constrains the design, no code change here).
- `Source/LordKuper.Common.Tests/StatRangesTests.cs:14-123`: all six existing tests assert only `!NaN && !Inf`; this sprint adds exact-bound assertions and a named regression test.
- `Source/LordKuper.Common.Tests/StaticStateTestBase.cs:107-111`: the reflection-based reset of `StatRanges.Ranges`; candidate to switch to the new public `StatRanges.Clear()`.
- `1.6/Assemblies/` (build output): rebuild-and-publish target for the corrected Common assembly.

## Existing implementation found
- `Source/LordKuper.Common/StatRanges.cs:11` — `internal static class StatRanges`. Static, process-global, holds `private static readonly Dictionary<StatDef, FloatRange> Ranges` (line 16). Static/global by design; consumed by `WorkTypeThingRule`.
- `Source/LordKuper.Common/StatRanges.cs:42-46` — `internal static float NormalizeStatValue(StatDef stat, float value)`: calls `UpdateStatRange(stat, value)` then `MathHelper.NormalizeValue(value, Ranges[stat])`. Carries the ADR-0002 adaptive `<remarks>` doc (lines 22-35).
- `Source/LordKuper.Common/StatRanges.cs:53-66` — `private static void UpdateStatRange(StatDef stat, float value)`. **Bug confirmed.** Line 55: `if (!Ranges.TryGetValue(stat, out var range)) Ranges[stat] = new FloatRange(value, value);` — on a miss, the dictionary entry is correctly seeded to `[value, value]`, but the local `range` variable remains its `default(FloatRange)` value `{min:0, max:0}`. Lines 56-65 then compare and write using that stale local `range`, not the seeded entry. For first value `v=50`: line 56 `0 > 50` false, line 61 `0 < 50` true -> sets `range.max = 50` and writes `Ranges[stat] = range` = `{min:0, max:50}`, overwriting the correct `[50,50]` seed with `[0,50]`. For a first negative `v=-10`: line 56 `0 > -10` true -> `[-10, 0]` instead of `[-10,-10]`. First `v=0` happens to be correct by coincidence (matches the `{0,0}` default).
- `Source/LordKuper.Common/Helpers/MathHelper.cs:20-32` — `public static float NormalizeValue(float value, FloatRange range)`. Confirms downstream effect of the bug: a correct degenerate `[50,50]` has `valueRange ≈ 0` (line 24) -> returns `0f`; the buggy `[0,50]` has `valueRange=50` -> `(50-0)/50 = 1f`. So the first-observation bug makes `NormalizeStatValue(stat, 50)` return `1` where `0` is correct — exactly the scope's `50->0` expectation. Already public; no change.
- `Source/LordKuper.Common/WorkTypeThingRule.cs:200-209, 234-242` — `GetThingDefScore` / `GetThingScore` call `StatRanges.NormalizeStatValue(...)` inside a `Sum(...)` over stat weights, accumulating ranges across all scored items. Documented (lines 181-198, 215-232) as intentionally order-dependent. This is the load-bearing consumer of the static global state; signature `NormalizeStatValue(StatDef, float)` is unchanged by the fix, so callers are unaffected.
- `Source/LordKuper.Common.Tests/StatRangesTests.cs:14-123` — `StatRangesTests : StaticStateTestBase`, `[NonParallelizable]`. Six tests (`NormalizeStatValue_FirstValue_ExpandsRange`, `_LargeRanges_Supported`, `_MultipleStats_IndependentRanges`, `_NegativeValues_Supported`, `_SecondValue_UpdatesRange`, `_ZeroValue`). Every assertion is the weak compound `(!float.IsNaN(result) && !float.IsInfinity(result)).Should().BeTrue();` — none assert exact bounds, so all six pass under both buggy and fixed code (they do not lock in the contract). Setup pattern is consistent across all tests: `new FakeDefProvider()` -> `AddDef(statDef)` -> `DefProvider.Current = fakeProvider` -> `StatHelper.Rebuild()`.
- `Source/LordKuper.Common.Tests/FakeDefProvider.cs:9-73` — `internal class FakeDefProvider : IDefProvider`. Fluent `AddDef<T>(T def)` (line 56) and `SetWorkTypeDefsInPriorityOrder(...)` (line 68). Sufficient for the new tests: build bare `StatDef { defName, label, category=null }` and register them.
- `Source/LordKuper.Common/Helpers/StatHelper.cs` — `public static void Rebuild()` (verified signature). Existing tests call it after installing the fake provider; the new tests reuse the same pattern.
- `Source/LordKuper.Common.Tests/StaticStateTestBase.cs:20-113` — abstract base; `[SetUp]` saves `DefProvider.Current`, `[TearDown]` restores it and reflection-resets all static caches. Lines 107-111 reset `StatRanges.Ranges`: `typeof(StatRanges).GetField("Ranges", NonPublic|Static)` then `((IDictionary)value).Clear()`. **Switch-to-Clear() candidate**: once `StatRanges.Clear()` exists, lines 108-111 can be replaced by a direct `StatRanges.Clear();` call, removing the reflection lookup of the `Ranges` backing field. Note the field is reflected by literal name `"Ranges"` — fragile to rename (see Risks).

## Gaps
- **First-observation bug** (`StatRanges.cs:55-65`): scope requires that on a `TryGetValue` miss, `UpdateStatRange` seeds a local `FloatRange(value, value)`, runs both `min`/`max` comparisons against that seeded value (never the stale `{0,0}` default), and writes the entry exactly once -> first value `v` yields `[v,v]`. Current code does not do this.
- **`StatRanges` class visibility** (`StatRanges.cs:11`): `internal` -> `public`. Not done.
- **`NormalizeStatValue` visibility** (`StatRanges.cs:42`): `internal static` -> `public static`. Not done.
- **`Clear()` member**: `public static void Clear() => Ranges.Clear();` does not exist; must be added.
- **Exact-bound tests** (`StatRangesTests.cs`): no test asserts exact ranges or `NormalizeStatValue` outputs, so none would fail if the fix were reverted. Scope requires: first `v=50` -> `[50,50]` with `50->0` and (later) `100->1`; sequence `-10,-5,0` -> `[-10,-5]` then `[-10,0]` with `-10->0` and `0->1`; a named regression test (e.g. `NormalizeStatValue_FirstPositiveValue_SeedsDegenerateRange`) that fails under the old `[0,v]` behavior. None exist.
- **Reset routing** (`StaticStateTestBase.cs:107-111`): scope asks to switch the reflection reset to call public `StatRanges.Clear()`. Not done (depends on `Clear()` being added first).
- **XML-doc coverage**: the visibility flip and new `Clear()` member need doc comments consistent with the ADR-0002 adaptive contract; the existing `NormalizeStatValue` remarks (lines 22-35) stay but must remain accurate. (Impl-side, not a doc migration.)
- **Build & publish**: corrected assembly must be rebuilt with 0 warnings (high warning level + warnings-as-errors) and republished to `1.6/Assemblies/`. Not yet done.
- **ADR drift (design-promote, not impl)**: ADR-0002 describes `NormalizeStatValue` as `internal static` and asserts "no signature or runtime-behavior change to any public member" — both become false after this sprint (visibility flips to public; first-observation behavior changes). ADR-0007 describes the reflection-null reset of `StatRanges.Ranges`, which becomes stale if the reset routes through `Clear()`. These reconciliations are flagged for design-promote, not impl. (No reverse-engineered ADR authored here — existing ADR-0001/0002/0007 already cover the area.)

## Risks
- **Public API surface commitment**: impact=high — flipping `StatRanges` + `NormalizeStatValue` to `public` makes them a binary/contract commitment consumed by the downstream EquipmentManager mod; future changes become breaking. mitigation=keep the signature exactly `NormalizeStatValue(StatDef, float)` (unchanged); document the adaptive/order-dependent contract on the public members per ADR-0002; record the surface decision in an ADR during design.
- **Reflection reset fragility on rename**: impact=medium — `StaticStateTestBase.cs:109` reflects the `Ranges` field by literal string `"Ranges"`; if `Ranges` is ever renamed and the reflection path is kept, the reset silently no-ops and tests leak state across the suite (order-dependent flakiness). mitigation=switch the reset to the new typed `StatRanges.Clear()` call (a scope item), eliminating the stringly-typed field lookup.
- **Warnings-as-errors strictness**: impact=medium — the build uses high warning level + warnings-as-errors; adding a public member with incomplete XML docs, or any new unused local in the rewritten `UpdateStatRange`, fails the build. mitigation=full XML docs on the new `Clear()` and on the now-public members; ensure the rewritten method has no unused/dead locals.
- **Assembly republish step**: impact=medium — the shipped `1.6/Assemblies/` artifact must be rebuilt from the fixed source; shipping a corrected source but a stale assembly is a silent regression for consumers. mitigation=treat rebuild-and-publish to `1.6/Assemblies/` as a hard acceptance gate verified after green tests; confirm net472 output lands in the version folder.
- **Behavior change for existing consumers**: impact=medium — `WorkTypeThingRule` scores shift for first-observed stats (first value now normalizes via a degenerate `[v,v]` -> `0` instead of the buggy `[0,v]` -> nonzero). This is the intended correction, but it changes emitted scores. mitigation=the tests lock the corrected values; the change is the point of the sprint, documented in ADR drift reconciliation.
- **ADR/test-doc staleness post-merge**: impact=low — ADR-0002/0007 and any reset-describing doc read as out-of-date until design-promote reconciles them. mitigation=already flagged in BA docs-side sections and Gaps above; handled in the design / design-promote pipeline.

## Related open stubs (optional)

No related open stubs: `.asd/project/stubs.md` does not exist in the repository, so there are no recorded TODO/stub entries to filter against this sprint's touched areas.

| Sprint of origin | File:Line | Reason | Owner |
|---|---|---|---|
| — | — | no related open stubs | — |

## Documentation migration plan

Items found outside ASD format/location that should become persistent docs in `design/`.
Items addressed by sprint design drafts are NOT listed here (they flow through design -> design-promote).
Items NOT covered by sprint scope but worth promoting wait for design-promote handling.

| # | Source (path/URL) | Format | Proposed target in `design/` | Type | Notes |
|---|---|---|---|---|---|
| — | — | — | — | — | no migrations |

No external/non-ASD documentation needs promotion. All relevant persistent docs already live in ASD format under `design/`. This sprint is a bug-fix + visibility change; the only doc work it warrants is reconciling existing ADR text to the as-built result, which flows through the normal design / design-promote pipeline, not through a migration:
- ADR-0002 wording ("internal static `NormalizeStatValue`", "no signature or runtime-behavior change to any public member") will be stale once visibility flips to public and first-observation behavior is corrected — flag for design-promote reconciliation.
- ADR-0007 (and any tech-reference) describing the reflection-null reset of `StatRanges.Ranges` will be stale if `StaticStateTestBase` is switched to call the new public `StatRanges.Clear()` — flag for design-promote reconciliation.
- New `public static void Clear()` and the visibility change need XML-doc coverage in `StatRanges.cs` consistent with the ADR-0002 adaptive contract — handled in impl, not a doc migration.
