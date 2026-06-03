---
responsibility:
  owns: project-owner custom rules read during impl and impl-review phases
  excludes: universal rules, design-only rules
  delegates_to: custom-common-rules.md (all phases), custom-design-rules.md (design/design-review)
---

# Custom Coding Rules

## Nullability

- Source uses `<Nullable>enable</Nullable>`. Keep nullable reference annotations (`string?`, `T?`, `[return:]`) consistent with this setting.
- Do NOT disable the nullable context.

## Zero warnings

- Source builds with `TreatWarningsAsErrors=true` and `WarningLevel 9999` in both Debug and Release. Code MUST compile warning-clean. A warning fails the build.

## Build / lint flow

- Before `build`: run `jb-cleanup` (applies solution code-cleanup profile).
- After `lint`: run `jb-inspect`, then verify `TestResults/jb-inspect.sarif` has no `error` or `warning` severity entries.
- Commands defined in `.asd/project/commands.yaml` (`jb-cleanup`, `jb-inspect`).

## Analyzer / linter suppressions

- Suppress findings only as a last resort — fix the real issue first.
- Prefer attribute-based suppression (`[SuppressMessage]`, `[UsedImplicitly]`, `[Pure]`) over comment pragmas (`#pragma warning disable`, `// ReSharper disable`). Use comments only when no attribute applies.
- Every suppression MUST carry a real reason saying *why*. "false positive" / "by design" alone is not enough.

## Logging

- Use the project `Logger` (`Source/Logger.cs`). Actionable, gated, no spam.

## Testing (xUnit)

- **Static state isolation** — tests mutating global/cached/static state MUST save/restore via the test constructor + `IDisposable` (or `IClassFixture`/`ICollectionFixture` for shared setup).
- Do not depend on test execution order.
- RimWorld APIs requiring live game context must be abstracted or guarded; don't call them directly in unit tests without isolation.
