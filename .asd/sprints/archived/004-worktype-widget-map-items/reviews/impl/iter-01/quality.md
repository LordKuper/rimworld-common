[REVIEW-impl-quality]: APPROVE

# Review — quality

- **Phase**: impl-review
- **Iteration**: 1

## Scope reviewed

Sprint logic only (the `style: apply jb cleanupcode` whitespace commit was ignored as instructed):

- `Source/LordKuper.Common/UI/Widgets/WorkTypeThingRuleWidget.cs` — `DoWidgetTab` (12 params), `DoBottomPart` width-split, `GetWorkTypeThingTooltip`, `GetBottomPartHeight`.
- `Source/LordKuper.Common/Resources.cs` — `AvailableItemsOnMapLabel` / `AvailableItemsOnMapTooltip`.
- `Source/LordKuper.Common.Tests/WorkTypeThingRuleTests.cs` — 4 new tests.

## Verification summary (focus checklist)

- **Null/empty `mapThings`**: `showMapList = mapThings is { Count: > 0 }` (WorkTypeThingRuleWidget.cs:44). Null and empty both fall to the single-list branch (`else`, :77-91) which is byte-for-byte the prior full-width path. No NRE; graceful no-op confirmed (AC-4, AC-5). `mapThings!` at :74 is only reached when `showMapList` is true, so the null-forgiveness is sound; `DoThingBox` also null-checks internally (ThingIconBox.cs:43).
- **Width-split math** (:48-50): `halfWidth = rect.width/2f`; List 1 rect `[rect.x, halfWidth]`, List 2 rect `[rect.x + halfWidth, halfWidth]` — disjoint, contiguous, no overlap, no gap, no negative width. `rem1Rect`/`rem2Rect` are derived from each half via `GetSectionHeaderRect` + `DoVerticalGap`, so each box stays inside its half. No off-by-one. List 1 unchanged when `mapThings` null/empty.
- **ADR-0010 compliance (no render-time scoring)**: `DoBottomPart` and the tooltip helper never call `WorkTypeThingRule.GetThingScore` / `GetThingDefScore` and never re-sort. The XML doc on `mapThings` (:229-240) documents the caller pre-sort contract. No render-time mutation of shared `StatRanges`. Confirmed.
- **Tooltip helper reads the live instance** (`GetWorkTypeThingTooltip`, :293-307): uses `StatHelper.GetStatValue(Thing, StatDef)` directly — no `ThingMaker.MakeThing` synthesis (unlike the def helper). `Current.Game == null` early-return guard present (:297), mirroring `GetWorkTypeDefTooltip`. Empty-weights guard (:301) avoids a blank stat section. Matches AC-2 and ADR-0009.
- **Two independent scroll positions, no aliasing**: `DoWidgetTab` copies `thingIconBoxScrollPosition`/`mapThingIconBoxScrollPosition` into distinct locals (:250-251), threads each as a separate `ref` into `DoBottomPart` (:257-258), and writes both back (:260-261). No aliasing; each list owns its scroll state.
- **Public param XML docs**: `mapThings` and `mapThingIconBoxScrollPosition` are fully documented including the nullable/opt-in semantics and the pre-sort contract. Self-contained — no ASD-artifact references in code/comments (custom-coding-rules "self-contained code" honored).
- **Strings**: both new entries follow the existing `$"{ModId}.{nameof(...)}.{nameof(...)}".Translate()` pattern with XML docs (Resources.cs:670-682). Consistent.
- **Tests**: `NormalizeStatValue(StatDef,float)`, `StaticStateTestBase`, and `GetThingScore` signatures all verified to exist and match usage. Tests use FluentAssertions `.Should()`, are `[NonParallelizable]`, and derive from the static-state base per the testing rules. Null-guard (param name "thing") and empty-weights-returns-zero contracts verified against `GetThingScore` source.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | WorkTypeThingRuleWidget.cs:299-300 and :321-322 | `GetWorkTypeThingTooltip` and `GetWorkTypeDefTooltip` contain identical stat-set extraction (`rule.StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef!).ToHashSet()`) and identical per-stat append loop; only the value source differs (live instance vs synthesized thing). Minor duplication. | Optional: extract a shared private helper taking the resolved `Thing` and the stat set, or leave as-is — the duplication is small and the two helpers are intentionally parallel. Not blocking. |
| 2 | low | WorkTypeThingRuleTests.cs:107-114 | `GetThingScore_SameWeights_ZeroDeviations` catches only `NullReferenceException`/`InvalidOperationException` before `Assert.Ignore`. If the live `thing.GetStatValue` path raises another exception type headlessly (e.g. `MissingMethodException`, `TypeInitializationException`), the test fails instead of being ignored, making it environment-fragile. | Broaden the `when` filter (or catch `Exception`) so any headless-stat failure routes to `Assert.Ignore`, keeping the test green across CI hosts. The `score1.Should().Be(score2)` assertion is only meaningful when no exception occurs anyway. |
| 3 | low | WorkTypeThingRuleTests.cs:112 | Uses `Assert.Ignore(...)` while custom-coding-rules forbid NUnit `Assert.*`. `Assert.Ignore` is a flow-control directive (not a value assertion), so it is arguably outside the prohibition's intent, but it is the only NUnit `Assert.*` use in the new tests and could read as a rule violation. | Confirm `Assert.Ignore` is acceptable as a non-assertion control directive (recommended — FluentAssertions has no ignore equivalent), or replace with `[Ignore]`/conditional skip. Borderline; flag for the team's call. |

## Verdict
APPROVE

No findings at or above the iteration-1 floor that block: zero critical/high/medium. Three low-severity items are reported (iter 1 reports all tiers) but none block the DoD. The core focus areas — null/empty handling, width-split math, ADR-0010 no-render-scoring, two independent scroll states, live-instance tooltip read, and public-param XML docs — are all correct.

## Next action
PM may proceed; the three low findings are autofixable by the dev at discretion (no escalation required) but do not gate impl-review. Findings #2 and #3 (test robustness / `Assert.Ignore` usage) are the most worth addressing.

## Escalations (optional)
- None. No finding requires user approval (no concept change, new abstraction, scope expansion, or contract change).

REVIEW_DONE: quality
