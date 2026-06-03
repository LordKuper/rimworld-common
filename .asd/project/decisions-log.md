---
responsibility:
  owns: append-only chronology of approved decisions across project lifetime
  excludes: sprint state, code review notes, custom rules
  delegates_to: .asd/sprints/ (sprint state), reviews/ (review notes), custom-common-rules.md / custom-design-rules.md / custom-coding-rules.md (rules)
---

# Decisions Log

Append-only. Never edited or removed. New entries appended below.

## Entry format

```markdown
## YYYY-MM-DD — <one-line summary>

- **Decision**: <what was decided>
- **Rationale**: <why>
- **Affected docs**: <links> (optional)
```

## Entries

<!-- entries appended below this line -->

## 2026-06-03 — ASD initialized for rimworld-common

- **Decision**: ASD workflow initialized (brownfield). Config: chat=ru, docs=en, subsystem_decomposition=disabled, backward_compat=none, external_review=enabled, os=windows. Tools detected ok: node v24.15.0, npm 11.16.0, codex 0.135.0, gh 2.90.0, jb (resharper.globaltools) 2026.1.2.
- **Rationale**: Shared RimWorld mod common library (net472, Harmony 2.4.2, xUnit tests). Flat design docs suffice; free to break public API; Codex external review enabled.
- **Affected docs**: `.asd/project/config.yaml`, `commands.yaml`, custom rule files.

## 2026-06-03 — Custom rules seeded

- **Decision**: Authored custom rules — common: project layout; design: modding/patchability, data-driven, determinism; coding: nullability (annotations mode), zero-warnings, XML docs, jb-cleanup/jb-inspect flow, suppression policy, logging, xUnit static-state isolation. Ported applicable rules from the Glings project, adapted to RimWorld/.NET-Framework; dropped Unity-only rules (UI Toolkit, Addressables, AppUI, Jobs/Burst).
- **Rationale**: Reuse vetted conventions where they transfer; nullability rule inverted vs Glings because Source uses `<Nullable>annotations</Nullable>`.
- **Affected docs**: `custom-common-rules.md`, `custom-design-rules.md`, `custom-coding-rules.md`, `commands.yaml`.

## 2026-06-03 — Project concept reverse-engineered from brownfield

- **Decision**: Authored `design/product/concept.html` via variant D (brownfield extraction). Included 6 sections — Vision, Target users (generic "dependent RimWorld mods"), Value proposition, Pillars, Anti-Pillars, Constraints. Skipped Core Identity, Unique Hook, Success metrics. provenance=reverse-engineered, status=draft.
- **Rationale**: Existing code library (LordKuper.Common) with no prior concept doc; grounded the concept in actual source/csproj/README rather than inventing product ambition. Marketing/competitive/metrics sections omitted as N/A for a private shared utility library.
- **Affected docs**: `design/product/concept.html`

## 2026-06-03 — Tech stack reverse-engineered from manifests

- **Decision**: Authored `design/architecture/stack.html` (variant D, brownfield) + 6 tech-reference docs. Stack: C# (LangVersion=latest) on net472; Lib.Harmony 2.4.2 (compile-only, host-provided at runtime); RimWorld Assembly-CSharp + UnityEngine Core/IMGUI/TextRendering (game-provided file refs, not NuGet); .NET Framework 4.7.2 / Mono host; RimWorld 1.5 + 1.6 both active targets via version-specific folders. Tooling: .NET SDK 10.0.300, jb 2026.1.2, xUnit 2.9.3 (v2, kept as-is), Microsoft.NET.Test.Sdk 17.14.1, xunit.runner.visualstudio 2.8.2, coverlet.collector 6.0.4. Sections included: Languages/Frameworks/Runtime/Tooling/Constraints; Architecture Principles + Layers diagram deferred to ADR/C4. Also corrected `concept.html`: RimWorld version note changed from "1.6+" to "currently 1.5 and 1.6".
- **Rationale**: Grounded stack in actual csproj/sln manifests. Production deps current; test tooling lags (coverlet 6.0.4 vs 10.0.1, Test.Sdk 17 vs 18) but contained to test-time. Risk summary: overall MEDIUM — coverlet test-tooling lag + RimWorld 1.5/1.6 API drift; runtime-shipping deps LOW.
- **Affected docs**: `design/architecture/stack.html`, `design/architecture/tech-reference/*.md`, `design/product/concept.html`
