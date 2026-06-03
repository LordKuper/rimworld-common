---
responsibility:
  owns: project-vetted reference for Lib.Harmony 2.4.2 (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# Lib.Harmony @ 2.4.2

## Canonical source
- Official docs: https://harmony.pardeike.net/
- NuGet: https://www.nuget.org/packages/Lib.Harmony/2.4.2
- Source: https://github.com/pardeike/Harmony
- Last verified: 2026-06-03

## Acquisition model
- Referenced as a NuGet `PackageReference` (version `2.4.2`) in `Source/LordKuper.Common.csproj`.
- **Compile-only**: `<PrivateAssets>all</PrivateAssets>` and `<ExcludeAssets>runtime</ExcludeAssets>`.
  The Harmony assembly is used to compile against but is **excluded from build output**.
- At runtime, Harmony is **provided by the host environment** (RimWorld + its Harmony mod / loader). The library must never ship its own copy, to avoid loading a conflicting Harmony instance into the game's AppDomain.

## API surface used in project
- `HarmonyLib.Harmony`: patch lifecycle / instance creation for applying runtime patches to RimWorld methods.
- `[HarmonyPatch]` and patch annotations (`Prefix` / `Postfix` / `Transpiler` where present): declarative method patching against `Assembly-CSharp` (Verse / RimWorld) targets.
- `AccessTools`: reflection helpers for resolving game members that are not part of the public surface.

> Exact patch sites live in source; this reference records the dependency contract, not the patch inventory.

## Version-specific notes
- 2.4.x targets .NET Framework / Mono runtimes compatible with `net472`, matching RimWorld's runtime.
- 2.4.2 is current/near-current on the 2.4 line — upgrade lag is minimal (LOW risk).
- Because runtime Harmony is host-provided, the **runtime** Harmony version is whatever the player's RimWorld + loaded Harmony mod supplies, which may differ from the compile-time 2.4.2. Patches must stay within the stable Harmony 2.x API to remain compatible across host versions.

## Deprecations and breaking changes from prior version
- No project-affecting breaking changes adopted within the 2.4 line. The public patching API (`Harmony`, `[HarmonyPatch]`, `AccessTools`) is stable across 2.x.

## Project conventions
- Never bundle Harmony in output — the compile-only configuration (`PrivateAssets=all`, `ExcludeAssets=runtime`) is mandatory and must be preserved on any version bump.
- Treat the runtime Harmony version as host-controlled; do not rely on features newer than the broadly-deployed 2.x baseline.
- Patches target RimWorld/Verse types resolved via the game-provided `Assembly-CSharp` reference — keep patch targets aligned with both supported game versions (1.5 and 1.6).

## Known issues and workarounds
- Shipping a private Harmony copy causes duplicate-assembly / version-conflict failures in the game's AppDomain — prevented here by the runtime-exclusion configuration.
