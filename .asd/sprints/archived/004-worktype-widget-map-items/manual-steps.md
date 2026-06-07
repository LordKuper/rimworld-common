# Manual Steps — Sprint 004

## Manual IMGUI Verification Checklist

The `WorkTypeThingRuleWidget.DoWidgetTab` render path is Unity IMGUI-bound and cannot be exercised
in a headless unit-test process. The following checklist documents the expected visual behaviour
that must be verified in-game after loading the mod with the EquipmentManager consumer updated to
the new 12-parameter `DoWidgetTab` signature.

### MV-1: Two lists render side by side with no overlap

- Open the work-type rule tab in the EquipmentManager window with a rule that has stat weights and
  a non-empty `mapThings` list supplied by the consumer.
- **Expected:** List 1 (globally-available ThingDef icons) occupies the left half of the bottom
  band; List 2 (on-map Thing instance icons) occupies the right half. The two boxes do not overlap
  horizontally. Neither box spills below the bottom band boundary.
- **Verification:** resize the window; the two halves should each be exactly half the available width.

### MV-2: List 2 renders in consumer-supplied descending-score order

- Supply `mapThings` pre-sorted descending by `WorkTypeThingRule.GetThingScore` from the consumer.
- **Expected:** the icons in List 2 appear left-to-right, top-to-bottom in the order provided.
  The widget does not re-sort: identical repeated calls with the same list produce the same icon
  order.

### MV-3: List 2 shows correct per-instance stat tooltips

- Hover over icons in List 2.
- **Expected:** tooltip shows the thing's `LabelCapNoCount` on the first line, then one line per
  stat weight in the rule (`- {stat.LabelCap} = {value:N2}`). Values reflect the live instance's
  actual stats including `equippedStatOffsets` (not a synthesised ThingMaker value).

### MV-4: List 1 is visually unchanged

- Open the widget with a rule that has stat weights and a non-empty `mapThings` list.
- **Expected:** List 1 (left half) is identical to the pre-sprint single-list rendering except it
  occupies half the width. Tooltips, scroll behaviour, and icon resolution are unchanged. The
  refresh button remains in the header row of List 1.

### MV-5: Null / empty `mapThings` — List 1 renders at full width, same band height

- Open the widget without supplying `mapThings` (pass `null`) or with an empty list.
- **Expected:** only List 1 renders, at full width, exactly as before this sprint. The bottom band
  height is identical to the two-list case (verified by `GetBottomPartHeight` being unchanged).
  No second header, no second box, no layout shift.
- **Code-path note (not unit-testable):** the branch selection (`showMapList = mapThings is
  { Count: > 0 }`) lives in `WorkTypeThingRuleWidget.DoBottomPart`, which is Unity IMGUI-bound.
  This headless gap is documented in `WorkTypeThingRuleTests.cs` class-level summary. This
  in-game step is the only verification that the null/empty branch renders correctly at runtime.
