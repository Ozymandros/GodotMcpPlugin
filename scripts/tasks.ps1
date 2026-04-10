param(
    [ValidateSet("all", "restore", "lint", "typecheck", "format", "build", "test", "verify")]
    [string]$Task = "all",

    [string]$Solution = "",

    [switch]$NoRestore,

    [switch]$FixFormat
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$Configuration = "Release"

function Resolve-SolutionPath {
    param([string]$Preferred)

    if ($Preferred -and (Test-Path -LiteralPath $Preferred)) {
        return $Preferred
    }

    if (Test-Path -LiteralPath "Godot-MCP-SK-Plugin.slnx") {
        return "Godot-MCP-SK-Plugin.slnx"
    }

    if (Test-Path -LiteralPath "GodotMcp.sln") {
        return "GodotMcp.sln"
    }

    throw "No solution found. Pass -Solution <path>."
}

function Invoke-Step {
    param([string]$Name, [scriptblock]$Action)
    Write-Host "==> $Name"
    & $Action
}

function Has-LocalToolManifest {
    if (Test-Path -LiteralPath ".config/dotnet-tools.json") {
        return $true
    }

    if (Test-Path -LiteralPath "dotnet-tools.json") {
        return $true
    }

    return $false
}

$solutionPath = Resolve-SolutionPath -Preferred $Solution
Write-Host "Using solution: $solutionPath"

function Do-Restore {
    if (Has-LocalToolManifest) {
        Invoke-Step "dotnet tool restore" { dotnet tool restore }
    }
    else {
        Write-Host "==> dotnet tool restore (skipped: no local tool manifest)"
    }

    if (-not $NoRestore) {
        Invoke-Step "dotnet restore $solutionPath" { dotnet restore $solutionPath }
    }
}

function Do-Lint {
    Invoke-Step "dotnet format analyzers --verify-no-changes" {
        dotnet format analyzers $solutionPath --verify-no-changes --no-restore
    }
}

function Do-Typecheck {
    Invoke-Step "dotnet build -c $Configuration --no-restore -warnaserror" {
        dotnet build $solutionPath -c $Configuration --no-restore -warnaserror
    }
}

function Do-Format {
    if ($FixFormat) {
        Invoke-Step "dotnet format (apply fixes)" {
            dotnet format $solutionPath --no-restore
        }
    }
    else {
        Invoke-Step "dotnet format Godot-MCP-SK-Plugin.slnx --verify-no-changes" {
            dotnet format $solutionPath --verify-no-changes --no-restore
        }
    }
}

function Do-Build {
    Invoke-Step "dotnet build $solutionPath -c $Configuration --no-restore" {
        dotnet build $solutionPath -c $Configuration --no-restore
    }
}

function Do-Test {
    Invoke-Step "dotnet test $solutionPath -c $Configuration --no-build" {
        dotnet test $solutionPath -c $Configuration --no-build --nologo
    }
}

function Do-Verify {
    Do-Format
    Do-Lint
    Do-Typecheck
    Do-Test
}

switch ($Task) {
    "restore" { Do-Restore }
    "lint" {
        if (-not $NoRestore) { Do-Restore }
        Do-Lint
    }
    "typecheck" {
        if (-not $NoRestore) { Do-Restore }
        Do-Typecheck
    }
    "format" {
        if (-not $NoRestore) { Do-Restore }
        Do-Format
    }
    "build" {
        if (-not $NoRestore) { Do-Restore }
        Do-Build
    }
    "test" {
        if (-not $NoRestore) { Do-Restore }
        Do-Build
        Do-Test
    }
    "verify" {
        if (-not $NoRestore) { Do-Restore }
        Do-Build
        Do-Verify
    }
    "all" {
        Do-Restore
        Do-Build
        Do-Verify
    }
    default {
        throw "Unknown task: $Task"
    }
}

Write-Host "Done."
