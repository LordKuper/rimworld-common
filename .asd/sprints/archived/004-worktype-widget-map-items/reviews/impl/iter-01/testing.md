[REVIEW-impl-testing]: CONCERNS: 2

# Review — Testing

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | `Source/LordKuper.Common.Tests/WorkTypeThingRuleTests.cs:72–119` | `GetThingScore_SameWeights_ZeroDeviations_ProducesEqualScores` test is skipped headless when `GetThingScore` requires live RimWorld stat context (line 112–114). This test claims to validate the ordering invariant (AC-3) — that things with equal deviations produce equal scores — but the skip clause means the assertion (line 118: `score1.Should().Be(score2)`) **never executes headlessly**. If a future impl change breaks score equality under zero deviations, this test cannot catch it. The test's catch-and-skip pattern masks a coverage gap: the ordering invariant is asserted only partially (test 4 uses simulated deviations via `StatRanges.NormalizeStatValue` directly, avoiding the live-context call, but test 3 cannot). Either the test must be refactored to execute headlessly (by mocking or simulating `GetStatValueDeviation` to avoid the `thing.GetStatValue` call), or the assertion should be documented as manual-only and moved to the manual-steps checklist. | Refactor test 3 to use `StatRanges.NormalizeStatValue` directly (mirroring test 4's pattern) instead of calling `GetThingScore`, so equal-score assertion runs headlessly; or, downgrade this test to a documentation note and rely solely on test 4. |
| 2 | medium | `Source/LordKuper.Common.Tests/WorkTypeThingRuleTests.cs` and `.asd/sprints/004-worktype-widget-map-items/manual-steps.md` | No headless test covers the null/empty `mapThings` no-op (AC-4, AC-5). Plan Task 5 (line 111 in plan.md) names this requirement — "Add a test (where reachable headlessly) that the bottom-section behaves as List-1-only when `mapThings` is `null`/empty" — but the four tests in `WorkTypeThingRuleTests.cs` do not include such a test. The related no-op logic lives in `WorkTypeThingRuleWidget.DoBottomPart` (which is IMGUI-bound and not headlessly reachable), and the class-level summary (lines 24–41) documents this as manual-only. However, plan.md Task 5 explicitly states "(where reachable headlessly)" and "(If the IMGUI path blocks a headless assertion, fold this into the manual verification note.)" The null/empty no-op is testable at the widget level if a minimal IMGUI render surface is available, but the tests as written do not attempt it. The manual-steps.md (MV-5) covers the visual side (List 1 full width, same band height), but does not capture the code-path assertion (that the render-when-null branch is reachable). AC-5 scope includes the width-split layout and the constant band height; both depend on the null/empty no-op being exercised without error. | Add a minimal headless test that exercises the null/empty mapThings case if IMGUI paths are reachable (e.g., via a mock or stub render surface), OR update plan.md Task 5 to reflect that this assertion is deferred entirely to manual-steps.md MV-5 (with explicit AC-4/AC-5 cross-reference). If the test cannot run headlessly, update the class-level summary to explicitly name this assumption so future maintainers know the null/empty no-op branch is never verified in CI. |

## Verdict

CONCERNS: 2

## Next action

The developer must resolve findings 1 and 2 through the impl⇄impl-review cycle. Return to impl, refactor the skipped test (or document the gap explicitly in the test class summary and plan.md), add a headless test for the null/empty no-op (or formally defer it to manual-steps MV-5 with AC cross-reference), and re-enter impl-review to confirm the fixes.

## Escalations (optional)

— none at iteration 1

## Manual verification

| # | Requirement (AC-ID) | Steps for user | Result reported by user |
|---|---|---|---|
| MV-1 | AC-5 | 1. Load the mod with EquipmentManager consumer adapted to 12-param signature.<br>2. Open work-type rule tab with stat weights and non-empty `mapThings`.<br>3. Verify List 1 occupies left half, List 2 occupies right half of bottom band.<br>4. Resize window; confirm each half is exactly half-width.<br>5. Verify neither box spills below band boundary. | {{pass / fail + notes}} |
| MV-2 | AC-3 | 1. Supply `mapThings` pre-sorted descending by `GetThingScore`.<br>2. Render the widget repeatedly without changing the list.<br>3. Verify icons appear left-to-right, top-to-bottom in the same order each time.<br>4. Confirm List 2 does not re-sort (widget respects consumer pre-sort, ADR-0010). | {{pass / fail + notes}} |
| MV-3 | AC-2 | 1. Hover over icons in List 2.<br>2. Verify tooltip shows `LabelCapNoCount` on line 1, then one line per stat weight: `- {stat.LabelCap} = {value:N2}`.<br>3. Confirm values reflect live instance stats including `equippedStatOffsets` (not synthesized ThingMaker values). | {{pass / fail + notes}} |
| MV-4 | AC-4 | 1. Open widget with stat weights and non-empty `mapThings`.<br>2. Verify List 1 (left half) is identical to pre-sprint single-list rendering, at half-width.<br>3. Confirm tooltips, scroll behavior, icon resolution, and refresh button are unchanged. | {{pass / fail + notes}} |
| MV-5 | AC-4, AC-5 | 1. Open widget without `mapThings` (pass `null`) or with empty list.<br>2. Verify only List 1 renders, at full width, exactly as before sprint.<br>3. Confirm bottom band height is identical to two-list case (verify via `GetBottomPartHeight` unchanged).<br>4. Verify no second header, box, or layout shift. | {{pass / fail + notes}} |

---

**REVIEW_DONE: testing**
