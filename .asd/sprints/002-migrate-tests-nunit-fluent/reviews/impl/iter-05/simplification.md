---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-simplification]: APPROVE

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 05

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no critical (over-engineering checklist) findings | — |

## Verdict
APPROVE

## Next action
Reviewer done. No simplification escalation. No autofix required.

## Assessment notes (non-blocking, below critical floor — recorded, not raised)

The reflection-heavy `TearDownStaticState` (~17 backing fields across StatHelper /
WorkTypeStatMap / SkillStatMap / PassionHelper / StatRanges) was scanned against the full
over-engineering checklist. No item trips. Rationale for clearing each candidate smell:

- **"Abstraction with no second use case" / new layer**: the reflection teardown is the
  *simpler* of the two viable options, not the complex one. The alternative — adding a
  public/internal `Reset()` to each production helper purely so test teardown can call it —
  would expose test-only surface on production types and itself trip "abstraction with no
  second use case" (no production caller resets these caches; they are idle-or-rebuilt by
  design). Reflection keeps the complication isolated in the test base and out of the
  production contract. The right call under Simplicity Default.

- **"Defensive code for impossible-by-contract case"**: the `if (field != null)` /
  `field?.SetValue` guards on private fields look defensive, but `GetField(name, ...)`
  by string genuinely returns null on a future rename — that case is *possible*, not
  impossible-by-contract. So the guards are not a checklist hit. (Sub-critical nuance:
  silently skipping a renamed field would leave cache state un-reset and surface as
  confusing cross-test bleed rather than a clear failure — a robustness trade-off, not
  over-engineering. Below floor; not raised.)

- **"Helper that wraps one stdlib call"**: no wrapper helper introduced; the teardown is
  inline procedural reflection. No hit.

- **"Comment that restates code"**: the block comments explain the *why* (the
  Unity-ECall / DefDatabase hazard that makes calling `Rebuild()` during teardown unsafe).
  That is load-bearing rationale, not restatement. No hit.

- **"Dead code in case we need it"**: every reset targets a field exercised by a test in
  `StatefulSubsystemTests`. All field names verified present in Source. No dead resets.

- **coverage.ps1**: procedural script; the long header comment justifies the AltCover-over-
  coverlet choice and the denominator exclusions with concrete reasons. No abstraction,
  no premature flag, no framework-wrapping-framework. No hit.

Sub-critical duplication (dropped, below floor): the `SkillStatMap._map` reflection poke
appears both in `StatefulSubsystemTests.WorkTypeStatMap_Rebuild_...` (pre-populating an
empty map) and in the teardown (nulling it). Two distinct intents, minor copy of the
`GetField("_map", ...)` access pattern. Not an over-engineering checklist item; medium-at-
most; not raised at iteration-05 critical floor.

## Cross-reviewer guard
No simplification escalation triggered. If a sibling reviewer proposes replacing the
reflection teardown with a new production `Reset()`/clear API on the helpers, that fix
would itself trip "abstraction with no second use case" and MUST go through Complication
Approval rather than being applied as a routine autofix.

## Escalations
- none
