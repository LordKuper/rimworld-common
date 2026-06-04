---
responsibility:
  owns: project-vetted reference for the UnityEngine CoreModule / IMGUIModule / TextRenderingModule surface used by Source/UI/** (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# UnityEngine (CoreModule + IMGUIModule + TextRenderingModule) @ install-resolved

> The Unity assembly version is **resolved from the local RimWorld install at build time** and is not pinned in source. It tracks whichever Unity build ships with the targeted RimWorld version (1.5 / 1.6). No local managed directory was available when this reference was written, so the filename is version-agnostic (`unity-imgui.md`); record the concrete version here once read from `$(RimWorldManagedDir)`.

## Canonical source
- Unity Scripting API: https://docs.unity3d.com/ScriptReference/
- IMGUI manual: https://docs.unity3d.com/Manual/GUIScriptingGuide.html
- Last verified: 2026-06-03

## Acquisition model
- Game-provided **file references**, not NuGet packages. Resolved from `$(RimWorldManagedDir)`:
  - `UnityEngine.CoreModule.dll`
  - `UnityEngine.IMGUIModule.dll`
  - `UnityEngine.TextRenderingModule.dll`
- All referenced with `<Private>False</Private>` (referenced, never copied to output). Versions are whatever the local RimWorld/Unity build supplies.

## API surface used in project
Used by the widget toolkit in `Source/UI/**` (buttons, fields, sliders, selectors, tabs, scroll views, labels, icons, pawn boxes):

- **CoreModule**: `Rect`, `Vector2`, `Color`, `Texture2D`, `Mathf`, `GUIUtility` — geometry, color, textures, math used across layout and widgets.
- **IMGUIModule**: `GUI`, `GUILayout`, `GUIStyle`, `GUIContent`, `Event` — immediate-mode controls and styling underpinning every widget render path.
- **TextRenderingModule**: `TextAnchor`, `Font`, font/alignment types — label rendering and field text layout.

> RimWorld's own `Verse.Widgets` builds on this same IMGUI surface; the library's UI layer composes both.

## Version-specific notes
- Unity version is bound to the RimWorld release; 1.5 and 1.6 may ship different Unity builds. The IMGUI API surface used here is long-stable across Unity LTS lines, so cross-version risk is low for these specific types.
- Because the references are file-resolved, there is no version pin to upgrade — the surface moves only when RimWorld upgrades its bundled Unity.

## Deprecations and breaking changes from prior version
- The classic IMGUI API (`GUI` / `GUILayout` / `GUIStyle`) has been stable across recent Unity versions; no project-affecting breaks observed between the 1.5 and 1.6 Unity builds for the types used.

## Project conventions
- Reference these three modules as non-private file references from `$(RimWorldManagedDir)` — never NuGet, never copy to output.
- Keep UI built on immediate-mode IMGUI to match RimWorld's own rendering model; do not introduce UI Toolkit / uGUI dependencies the game does not load.
- Centralize Unity-facing drawing in `Source/UI/**` rather than scattering `GUI`/`GUILayout` calls through domain code.

## Known issues and workarounds
- Missing `$(RimWorldManagedDir)` breaks the build with unresolved `UnityEngine.*Module` references — requires a valid local RimWorld install.
- Record the concrete Unity assembly version in this doc once a local managed directory is available, and rename the file to `unity-imgui-<version>.md` to follow the `<tech>-<version>` convention.
