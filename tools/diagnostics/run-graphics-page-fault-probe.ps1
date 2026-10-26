param([string]$OutputDirectory, [switch]$WindowBuffers)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (!$OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'tests/bin/PageFaultInvestigation' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/Roslyn/csc.exe' | Select-Object -First 1
if (!$compiler) { throw 'The Visual Studio C# compiler is required.' }
$exe = Join-Path $OutputDirectory 'GraphicsPageFaultProbe.exe'
& $compiler /nologo /target:exe /platform:x64 /optimize+ /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "/out:$exe" (Join-Path $PSScriptRoot 'GraphicsPageFaultProbe.cs') (Join-Path $PSScriptRoot 'GraphicsPageFaultProbe.Windows.cs')
if ($LASTEXITCODE -ne 0) { throw 'Probe compilation failed.' }
if ($WindowBuffers) { & $exe (Join-Path $OutputDirectory 'window-buffer-page-faults.csv') --window-buffers }
else { & $exe (Join-Path $OutputDirectory 'graphics-page-faults.csv') }
if ($LASTEXITCODE -ne 0) { throw 'Graphics probe failed.' }
