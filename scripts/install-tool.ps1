param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

dotnet pack ".\src\GodotMcp.Plugin\GodotMcp.Plugin.csproj" -c $Configuration -o .\nupkg
$package = Get-ChildItem .\nupkg\GodotMcp.SemanticKernel.Plugin.*.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $package) { throw "No package produced." }

Write-Host "Built package: $($package.FullName)"
Write-Host "Reference this package from your host app via local source or direct PackageReference."
