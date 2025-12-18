[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$VersionSuffix,
    [string]$NuGetSource = 'https://api.nuget.org/v3/index.json',
    [string]$NuGetApiKey,
    [switch]$UseNet10
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-DotNet {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host "dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
    dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

$repositoryRoot = Split-Path -Path $PSScriptRoot -Resolve
Push-Location $repositoryRoot
try {
    $solutionPath = Join-Path $repositoryRoot 'Xcaciv.Command.sln'
    $packageProjectPath = Join-Path $repositoryRoot 'src\Xcaciv.Command\Xcaciv.Command.csproj'
    $artifactDirectory = Join-Path $repositoryRoot 'artifacts\packages'

    if (-not (Test-Path -Path $artifactDirectory)) {
        New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
    }

    $msbuildProperties = @()
    if ($UseNet10.IsPresent) {
        $msbuildProperties += '-p:UseNet10=true'
    }

    $restoreArguments = @('restore', $solutionPath)
    $restoreArguments += $msbuildProperties
    Invoke-DotNet -Arguments $restoreArguments

    $buildArguments = @('build', $solutionPath, '--configuration', $Configuration, '--nologo')
    $buildArguments += $msbuildProperties
    Invoke-DotNet -Arguments $buildArguments

    $packArguments = @('pack', $packageProjectPath, '--configuration', $Configuration, '--output', $artifactDirectory, '--nologo')
    if ($VersionSuffix) {
        $packArguments += "-p:VersionSuffix=$VersionSuffix"
    }
    $packArguments += $msbuildProperties

    Invoke-DotNet -Arguments $packArguments

    $packageFiles = Get-ChildItem -Path $artifactDirectory -Filter '*.nupkg' | Where-Object { $_.Name -notlike '*.snupkg' }
    $symbolPackageFiles = Get-ChildItem -Path $artifactDirectory -Filter '*.snupkg' -ErrorAction SilentlyContinue

    if ($NuGetApiKey) {
        foreach ($packageFile in $packageFiles) {
            Invoke-DotNet -Arguments @('nuget', 'push', $packageFile.FullName, '--source', $NuGetSource, '--api-key', $NuGetApiKey, '--skip-duplicate')
        }
        foreach ($symbolPackageFile in $symbolPackageFiles) {
            Invoke-DotNet -Arguments @('nuget', 'push', $symbolPackageFile.FullName, '--source', $NuGetSource, '--api-key', $NuGetApiKey, '--skip-duplicate')
        }
    }
    else {
        Write-Host "Packages saved to $artifactDirectory (skipping push because no API key was provided)." -ForegroundColor Green
    }
}
finally {
    Pop-Location
}