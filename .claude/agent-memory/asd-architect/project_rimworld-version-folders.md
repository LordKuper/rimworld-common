---
name: rimworld-version-folders
description: LordKuper.Common ships per-RimWorld-version assembly folders; build emits to <version>/Assemblies/
metadata:
  type: project
---

`LordKuper.Common` supports multiple RimWorld versions via version-named top-level folders (e.g. `1.5/`, `1.6/`). The production csproj emits to `..\1.6\Assemblies\` and pins `<Version>1.6.4.0</Version>`. New RimWorld versions are onboarded by adding a new version folder, not by breaking existing ones.

**Why:** RimWorld loads a managed assembly per its own version; the mod must ship a matching binary per game version. This is the project's backward-compat model.

**How to apply:** When proposing contract/target-framework changes, treat each `<version>/` folder as an independent deliverable. Do not assume a single output. `OutputPath`, `<Version>`, and the version folder must stay aligned. Target framework is locked to `net472` to match the RimWorld/Unity Mono runtime — do not propose newer TFMs.
