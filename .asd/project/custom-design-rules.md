---
responsibility:
  owns: project-owner custom rules read during design and design-review phases
  excludes: universal rules, code/test rules
  delegates_to: custom-common-rules.md (all phases), custom-coding-rules.md (impl/impl-review)
---

# Custom Design Rules

## Modding & patchability

- Harmony-patchable: prefer small methods, stable public entry points, predictable side effects.
- Don't seal mod extension points without strong reason.
- No static constructors with heavy side effects.
- This is a shared library consumed by other mods — public surface is an integration contract; design it as one.

## Data-driven over hardcoded

- Stat / balance / tuning values come from RimWorld `Def`s or config, never hardcoded literals in code. ADRs/PRDs introducing new tunables MUST specify the Def/config surface, not literal constants.

## Determinism

- Stat calculation, filtering, and caching logic: same inputs → same outputs.
- No time- or order-dependent behavior in core logic unless explicitly required.
