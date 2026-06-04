[REVIEW-impl-documentation]: CONCERNS

# Review — documentation

- **Phase**: impl-review
- **Iteration**: 02

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | `design/architecture/adr/adr-0006-...html` (Decision, Accepted-risk, Alternatives) vs `Source/LordKuper.Common.Tests/LordKuper.Common.Tests.csproj:73-94` | **Doc-vs-impl drift on the chosen resolver fallback.** ADR-0006 documents the discovery-time ordering risk and names `[ModuleInitializer]` as the sole *anticipated fallback* if discovery-time resolution fails; it explicitly **rejects** "Copy the RimWorld DLLs next to the test output instead of resolving at runtime". The implementation shows the ordering risk *did* manifest — the csproj `CopyRimWorldTestDeps` target copies Assembly-CSharp / Unity modules / netstandard into the test bin precisely because "the AssemblyResolve hook fires too late for reflection-only scanning". So the actually-adopted fallback is the DLL-copy the ADR rejected, not the `[ModuleInitializer]` the ADR anticipated. The persistent ADR no longer reflects how the resolver problem was solved in code. | Update ADR-0006 in design-promote: record that the asserted ordering did fail at discovery, that the adopted resolution is a build-time RimWorld-DLL copy to the test bin (move it out of the rejected-alternatives list), and that `[ModuleInitializer]` was *not* taken. Architect owns this edit; reviewer does not write `design/`. |
| 2 | medium | `design/architecture/adr/adr-0006-...html` Decision bullet "Idempotency guard retained" + Negative consequence #3 | **Stale claim of a retained idempotency guard.** ADR-0006 states the `AppDomain.GetData/SetData("RimWorldResolverInitialized")` guard is "kept"/"retained" and justifies it by the `[ModuleInitializer]`+`[OneTimeSetUp]` coexistence. `RimWorldResolverSetup.cs` contains **no** such guard (no `GetData/SetData`, no `RimWorldResolverInitialized`), consistent with `[ModuleInitializer]` never being adopted. The ADR asserts code that does not exist. | In design-promote, drop the "retained idempotency guard" claim from ADR-0006 (or note it was removed once the single-path `[OneTimeSetUp]` registration made it unnecessary). |
| 3 | medium | `design/architecture/adr/adr-0007-...html` Decision bullet (AC-22) "reusable logic stays in the renamed `StaticStateFixture` type" | **Phantom type in persistent doc.** ADR-0007 says the snapshot/restore body "stays in the renamed `StaticStateFixture` type". No `StaticStateFixture` type exists in the test project — the snapshot/restore logic is inline in `StaticStateTestBase.{SetUpStaticState,TearDownStaticState}` (`StaticStateTestBase.cs`). `StaticStateFixture` appears only in stale XML-doc comments in `StatRangesTests.cs:9` and `StatefulSubsystemTests.cs:13,151`. The ADR's described code structure does not match the implementation. | In design-promote, correct ADR-0007 to describe the inline `StaticStateTestBase` `[SetUp]`/`[TearDown]` placement (no separate fixture type). Separately, the stale `StaticStateFixture` test-comment references are a code-side concern for the Implementation reviewer, not this gate. |

## Verdict
CONCERNS: 3

## Next action
Architect (design-promote phase) reconciles ADR-0006 and ADR-0007 with the implemented resolver-fallback (build-time DLL copy, no `[ModuleInitializer]`, no idempotency guard) and the inline `StaticStateTestBase` placement (no `StaticStateFixture` type). Reviewer does not edit persistent `design/`. Confirmed actual: `commands.yaml` coverage floor (>=37.2%), `stack.html`, and `nunit-4.6.1.md` tech-reference all match the implemented code — no drift there.

## Escalations (optional)
- none
