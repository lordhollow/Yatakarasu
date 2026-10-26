param([string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuildPath = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/amd64/MSBuild.exe' | Select-Object -First 1
$compilerPath = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/Roslyn/csc.exe' | Select-Object -First 1
if (!$msbuildPath -or !$compilerPath) { throw 'Visual Studio MSBuild and C# compiler are required.' }
$testOutput = Join-Path $PSScriptRoot "bin/Configuration/$Configuration"
New-Item -ItemType Directory -Path $testOutput -Force | Out-Null
& $msbuildPath (Join-Path $repoRoot 'Yata.sln') /t:Build "/p:Configuration=$Configuration" /p:Platform=x64 "/p:OutDir=$testOutput/" /nr:false /m:1 /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Build failed. Restore NuGet packages first.' }
$testExe = Join-Path $testOutput 'ConfigurationRegression.exe'
Copy-Item -LiteralPath (Join-Path $testOutput 'Yata.exe.config') -Destination "$testExe.config"
& $compilerPath /nologo /target:exe /platform:x64 /langversion:7.3 "/out:$testExe" `
    "/reference:$(Join-Path $testOutput 'Yata.exe')" `
    "/reference:$(Join-Path $testOutput 'LibreHardwareMonitorLib.dll')" `
    /reference:System.Drawing.dll /reference:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'ConfigurationRegression.cs')
if ($LASTEXITCODE -ne 0) { throw 'Configuration test compilation failed.' }
& $testExe
if ($LASTEXITCODE -ne 0) { throw 'Configuration tests failed.' }
