---
responsibility:
  owns: project-vetted reference for the xUnit test stack (xunit 2.9.3 + Microsoft.NET.Test.Sdk 17.14.1 + xunit.runner.visualstudio 2.8.2) — apis used, version specifics, project conventions
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# xUnit @ 2.9.3 (test stack: + Microsoft.NET.Test.Sdk 17.14.1 + xunit.runner.visualstudio 2.8.2)

> **SUPERSEDED (2026-06-04, sprint 002).** The project migrated off xUnit. The test framework role is now
> [NUnit @ 4.6.1](nunit-4.6.1.md) + [NUnit3TestAdapter @ 6.2.0](nunit3-testadapter-6.2.0.md); assertions are
> [FluentAssertions @ 7.2.2](fluentassertions-7.2.2.md). `Microsoft.NET.Test.Sdk` 17.14.1 is retained (the
> NUnit adapter needs the same VSTest host). The decision is governed by
> [ADR-0004](../adr/adr-0004-test-framework-xunit-to-nunit.html) (framework) and
> [ADR-0005](../adr/adr-0005-fluentassertions-7x.html) (assertions). This document is kept for history only —
> do **not** treat it as current; `xunit` / `xunit.runner.visualstudio` are no longer referenced by the test project.

This reference covers the whole (now retired) xUnit-based test stack as one unit, since the three packages are version-coupled:

| Package | Pinned | Role |
|---|---|---|
| `xunit` | 2.9.3 | Test framework: `[Fact]` / `[Theory]`, assertions. |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | Test host + VSTest adapter integration that makes the project runnable. |
| `xunit.runner.visualstudio` | 2.8.2 | VSTest runner enabling xUnit discovery/execution in VS and CI. |

## Canonical source
- xUnit docs: https://xunit.net/
- xunit NuGet: https://www.nuget.org/packages/xunit/2.9.3
- Microsoft.NET.Test.Sdk: https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/17.14.1
- xunit.runner.visualstudio: https://www.nuget.org/packages/xunit.runner.visualstudio/2.8.2
- Last verified: 2026-06-03

## Acquisition model
- All three are NuGet `PackageReference`s in `Tests/LordKuper.Common.Tests.csproj`.
- `xunit.runner.visualstudio` is configured `PrivateAssets=all` with the full `IncludeAssets` set (runtime; build; native; contentfiles; analyzers; buildtransitive) — a test-time-only tool that is not a transitive runtime dependency.
- Tests run on `net472` (matching the library target).

## API surface used in project
- `Xunit.FactAttribute` (`[Fact]`) and `Xunit.TheoryAttribute` (`[Theory]`) with `[InlineData]` / data attributes for parameterized cases.
- `Xunit.Assert.*` assertion API.
- Global `Using Include="Xunit"` so `Xunit` is implicitly available in every test file.
- `Microsoft.NET.Test.Sdk` provides the test host entry point; `xunit.runner.visualstudio` provides the VSTest adapter for discovery/run.

## Version-specific notes
- **xUnit 2.x is the legacy line** (a 3.x line exists upstream). This is recorded as a factual statement only — v2 remains fully supported and the project intentionally stays on v2.9.3 with no planned migration.
- `xunit.runner.visualstudio` 2.8.2 is the runner paired with the xUnit v2 line; it is intentionally kept on the v2-compatible release rather than the 3.x runner line.
- `Microsoft.NET.Test.Sdk` 17.14.1 is current/near-current — minimal lag.

## Deprecations and breaking changes from prior version
- No project-affecting breaking changes adopted; the stack is held on the v2 line. (A future v2→v3 move would be a deliberate, breaking decision belonging to an ADR — explicitly **not** recommended here.)

## Project conventions
- Stay on the xUnit **v2** stack (2.9.3 + runner 2.8.2); do not introduce a v3 migration without an explicit decision.
- Use the global `Using Include="Xunit"` rather than per-file `using Xunit;`.
- Keep the test runner / collector packages `PrivateAssets=all` so they never leak as runtime dependencies.
- Tests target `net472` with nullable `enable` and implicit usings.

## Known issues and workarounds
- Mixing v2 framework with the v3 runner (or vice versa) breaks discovery — keep `xunit` and `xunit.runner.visualstudio` on matching major lines (both v2 here).
