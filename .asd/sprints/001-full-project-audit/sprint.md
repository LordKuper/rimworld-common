---
responsibility:
  owns: sprint scope, goal, top-level acceptance criteria
  excludes: task breakdown, design decisions, code, audit findings
  delegates_to: plan.md (tasks), design/ docs (decisions), audit.md (audit)
---

# Sprint 001-full-project-audit

## Goal
Conduct an end-to-end audit of the entire `LordKuper.Common` codebase and its
documentation against all governing rules — the `.asd/rules/*` workflow rules and
the project custom rules (`custom-common-rules.md`, `custom-design-rules.md`,
`custom-coding-rules.md`), including the recently added ASD conventions: coding
rules (nullability = `enable`, zero-warnings under `TreatWarningsAsErrors`, XML
docs, the `jb-cleanup`→build / `jb-inspect`→SARIF lint flow, suppression policy,
project `Logger` usage, xUnit static-state isolation) and design rules
(modding/patchability, data-driven over hardcoded, determinism). For each rule
category, record a pass/fail finding. Surface every opportunity for
simplification, optimization, or other improvement; for each finding/opportunity,
log it and record a per-item decision to act on it this sprint or defer. Fold in
known starting findings: the legacy unused `Source/packages/` folder (Harmony
2.3.6, MSTest 3.10.2) as a cleanup candidate, and the hardcoded RimWorld path in
`Source/Directory.Build.props` (overridable via `RIMWORLD_DIR`). In-scope approved
improvements are implemented through to PR this sprint; the rest are deferred.

**Sprint shape: B — Audit + all fixes.** Full audit of code + docs against all
rules, AND carry every approved in-scope improvement through the full phase chain
(design → plan → impl → review → pr) this same sprint. Deferred items are recorded
for later.

## Acceptance
- Every rule category (all `.asd/rules/*` workflow rules and the three custom rule
  files, including the newly added coding and design ASD conventions) has a
  recorded pass/fail finding in `audit.md`.
- Every simplification / optimization / improvement opportunity is logged in
  `audit.md` with a recorded per-item decision: in-scope (this sprint) or deferred.
- All in-scope fixes are implemented and carried through the full phase chain to
  PR, with build + tests + lint green — including a clean `jb-inspect` SARIF run.
- The two seeded known findings are recorded with explicit in-scope/deferred
  decisions: the legacy unused `Source/packages/` folder (Harmony 2.3.6, MSTest
  3.10.2) cleanup candidate, and the hardcoded RimWorld path in
  `Source/Directory.Build.props` (overridable via `RIMWORLD_DIR`).
- Findings + per-item decisions are captured in `audit.md`; approved decisions are
  mirrored into `.asd/project/decisions-log.md`; the user approves at the
  audit-phase gate.

## Out of scope
- Improvement opportunities the user marks as deferred at the audit-phase gate —
  recorded in `audit.md` for a later sprint, not implemented this sprint.
