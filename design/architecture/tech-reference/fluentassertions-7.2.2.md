---
responsibility:
  owns: project-vetted reference for the FluentAssertions assertion library (7.2.2, Apache-2.0) on net472 — apis used, version specifics, project conventions
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# FluentAssertions @ 7.2.2

The assertion library for `Source/LordKuper.Common.Tests` (net472). Replaces all `Xunit.Assert.*` calls with `.Should()`-style assertions. Pinned to the **7.x** line because it is the last free (Apache-2.0) major; **8.x and later require a paid commercial license**. Pairs with `NUnit` 4.6.1 (see [NUnit @ 4.6.1](nunit-4.6.1.md)).

## Canonical source
- Docs: https://fluentassertions.com/
- Releases / license: https://fluentassertions.com/releases/
- NuGet: https://www.nuget.org/packages/FluentAssertions/7.2.2
- Last verified: 2026-06-04

## Acquisition model
- NuGet `PackageReference` in `Source/LordKuper.Common.Tests/LordKuper.Common.Tests.csproj`, version `7.2.2` (latest 7.x; not currently referenced — purely additive).
- Pin to a 7.x version. Do **not** float to 8.x: version 8.0 introduced a commercial-licensing model ("Versions 8 and beyond ... commercial use requires a paid license"); version 7 "will remain fully open-source indefinitely" under Apache-2.0.
- Tests run on `net472`; FluentAssertions 7.x supports .NET Framework (it dropped only .NET Core 2.x/3.x).

## API surface used in project
The authoritative conversion contract is the PRD's assertion-mapping table (`prd.html` §Assertion mapping reference, AC-10 … AC-16). Surface used:
- `act.Should().Be(exp)` — from `Assert.Equal(exp, act)` (scalar equality; preserve expected/actual orientation).
- `act.Should().Equal(exp)` — from `Assert.Equal(exp, act)` on collections (element-sequence equality).
- `act.Should().BeApproximately(exp, tolerance)` — from `Assert.Equal(exp, act, precision)`. **Not 1:1**: xUnit `precision` is decimal places; FA `tolerance` is an absolute value. Convert `precision = N` to `tolerance = 10^-N` (or the test author's intended epsilon) per site. See [ADR-0005](../adr/adr-0005-fluentassertions-7x.html).
- `x.Should().BeTrue()` / `.BeFalse()` — from `Assert.True/False`. Prefer the value-comparing form (`x.Should().Be(y)`) when `x` is itself a comparison, to keep a useful failure diff (PRD AC-13).
- `x.Should().BeNull()` / `.NotBeNull()` — from `Assert.Null` / `Assert.NotNull`.
- `x.Should().Contain(...)` / `.NotContain(...)` — from `Assert.Contains` / `Assert.DoesNotContain`. Both collection-element and string-substring overloads exist; pick the one matching the site's argument type.
- `x.Should().ContainSingle()` (or `.HaveCount(1)`) — from `Assert.Single`.
- `x.Should().BeEmpty()` / `.NotBeEmpty()` — from `Assert.Empty` / `Assert.NotEmpty`.
- `x.Should().NotBeSameAs(y)` — from `Assert.NotSame` (reference inequality).
- `action.Should().Throw<T>()` — from `Assert.Throws<T>(() => ...)`. The asserted exception type and any message/inner-exception assertion at the site are preserved; the lambda-wrapping shape changes.

The 12 distinct xUnit assert methods in use are `Equal, True, False, Null, NotNull, Throws, Contains, DoesNotContain, Single, NotSame, Empty, NotEmpty`. No `Same`, `IsType`, `Collection`, `Raises`, or `Multiple` appear in the suite.

## Version-specific notes
- **Licensing is the reason for the 7.x pin.** 7.2.2 (latest 7.x, released 2026-03-16) is Apache-2.0. 8.x+ is commercial for non-open-source/commercial use. The project pins 7.x deliberately and must not float past it.
- FluentAssertions 7.x emits no license-nag / obsolete warnings that would trip `TreatWarningsAsErrors`; if a future patch introduces an analyzer warning, it must be resolved (not suppressed wholesale) to keep the build clean under `WarningLevel 9999`.

## Deprecations and breaking changes from prior version
- Migrating **from xUnit asserts** (FA was not previously referenced): the only semantic non-equivalence is the `Assert.Equal(exp, act, precision)` → `BeApproximately(exp, tolerance)` shift (decimal-places vs absolute tolerance) — every such site is an explicit, reviewable edit, not a mechanical swap (PRD AC-12, AC-16).
- 8.x is out of scope (commercial license) — do not adopt without a new ADR + Complication Approval.

## Project conventions
- Pin `FluentAssertions` `7.2.2`; never auto-upgrade to 8.x.
- Use the global `<Using Include="FluentAssertions" />` so `.Should()` is implicitly available (mirrors the NUnit/Xunit global-using convention). `ImplicitUsings=enable`.
- Assertions are FluentAssertions only — do not mix in NUnit `Assert.*` / `Assert.That`.
- When converting `Assert.True/False` over a comparison, prefer the value-comparing FA form to preserve the failure diff (PRD AC-13).
- Treat any assertion-strength change (tightening/pruning during the refactor+cleanup) as an explicit, individually reviewable edit, never a side effect of mechanical conversion (PRD AC-17).
- Must compile warning-clean under `Source/Directory.Build.props` governance ([ADR-0003](../adr/adr-0003-build-governance.html)).

## Known issues and workarounds
- **`BeApproximately` tolerance choice.** A wrong tolerance can silently loosen or tighten a float assertion (masking or inventing failures, e.g. MathHelper normalization tests). Convert `precision N` to `10^-N` and spot-check boundary/clamp cases (PRD AC-12).
- **Throw assertion target.** `action.Should().Throw<T>()` requires the code-under-test to be wrapped in an `Action`/`Func`; ensure the lambda actually invokes the throwing call (a non-invoked delegate asserts nothing).
