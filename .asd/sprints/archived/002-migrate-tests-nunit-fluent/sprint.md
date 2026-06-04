---
responsibility:
  owns: sprint scope, goal, top-level acceptance criteria
  excludes: task breakdown, design decisions, code, audit findings
  delegates_to: plan.md (tasks), design/ docs (decisions), audit.md (audit)
---

# Sprint 002-migrate-tests-nunit-fluent

## Goal
Migrate the test suite to NUnit + FluentAssertions.

Migrate `Source/LordKuper.Common.Tests` (net472) from xUnit to NUnit, and convert all assertions to FluentAssertions `.Should()` style wherever possible, replacing `Assert.*` (~236 call sites across ~146 test methods in 11 files). This is a behaviour-preserving refactor with opportunistic cleanup.

In scope:

- **Packages (`.csproj`)**: remove `xunit` and `xunit.runner.visualstudio`; add `NUnit`, `NUnit3TestAdapter`, and `FluentAssertions 7.x` (free, Apache-2.0); update `<Using>` includes (drop `Xunit`).
- **Test infrastructure**: re-implement the custom `RimWorldTestFramework` / assembly `AssemblyResolve` handler (currently `[assembly: TestFramework(...)]` + `XunitTestFramework`) as an NUnit assembly-level `[SetUpFixture]` with `OneTimeSetUp`; map `StaticState` isolation (`StaticStateFixture`, `[CollectionDefinition("StaticState", DisableParallelization=true)]`, `[Collection]`, `StaticStateTestBase`) to NUnit `[NonParallelizable]` + `[SetUp]`/`[TearDown]`.
- **Attribute mapping**: `[Fact]` → `[Test]`; `[Theory]` + `[InlineData]` → `[TestCase]`; `[Fact(Skip="…")]` → `[Test, Ignore("…")]` (3 cases in `PawnFilterTests.cs`).
- **CI / coverage**: update `scripts/coverage.ps1` (AltCover) and any CI config to drive the NUnit3 runner.
- **Behaviour**: refactor + cleanup — overall coverage stays equivalent; weak/redundant assertions may be tightened or pruned.

## Acceptance
- Project builds.
- All tests (including the 3 ignored) run and pass under the NUnit3 adapter.
- No `Assert.*` calls remain where a FluentAssertions equivalent exists.
- Coverage script (`scripts/coverage.ps1`) works under NUnit.

## Out of scope
- Adding new test coverage beyond the existing suite (coverage stays equivalent overall, not expanded).
- Changes to production code in `Source/LordKuper.Common` except as strictly required by the test migration.
