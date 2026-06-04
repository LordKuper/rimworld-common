# Coverage measurement for LordKuper.Common.
#
# Why this script and not plain `coverlet --collect`:
#   The Source assembly references RimWorld's Assembly-CSharp / UnityEngine.* with
#   <Private>False</Private>, so a coverage tool that loads the assembly at the
#   data-collector stage cannot resolve those dependencies (coverlet.collector
#   silently yields 0%). AltCover instruments statically via Cecil, which only needs
#   the RimWorld managed assemblies *present during instrumentation*. We therefore:
#     1. copy the referenced RimWorld assemblies into the test bin (build does this via AfterBuild
#        target; this script also copies them explicitly to ensure they are present before AltCover
#        instruments),
#     2. instrument LordKuper.Common in place (UI + game-bound types excluded from the denominator),
#     3. run the tests against the instrumented assembly (NUnit 4.x scans fixture types via
#        GetTypes()/GetCustomAttributes at discovery time, so the RimWorld DLLs must remain in bin),
#     4. collect the Cobertura report and print the coverage summary.
#
# Denominator exclusions (game-bound, not unit-testable without a full RimWorld harness):
#   UI.*  Resources  CustomStats.*WeaponStats/ToolStats  CommonMod  Compatibility.Vse  Logger  PawnHelper  PassionHelper
#
# Requires: RIMWORLD_DIR env var; altcover global tool (dotnet tool install --global altcover.global).

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$bin  = Join-Path $root 'Source\LordKuper.Common.Tests\bin\Release\net472'
$managed = Join-Path $env:RIMWORLD_DIR 'RimWorldWin64_Data\Managed'
$report  = Join-Path $root 'TestResults\coverage.altcover.xml'
$rimworldDlls = @('Assembly-CSharp','Assembly-CSharp-firstpass','UnityEngine','UnityEngine.CoreModule','UnityEngine.IMGUIModule','UnityEngine.TextRenderingModule','netstandard')

if (-not (Test-Path $managed)) { throw "RIMWORLD_DIR managed dir not found: $managed" }
New-Item -ItemType Directory -Force (Split-Path $report) | Out-Null

Write-Host '== build tests =='
dotnet build (Join-Path $root 'Source\LordKuper.Common.Tests\LordKuper.Common.Tests.csproj') -c Release -v quiet -nologo | Out-Null

# 1. copy RimWorld assemblies into test bin for instrument-time and discovery-time resolution.
#    NUnit 4.x calls Assembly.GetTypes() and GetCustomAttributes(true) during fixture discovery;
#    both require the RimWorld dependency chain (including netstandard 2.1) to be resolvable
#    directly from the bin directory. The AfterBuild target in the csproj handles this for normal
#    builds; this script also copies explicitly to ensure they are present after a fresh build.
foreach ($d in $rimworldDlls) { Copy-Item (Join-Path $managed "$d.dll") $bin -Force -ErrorAction SilentlyContinue }

# 2. instrument LordKuper.Common only, excluding UI + game-bound types from the denominator
Write-Host '== instrument =='
altcover --inplace --save -i $bin `
  --assemblyFilter Tests --assemblyFilter nunit --assemblyFilter Microsoft `
  --assemblyFilter System --assemblyFilter mscorlib --assemblyFilter UnityEngine --assemblyFilter Assembly-CSharp --assemblyFilter netstandard `
  --typeFilter 'LordKuper\.Common\.UI' --typeFilter 'LordKuper\.Common\.Resources' --typeFilter 'WeaponStats' `
  --typeFilter 'ToolStats' --typeFilter 'CommonMod' --typeFilter 'Compatibility' --typeFilter 'Logger' `
  --typeFilter 'PawnHelper' --typeFilter 'PassionHelper' `
  --reportFormat Cobertura -r $report | Out-Null

# 3. run tests against the instrumented assembly
Write-Host '== test =='
dotnet test (Join-Path $root 'Source\LordKuper.Common.Tests\LordKuper.Common.Tests.csproj') -c Release --no-build --nologo `
  --settings (Join-Path $root 'Source\LordKuper.Common.Tests\.runsettings')

# 4. collect + report
Write-Host '== coverage =='
altcover runner --collect -r $bin

# 5. restore the un-instrumented assembly
$saved = Join-Path $bin '__Saved\LordKuper.Common.dll'
if (Test-Path $saved) { Copy-Item $saved (Join-Path $bin 'LordKuper.Common.dll') -Force; Remove-Item (Join-Path $bin '__Saved') -Recurse -Force }
