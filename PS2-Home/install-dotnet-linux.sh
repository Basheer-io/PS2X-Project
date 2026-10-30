#!/usr/bin/env bash
set -euo pipefail

DOTNET_DIR="${DOTNET_ROOT:-${HOME}/.dotnet}"
INSTALL_SCRIPT="$(mktemp)"
trap 'rm -f "$INSTALL_SCRIPT"' EXIT

if command -v dotnet >/dev/null 2>&1 &&
   dotnet --version 2>/dev/null | grep -q '^10\.'; then
    printf '%s\n' "A .NET 10 SDK is already installed."
    exit 0
fi

printf '%s\n' "PS2 Home needs the .NET 10 SDK to run from source."
printf '%s' "Install it for your user from Microsoft's official installer? [Y/n] "
read -r answer
if [[ -n "$answer" && ! "$answer" =~ ^[Yy]$ ]]; then
    printf '%s\n' "Installation cancelled." >&2
    exit 1
fi

if ! command -v curl >/dev/null 2>&1; then
    printf '%s\n' "curl is required to download the official .NET installer." >&2
    exit 1
fi

curl --fail --location --proto '=https' --tlsv1.2 \
    --output "$INSTALL_SCRIPT" \
    https://dot.net/v1/dotnet-install.sh

mkdir -p "$DOTNET_DIR"
bash "$INSTALL_SCRIPT" \
    --channel 10.0 \
    --install-dir "$DOTNET_DIR" \
    --no-path

printf '%s\n' "Installed .NET 10 SDK in $DOTNET_DIR."
