Param(
    [string]$Solution = "GodotMcp.sln"
)

Write-Host "Cleaning bin/obj..."
Get-ChildItem -Path . -Recurse -Directory -Include bin,obj | ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }

Write-Host "Restoring and building $Solution"
dotnet restore $Solution --force
dotnet build $Solution -c Release

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
