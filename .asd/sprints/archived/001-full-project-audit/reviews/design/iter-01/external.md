[REVIEW-design-external]: FAIL

# External Review Report

- **Phase**: design-review
- **Iteration**: 1
- **Severity floor (this iter)**: low (all severities reported)
- **Reviewer**: Codex CLI (`codex-cli 0.136.0`, model `gpt-5.5`), non-interactive `codex review -` with embedded design-phase rubric + full draft payload
- **Payload**: full content of `design/prd.html` + `design/adr.html` (iter 1 = full, per external-review.md)

> **Invocation note.** The `external-review.md` rule documents `codex.exe review --json --input <file> --output <file>`. The installed Codex CLI 0.136.0 `review` subcommand does not expose `--json`, `--input`, or `--output`; its non-interactive form takes a prompt argument / stdin (`-`) and emits prose. The agent therefore fed the design-phase rubric plus both drafts via stdin and parsed the structured prose verdict (which Codex returned in the exact requested `FAIL/CONCERNS/APPROVE` shape). Codex authenticated and ran cleanly (exit 0); no skip was needed.

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | adr.html · ADR-0001 Decision | ADR-0001 extracts the heavy `StatHelper`/`WorkTypeStatMap` static-ctor bodies into `Rebuild()` but **still calls them from the static constructors**, preserving DefDatabase-walking work at type-load. This sits in tension with the custom-design rule "no static constructors with heavy side effects" — the decision explicitly keeps that load-time behavior rather than moving to lazy/explicit init. | Either make DefDatabase cache construction explicit/lazy through the new seam rather than heavy work in static ctors, OR record an explicit rationale in the ADR that the idiomatic-RimWorld load-time build is intentionally retained (the audit already treats this as idiomatic + Fail-Fast-on-Load; the ADR should state that exception to the rule rather than leave it implicit). |
| 2 | high | adr.html · ADR-0003 Decision | ADR-0003 asserts `Source/Directory.Build.props` will inherit the new repo-root `Directory.Build.props` "via the standard MSBuild import chain." MSBuild only auto-imports the **nearest** `Directory.Build.props` walking up from the project; once `Source/Directory.Build.props` exists, the root one is **not** auto-imported unless the child explicitly imports it. Source projects could silently miss the root zero-warnings / nullable governance — defeating ADR-0003's own SSoT goal. | Require `Source/Directory.Build.props` to explicitly `<Import>` the repo-root props (e.g. via `$([MSBuild]::GetPathOfFileAbove(...))`), or move RimWorld-path resolution into the root file / a separately-imported props file so there is no shadowing child. The decision's "verify MSBuild import precedence" bullet should be tightened to mandate the explicit import. |
| 3 | medium | prd.html · AC-2 | AC-2 is non-atomic / ambiguous: it permits *either* documenting `RIMWORLD_DIR`+fail-fast *or* merely removing the machine-specific literal default ("...or the machine-specific literal default is removed"). ADR-0003 requires **both** removing the fallback **and** emitting a clear MSBuild build error when neither var resolves. The PRD criterion is looser than the decision it traces to. | Make AC-2 a single atomic assertion aligned to ADR-0003: remove the hardcoded fallback, document `RIMWORLD_DIR`/`RimWorldDir` as the supported override, and require a clear fail-fast MSBuild error when neither resolves. |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | none — iteration 1 floor is low, all severities retained | N/A |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | none — Codex returned no wording/style/naming-only items; all three findings identify concrete defects | N/A |

## Verdict
FAIL: 3 (high=2, medium=1, low=0)

The two high-severity findings are genuine design defects worth resolving before the design gate closes:
- ADR-0003's MSBuild-inheritance assumption (#2) is a concrete technical error that would silently undermine the sprint's own governance SSoT goal (AC-3 / AC-5). This is the strongest finding.
- ADR-0001's heavy-static-ctor retention (#1) is a real rule-tension that the ADR resolves by preservation but does not explicitly justify against the custom-design rule; the audit's reasoning (idiomatic RimWorld + Fail-Fast-on-Load) should be lifted into the ADR's consequences/alternatives so the exception is on record, not implicit.
- AC-2 (#3) is a low-cost atomicity tightening to bring the PRD criterion in line with ADR-0003.

The previously contentious areas (WorkTypeStatMap data-driven "FAIL", StatRanges determinism) were correctly NOT re-raised — Codex respected the recorded user reclassification / ADR-0002 adaptive-contract decision.

## Next action
PM / creator (BA + Architect): address the three findings.
- ADR-0003 (#2): add an explicit import of the repo-root props from `Source/Directory.Build.props` (or restructure so no shadowing child exists), and harden the "verify import precedence" bullet into a mandate. **Highest priority — technical correctness of the governance SSoT.**
- ADR-0001 (#1): add an explicit rationale/consequence stating the load-time DefDatabase build is an intentional, idiomatic-RimWorld exception to "no heavy static ctors," guarded by Fail-Fast-on-Load + the AC-24 `Logger` context requirement.
- AC-2 (#3): rewrite as a single atomic criterion matching ADR-0003 (remove fallback AND fail fast AND document `RIMWORLD_DIR`).

This external verdict counts as one reviewer in the DoD check (external_review: enabled). It does not auto-block on its own — it merges with the internal design reviewers' pool; PM aggregates and re-dispatches iteration 2 with the previous finding set for stalemate detection.
