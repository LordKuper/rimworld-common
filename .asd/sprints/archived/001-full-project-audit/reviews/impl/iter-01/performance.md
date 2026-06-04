---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: APPROVE

# Review — performance

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | `Source/DefProvider.cs:54-57` (`VerseDefProvider.WorkTypeDefsInPriorityOrder`) | The interface wrapper adds a `.ToList()` over `WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder` that the prior direct call did not — a new per-call list allocation. Not a hot path: the sole production caller is `WorkTypeStatMap.Rebuild` (`Source/WorkTypeStatMap.cs:101`), a one-time build. Acceptable, but the allocation is genuinely new vs baseline. | If ever called per-tick/per-frame, return the underlying `IReadOnlyList` directly (the source is already a list) instead of copying. For the current one-time caller, leave as-is. |

## Verdict
APPROVE

Hot paths assessed against "no regression on hot paths" + RimWorld modding norms (no project perf budgets defined in `custom-coding-rules.md`).

**`DefProvider.Current` indirection — not a regression on any hot path.** The scoring hot paths (`WorkTypeThingRule.GetThingScore`, `GetThingDefScore`, `StatRanges.NormalizeStatValue`, `StatHelper.GetStatValueDeviation`) do **not** call `DefProvider.Current` at all — confirmed by grep; they operate on already-resolved `StatDef`/`StatWeight` state. Every production `DefProvider.Current` call site is in one-time setup or a cached/gated path:
- `DefCache.Initialize` (`Source/Cache/DefCache.cs:98`) — gated by `_isInitialized`, runs once per cache instance.
- `WorkTypeThingRule.AllRelevantThings` (`:67`) — result cached in `_allRelevantThings`.
- `WorkTypeThingRule.Initialize` (`:253`) — gated by `_isInitialized`.
- `StatHelper.InitializeDefaultStats`/`InitializeUnionStats` (`:282`, `:342`) — one-time init.
- `SkillStatMap` (`:47`, `:58`), `WorkTypeStatMap.Rebuild` (`:101`, `:112`, `:134`) — one-time build.

The one extra interface dispatch is paid once per def-resolution / build, never inside a per-frame or per-tick loop. Overhead is negligible and not in a tight loop without caching.

**Resources tooltip Dictionary-cache — equal-or-better than prior cached fields.** `Resources.Strings.PawnFilter.GetFilterTooltip` (`Source/Resources.cs:472-489`) builds each tooltip's `string.Concat` once and stores it in `TooltipCache`/`TriStateTooltipCache`, then serves from the dictionary on subsequent calls. Tooltips are queried per-frame on hover; the cache moves the concat off the per-frame path entirely (one `Dictionary.TryGetValue` per frame vs a string concat). This is a net improvement over per-frame rebuilding and at least as good as static cached fields, while supporting the on/off + tri-state key matrix. The `ConcurrentDictionary` label/tooltip caches (`PawnHealthState`, `PawnPrimaryWeaponType`, `PawnType`) use `GetOrAdd` and translate-once — also correct.

**No new per-call allocation introduced on a hot path by the refactors.**
- The genuinely hot per-pawn filter path `PawnFilter.SatisfiesFilter` (`Source/Filters/PawnFilter/PawnFilter.cs:508-677`) is allocation-free: plain `foreach`, no LINQ, no `.ToList()`. The `Combine` split (`:173-259`) and `Copy` (`:269-334`) allocate via collection spreads/`Select`, but both are user-action operations (filter merge / clone), not per-frame/per-tick, and the allocation pattern is pre-existing, not introduced here.
- The scoring methods use pre-existing `.Where().Sum()` LINQ — unchanged by this sprint (no regression introduced).

**Rebuild() extraction** (`WorkTypeStatMap.Rebuild`, `StatHelper.Rebuild`) is structural refactoring of one-time build logic; no algorithmic-complexity change, no hot-path impact.

Nullable `!`/guards and the test harness confirmed not runtime-hot, per instructions — not assessed.

## Next action
None required for performance. Finding #1 is a low-severity, informational note about a new (but non-hot) allocation; no fix needed for current callers. PM may proceed.
