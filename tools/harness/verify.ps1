#requires -Version 5.1
<#
.SYNOPSIS
    Harness verification gate for escape-from-supermarket.
.DESCRIPTION
    Two-layer executable verification (see docs/harness-gap-plan.md, Tier 1 A):
      1. dotnet build  — compile gate; any build error fails fast.
      2. headless Godot — load Scenes/Main.tscn for N frames, scan output for
         script/parse/runtime error markers.
    Exit code 0 = all pass, non-zero = a layer failed. Logs land in tools/harness/logs/.
.PARAMETER Frames
    Frames to run the headless scene before quitting. Default 120 (~2s at 60fps).
.PARAMETER GodotBin
    Godot console executable. Resolution order: this param -> $env:GODOT_BIN ->
    D:\Program Files\Godot\Godot_v4.6.3-stable_mono_win64_console.exe -> 'godot' on PATH.
#>
[CmdletBinding()]
param(
    [int]$Frames = 120,
    [string]$GodotBin = $env:GODOT_BIN
)

$ErrorActionPreference = 'Stop'

$RepoRoot     = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$GodotProject = Join-Path $RepoRoot 'escape-from-supermarket'
$Solution     = Join-Path $GodotProject 'EscapeFromSupermarket.sln'
$MainScene    = 'res://Scenes/Main.tscn'

$LogDir   = Join-Path $PSScriptRoot 'logs'
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir | Out-Null }
$BuildLog = Join-Path $LogDir 'build.log'
$RunOut   = Join-Path $LogDir 'godot-stdout.log'
$RunErr   = Join-Path $LogDir 'godot-stderr.log'

function Resolve-GodotBin {
    param([string]$Explicit)
    if ($Explicit) {
        if (Test-Path $Explicit) { return $Explicit }
        throw "GodotBin '$Explicit' does not exist."
    }
    $default = 'D:\Program Files\Godot\Godot_v4.6.3-stable_mono_win64_console.exe'
    if (Test-Path $default) { return $default }
    $cmd = Get-Command 'godot' -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    throw "Godot binary not found. Set `$env:GODOT_BIN, pass -GodotBin, or put 'godot' on PATH. Tried '$default'."
}

function Invoke-LoggedProcess {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$StdOutPath,
        [string]$StdErrPath
    )

    $psi = [System.Diagnostics.ProcessStartInfo]::new()
    $psi.FileName = $FilePath
    $escapedArgs = foreach ($arg in $Arguments) {
        if ($arg -match '[\s"]') {
            '"' + ($arg -replace '"', '\"') + '"'
        } else {
            $arg
        }
    }
    $psi.Arguments = $escapedArgs -join ' '
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $psi
    [void]$process.Start()
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($StdOutPath, $stdout, $utf8NoBom)
    [System.IO.File]::WriteAllText($StdErrPath, $stderr, $utf8NoBom)
    return $process.ExitCode
}

Write-Host '== verify.ps1: harness gate ==' -ForegroundColor Cyan

# --- Step 1/2: dotnet build ---
Write-Host '[1/2] dotnet build...' -ForegroundColor Cyan
$buildExitCode = Invoke-LoggedProcess -FilePath 'dotnet' -Arguments @('build', $Solution, '-nologo') `
    -StdOutPath $BuildLog -StdErrPath "$BuildLog.err"
if ($buildExitCode -ne 0) {
    Write-Host "FAIL: dotnet build exit $buildExitCode" -ForegroundColor Red
    Get-Content $BuildLog -Tail 40 | ForEach-Object { Write-Host "  $_" }
    exit 1
}
Write-Host 'PASS: build' -ForegroundColor Green

# --- Step 2/2: headless scene-load smoke ---
$godot = Resolve-GodotBin -Explicit $GodotBin
Write-Host "[2/2] headless scene load ($MainScene, $Frames frames)" -ForegroundColor Cyan
Write-Host "  godot: $godot"
$godotArgs = @('--headless', '--path', $GodotProject, '--quit-after', $Frames, $MainScene)
$runExitCode = Invoke-LoggedProcess -FilePath $godot -Arguments $godotArgs `
    -StdOutPath $RunOut -StdErrPath $RunErr

$errorMarkers = @(
    'SCRIPT ERROR',
    'USER ERROR',
    'Parser Error',
    'Parse Error',
    'Unhandled exception',
    'Unhandled Exception',
    'modulo by zero',
    'Failed to load script',
    'Failed loading resource',
    'Failed to instantiate',
    'Invalid call',
    'Invalid get index',
    'Invalid set index',
    'Cannot instantiate'
)
$logFiles = @($RunOut, $RunErr) | Where-Object { Test-Path $_ }
$hits = @()
if ($logFiles) {
    $hits = Select-String -Path $logFiles -SimpleMatch -Pattern $errorMarkers -ErrorAction SilentlyContinue
}

if ($runExitCode -ne 0) {
    Write-Host "FAIL: godot headless exit $runExitCode. See $RunErr" -ForegroundColor Red
    exit 1
}
if ($hits) {
    Write-Host 'FAIL: error markers in Godot output:' -ForegroundColor Red
    $hits | ForEach-Object { Write-Host "  $($_.Filename):$($_.LineNumber): $($_.Line.Trim())" -ForegroundColor Red }
    exit 1
}
Write-Host 'PASS: scene load smoke' -ForegroundColor Green

Write-Host '== ALL PASS ==' -ForegroundColor Green
exit 0
