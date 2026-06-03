---
responsibility:
  owns: project-vetted reference for the RimWorld / Verse (Assembly-CSharp) API surface (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# RimWorld / Verse (Assembly-CSharp) @ 1.5 + 1.6

> Both RimWorld **1.5 and 1.6** are active supported targets. The filename uses `1.6` as the current head version; the contents apply to both supported versions unless a note says otherwise.

## Canonical source
- Modding wiki: https://rimworldwiki.com/wiki/Modding
- Official modding info: https://store.steampowered.com/news/ (Ludeon dev/modder update posts per version)
- API is not published as formal docs; surface is read from the local `Assembly-CSharp.dll` and community references (e.g. decompiled sources).
- Last verified: 2026-06-03

## Acquisition model
- Game-provided **file reference**, not a NuGet package.
- Resolved at build time from `$(RimWorldManagedDir)\Assembly-CSharp.dll` with `<Private>False</Private>` (referenced, never copied to output).
- The assembly version is whatever the local RimWorld install ships; the build does not pin it. Builds require a local RimWorld install.

## API surface used in project
- `Verse.Pawn`, `Verse.Thing`, `Verse.ThingDef`: core entity / definition types wrapped by pawn helpers, caches, and filters.
- `RimWorld.StatDef`, `Verse.StatRequest`, stat workers: backing for custom-stat models (melee / ranged / tool) and stat helpers.
- `Verse.Def` / `Verse.DefDatabase<T>`: definition lookup feeding the definition cache (`DefCache`).
- `Verse.Log`: diagnostics.
- RimWorld/Verse enums and domain types (work types, filters) consumed by pawn and limit filters.

> Exact member usage lives in `Source/`; this reference records the dependency contract and the families of types relied upon.

## Version-specific notes
- **1.5 and 1.6 are both supported.** The codebase is structured so version-specific assemblies build into per-version folders (`1.5/Assemblies/`, `1.6/Assemblies/`); the current csproj `OutputPath` points at `..\1.6\Assemblies\`.
- The `Assembly-CSharp` surface differs between 1.5 and 1.6 (Ludeon revises types each major version). Code must compile and behave against both — avoid relying on members that exist in only one version without guarding, and verify cross-version members on any change.
- Unity types referenced by RimWorld track the engine version bundled with each RimWorld release (see `unity-imgui.md`).

## Deprecations and breaking changes from prior version
- RimWorld major versions (1.5 → 1.6) routinely rename / move / remove API members. Treat any 1.6-only or 1.5-only member as a potential break; keep shared code on members common to both supported versions.
- New game versions are onboarded by adding a version folder rather than dropping older supported versions.

## Project conventions
- Always consume RimWorld/Verse types through the library's helpers/caches rather than scattering raw game calls, to centralize version-sensitive surface.
- Keep the `Assembly-CSharp` reference as a non-private file reference resolved from `$(RimWorldManagedDir)` — never vendor or NuGet it.
- Maintain compatibility with both 1.5 and 1.6; the build emits into the matching `<version>/Assemblies/` directory.

## Known issues and workarounds
- Missing/incorrect `$(RimWorldManagedDir)` breaks the build with unresolved `Assembly-CSharp` — requires a valid local RimWorld install path.
- Cross-version member drift between 1.5 and 1.6 is the primary source of breakage; mitigated by funneling game access through shared helpers and verifying against both installs.
