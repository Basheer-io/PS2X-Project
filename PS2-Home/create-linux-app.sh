#!/usr/bin/env bash
set -euo pipefail

BIN_DIR="${HOME}/.local/bin"
DESKTOP_DIR="${HOME}/.local/share/applications"
ICON_DIR="${HOME}/.local/share/icons/hicolor/scalable/apps"
DESKTOP_FILE="${DESKTOP_DIR}/ps2home.desktop"
ICON_FILE="${ICON_DIR}/ps2home.svg"
ICON_SOURCE="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/assets/ps2home.svg"

if [[ ! -x "${BIN_DIR}/ps2home" ]]; then
    echo "PS2 Home is not installed at ${BIN_DIR}/ps2home." >&2
    echo "Install PS2 Home first, then run this script again." >&2
    exit 1
fi

mkdir -p "${DESKTOP_DIR}" "${ICON_DIR}"
cp "${ICON_SOURCE}" "${ICON_FILE}"

cat > "${DESKTOP_FILE}" <<EOF
[Desktop Entry]
Type=Application
Name=PS2 Home
Comment=Organize and launch legally owned PlayStation 2 game images
Exec=${BIN_DIR}/ps2home %f
Icon=ps2home
Terminal=false
Categories=Game;Emulator;
MimeType=application/x-iso9660-image;application/x-cd-image;
Keywords=PlayStation;PS2;PCSX2;Emulator;Games;
StartupNotify=true
EOF

if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "${DESKTOP_DIR}" >/dev/null 2>&1 || true
fi

echo "PS2 Home is now available in your application menu."
echo "It can also open .iso files from your file manager."
