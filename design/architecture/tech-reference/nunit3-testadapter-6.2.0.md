---
responsibility:
  owns: project-vetted reference for the NUnit3TestAdapter VSTest adapter (6.2.0) on net472 — apis used, version specifics, project conventions
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# NUnit3TestAdapter @ 6.2.0

The VSTest adapter that makes the NUnit suite discoverable and runnable under `dotnet test`, Visual Studio Test Explorer, and the AltCover coverage flow. Replaces `xunit.runner.visualstudio` 2.8.2. Pairs with `NUnit` 4.6.1 (see [NUnit @ 4.6.1](nunit-4.6.1.md)) and `Microsoft.NET.Test.Sdk` 17.14.1.

## Canonical source
- Adapter docs: https://docs.nunit.org/articles/vs-test-adapter/Index.html
- NuGet: https://www.nuget.org/packages/NUnit3TestAdapter/6.2.0
- Last verified: 2026-06-04

## Acquisition model
- NuGet `PackageReference` in `Source/LordKuper.Common.Tests/LordKuper.Common.Tests.csproj`, version `6.2.0`.
- Configured `PrivateAssets=all` with the full `IncludeAssets` set (`runtime; build; native; contentfiles; analyzers; buildtransitive`) — a test-time-only tool, exactly mirroring the convention previously used for `xunit.runner.visualstudio`. Never leaks as a transitive runtime dependency.
- Requires `Microsoft.NET.Test.Sdk` (kept at 17.14.1) as the VSTest host.
- Tests run on `net472`; the adapter targets `.NETFramework 4.6.2` minimum (covered by "4.6.2 or higher").

## API surface used in project
- No code-level API. The adapter is a build/run-time component only: it provides the VSTest discoverer/executor that bridges the NUnit engine to `dotnet test` and Test Explorer.
- Despite the "NUnit3" name, the adapter discovers and runs **NUnit 4.x** tests too — the "3" refers to the adapter/engine lineage, not the framework major. The package name stays `NUnit3TestAdapter` even when paired with NUnit 4.

## Version-specific notes
- 6.2.0 is the current line and supports the `Microsoft.Testing.Platform` bridge in addition to classic VSTest; this project uses the classic VSTest path via `dotnet test` and AltCover, unchanged from the xUnit setup.
- The adapter assemblies are named `nunit.framework` (framework) and `NUnit3.TestAdapter` (adapter). These names matter for the AltCover `--assemblyFilter` in `scripts/coverage.ps1`: the old `--assemblyFilter xunit` becomes `--assemblyFilter nunit` so the test-framework/adapter assemblies are excluded from the coverage denominator rather than the (now absent) xunit ones.

## Deprecations and breaking changes from prior version
- Migrating from `xunit.runner.visualstudio` 2.8.2: same `PrivateAssets`/`IncludeAssets` shape; the `dotnet test --no-build` invocation in `scripts/coverage.ps1` is runner-agnostic and works unchanged once the adapter swaps. Only the `--assemblyFilter` token changes (`xunit` → `nunit`).

## Project conventions
- Keep the adapter `PrivateAssets=all` so it never becomes a runtime dependency.
- Keep `Microsoft.NET.Test.Sdk` 17.14.1 alongside it (required host).
- Keep adapter and `NUnit` framework on matching/compatible lines (adapter 6.x + NUnit 4.6.x).
- The `.asd/project/commands.yaml` `test`/`coverage` command strings are runner-agnostic and need no change.

## Known issues and workarounds
- **Coverage assembly filter.** Leaving `--assemblyFilter xunit` stale after the swap silently counts NUnit adapter assemblies into the AltCover coverage denominator (coverage number drifts, not an obvious break). Update to `--assemblyFilter nunit` in the same change. See [ADR-0004](../adr/adr-0004-test-framework-xunit-to-nunit.html) consequences.
- **Runtime resolver cooperation.** `scripts/coverage.ps1` step 3 deletes the copied RimWorld DLLs and relies on the runtime `AssemblyResolve` handler to reload them lazily during the adapter-driven run; the ported NUnit `[SetUpFixture]` resolver (see [ADR-0006](../adr/adr-0006-rimworld-assemblyresolve-setupfixture.html)) must keep working or coverage runs break.
