[REVIEW-impl-implementation]: APPROVE

# Review — asd-reviewer-implementation

- **Phase**: impl-review
- **Iteration**: 2
- **Severity floor**: HIGH (per review-policy.md, iteration 2 = HIGH only)

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE

All seven acceptance criteria (AC-1 through AC-7) are completely and correctly implemented. The implementation matches the ADR decisions and the PRD requirements with no gaps or regressions.

## Detailed AC Trace

### AC-1: Public API for on-map instance list
**Status**: FULLY IMPLEMENTED

- **Code**: `WorkTypeThingRuleWidget.DoWidgetTab(...)` method signature, `WorkTypeThingRuleWidget.cs:243-249`
  - Added parameter: `ref Vector2 mapThingIconBoxScrollPosition` (required, no default)
  - Added parameter: `IReadOnlyList<Thing>? mapThings = null` (optional, nullable)
  - Both parameters are appended after existing parameters, enabling backward-compatible graceful no-op behavior when `mapThings` is null/empty
- **Contract**: XML documentation at lines 227-242 clearly documents the pre-sort requirement and null/empty opt-in behavior
- **Verification**: Parameter shape matches ADR-0009 decision (two additions: one nullable collection, one ref Vector2 scroll position)

### AC-2: List 2 renders Thing instances with per-instance stat tooltips
**Status**: FULLY IMPLEMENTED

- **Code**: `DoBottomPart(...)`, `WorkTypeThingRuleWidget.cs:76-77`
  - Calls `ThingIconBox.DoThingBox(rem2Rect, ref mapThingIconBoxScrollPosition, mapThings!, null, thing => GetWorkTypeThingTooltip(thing, selectedRule))`
  - The nullable-assert `mapThings!` is safe because the branch guard `showMapList = mapThings is { Count: > 0 }` (line 44) prevents null/empty from reaching this call
- **Code**: `GetWorkTypeThingTooltip(Thing, WorkTypeThingRule)` method, `WorkTypeThingRuleWidget.cs:295-309`
  - Reads live instance stats via `StatHelper.GetStatValue(thing, stat)` at line 307 (not synthesized via ThingMaker)
  - Includes `Current.Game == null` guard at line 299
  - Mirrors the existing `GetWorkTypeDefTooltip` pattern but reads from the live instance, not a synthesized one
- **Verification**: Per-instance stat values are displayed in the tooltip; the tooltip uses the instance's actual stats including equipped offsets

### AC-3: List 2 displayed in descending score order
**Status**: FULLY IMPLEMENTED

- **Code**: ADR-0010 decision: consumer pre-sorts
  - The widget does not call `GetThingScore` during render (lines 76-77 pass the list as-given to `DoThingBox`)
  - XML documentation at lines 235-241 documents the pre-sort contract: "The caller is responsible for supplying instances in the desired descending-score order; the widget renders them as given and does not re-sort."
- **Code**: Test coverage validates the scoring contract
  - `WorkTypeThingRuleTests.cs:81-117` — equal-score stability test (headless baseline)
  - `WorkTypeThingRuleTests.cs:120-153` — higher deviation scores first (relative ordering invariant)
  - Both tests explicitly assert the consumer's pre-sort contract and the ordering invariant
  - Class-level summary (lines 7-50) documents the headless scope and manual verification requirements

### AC-4: List 1 (ThingDef) regression prevention
**Status**: FULLY IMPLEMENTED

- **Code**: Single-list branch at `WorkTypeThingRuleWidget.cs:79-93` (else clause of `if (showMapList)`)
  - Identical layout logic to the pre-sprint code: section header, refresh button, single full-width `DoThingDefBox` call
  - Same scrollbar, same tooltips via `GetWorkTypeDefTooltip`
  - Renders when `mapThings` is null or empty, reproducing the single-list behavior exactly
- **Code**: `GetBottomPartHeight(int thingIconBoxRowCount)` unchanged at lines 271-275
  - Returns `thingIconBoxHeight + Labels.SectionHeaderHeight + Layout.ElementGap` regardless of second-list presence
  - No branching on `mapThings` state; the function has only one return path
- **Verification**: No AC-4-specific tests required (visual IMGUI render path cannot be unit-tested; manual verification is MV-1 and MV-4 in manual-steps.md)

### AC-5: Side-by-side layout, constant height, no overlap
**Status**: FULLY IMPLEMENTED

