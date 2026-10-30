#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v dotnet >/dev/null 2>&1 ||
   ! dotnet --version 2>/dev/null | grep -q '^10\.'; then
    "${ROOT_DIR}/install-dotnet-linux.sh"
fi

if [[ -x "${DOTNET_ROOT:-${HOME}/.dotnet}/dotnet" ]]; then
    export DOTNET_ROOT="${DOTNET_ROOT:-${HOME}/.dotnet}"
    export PATH="${DOTNET_ROOT}:${PATH}"
fi

exec dotnet run \
    --project "${ROOT_DIR}/src/PS2Home.App/PS2Home.App.csproj" \
    "$@"
