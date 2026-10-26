param([string]$Configuration = 'Debug', [string]$ProjectPath, [string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuildPath = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/amd64/MSBuild.exe' | Select-Object -First 1
$compilerPath = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/Roslyn/csc.exe' | Select-Object -First 1
if (!$msbuildPath -or !$compilerPath) { throw 'Visual Studio MSBuild and C# compiler are required.' }
if (!$ProjectPath) { $ProjectPath = Join-Path $repoRoot 'Yata.sln' }
if (!$OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot "bin/Configuration/$Configuration" }
$testOutput = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $testOutput -Force | Out-Null
& $msbuildPath $ProjectPath /t:Build "/p:Configuration=$Configuration" /p:Platform=x64 "/p:OutDir=$testOutput/" /nr:false /m:1 /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Build failed. Restore NuGet packages first.' }
if ([IO.Path]::GetExtension($ProjectPath) -eq '.csproj') {
    # A designer reproduction project does not include the companion executable.
    & $msbuildPath (Join-Path $repoRoot 'Karasu/Karasu.csproj') /t:Build "/p:Configuration=$Configuration" /p:Platform=x64 "/p:OutDir=$testOutput/" /nr:false /m:1 /nologo /v:minimal
    if ($LASTEXITCODE -ne 0) { throw 'Karasu build failed.' }
}
$testExe = Join-Path $testOutput 'ConfigurationRegression.exe'
Copy-Item -LiteralPath (Join-Path $testOutput 'Yata.exe.config') -Destination "$testExe.config"
& $compilerPath /nologo /target:exe /platform:x64 /langversion:7.3 "/out:$testExe" `
    "/reference:$(Join-Path $testOutput 'Yata.exe')" `
    "/reference:$(Join-Path $testOutput 'LibreHardwareMonitorLib.dll')" `
    /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Design.dll (Join-Path $PSScriptRoot 'ConfigurationRegression.cs')
if ($LASTEXITCODE -ne 0) { throw 'Configuration test compilation failed.' }
& $testExe
if ($LASTEXITCODE -ne 0) { throw 'Configuration tests failed.' }
