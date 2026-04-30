[CmdletBinding()]
param(
    [switch]$Rebuild,
    [switch]$RebuildAll,
    [switch]$SkipBuild,
    [switch]$SkipDocker,
    [switch]$SkipFrontend,
    [switch]$StartApiLocally,
    [switch]$ApplyMigrations,
    [switch]$RunSync,
    [switch]$DryRun,
    [string]$ApiBaseUrl = "http://localhost:8080",
    [int]$MediaExerciseLimit = 25,
    [int]$SyncTimeoutSeconds = 600
)

$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Assert-CommandExists {
    param([string]$CommandName)

    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        throw "Required command '$CommandName' was not found on PATH."
    }
}

function Invoke-Step {
    param(
        [string]$Message,
        [scriptblock]$Action
    )

    Write-Host "`n==> $Message" -ForegroundColor Cyan

    if ($DryRun) {
        Write-Host "[dry-run] skipped"
        return
    }

    & $Action
}

function Invoke-ExternalCommand {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$WorkingDirectory
    )

    Push-Location $WorkingDirectory
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            $argumentText = if ($Arguments) { $Arguments -join ' ' } else { '' }
            throw "Command failed: $FilePath $argumentText"
        }
    }
    finally {
        Pop-Location
    }
}

function Start-DetachedPowerShell {
    param(
        [string]$WindowTitle,
        [string]$WorkingDirectory,
        [string]$Command
    )

    $escapedWorkingDirectory = $WorkingDirectory.Replace("'", "''")
    $escapedWindowTitle = $WindowTitle.Replace("'", "''")
    $commandText = "Set-Location '$escapedWorkingDirectory'; `$host.UI.RawUI.WindowTitle = '$escapedWindowTitle'; $Command"

    Start-Process -FilePath "powershell.exe" -WorkingDirectory $WorkingDirectory -ArgumentList @(
        "-NoExit",
        "-ExecutionPolicy", "Bypass",
        "-Command", $commandText
    ) | Out-Null
}

$repoRoot = Get-RepoRoot
$solutionPath = Join-Path $repoRoot "Exercise-Microservice.sln"
$frontendPath = Join-Path $repoRoot "frontend"
$syncScriptPath = Join-Path $PSScriptRoot "Invoke-ExerciseSync.ps1"

if ($Rebuild -and $RebuildAll) {
    throw "Use either -Rebuild or -RebuildAll, not both."
}

if ($StartApiLocally -and -not $SkipDocker) {
    Write-Host "Starting the API locally, so Docker startup will be skipped." -ForegroundColor Yellow
    $SkipDocker = $true
}

Assert-CommandExists -CommandName "dotnet"

if (-not $SkipDocker) {
    Assert-CommandExists -CommandName "docker"
}

if (-not $SkipFrontend) {
    Assert-CommandExists -CommandName "npm"
}

if (-not $SkipBuild) {
    if ($Rebuild -or $RebuildAll) {
        Invoke-Step -Message "Cleaning the .NET solution" -Action {
            Invoke-ExternalCommand -FilePath "dotnet" -Arguments @("clean", $solutionPath) -WorkingDirectory $repoRoot
        }
    }

    Invoke-Step -Message "Building the .NET solution" -Action {
        Invoke-ExternalCommand -FilePath "dotnet" -Arguments @("build", $solutionPath) -WorkingDirectory $repoRoot
    }
}

if ($ApplyMigrations) {
    Invoke-Step -Message "Applying database migrations" -Action {
        Invoke-ExternalCommand -FilePath "dotnet" -Arguments @(
            "ef", "database", "update",
            "--project", "Exercise.Infrastructure",
            "--startup-project", "Exercise.API"
        ) -WorkingDirectory $repoRoot
    }
}

if (-not $SkipDocker) {
    $dockerArguments = if ($RebuildAll) {
        @("compose", "up", "-d", "--build")
    }
    elseif ($Rebuild) {
        @("compose", "up", "-d", "--build", "api")
    }
    else {
        @("compose", "up", "-d")
    }

    Invoke-Step -Message "Starting Docker services" -Action {
        Invoke-ExternalCommand -FilePath "docker" -Arguments $dockerArguments -WorkingDirectory $repoRoot
    }
}

if ($StartApiLocally) {
    Invoke-Step -Message "Starting the API in a new PowerShell window" -Action {
        Start-DetachedPowerShell -WindowTitle "Exercise API" -WorkingDirectory $repoRoot -Command "dotnet run --project .\Exercise.API"
    }
}

if (-not $SkipFrontend) {
    Invoke-Step -Message "Starting the Expo web frontend in a new PowerShell window" -Action {
        Start-DetachedPowerShell -WindowTitle "Exercise Frontend" -WorkingDirectory $frontendPath -Command "npm run web"
    }
}

if ($RunSync) {
    Invoke-Step -Message "Running exercise sync" -Action {
        Invoke-ExternalCommand -FilePath "powershell.exe" -Arguments @(
            "-NoProfile",
            "-ExecutionPolicy", "Bypass",
            "-File", $syncScriptPath,
            "-ApiBaseUrl", $ApiBaseUrl,
            "-MediaExerciseLimit", $MediaExerciseLimit,
            "-SyncTimeoutSeconds", $SyncTimeoutSeconds
        ) -WorkingDirectory $repoRoot
    }
}

Write-Host "`nFinished."

if ($DryRun) {
    Write-Host "Dry run only. No commands were executed." -ForegroundColor Yellow
}
else {
    Write-Host "Use -Rebuild for the API container, -RebuildAll for the full stack, and -RunSync when you want a fresh exercise sync." -ForegroundColor Green
}