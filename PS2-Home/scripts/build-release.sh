#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-1.0.0}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DIST_DIR="${ROOT_DIR}/dist"
STAGE_DIR="${DIST_DIR}/ps2home-linux-x64"
ARCHIVE_NAME="ps2home-linux-x64-v${VERSION}.tar.gz"

echo "=== Building PS2 Home v${VERSION} for Linux (x64) ==="

# Clean staging area
rm -rf "${DIST_DIR}"
mkdir -p "${STAGE_DIR}"

# 1. Build C++ Native Diagnostics Helper
echo "--- Building C++ Native Helper (ps2home-diag) ---"
cmake -B "${ROOT_DIR}/native/PS2Home.Native/build" -S "${ROOT_DIR}/native/PS2Home.Native" -DCMAKE_BUILD_TYPE=Release
cmake --build "${ROOT_DIR}/native/PS2Home.Native/build" --config Release

# 2. Publish Self-Contained .NET 10 App
echo "--- Publishing Self-Contained .NET 10 Application ---"
DOTNET_CLI_HOME=/tmp dotnet publish "${ROOT_DIR}/src/PS2Home.App/PS2Home.App.csproj" \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --output "${STAGE_DIR}"

# 3. Bundle Native Helper & Documentation
echo "--- Bundling Native Helper & Docs ---"
cp "${ROOT_DIR}/native/PS2Home.Native/build/ps2home-diag" "${STAGE_DIR}/"
chmod +x "${STAGE_DIR}/ps2home-diag"
chmod +x "${STAGE_DIR}/PS2Home.App"

cp "${ROOT_DIR}/README.md" "${STAGE_DIR}/"
cp "${ROOT_DIR}/LICENSE" "${STAGE_DIR}/"
cp "${ROOT_DIR}/THIRD-PARTY-NOTICES.md" "${STAGE_DIR}/"

# 4. Create Archive
echo "--- Packaging Tarball ---"
cd "${DIST_DIR}"
tar -czf "${ARCHIVE_NAME}" -C "${DIST_DIR}" "ps2home-linux-x64"

# 5. Compute SHA-256 Checksum
sha256sum "${ARCHIVE_NAME}" > "${ARCHIVE_NAME}.sha256"

echo "=== Release package successfully created! ==="
echo "Package:  ${DIST_DIR}/${ARCHIVE_NAME}"
echo "Checksum: $(cat "${DIST_DIR}/${ARCHIVE_NAME}.sha256")"
