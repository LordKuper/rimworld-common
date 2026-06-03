# Coverage measurement for LordKuper.Common (sprint 001 / IMP-11, AC-21).
#
# Why this script and not plain `coverlet --collect`:
#   The Source assembly references RimWorld's Assembly-CSharp / UnityEngine.* with
#   <Private>False</Private>, so a coverage tool that loads the assembly at the
#   data-collector stage cannot resolve those dependencies (coverlet.collector
#   silently yields 0%). AltCover instruments statically via Cecil, which only needs
#   the RimWorld managed assemblies *present during instrumentation*. We therefore:
#     1. copy the referenced RimWorld assemblies into the test bin (instrument-time only),
#     2. instrument LordKuper.Common in place (UI + game-bound types excluded from the denominator),
#     3. REMOVE the copied RimWorld assemblies so the test process resolves them lazily via the
#        runtime RimWorldTestFramework AssemblyResolve handler (avoids eager [StaticConstructorOnStartup] failures),
#     4. run the tests against the instrumented assembly,
#     5. collect the Cobertura report and print the coverage summary.
#
# Denominator exclusions (game-bound, not unit-testable without a full RimWorld harness):
#   UI.*  Resources  CustomStats.*WeaponStats/ToolStats  CommonMod  Compatibility.Vse  Logger  PawnHelper  PassionHelper
#
# Requires: RIMWORLD_DIR env var; altcover global tool (dotnet tool install --global altcover.global).

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$bin  = Join-Path $root 'Tests\bin\Release\net472'
$managed = Join-Path $env:RIMWORLD_DIR 'RimWorldWin64_Data\Managed'
$report  = Join-Path $root 'coverage.altcover.xml'
$rimworldDlls = @('Assembly-CSharp','UnityEngine','UnityEngine.CoreModule','UnityEngine.IMGUIModule','UnityEngine.TextRenderingModule')

if (-not (Test-Path $managed)) { throw "RIMWORLD_DIR managed dir not found: $managed" }

Write-Host '== build tests =='
dotnet build (Join-Path $root 'Tests\LordKuper.Common.Tests.csproj') -c Release -v quiet -nologo | Out-Null

# 1. copy RimWorld assemblies for instrument-time resolution
foreach ($d in $rimworldDlls) { Copy-Item (Join-Path $managed "$d.dll") $bin -Force -ErrorAction SilentlyContinue }

# 2. instrument LordKuper.Common only, excluding UI + game-bound types from the denominator
Write-Host '== instrument =='
altcover --inplace --save -i $bin `
  --assemblyFilter Tests --assemblyFilter xunit --assemblyFilter coverlet --assemblyFilter Microsoft `
  --assemblyFilter System --assemblyFilter mscorlib --assemblyFilter UnityEngine --assemblyFilter Assembly-CSharp --assemblyFilter netstandard `
  --typeFilter 'LordKuper\.Common\.UI' --typeFilter 'LordKuper\.Common\.Resources' --typeFilter 'WeaponStats' `
  --typeFilter 'ToolStats' --typeFilter 'CommonMod' --typeFilter 'Compatibility' --typeFilter 'Logger' `
  --typeFilter 'PawnHelper' --typeFilter 'PassionHelper' `
  --reportFormat Cobertura -r $report | Out-Null

# 3. remove copied RimWorld assemblies so the runtime resolver handles them lazily
foreach ($d in $rimworldDlls) { Remove-Item (Join-Path $bin "$d.dll") -Force -ErrorAction SilentlyContinue }

# 4. run tests against the instrumented assembly
Write-Host '== test =='
dotnet test (Join-Path $root 'Tests\LordKuper.Common.Tests.csproj') -c Release --no-build --nologo

# 5. collect + report
Write-Host '== coverage =='
altcover runner --collect -r $bin

# 6. restore the un-instrumented assembly
$saved = Join-Path $bin '__Saved\LordKuper.Common.dll'
if (Test-Path $saved) { Copy-Item $saved (Join-Path $bin 'LordKuper.Common.dll') -Force; Remove-Item (Join-Path $bin '__Saved') -Recurse -Force }
