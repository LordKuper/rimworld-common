[REVIEW-impl-documentation]: APPROVE

# Review — documentation

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict
APPROVE

The persistent design docs now match the implemented code on every checked axis. Detail of what was verified:

**ADR-0008 D1–D4 ↔ code — all match.**
- **D1** (seed degenerate `[v, v]`, compare against seeded local, write once): `StatRanges.cs:79–85` `UpdateStatRange` seeds `new FloatRange(value, value)` on `TryGetValue` miss, runs both min/max comparisons against the seeded local (never the stale `{0,0}` default), and writes `Ranges[stat]` exactly once. Matches D1.
- **D2** (public exposure + `Clear()`): `public static class StatRanges` (`:24`), `public static float NormalizeStatValue(StatDef stat, float value)` with the unchanged signature (`:64`), and `public static void Clear() => Ranges.Clear();` (`:38`). Matches D2 / PRD AC-3, AC-4, AC-5.
- **D3** (static, process-global, not instance): type is `static`, `Ranges` is a `private static readonly Dictionary` (`:29`). No instance conversion. Matches D3 / AC-6.
- **D4** (reset routed through public `Clear()`): `StaticStateTestBase.cs:107–108` resets the range cache via `StatRanges.Clear();`, no reflection on a `"Ranges"` field. Matches D4 / AC-5.

**ADR-0002 reconciliation ↔ code — consistent.** The "Amended by ADR-0008" note (`:159–173`), the superseded "no signature or runtime-behavior change" clause in Consequences (`:244`), and the inline "now `public static`" annotations on the `NormalizeStatValue` references (`:206–221`, `:256`) all correctly describe the as-built code: `NormalizeStatValue` is public, the first-observation behavior is corrected, and the adaptive/order-dependent contract is retained (not redesigned). No contradiction with the implementation.

**ADR-0007 reconciliation ↔ code — consistent.** The "Amended by ADR-0008" note (`:170–183`) and the reconciled Decision/AC-22 prose (`:222–228`, `:248–250`, `:315`) describe the `StatRanges.Ranges` reset as now routed through public `StatRanges.Clear()` while all other caches stay reflection-nulled. This matches `StaticStateTestBase.cs` exactly: `StatRanges.Clear()` at `:108`, reflection-null of WorkTypeStatMap/StatHelper/SkillStatMap/PassionHelper backing fields elsewhere. Reset-set membership unchanged; only the StatRanges mechanism changed.

**XML docs ↔ ADR-0002 adaptive contract — consistent and regenerated.** Source XML on `StatRanges` (`:8–23`), `Clear` (`:31–37`), and `NormalizeStatValue` (`:40–63`) all document the adaptive, order-dependent, process-global, not-reproducible contract and direct consumers to `Clear()` for a clean baseline — the exact contract ADR-0002 blesses. The shipped `1.6/Assemblies/LordKuper.Common.xml` was regenerated: members `T:…StatRanges`, `M:…StatRanges.Clear`, and `M:…StatRanges.NormalizeStatValue(RimWorld.StatDef,System.Single)` (xml `:2643–2700`) carry the same prose. The `WorkTypeThingRule.GetThingScore/GetThingDefScore` docs (xml `:4517–4548`) consistently cross-reference `NormalizeStatValue`'s adaptive behavior. No stale/garbled markup in the file (verified the `<em>` tags and `min/max` text are intact).

**commands.yaml jb-inspect rationale — documented.** `.asd/project/commands.yaml:13–19` documents the `--toolset-path` pin to the .NET SDK MSBuild and the MSB4236 failure it avoids, with an upgrade reminder. Rationale is present and self-explaining.

**SSoT — intact.** PRD owns requirements and delegates reconciliation to ADR-0008's Amends note and audit.md (prd.html follow-ups `:244–248`); ADR-0008 owns the new decision; ADR-0002 / ADR-0007 link to ADR-0008 as governing rather than restating it. No fact is duplicated-and-diverged or contradicted across docs vs code. Provenance fields all `original` (no badge required); responsibility frontmatter present and correct on all three ADRs and the PRD.

**Traceability PRD AC ↔ ADR ↔ code — holds.** AC-1/AC-2 → D1 → `UpdateStatRange`; AC-3/AC-4 → D2 → class+method visibility; AC-5 → D2+D4 → `Clear()` + `StaticStateTestBase`; AC-6 → D3 → static cache retained. ADR-0008 acceptance section (`:308–317`) maps each AC to a decision and to the code, matching the implementation.

Note (informational, not a finding): ADR-0008 Context cites pre-fix line numbers (`StatRanges.cs:53–66`, `StaticStateTestBase :107–111`) when describing the defect being corrected. These describe the state at decision time (the bug location in the old code), which is the correct frame for an ADR's Context section; they are not post-fix drift and require no change.

## Next action
None. Documentation lens passes for impl-review iteration 1. PM may proceed to aggregate reviewer verdicts.

## Escalations (optional)
- none
