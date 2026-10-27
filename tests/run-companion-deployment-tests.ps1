param([string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuildPath = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/amd64/MSBuild.exe' | Select-Object -First 1
if (!$msbuildPath) { throw 'Visual Studio MSBuild is required.' }
$testRoot = Join-Path $PSScriptRoot "bin/CompanionDeployment/$Configuration"
New-Item -ItemType Directory -Path (Join-Path $testRoot 'Yata') -Force | Out-Null
$testRoot = [IO.Path]::GetFullPath($testRoot)
$overridePath = Join-Path $testRoot 'output-paths.targets'
$escapedRoot = [Security.SecurityElement]::Escape($testRoot)
# Keep the two projects in separate output directories, as in a normal VS build.
$overrideXml = '<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><PropertyGroup><OutDir>' + $escapedRoot + '\$(MSBuildProjectName)\</OutDir></PropertyGroup></Project>'
[IO.File]::WriteAllText($overridePath, $overrideXml)
$deployed = Join-Path $testRoot 'Yata/Karasu.exe'
[IO.File]::WriteAllText($deployed, 'stale companion fixture')
& $msbuildPath (Join-Path $repoRoot 'Yata/Yata.csproj') /t:Build "/p:Configuration=$Configuration" /p:Platform=x64 "/p:CustomBeforeMicrosoftCommonTargets=$overridePath" /nr:false /m:1 /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Yata project build failed.' }
$companion = Join-Path $testRoot 'Karasu/Karasu.exe'
if (!(Test-Path -LiteralPath $companion)) { throw 'Building Yata did not build Karasu.' }
if ((Get-FileHash -LiteralPath $companion).Hash -ne (Get-FileHash -LiteralPath $deployed).Hash) { throw 'Yata retained a stale Karasu.exe.' }
if (!(Test-Path -LiteralPath (Join-Path $testRoot 'Yata/Yata.exe'))) { throw 'Yata output is missing.' }
Write-Output 'PASS: Building Yata alone builds and deploys the matching Karasu over a stale copy.'
