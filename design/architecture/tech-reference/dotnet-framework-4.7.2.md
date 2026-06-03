---
responsibility:
  owns: project-vetted reference for the .NET Framework 4.7.2 (net472) target (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# .NET Framework @ 4.7.2 (net472)

## Canonical source
- Target framework docs: https://learn.microsoft.com/en-us/dotnet/framework/
- TFM reference: https://learn.microsoft.com/en-us/dotnet/standard/frameworks
- Last verified: 2026-06-03

## Acquisition model
- Set via `<TargetFramework>net472</TargetFramework>` in both `Source/LordKuper.Common.csproj` and `Tests/LordKuper.Common.Tests.csproj`.
- Chosen to match the runtime RimWorld loads (Mono/CLR compatible with .NET Framework 4.7.2). This is a host-imposed constraint, not a free choice.

## API surface used in project
- BCL surface available to `net472`: `System.*` collections, LINQ, reflection, `System.Math`, string/IO primitives used throughout helpers, caches, and filters.
- `<Nullable>` reference-type annotations: `annotations` in the library, `enable` in the test project.
- `<LangVersion>latest</LangVersion>`: C# language version is left literal as `latest`; the compiler resolves it to the highest version the toolchain supports for this TFM.
- Test project uses `<ImplicitUsings>enable</ImplicitUsings>`.

## Version-specific notes
- `net472` is fixed by the RimWorld runtime; newer TFMs (e.g. `net48`, `net8.0`) cannot be adopted because the game will not load them.
- Despite `net472`, `LangVersion=latest` allows modern C# syntax that the compiler can lower onto the 4.7.2 BCL; nullable annotations are compile-time only.

## Deprecations and breaking changes from prior version
- None adopted — the target is held constant. Any move off `net472` would be a breaking, host-gated decision (out of scope; belongs to an ADR), not a routine upgrade.

## Project conventions
- Keep `net472` on every project (library and tests) to stay loadable by RimWorld.
- Preserve `LangVersion=latest` literally (do not substitute the effective C# version), matching the csproj source of truth.
- Maintain nullable annotations (`annotations`/`enable`) and warnings-as-errors at max warning level — code must compile clean against the 4.7.2 toolchain.

## Known issues and workarounds
- Features depending on newer runtimes (e.g. `Span<T>` ergonomics, default-interface members) are unavailable or limited on `net472`; stay within the 4.7.2-compatible surface.
