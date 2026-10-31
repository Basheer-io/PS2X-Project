#!/usr/bin/env bash
set -euo pipefail

SOURCE_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
INSTALL_DIR="${HOME}/.local/share/ps2home"
BIN_DIR="${HOME}/.local/bin"
DESKTOP_DIR="${HOME}/.local/share/applications"

mkdir -p "${INSTALL_DIR}" "${BIN_DIR}" "${DESKTOP_DIR}"
cp -a "${SOURCE_DIR}/." "${INSTALL_DIR}/"
chmod +x "${INSTALL_DIR}/PS2Home.App"

ln -sfn "${INSTALL_DIR}/PS2Home.App" "${BIN_DIR}/ps2home"

cat > "${DESKTOP_DIR}/ps2home.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=PS2 Home
Comment=Organize and launch legally owned PlayStation 2 game images
Exec=${BIN_DIR}/ps2home %f
Icon=${INSTALL_DIR}/assets/ps2home.svg
Terminal=false
Categories=Game;Emulator;
MimeType=application/x-iso9660-image;application/x-cd-image;
Keywords=PlayStation;PS2;PCSX2;Emulator;Games;
StartupNotify=true
EOF

if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "${DESKTOP_DIR}" >/dev/null 2>&1 || true
fi

echo "PS2 Home installed."
echo "Launch it from your application menu or with: ${BIN_DIR}/ps2home"
echo "If ${BIN_DIR} is not on PATH, run: export PATH=\"${BIN_DIR}:\$PATH\""
