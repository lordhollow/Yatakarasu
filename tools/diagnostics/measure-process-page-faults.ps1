param(
    [string]$ProcessName = 'Yata',
    [int]$ProcessId = 0,
    [ValidateRange(2, 3600)][int]$DurationSeconds = 30,
    [ValidateRange(1000, 60000)][int]$IntervalMilliseconds = 1000,
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
if (!$OutputPath) {
    $repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    $OutputPath = Join-Path $repoRoot ('tests/bin/PageFaultInvestigation/live-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss') + '.csv')
}
if (!$ProcessId) {
    $targets = @(Get-Process -Name $ProcessName -ErrorAction SilentlyContinue)
    if ($targets.Count -ne 1) { throw "Expected one $ProcessName process; found $($targets.Count). Specify -ProcessId if necessary." }
    $ProcessId = $targets[0].Id
}
$samples = [System.Collections.Generic.List[object]]::new()
$clock = [System.Diagnostics.Stopwatch]::StartNew()
do {
    $sampleClock = [System.Diagnostics.Stopwatch]::StartNew()
    $current = Get-CimInstance Win32_PerfRawData_PerfProc_Process -Filter "IDProcess = $ProcessId"
    if (!$current) { break }
    $systemMemory = Get-CimInstance Win32_PerfFormattedData_PerfOS_Memory
    $samples.Add([pscustomobject]@{
        Utc = [DateTime]::UtcNow.ToString('O')
        Pid = $current.IDProcess
        PageFaults = $current.PageFaultsPersec
        PrivateBytes = $current.PrivateBytes
        WorkingSet = $current.WorkingSet
        ReadBytes = $current.IOReadBytesPersec
        WriteBytes = $current.IOWriteBytesPersec
        Cpu100ns = $current.PercentProcessorTime
        Clock100ns = $current.Timestamp_Sys100NS
        Handles = $current.HandleCount
        Threads = $current.ThreadCount
        AvailableMB = $systemMemory.AvailableMBytes
        SystemPageReadsPerSec = $systemMemory.PageReadsPersec
        SystemPagesInputPerSec = $systemMemory.PagesInputPersec
        SystemDemandZeroFaultsPerSec = $systemMemory.DemandZeroFaultsPersec
        SystemTransitionFaultsPerSec = $systemMemory.TransitionFaultsPersec
    })
    if ($clock.Elapsed.TotalSeconds -ge $DurationSeconds) { break }
    $wait = [Math]::Max(0, $IntervalMilliseconds - $sampleClock.ElapsedMilliseconds)
    if ($wait -gt 0) { Start-Sleep -Milliseconds $wait }
} while ($true)
if ($samples.Count -lt 2) { throw 'The target exited or insufficient samples were collected.' }
$first = $samples[0]; $last = $samples[$samples.Count - 1]
$seconds = ([decimal]$last.Clock100ns - [decimal]$first.Clock100ns) / 10000000
if ($seconds -le 0) { throw 'Performance counter timestamps did not advance.' }
# The process PageFaultCount is a cumulative DWORD. Account for one wrap.
$faults = [decimal]$last.PageFaults - [decimal]$first.PageFaults
if ($faults -lt 0) { $faults += 4294967296 }
$summary = [pscustomobject]@{
    ProcessId = $ProcessId; Samples = $samples.Count; Seconds = [double]$seconds
    PageFaultsPerSecond = [double]($faults / $seconds)
    PrivateBytesDelta = [decimal]$last.PrivateBytes - [decimal]$first.PrivateBytes
    WorkingSetDelta = [decimal]$last.WorkingSet - [decimal]$first.WorkingSet
    ReadBytesDelta = [decimal]$last.ReadBytes - [decimal]$first.ReadBytes
    WriteBytesDelta = [decimal]$last.WriteBytes - [decimal]$first.WriteBytes
    CpuPercentOneCore = [double](100 * ([decimal]$last.Cpu100ns - [decimal]$first.Cpu100ns) / ($seconds * 10000000))
    MinimumAvailableMB = ($samples.AvailableMB | Measure-Object -Minimum).Minimum
    SystemPageReadsPerSecondAverage = ($samples.SystemPageReadsPerSec | Measure-Object -Average).Average
    SystemPagesInputPerSecondAverage = ($samples.SystemPagesInputPerSec | Measure-Object -Average).Average
    Note = 'PageFaults includes soft and hard faults. System memory counters cover all processes; process I/O is not a hard-fault counter.'
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path ([System.IO.Path]::GetDirectoryName($OutputPath)) -Force | Out-Null
$samples | Export-Csv -LiteralPath $OutputPath -NoTypeInformation -Encoding utf8
$summary | ConvertTo-Json | Set-Content -LiteralPath ([System.IO.Path]::ChangeExtension($OutputPath, '.summary.json')) -Encoding utf8
$summary
Write-Output "CSV: $OutputPath"
