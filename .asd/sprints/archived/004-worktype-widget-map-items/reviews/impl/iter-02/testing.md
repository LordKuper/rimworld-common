[REVIEW-impl-testing]: APPROVE

# Review — Testing

- **Phase**: impl-review
- **Iteration**: 2

## Findings

| — | — | — | no findings | — |

## Verdict

APPROVE

## Next action

All HIGH and CRITICAL findings from iter-01 have been resolved. The refactored test now runs headlessly in CI (+1 passed, –1 skipped), and the null/empty `mapThings` deferral is formally documented in code and cross-referenced to manual verification. Testing is ready to advance.

## Escalations (optional)

— none

## Manual verification

Manual IMGUI verification steps are documented in `.asd/sprints/004-worktype-widget-map-items/manual-steps.md` and require user execution post-deployment:

| # | Requirement (AC-ID) | Steps for user | Result reported by user |
|---|---|---|---|
| MV-1 | AC-5 | 1. Load the mod with EquipmentManager consumer adapted to 12-param signature.<br>2. Open work-type rule tab with stat weights and non-empty `mapThings`.<br>3. Verify List 1 occupies left half, List 2 occupies right half of bottom band.<br>4. Resize window; confirm each half is exactly half-width.<br>5. Verify neither box spills below band boundary. | {{pass / fail + notes}} |
| MV-2 | AC-3 | 1. Supply `mapThings` pre-sorted descending by `GetThingScore`.<br>2. Render the widget repeatedly without changing the list.<br>3. Verify icons appear left-to-right, top-to-bottom in the same order each time.<br>4. Confirm List 2 does not re-sort (widget respects consumer pre-sort, ADR-0010). | {{pass / fail + notes}} |
| MV-3 | AC-2 | 1. Hover over icons in List 2.<br>2. Verify tooltip shows `LabelCapNoCount` on line 1, then one line per stat weight: `- {stat.LabelCap} = {value:N2}`.<br>3. Confirm values reflect live instance stats including `equippedStatOffsets` (not synthesized ThingMaker values). | {{pass / fail + notes}} |
| MV-4 | AC-4 | 1. Open widget with stat weights and non-empty `mapThings`.<br>2. Verify List 1 (left half) is identical to pre-sprint single-list rendering, at half-width.<br>3. Confirm tooltips, scroll behavior, icon resolution, and refresh button are unchanged. | {{pass / fail + notes}} |
| MV-5 | AC-4, AC-5 | 1. Open widget without `mapThings` (pass `null`) or with empty list.<br>2. Verify only List 1 renders, at full width, exactly as before sprint.<br>3. Confirm bottom band height is identical to two-list case (verify via `GetBottomPartHeight` unchanged).<br>4. Verify no second header, box, or layout shift. | {{pass / fail + notes}} |

---

REVIEW_DONE: testing
