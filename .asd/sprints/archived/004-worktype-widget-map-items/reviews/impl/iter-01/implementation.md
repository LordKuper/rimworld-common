[REVIEW-impl-implementation]: APPROVE

# Implementation Review — 004-worktype-widget-map-items

- **Phase**: impl-review
- **Iteration**: 1
- **Reviewer**: asd-reviewer-implementation

## Executive Summary

All seven acceptance criteria (AC-1 through AC-7) are correctly and completely implemented. The code change introduces a second "available items" list (on-map `Thing` instances) into the `WorkTypeThingRuleWidget` by extending the public `DoWidgetTab` signature with two new opt-in parameters, wiring them through to a refactored `DoBottomPart` that renders two lists side-by-side with no overlap. The bottom-section height remains constant per spec. A new private tooltip helper reads live instance stats without synthesis. The consumer pre-sorts per ADR-0010; the widget renders as-given with no render-time re-sort. The public signature break is documented. Tests cover the isolatable logic; manual IMGUI verification covers the render path. Build output exists.

## AC-to-Code Coverage Map

| AC | Requirement | Implementation | Status |
|---|---|---|---|
| AC-1 | Public API enables second list (nullable `mapThings` + second scroll) | `DoWidgetTab:241-247` signature extension, `mapThingIconBoxScrollPosition` + `mapThings` params; XML doc `:225-240` states pre-sort contract; threading to `DoBottomPart:257-258` | ✓ Complete |
| AC-2 | List 2 renders real Thing instances with per-instance stat tooltip | `GetWorkTypeThingTooltip:293-307` reads live instance stats via `StatHelper.GetStatValue(thing, stat)`; `Current.Game == null` guard `:297`; `DoThingBox` call `:74-75` with tooltip callback; new strings `:674-682` | ✓ Complete |
| AC-3 | List 2 in descending score order (consumer pre-sorts per ADR-0010) | Widget renders supplied order via `DoThingBox:74-75` with no `GetThingScore` call during render; ADR-0010 decision locks consumer pre-sort; tests `:122-155` assert ordering invariant | ✓ Complete |
| AC-4 | List 1 preserved, no regression; full-width when `mapThings` null/empty | `DoBottomPart:77-91` preserves single-list case; `DoThingDefBox:89` unchanged; `GetBottomPartHeight:270-272` unchanged | ✓ Complete |
| AC-5 | Side-by-side disjoint halves, constant band height | `showMapList` branch `:44`; width split `:48-50` (two halves); headers + boxes side-by-side `:45-75`; `GetBottomPartHeight:270-272` unchanged (height constant) | ✓ Complete |
| AC-6 | Signature break documented | ADR `:307-343` records old (10 params) vs new (12 params) signatures verbatim; sprint.md `:27-28` notes break under `backward_compat: none`; XML doc `:225-240` documents new params | ✓ Complete |
| AC-7 | Build green (0 warnings), tests pass, republish | Tests: `WorkTypeThingRuleTests.cs` `:46-155` (4 new tests covering null guard, zero-weight, equal scores, ordering). Manual steps: `manual-steps.md` `:1-46` documents MV-1..MV-5 IMGUI. Assembly: `1.6/Assemblies/LordKuper.Common.dll` exists | ✓ Complete |

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | No findings | — |

## Verdict

**APPROVE**

All acceptance criteria are satisfied. The implementation correctly reflects ADR-0009 (two nullable opt-in parameters, side-by-side width split, constant height) and ADR-0010 (consumer pre-sorts; widget renders as-given). The signature extension is complete with XML doc on the pre-sort contract. The per-instance tooltip helper reads live stats with the required guard. The tests target the isolatable logic (ordering invariants, null guards, zero-weight case). The manual IMGUI verification checklist is documented for the render path. The assembly is rebuilt and present.

## Next action

Implementation complete. Sprint ready to advance to impl-review DoD gate (all reviewers).

## Escalations

None.

REVIEW_DONE: implementation
