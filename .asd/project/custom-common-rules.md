---
responsibility:
  owns: project-owner custom rules read by all agents in all phases
  excludes: phase-specific rules (design-only, coding-only)
  delegates_to: custom-design-rules.md (design/design-review), custom-coding-rules.md (impl/impl-review), .asd/rules/ (workflow rules)
---

# Custom Common Rules

## Project layout

- All source lives under `Source/`: the solution `Source/LordKuper.Common.slnx`, the shared `Source/Directory.Build.props`, and one folder per project.
- **Production**: `Source/LordKuper.Common/` (`LordKuper.Common.csproj`). Target framework `net472`. References RimWorld `Assembly-CSharp` + Unity modules (via `$(RimWorldManagedDir)`) and `Lib.Harmony` 2.4.2 (compile-only: `PrivateAssets=all`, `ExcludeAssets=runtime`). Build output goes to `1.6/Assemblies/`.
- **Tests**: `Source/LordKuper.Common.Tests/` (`LordKuper.Common.Tests.csproj`). NUnit 4.6.x + NUnit3TestAdapter + FluentAssertions 7.x, `net472`.
