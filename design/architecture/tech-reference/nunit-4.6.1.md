---
responsibility:
  owns: project-vetted reference for the NUnit test framework (NUnit 4.6.1) on net472 — apis used, version specifics, project conventions
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# NUnit @ 4.6.1

The unit-test framework for `Source/LordKuper.Common.Tests` (net472), replacing xUnit 2.9.3. Paired with `NUnit3TestAdapter` 6.2.0 (see [NUnit3TestAdapter @ 6.2.0](nunit3-testadapter-6.2.0.md)) and `Microsoft.NET.Test.Sdk` 17.14.1. Assertions are done with FluentAssertions, not NUnit's `Assert` API (see [FluentAssertions @ 7.2.2](fluentassertions-7.2.2.md)).

## Canonical source
- NUnit docs: https://docs.nunit.org/
- NUnit NuGet: https://www.nuget.org/packages/NUnit/4.6.1
- Framework release notes: https://docs.nunit.org/articles/nunit/release-notes/framework.html
- Last verified: 2026-06-04

## Acquisition model
- NuGet `PackageReference` in `Source/LordKuper.Common.Tests/LordKuper.Common.Tests.csproj`, version `4.6.1`.
- The test runner (`NUnit3TestAdapter`) is a separate package and carries the `PrivateAssets=all` / `IncludeAssets` runner convention; the `NUnit` framework package itself is a normal compile + runtime reference.
- Tests run on `net472` (matching the library target). NUnit 4.6.1's framework floor is `.NETFramework 4.6.2`; `net472` satisfies "4.6.2 or higher", so the package is compatible. (`.NET Framework 4.7.2` is not listed verbatim in the NuGet computed-target list but is covered by the 4.6.2-or-higher rule.)

## API surface used in project
- `[Test]` — single test case (was xUnit `[Fact]`).
- `[TestCase(...)]` — inline parameterized case (was xUnit `[Theory]` + `[InlineData]`); one `[TestCase]` per data row, stacked on a single `[Test]`-style method. Enum-flag expressions (`TestFlags.FlagA | TestFlags.FlagB`) are valid `[TestCase]` arguments because they are constant expressions.
- `[Ignore("reason")]` — skips a test but keeps it discovered/reported as ignored (was xUnit `[Fact(Skip="...")]`). Applied alongside `[Test]`.
- `[SetUpFixture]` — assembly-level (namespace-less / global) fixture whose `[OneTimeSetUp]` method runs once before any test in the assembly. Used to register the RimWorld `AppDomain.AssemblyResolve` handler before any RimWorld-typed test type loads. See [ADR-0006](../adr/adr-0006-rimworld-assemblyresolve-setupfixture.html).
- `[OneTimeSetUp]` / `[OneTimeTearDown]` — once-per-fixture (or once-per-assembly inside a global `[SetUpFixture]`).
- `[SetUp]` / `[TearDown]` — per-test before/after hooks; map cleanly to the previous xUnit constructor + `IDisposable` per-test pattern. Used to drive the StaticState snapshot/restore. See [ADR-0007](../adr/adr-0007-staticstate-isolation-nunit-remap.html).
- `[NonParallelizable]` — marks a test/fixture as not eligible for parallel execution. Belt-and-suspenders only: NUnit runs **non-parallel by default** unless `[Parallelizable]` / `[assembly: Parallelizable]` is present, and this project sets neither.
- The NUnit `Assert.*` API is **deliberately not used** — all assertions are FluentAssertions `.Should()`. NUnit is used only for the attribute/lifecycle model and the runner integration.

## Version-specific notes
- **Classic `Assert` API restored as extension methods (4.5.0+).** `Assert.AreEqual` and friends were removed in 4.0 and brought back in 4.5.0 as extension methods on `Assert`, available via the `NUnit.Framework` namespace. This project does not call them (FluentAssertions instead), so the change is informational.
- **`TestDelegate` / `ActualValueDelegate` became `Action` / `Func<T>` (4.6).** This can cause overload-ambiguity errors when those delegate types are used explicitly **unless** the project compiles with C# 13+ (.NET 9 SDK or newer). This project uses `LangVersion=latest` on .NET SDK 10.0.300 (C# 14), so the disambiguation requirement is satisfied. The project also does not use `Assert.Throws`/`Assert.That(..., Throws...)` (exception assertions go through FluentAssertions `action.Should().Throw<T>()`), so `TestDelegate` is not referenced regardless.
- **Params-based message syntax removed (4.0).** The old `Assert.X(..., "msg {0}", arg)` form is gone; not used here.
- **Non-parallel by default.** Unlike xUnit (parallel-by-default with `[Collection]` for serialization), NUnit does not parallelize unless explicitly told to. The xUnit `[CollectionDefinition(DisableParallelization=true)]` serialization marker therefore has no required NUnit counterpart; `[NonParallelizable]` is applied only to make the static-isolation intent explicit on the three static-touching classes.

## Deprecations and breaking changes from prior version
- Migrating **from xUnit 2.9.3** (not from a prior NUnit): attribute model changes are `[Fact]`→`[Test]`, `[Theory]`+`[InlineData]`→`[TestCase]`, `[Fact(Skip)]`→`[Test, Ignore]`; lifecycle changes are ctor/`IDisposable`→`[SetUp]`/`[TearDown]` and `[assembly: TestFramework]`→global `[SetUpFixture]`/`[OneTimeSetUp]`. There is no NUnit analog to xUnit's `[assembly: TestFramework]` framework-swap hook.
- The supersedes/test-stack swap is governed by [ADR-0004](../adr/adr-0004-test-framework-xunit-to-nunit.html); this reference fully supersedes [xUnit @ 2.9.3](xunit-2.9.3.md) for the test-framework role.

## Project conventions
- Stay on the NUnit **4.x** line; pin `NUnit` `4.6.1` and keep it in lockstep with `NUnit3TestAdapter` 6.x.
- Use the global `<Using Include="NUnit.Framework" />` rather than per-file `using NUnit.Framework;` (mirrors the previous `Using Include="Xunit"` convention). `ImplicitUsings=enable`.
- Assertions are FluentAssertions only; do **not** introduce NUnit `Assert.*` or `Assert.That` calls.
- Static-state isolation uses per-test `[SetUp]`/`[TearDown]` for true per-test granularity (not `[OneTimeSetUp]` per class), preserving the original xUnit ctor+`Dispose` semantics.
- The assembly-level RimWorld resolver lives in a **namespace-less** `[SetUpFixture]` so it applies to the whole assembly.
- Must compile warning-clean under the inherited `Source/Directory.Build.props` governance (`TreatWarningsAsErrors`, `WarningLevel 9999`, `Nullable=enable`) — see [ADR-0003](../adr/adr-0003-build-governance.html).

## Known issues and workarounds
- **Resolver timing.** If the RimWorld `AssemblyResolve` handler is not registered before the first RimWorld-typed test type is JIT-resolved, RimWorld/Unity-referencing tests fail at discovery/first-touch with `FileNotFoundException`/`TypeLoadException` (looks like mass test failure, not a wiring bug). The global `[SetUpFixture]` + `[OneTimeSetUp]` runs before any test, which is the guarantee relied on; verify by running a RimWorld-typed suite (`StatWeightTests`, `RimWorldTimeTests`, `PawnFilterTests`) first.
- **Parallelism assumption.** Do not add `[assembly: Parallelizable]` or `[Parallelizable]` — the static-state snapshot/restore isolation assumes serial execution within the assembly.
