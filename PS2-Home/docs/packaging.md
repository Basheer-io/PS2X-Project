# Packaging and Distribution Guide

This document describes the distribution strategy, packaging formats, and build automation for PS2 Home on Linux and Windows.

---

## 1. Distribution Strategy: Self-Contained vs Framework-Dependent

When publishing .NET applications, two deployment models are available:

| Feature | Self-Contained (Chosen) | Framework-Dependent |
|---|---|---|
| **Runtime Requirement** | None. .NET 10 runtime is bundled. | User must install .NET 10 Desktop/Core runtime. |
| **Download Size** | ~30–35 MB (compressed) / ~75 MB (uncompressed) | ~10–15 MB |
| **User Experience** | Plug-and-play. Runs out of the box. | Fragile. Fails to start if correct .NET 10 runtime is absent. |
| **Cross-Distro Compatibility** | High (bundled standard libraries). | Depends on distro packaging of .NET 10. |

**Decision**: All official PS2 Home releases are built as **Self-Contained** applications with the native helper (`ps2home-diag`) bundled alongside the application executable.

---

## 2. Packaging Targets

### Windows (x64)
- **Primary Format**: Portable `.zip` archive (`ps2home-win-x64-vX.Y.Z.zip`).
- **Contents**:
  - `PS2Home.App.exe`
  - `ps2home-diag.exe` (Hardware probe)
  - Core .NET runtime and Avalonia libraries
  - `README.md` and `LICENSE`
- **Installation**: Users extract the folder to their preferred location (e.g., `C:\Tools\PS2Home` or `D:\Games\PS2Home`) and run `PS2Home.App.exe`.
- **Installer**: A Windows installer (MSI / Inno Setup) can be generated in the future, but a portable archive requires no administrative privileges, does not modify the Windows registry, and stores configuration cleanly in `%APPDATA%\PS2Home`.

### Linux (x64)
- **Primary Format**: Compressed tarball (`ps2home-linux-x64-vX.Y.Z.tar.gz`) and AppImage.
- **Contents**:
  - `PS2Home.App` (Executable)
  - `ps2home-diag` (Hardware capability probe CLI)
  - Native Avalonia runtime libraries (`libSkiaSharp.so`, `libHarfBuzzSharp.so`)
  - Bundled .NET 10 runtime libraries
- **Installation**:
  ```bash
  tar -xzf ps2home-linux-x64-v1.0.0.tar.gz
  cd ps2home-linux-x64
  ./PS2Home.App
  ```

### Flatpak Considerations and Sandboxing Analysis
While a Flatpak is attractive for desktop discovery, running a launcher inside a Flatpak introduces critical interaction hurdles:
1. **Launching Another Flatpak (PCSX2)**: If PCSX2 is installed via Flatpak (`net.pcsx2.PCSX2`), PS2 Home cannot directly invoke `flatpak run net.pcsx2.PCSX2` from within its own sandbox without permission to talk to the host flatpak daemon (`--talk-name=org.freedesktop.Flatpak`) and using `flatpak-spawn --host`.
2. **Filesystem Isolation**: Users often store multi-gigabyte PS2 `.iso` images on secondary drives, external hard drives, or network shares. An isolated Flatpak requires blanket `--filesystem=host` permissions to read game images and pass accessible file paths to PCSX2.
3. **Driver Probing**: The native hardware probe (`ps2home-diag`) needs access to the host's actual GPU Vulkan driver stack, which may differ from the Flatpak runtime's bundled Mesa runtime.

For these reasons, the standalone portable tarball / AppImage is the recommended distribution target. A Flatpak manifest can be provided with the explicit understanding that `--talk-name=org.freedesktop.Flatpak` and `--filesystem=host` permissions are necessary.

---

## 3. Automated Packaging Scripts

Automated scripts in the `scripts/` directory automate building both the C++ native helper and the .NET application:

- `scripts/build-release.sh`: Universal Linux build script that compiles `ps2home-diag`, publishes self-contained `linux-x64`, bundles files, and generates a `.tar.gz` with SHA-256 checksum.
- `scripts/package-windows.ps1`: Windows PowerShell script that compiles `ps2home-diag.exe`, publishes self-contained `win-x64`, and packages into a `.zip` with SHA-256 checksum.
