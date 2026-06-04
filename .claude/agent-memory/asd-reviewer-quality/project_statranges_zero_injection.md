---
name: statranges-zero-injection
description: Latent bug in StatRanges.UpdateStatRange — first observed value injects 0 into the range; retained by ADR-0002 (no behavior change)
metadata:
  type: project
---

`Source/StatRanges.cs` `UpdateStatRange` has a stale-local bug: on a new stat, `TryGetValue` misses and `range` stays `default` `(0,0)`, while `Ranges[stat]` is set to `(value,value)`. The subsequent `range.min > value` / `range.max < value` checks run against the stale `(0,0)`, so the stored range becomes `(min(0,value), max(0,value))` — 0 is silently injected into every stat's range on first observation.

**Why:** ADR-0002 (`.asd/sprints/001-full-project-audit/design/adr.html`) explicitly retains StatRanges adaptive behavior with "no behavioral change / only XML doc changes" (AC-9). So this is pre-existing and intentionally NOT fixed this sprint.

**How to apply:** Flag as low/informational only; do NOT escalate as a sprint regression — it predates the sprint and is covered by the accepted ADR-0002 decision. Distinct from the documented order-dependence; if a future sprint touches normalization correctness, this is the spot.
