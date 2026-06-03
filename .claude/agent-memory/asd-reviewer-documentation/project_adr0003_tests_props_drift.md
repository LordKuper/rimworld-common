---
name: project-adr0003-tests-props-drift
description: RESOLVED in sprint 001 iter-02 — ADR-0003 build-governance now matches the as-built Directory.Build.props layout (root SSoT + thin import wrappers)
metadata:
  type: project
---

**Status: RESOLVED** as of sprint 001 impl-review iter-02 (verified 2026-06-04).

History: iter-01 HIGH finding — ADR-0003 (`design/architecture/adr/adr-0003-build-governance.html`) claimed the Tests project had **no** child `Directory.Build.props` and inherited root governance via MSBuild auto-import. In reality `Source/` and `Tests/` each had a child `Directory.Build.props` carrying an explicit `GetPathOfFileAbove` import **plus** a duplicated RimWorld path-resolution block — both a doc-vs-code drift and an SSoT duplication.

Fix-round (commits e6e6db5 props, 1dc4f5b ADR-0003, ADR-0002): the path-resolution block (`RimWorldDir`/`RimWorldManagedDir` + `CheckRimWorldDir` fail-fast `<Error>`) now lives ONLY in repo-root `Directory.Build.props`. `Source/Directory.Build.props` and `Tests/Directory.Build.props` are thin wrappers containing only the explicit import. ADR-0003 was rewritten to describe exactly this (root SSoT + thin import wrappers, "removed from them" language); the stale "no child props" claim is gone. ADR-0002 also corrected `NormalizeStatValue` "public"→"internal" to match `Source/StatRanges.cs:43` (`internal static`).

**Why kept:** documents a recurring drift class for this project — ADR descriptions of MSBuild import/inheritance mechanics drifting from the actual `Directory.Build.props` files. Useful baseline if these props files change again.

**How to apply:** No longer an open finding. If `Directory.Build.props` files are edited in a future sprint, re-verify ADR-0003 wording still matches (root holds path block; children are import-only). Reviewer never edits persistent `design/` — flag drift to domain creators for design-promote.

Related: [[../MEMORY]].
