param(
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuildPath = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/amd64/MSBuild.exe' | Select-Object -First 1
$compilerPath = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/Roslyn/csc.exe' | Select-Object -First 1
if (!$msbuildPath -or !$compilerPath) { throw 'Visual Studio MSBuild and C# compiler are required.' }

$testOutput = Join-Path $PSScriptRoot "bin/$Configuration"
$appOutput = Join-Path $testOutput 'app'
New-Item -ItemType Directory -Path $appOutput -Force | Out-Null
# Keep the test build separate so an open Yata instance is not overwritten.
& $msbuildPath (Join-Path $repoRoot 'Yata.sln') /t:Build "/p:Configuration=$Configuration" /p:Platform=x64 "/p:OutDir=$appOutput/" /nr:false /m:1 /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Build failed. Restore NuGet packages first (see Yata/Readme.md).' }

Get-ChildItem -LiteralPath $appOutput -Filter '*.dll' | Copy-Item -Destination $testOutput
Copy-Item -LiteralPath (Join-Path $appOutput 'Yata.exe') -Destination $testOutput
$testExe = Join-Path $testOutput 'HardwareMonitorRegression.exe'
Copy-Item -LiteralPath (Join-Path $appOutput 'Yata.exe.config') -Destination "$testExe.config"
& $compilerPath /nologo /target:exe /platform:x64 /langversion:7.3 "/out:$testExe" `
    "/reference:$(Join-Path $testOutput 'Yata.exe')" `
    "/reference:$(Join-Path $testOutput 'LibreHardwareMonitorLib.dll')" `
    "/reference:$(Join-Path $testOutput 'DiskInfoToolkit.dll')" `
    /reference:System.Drawing.dll (Join-Path $PSScriptRoot 'HardwareMonitorRegression.cs')
if ($LASTEXITCODE -ne 0) { throw 'Regression test compilation failed.' }
& $testExe
if ($LASTEXITCODE -ne 0) { throw 'Regression tests failed.' }
