---
responsibility:
  owns: project-owner custom rules read by all agents in all phases
  excludes: phase-specific rules (design-only, coding-only)
  delegates_to: custom-design-rules.md (design/design-review), custom-coding-rules.md (impl/impl-review), .asd/rules/ (workflow rules)
---

# Custom Common Rules

## Project layout

- **Production**: `LordKuper.Common` (`Source/`). Target framework `net472`. References RimWorld `Assembly-CSharp` + Unity modules (via `$(RimWorldManagedDir)`) and `Lib.Harmony` 2.4.2 (compile-only: `PrivateAssets=all`, `ExcludeAssets=runtime`).
- **Tests**: `LordKuper.Common.Tests` (`Tests/`). xUnit, `net472`.
