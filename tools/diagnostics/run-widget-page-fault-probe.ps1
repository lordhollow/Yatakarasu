param([string]$AssetDirectory, [string]$LauncherFile, [switch]$OsQueries, [switch]$IdleQueries, [switch]$NvidiaQueries, [ValidateRange(0, 2147483647)][int]$ProcessId = 0, [ValidatePattern('^[a-zA-Z0-9_-]+$')][string]$Tag = 'current')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (!$AssetDirectory) { $AssetDirectory = Join-Path $repoRoot 'Yata/bin/x64/Release/files' }
if (!$LauncherFile) {
    $LauncherFile = Join-Path $AssetDirectory 'karasu.exec.txt'
    $preferences = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'Yatakarasu/configuration.xml'
    if (Test-Path -LiteralPath $preferences) {
        $saved = [System.Xml.Linq.XElement]::Load($preferences).Element('LauncherFile').Value
        if ($saved -and (Test-Path -LiteralPath $saved)) { $LauncherFile = $saved }
    }
}
$output = Join-Path $repoRoot 'tests/bin/PageFaultInvestigation/Widgets'
New-Item -ItemType Directory -Path (Join-Path $output 'files') -Force | Out-Null
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/amd64/MSBuild.exe' | Select-Object -First 1
$compiler = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/Roslyn/csc.exe' | Select-Object -First 1
if (!$msbuild -or !$compiler) { throw 'Visual Studio build tools are required.' }
& $msbuild (Join-Path $repoRoot 'Yata.sln') /t:Build /p:Configuration=Release /p:Platform=x64 "/p:OutDir=$output/" /nr:false /m:1 /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
foreach ($file in @('clockback.png', 'SystemApp.png')) { Copy-Item -LiteralPath (Join-Path $AssetDirectory $file) -Destination (Join-Path $output 'files') }
$exe = Join-Path $output 'WidgetPageFaultProbe.exe'
& $compiler /nologo /target:exe /platform:x64 /optimize+ /main:WidgetProbeRunner "/out:$exe" `
    /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "/reference:$(Join-Path $output 'Yata.exe')" "/reference:$(Join-Path $output 'LibreHardwareMonitorLib.dll')" `
    (Join-Path $PSScriptRoot 'GraphicsPageFaultProbe.cs') (Join-Path $PSScriptRoot 'GraphicsPageFaultProbe.Windows.cs') (Join-Path $PSScriptRoot 'GraphicsPageFaultProbe.Widgets.cs') (Join-Path $PSScriptRoot 'GraphicsPageFaultProbe.Nvidia.cs')
if ($LASTEXITCODE -ne 0) { throw 'Probe compilation failed.' }
if ($ProcessId) { & $exe (Join-Path $output "process-samples-$Tag.csv") --process-samples $ProcessId }
elseif ($OsQueries) { & $exe (Join-Path $output "os-query-page-faults-$Tag.csv") --os-queries }
elseif ($IdleQueries) { & $exe (Join-Path $output "idle-query-page-faults-$Tag.csv") --idle-queries }
elseif ($NvidiaQueries) { & $exe (Join-Path $output "nvidia-query-page-faults-$Tag.csv") --nvidia-queries }
else { & $exe (Join-Path $output "widget-page-faults-$Tag.csv") $LauncherFile }
if ($LASTEXITCODE -ne 0) { throw 'Probe failed.' }
