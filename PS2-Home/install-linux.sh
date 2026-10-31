#!/usr/bin/env bash
set -euo pipefail

APP_NAME="PS2 Home"
RELEASE_API="https://api.github.com/repos/Basheer-io/PS2X-Project/releases/latest"
SOURCE_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
INSTALL_DIR="${HOME}/.local/share/ps2home"
BIN_DIR="${HOME}/.local/bin"
DESKTOP_DIR="${HOME}/.local/share/applications"
CONFIG_DIR="${XDG_CONFIG_HOME:-${HOME}/.config}/PS2Home"

uninstall() {
    rm -f "${BIN_DIR}/ps2home" "${DESKTOP_DIR}/ps2home.desktop"
    rm -rf "${INSTALL_DIR}"
    if command -v update-desktop-database >/dev/null 2>&1; then
        update-desktop-database "${DESKTOP_DIR}" >/dev/null 2>&1 || true
    fi
    echo "${APP_NAME} was uninstalled. Your settings remain in ${CONFIG_DIR}."
}

if [[ "${1:-}" == "--uninstall" ]]; then
    uninstall
    exit 0
fi

if [[ "$(uname -m)" != "x86_64" ]]; then
    echo "PS2 Home currently provides a Linux x64 package only." >&2
    exit 1
fi

if [[ ! -x "${SOURCE_DIR}/PS2Home.App" ]]; then
    command -v curl >/dev/null 2>&1 || {
        echo "curl is required for automatic installation. Download the release manually instead." >&2
        exit 1
    }
    command -v tar >/dev/null 2>&1 || {
        echo "tar is required for automatic installation." >&2
        exit 1
    }

    DOWNLOAD_URL="$(
        curl -fsSL "${RELEASE_API}" |
            sed -n 's/.*"browser_download_url": "\(.*ps2home-linux-x64-.*\.tar\.gz\)".*/\1/p' |
            head -n 1
    )"
    [[ -n "${DOWNLOAD_URL}" ]] || {
        echo "Could not find the latest Linux release package." >&2
        exit 1
    }

    TEMP_DIR="$(mktemp -d)"
    trap 'rm -rf "${TEMP_DIR}"' EXIT
    echo "Downloading the latest ${APP_NAME} release..."
    curl -fL --progress-bar "${DOWNLOAD_URL}" -o "${TEMP_DIR}/ps2home.tar.gz"
    tar -xzf "${TEMP_DIR}/ps2home.tar.gz" -C "${TEMP_DIR}"
    SOURCE_DIR="$(find "${TEMP_DIR}" -mindepth 1 -maxdepth 1 -type d -print -quit)"
fi

mkdir -p "${INSTALL_DIR}" "${BIN_DIR}" "${DESKTOP_DIR}"
find "${SOURCE_DIR}" -mindepth 1 -maxdepth 1 ! -name "install-linux.sh" \
    -exec cp -a {} "${INSTALL_DIR}/" \;
cp -a "${SOURCE_DIR}/install-linux.sh" "${INSTALL_DIR}/"
if [[ -f "${SOURCE_DIR}/create-linux-app.sh" ]]; then
    cp -a "${SOURCE_DIR}/create-linux-app.sh" "${INSTALL_DIR}/"
else
    cp -a "$(dirname -- "${BASH_SOURCE[0]}")/create-linux-app.sh" "${INSTALL_DIR}/"
fi
chmod +x "${INSTALL_DIR}/PS2Home.App"
chmod +x "${INSTALL_DIR}/create-linux-app.sh"

ln -sfn "${INSTALL_DIR}/PS2Home.App" "${BIN_DIR}/ps2home"
"${INSTALL_DIR}/create-linux-app.sh"

echo "${APP_NAME} installed for your user."
echo "Launch it from your application menu or with: ${BIN_DIR}/ps2home"
if [[ ":${PATH}:" != *":${BIN_DIR}:"* ]]; then
    echo "Add ${BIN_DIR} to PATH for the 'ps2home' command:"
    echo "  export PATH=\"${BIN_DIR}:\$PATH\""
fi
echo "To uninstall later, run: ${INSTALL_DIR}/install-linux.sh --uninstall"