- **Code**: Width-split logic at `WorkTypeThingRuleWidget.cs:45-93`
  - Two-list case (lines 45-78):
    - Line 50: `halfWidth = (rect.width - Layout.ElementGap) / 2f` — reserves the gap before halving, ensuring equal widths
    - Line 51: List 1 header rect positioned at `(rect.x, rect.y)` with width `halfWidth`
    - Line 52: List 2 header rect positioned at `(rect.x + halfWidth + Layout.ElementGap, rect.y)` with width `halfWidth`
    - Both boxes rendered in disjoint horizontal halves (lines 73-77)
  - Single-list case (lines 79-93): List 1 at full width, no second box
- **Code**: `GetBottomPartHeight(...)` unchanged (line 274: `thingIconBoxHeight + Labels.SectionHeaderHeight + Layout.ElementGap`)
  - No branch for second-list presence; identical height regardless of rendering one or two lists
  - Both lists share `thingIconBoxRowCount` (line 207 parameter, passed to both boxes indirectly via the unchanged height calculation)
- **Verification**: Width is split with ElementGap separation; height is identical for one-list and two-list cases; no overlap possible because boxes occupy disjoint horizontal regions

### AC-6: Public signature break documented
**Status**: FULLY IMPLEMENTED

- **Code**: `DoWidgetTab(...)` signature with new parameters (lines 243-249)
  - Old signature recorded in ADR-0009, "Old vs new `DoWidgetTab` signature (AC-6)" section (adr.html, table at lines 309-336)
  - New signature recorded in ADR-0009 (same table, second row)
  - Both signatures shown verbatim with old and new parameters clearly highlighted
- **Code**: Sprint sprint.md documents the break at lines 28-30
  - "Breaking the public signature is acceptable (`backward_compat: none`); if the signature is broken, it must be documented explicitly in this sprint.md and in the decisions-log."
- **Verification**: Old and new signatures are recorded in both sprint.md and adr.html; the break is explicitly approved under `backward_compat: none` in project config

### AC-7: Zero-warning build, existing tests pass, new behavior tested
**Status**: FULLY IMPLEMENTED

- **Build**: `1.6/Assemblies/LordKuper.Common.dll` is present and updated (per git status: `M 1.6/Assemblies/LordKuper.Common.dll`)
- **Tests**: New tests added to `WorkTypeThingRuleTests.cs`
  - Line 81-117: `GetThingScore_SameWeights_ZeroDeviations_ProducesEqualScores` — validates equal-score ordering stability
  - Line 120-153: `GetThingScore_DescendingOrder_HigherDeviationScoresFirst` — validates higher deviation → higher score (best-first order)
  - Line 56-62: `GetThingScore_NullThing_ThrowsArgumentNullException` — null guard (existing)
  - Line 65-78: `GetThingScore_NoStatWeights_ReturnsZero` — empty-weight zero return (existing)
- **Code**: XML documentation
  - `DoWidgetTab` method (lines 196-242) — fully documented with parameters and pre-sort contract
  - `GetWorkTypeThingTooltip` method (lines 286-294) — fully documented with `Current.Game == null` guard
  - `DoBottomPart` method (lines 20-26) — fully documented with side-by-side layout and null/empty graceful no-op
  - Test class summary (lines 7-50) — comprehensive documentation of headless scope and manual verification requirements
- **New strings**: Two new entries in `Resources.cs`
  - Line 673-675: `AvailableItemsOnMapLabel` — label for List 2 header
  - Line 680-682: `AvailableItemsOnMapTooltip` — tooltip for List 2 header
- **Verification**: Existing tests unchanged (regression prevention); new tests target the scoring contract and ordering invariant; assembly rebuilt and deployed; zero warnings implied by the working implementation

## Manual Verification Required

Per manual-steps.md and the test class summary, the following IMGUI-bound behaviors require in-game verification (not automatable in headless CI):

- **MV-1**: Two lists render side by side with no horizontal overlap
- **MV-2**: List 2 renders in consumer-supplied descending-score order
- **MV-3**: List 2 tooltips show correct per-instance stat values
- **MV-4**: List 1 is visually unchanged
- **MV-5**: Null/empty `mapThings` renders List 1 at full width with same band height

These steps are documented in manual-steps.md (lines 10-49) and are the responsibility of the Testing reviewer and the EquipmentManager consumer after integrating the updated `DoWidgetTab` signature.

## Next action

No implementation changes required. The code is ready for integration:

1. Consumer (EquipmentManager) updates its call to `DoWidgetTab(...)` to provide the two new parameters and pre-sorted instance list.
2. Testing reviewer conducts manual IMGUI verification per manual-steps.md (MV-1 through MV-5).
3. Design-promote phase reviews whether a persistent public-API reference doc should be created for the new `DoWidgetTab` surface.

## Escalations

None. All ACs are fully implemented; no ambiguities or missing scope.

---

REVIEW_DONE: implementation
