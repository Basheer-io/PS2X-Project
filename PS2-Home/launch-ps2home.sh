#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v dotnet >/dev/null 2>&1; then
    printf '%s\n' "dotnet was not found. Install the .NET 10 SDK, then run this launcher again." >&2
    exit 1
fi

exec dotnet run \
    --project "${ROOT_DIR}/src/PS2Home.App/PS2Home.App.csproj" \
    "$@"
