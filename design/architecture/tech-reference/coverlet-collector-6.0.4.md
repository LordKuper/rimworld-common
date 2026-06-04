---
responsibility:
  owns: project-vetted reference for coverlet.collector 6.0.4 (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# coverlet.collector @ 6.0.4

> **SUPERSEDED / RETIRED (2026-06-04, sprint 002).** This reference described a coverage path the project does
> **not** actually use. The real coverage harness is the **AltCover** global tool, invoked via
> `scripts/coverage.ps1` (Cecil static instrumentation), because `coverlet.collector` silently yields 0%
> against the RimWorld-referencing assembly. The test project does **not** reference `coverlet.collector`.
> See [stack.html](../stack.html) (Tooling / Version-pinning) for the current AltCover framing and
> [ADR-0004](../adr/adr-0004-test-framework-xunit-to-nunit.html) for the AltCover `--assemblyFilter` change
> (`xunit` → `nunit`). Kept for history only — do **not** treat the content below as current.

## Canonical source
- Project: https://github.com/coverlet-coverage/coverlet
- NuGet: https://www.nuget.org/packages/coverlet.collector/6.0.4
- Last verified: 2026-06-03

## Acquisition model
- NuGet `PackageReference` in `Tests/LordKuper.Common.Tests.csproj`, version `6.0.4`.
- Configured `PrivateAssets=all` with the full `IncludeAssets` set (runtime; build; native; contentfiles; analyzers; buildtransitive) — a **test-time-only** data collector, never a runtime dependency of the library.

## API surface used in project
- VSTest **data collector** form (`--collect:"XPlat Code Coverage"` / collector integration), not the MSBuild or global-tool form.
- Plugs into the `Microsoft.NET.Test.Sdk` / VSTest pipeline to emit coverage during `dotnet test` runs; no in-test API is called from project code.

## Version-specific notes
- **Large upstream lag**: pinned at `6.0.4` while the latest stable upstream is `10.0.1` — several major versions behind. This is the biggest version gap in the dependency set.
- Risk is contained: coverage collection is test-time only, so the blast radius is limited to coverage reporting and does not affect shipped assemblies or runtime behaviour. Rated MEDIUM overall on that basis.
- The 6.0.x collector is compatible with the pinned `Microsoft.NET.Test.Sdk` 17.14.1 and xUnit v2 stack.

## Deprecations and breaking changes from prior version
- Several major versions (7.x → 10.x) exist upstream and may carry collector configuration / output-format changes. None are adopted here; any upgrade should verify collector compatibility with the pinned test SDK before bumping.

## Project conventions
- Keep `PrivateAssets=all` so the collector never leaks into runtime output.
- Use the collector (VSTest) form, consistent with the `Microsoft.NET.Test.Sdk` pipeline; do not switch to the MSBuild form without cause.
- Treat the version gap as a known, accepted lag — upgrading is low-urgency (test-time only) but should be verified against the test SDK when undertaken.

## Known issues and workarounds
- A coverlet collector mismatched with the test SDK can silently produce no coverage output — verify a coverage file is emitted after any version change to either package.
