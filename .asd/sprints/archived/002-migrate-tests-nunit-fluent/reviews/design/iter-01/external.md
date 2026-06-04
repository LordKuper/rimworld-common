[REVIEW-design-external]: FAIL

# External Review Report

- **Phase**: design-review
- **Iteration**: 1
- **Severity floor (this iter)**: low
- **Codex CLI**: ran (codex-cli 0.136.0, `codex exec`, model gpt-5.5, read-only sandbox). Probe `codex --version` succeeded; authenticated. The installed `codex review` subcommand exposes only git-diff inputs (`--base`/`--commit`/`--uncommitted`), not the rules' `--json --input --output` signature; design-review iter-1 requires full design-file content as payload, so the drafts (prd.html, adr.html) plus supporting consistency context were supplied to `codex exec` as a structured review prompt with an explicit verdict contract. Output mapped below.

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | critical | adr.html : ADR-0006 (resolver seam) / prd.html AC-19–AC-21 | Codex `severity=critical`. The seam relies on a global namespace-less `[SetUpFixture]`'s `[OneTimeSetUp]` to register the `AssemblyResolve` handler "before any RimWorld-typed test class loads" (AC-20). `[OneTimeSetUp]` is an **execution-time** lifecycle hook; NUnit **discovery** can reflect over / JIT-load RimWorld-typed fixtures before any `[OneTimeSetUp]` runs. The design's own "highest-risk piece" mitigation does not establish that the execution-phase hook precedes discovery-phase type load, so the AC-20 before-load contract is not demonstrably satisfied by the chosen mechanism. | Either (a) register the resolver via a guaranteed-early hook (e.g. `[ModuleInitializer]`, currently rejected in the ADR on ordering-uncertainty grounds — but the same uncertainty undercuts `[OneTimeSetUp]`), or (b) tighten ADR-0006/AC-20 to state and verify the actual NUnit discovery-vs-OneTimeSetUp ordering on net472 with the NUnit3 adapter (the ADR already mandates run-not-inspection verification; make the ordering claim explicit and evidence-backed rather than assumed). |
| 2 | high | prd.html : AC-9 (also stats header "28 AC" vs count, and ADR-0004 "~146") | Codex `severity=high`. AC-9's case arithmetic is internally inconsistent: 132 single-case tests + 13 expanded parameterized rows = **145**, not "~146"; and the criterion labels ignored tests as "executed cases" while 3 are `[Ignore]` (discovered/reported, not executed). The criterion conflates discovered vs executed counts, leaving the "no test silently dropped/duplicated" check ambiguous to verify. ADR-0004 repeats the "~146" figure. | Restate AC-9 with exact, separated counts, e.g.: 145 discovered cases = 129 active single-case + 3 ignored single-case + 13 active parameterized; executed-active = 142; ignored = 3. Align the ADR-0004 "~146" reference to the same exact figures. |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | none | floor is `low`; nothing below floor |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | none reported by Codex | — |

## Reviewer note (cross-doc consistency, surfaced during payload prep)

Not raised by Codex but observed while assembling the consistency context and recorded for the PM/Architect (does not change the verdict, which already FAILs):

- **ADR-id collision with persistent tech-references.** The new persistent tech-references point the resolver seam at `adr-0005-rimworld-assemblyresolve-setupfixture.html` and the isolation remap at `adr-0006-staticstate-isolation-nunit-remap.html`, while this sprint's `adr.html` numbers FluentAssertions = ADR-0005, the resolver seam = ADR-0006, and the isolation remap = ADR-0007. The cross-references will not resolve once these ADRs are promoted. Reconcile the ADR numbering between `adr.html` and the three tech-references before design-promote. (Severity if formally raised: medium — a concrete dangling-reference defect, not a nitpick.)

## Verdict
FAIL: 2

## Next action
Escalation required (per review-policy.md, FAIL with a critical finding). Finding 1 is a contract-soundness concern on the load-bearing resolver-timing seam: the Architect must either change the registration mechanism or make ADR-0006/AC-20's discovery-vs-execution ordering claim explicit and verifiable, since the consequence (mass test failure presenting as a non-obvious wiring bug) is exactly the audited primary risk. Finding 2 is an Architect/BA autofix (restate AC-9 + the ADR-0004 echo with exact counts). The reviewer-note ADR-id collision should be folded into the same revision pass. Then re-enter design-review iteration 2.
