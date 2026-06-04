[REVIEW-impl-quality]: CONCERNS

# Review — quality

- **Phase**: impl-review
- **Iteration**: 2

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | `Source/LordKuper.Common.Tests/LordKuper.Common.Tests.csproj:79-94` (`CopyRimWorldTestDeps`); `scripts/coverage.ps1:35-40` | The approved resolver design (ADR-0006 + AC-20/AC-27) is that the runtime `AssemblyResolve` handler lazily resolves RimWorld/Unity assemblies and the coverage harness *deletes* the copied RimWorld DLLs (the documented "step-3 DLL removal" + lazy-reload contract). The implementation instead **copies the RimWorld DLLs (incl. `netstandard.dll` 2.1) into the test bin and keeps them there**, because — per the in-code comment — "the AssemblyResolve hook fires too late for reflection-only scanning" at NUnit discovery time. This is the exact discovery-vs-execution ordering risk ADR-0006 flagged as "asserted, not proven," and it materialized. The fix is sound and makes the suite functional, but it diverges from the load-bearing mechanism described in the approved ADR/PRD: the runtime resolver is now only a fallback, and `coverage.ps1` has no DLL-removal step at all (the `--assemblyFilter nunit` change for AC-27 is correctly present). Net effect on behavior is positive (tests discover/run), but the as-built mechanism no longer matches the approved contract. | Reconcile the ADR-0006 "lazy resolve / step-3 DLL removal" wording with the as-built "copy DLLs into bin" approach via design-promote, or obtain explicit user acceptance of the deviation. No code change required for correctness; this is a contract-alignment item, not a defect. |

## Verdict
CONCERNS: 1

## Next action
Route to `impl` (fix mode) for reconciliation, or escalate finding #1 to the user as a contract deviation to accept. The divergence is between the as-built copy-into-bin mechanism and the approved ADR-0006/AC-27 lazy-resolve + DLL-removal contract; the code itself is correct and functional.

## Notes (sub-medium / dropped, not counted at iter-2 floor)
- `RimWorldResolverSetup.cs`: the ADR-0006 idempotency guard (`AppDomain.GetData/SetData("RimWorldResolverInitialized")`) that the ADR said would be "kept" was dropped. No behavioral risk today — `[OneTimeSetUp]` registers exactly once and the `[ModuleInitializer]` fallback that justified the guard was not added. Low; dropped.
- Assertion conversions verified faithful across all 11 files: `Assert.Equal`→`.Should().Be`/`.Equal`, `Assert.Throws<T>`→`action.Should().Throw<T>()`, `Single`→`ContainSingle`, `Empty`/`NotEmpty`, `NotSame`→`NotBeSameAs`, `Null`/`NotNull`, `Contains`/`DoesNotContain` overloads chosen correctly. `Assert.Equal(exp,act,4)`→`BeApproximately(exp, 5e-5f)` with correct band (±0.5e-4) and documented rationale (AC-12). Value-comparing forms preserved (`BeLessThan`/`BeGreaterThan`/`BeGreaterThanOrEqualTo`) per AC-13. No `Assert.*`/`[Fact]`/`[Theory]`/`[InlineData]`/`xUnit` remnant anywhere.
- Attribute migration verified: parameterized methods carry `[TestCase(...)]` only, no stray `[Test]` (AC-6/AC-9); 3 ignored tests preserved as `[Test]`+`[Ignore("Requires live RimWorld context for Verse.Translator")]` (AC-8). MathHelper 5 + EnumHelper 8 = 13 TestCases preserved.
- Isolation verified: `StaticStateTestBase` uses per-test `[SetUp]` (snapshot `DefProvider.Current`) / `[TearDown]` (restore + `StatHelper.Rebuild`, `WorkTypeStatMap.Rebuild`, reflection resets of `SkillStatMap._map`, `PassionHelper.{_isInitialized,_cachedPassions,PassionCache}`, `StatRanges.Ranges`) — snapshot/restore set matches ADR-0007/AC-22 verbatim; per-test granularity preserved (AC-23); `[NonParallelizable]` on the 3 static-touching classes (AC-24).
- Resolver match set (`IsRimWorldAssembly`) matches AC-20 exactly; env-var lookup `RIMWORLD_DIR`/`RimWorldDir` present; fail-fast on missing dir is the deliberate, in-scope replacement for the old hardcoded fallback path.
- KNOWN/DEFERRED (not re-flagged): StatLimit parameterless/string-ctor infinite-recursion is tracked out-of-scope; `StatLimitTests` deliberately use `new StatLimit(StatDef)` to avoid it. StatRanges first-value zero-injection latent bug retained by ADR-0002 (informational only).

## Escalations (optional)
- finding #1: may require user approval (contract deviation from approved ADR-0006/AC-27 mechanism). Behavior is correct; the deviation is in the as-built resolution mechanism vs. the documented one.
