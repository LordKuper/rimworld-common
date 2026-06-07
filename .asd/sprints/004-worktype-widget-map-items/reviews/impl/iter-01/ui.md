[REVIEW-impl-ui]: APPROVE

# Review — UI Implementation

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE

## Next action

None. Implementation is ready to proceed to impl-review (test/code-quality reviewer). Manual verification checklist (MV-1 through MV-5) in `manual-steps.md` is comprehensive and covers all layout, header, tooltip, and fallback scenarios required by ADR-0009 and ADR-0010.

## Manual verification (UI-specific, for user reporting)

User must verify the following in-game after the consumer (EquipmentManager) is updated to call the 12-parameter `DoWidgetTab` signature:

| # | Requirement (AC-ID) | Steps for user | Result reported by user |
|---|---|---|---|
| 1 | AC-5 (side-by-side, no overlap) | Open work-type rule tab with non-empty `mapThings`. Resize window. | List 1 left half, List 2 right half; no horizontal overlap; both halves equal width. |
| 2 | AC-3 (pre-sorted order) | Supply `mapThings` pre-sorted by `GetThingScore` descending. Repeated opens show same order. | Icons appear left-to-right, top-to-bottom in supplied order; identical renders produce identical layouts. |
| 3 | AC-2 (per-instance tooltips) | Hover icons in List 2. | Tooltip shows thing's `LabelCapNoCount`, then one line per stat weight (`- {stat.LabelCap} = {value:N2}`); values reflect live instance stats including `equippedStatOffsets`. |
| 4 | AC-4 (List 1 unchanged) | Open widget with non-empty `mapThings`. | List 1 (left half) is visually identical to pre-sprint single-list except half-width; refresh button in header; tooltips and scroll unchanged. |
| 5 | AC-4 (single-list fallback) | Open widget with `mapThings = null` or empty. | Only List 1 renders at full width; same band height as two-list case; no second header or box; no layout shift. |

---

## Detailed assessment

### Layout correctness

**Width split (AC-5):** `DoBottomPart` lines 48–50 split the band width exactly in half (`halfWidth = rect.width / 2f`), creating two disjoint horizontal rects. No overlap by construction. Each box receives its half-width rect via `rem1Rect` and `rem2Rect` (lines 71–75), which derive from the halved header rects via standard layout-extraction functions (`GetSectionHeaderRect`, `DoVerticalGap`). Consistent with existing IMGUI widget spacing.

**Header placement (AC-5):** Both headers are drawn on a single row at `rect.y` (lines 49–50, split rects share the same `y`). Each box's header is extracted from its respective half-rect. The refresh button remains in List 1's header (line 60); List 2 has no button (line 65). Two headers, one row. ✓

**Height invariant (AC-5):** `GetBottomPartHeight` (lines 269–273) is unchanged and does not branch on `mapThings` presence. The band height is:
```
ThingIconBox.GetThingIconBoxHeight(thingIconBoxRowCount) 
  + Labels.SectionHeaderHeight 
  + Layout.ElementGap
```
This is identical whether one or two lists render, since both lists share `thingIconBoxRowCount` (ADR decision). Band height is constant. ✓

**Single-list fallback (AC-4):** When `mapThings` is null/empty (line 77), the code path is identical to pre-sprint behavior: `List 1` draws full width, same header style, same refresh button placement, same scroll position management (lines 80–90). No second header, no second box, no layout shift. ✓

### Component fidelity

**List 1 rendering:** `DoThingDefBox` called with `rem1Rect`, `thingIconBoxScrollPosition`, the `things` list, and the def-based tooltip helper (lines 71–72). Matches pre-sprint contract exactly when `mapThings` is absent. When present, the box width is halved, so `itemsPerRow` (computed internally by `DoThingDefBox` from `rect.width`) will be smaller, leading to more internal scrolling—expected and acceptable per ADR §3.3. ✓

**List 2 rendering:** `DoThingBox` called with `rem2Rect`, `mapThingIconBoxScrollPosition`, the `mapThings` instance list, null for right-click action, and the per-instance tooltip helper (lines 74–75). The `!` non-null assertion is safe: `mapThings` is guaranteed non-null and non-empty here (line 44 guard). ✓

**Tooltip helper for instances:** `GetWorkTypeThingTooltip` (lines 293–307) reads live stats via `StatHelper.GetStatValue(thing, stat)` without synthesizing a temporary `Thing` via `ThingMaker`. Matches MV-3 requirement (per-instance, including `equippedStatOffsets`). Guards against `Current.Game == null` (line 297), mirroring the def helper. Format is consistent: label line + blank line + stat lines. ✓

### String resources

Two new string constants added to `Resources.Strings.WorkTypeThingRuleWidget` (lines 673–675 and 680–682):
- `AvailableItemsOnMapLabel` — wired to header label for List 2 (line 66, `DoBottomPart`)
- `AvailableItemsOnMapTooltip` — wired to header tooltip for List 2 (line 67, `DoBottomPart`)

Both follow the project's string-key pattern and will be localized via `.Translate()` calls. Correct. ✓

### Public API contract

The `DoWidgetTab` signature is updated per ADR-0009 §Decision:
- Added `ref Vector2 mapThingIconBoxScrollPosition` (no default; must be provided by caller)
- Added `IReadOnlyList<Thing>? mapThings = null` (nullable with default for opt-in)

The XML doc (lines 225–239) fully documents the two new parameters:
- `mapThingIconBoxScrollPosition`: required, holds scroll state for List 2
- `mapThings`: nullable, defaults to null; supplier responsible for pre-sorting per ADR-0010

This breaks the public call site (EquipmentManager must update), as documented in ADR-0009 §Signature and permitted under `backward_compat: none`. ✓

### Pre-sort contract (ADR-0010)

The widget does not sort or call `GetThingScore` at render time. `DoThingBox` receives the list and renders items in the supplied order (per `ThingIconBox.DoThingBox`, lines 54–56 iterates `for (var i = 0; i < things.Count; i++)`). No mutation of shared `StatRanges` during render. Pre-sort obligation is documented in the XML doc (lines 234–238). ✓

### Graceful no-op

When `mapThings` is null or empty, the code path (line 77 onward) is identical to the pre-sprint single-list behavior. No branching of height calculation, no second header or box. Full-width List 1. ✓

### Code quality

- No new primitives; both `DoThingBox` and `DoThingDefBox` already exist and are used as designed. ✓
- No new dependencies or external references. ✓
- XML doc is complete and accurate. ✓
- Compiler assertions (`mapThings!`) guard null-dereference correctly. ✓
- String keys follow project pattern. ✓

---

## MV Checklist alignment

The manual-steps.md checklist is comprehensive:

- **MV-1:** Verifies side-by-side layout with no overlap and equal-width halves. ✓
- **MV-2:** Verifies consumer-supplied descending-score order is preserved (ADR-0010 contract). ✓
- **MV-3:** Verifies per-instance tooltips show correct stats with live values. ✓
- **MV-4:** Verifies List 1 is unchanged except for width (ADR-0009 AC-4). ✓
- **MV-5:** Verifies null/empty `mapThings` produces full-width List 1 fallback. ✓

All MV steps map to AC requirements and are achievable in-game.

---

REVIEW_DONE: ui
