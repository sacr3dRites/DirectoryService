[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$projectPath = Join-Path $PSScriptRoot "..\src\DirectoryService.IntegrationTests\DirectoryService.IntegrationTests.csproj"
$arguments = @(
    "test",
    $projectPath,
    "--configuration",
    $Configuration,
    "--nologo"
)

if ($NoBuild) {
    $arguments += "--no-build"
}

dotnet @arguments
exit $LASTEXITCODE
