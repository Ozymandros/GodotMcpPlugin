#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="${1:-Release}"

dotnet pack "./src/GodotMcp.Plugin/GodotMcp.Plugin.csproj" -c "$CONFIGURATION" -o "./nupkg"
LATEST_PACKAGE="$(ls -t ./nupkg/GodotMcp.SemanticKernel.Plugin.*.nupkg | head -n 1)"

if [[ -z "${LATEST_PACKAGE}" ]]; then
  echo "No package produced."
  exit 1
fi

echo "Built package: ${LATEST_PACKAGE}"
echo "Reference this package from your host app via local source or direct PackageReference."
