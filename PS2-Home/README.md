# PS2 Home

An open-source, cross-platform launcher and setup assistant for PlayStation 2 game images, for Linux and Windows. It uses your own existing [PCSX2](https://pcsx2.net/) installation as the compatibility engine.

> **Status:** Phase 6 complete (Library, PCSX2 detection, launch engine, native diagnostics, controller guidance, couch layout, cover art, and backup/restore).

## Features

- **Game Library**: Recursive disc scanning (`.iso`), fast filtering, missing-file tracking, and persisted metadata.
- **Recently Played**: Instant access to your most recently launched titles.
- **User-Supplied Cover Art**: Pick custom local box art images with size validation, local storage, and fallback disc placeholders.
- **Keyboard Navigation & Couch Mode**: Full arrow-key and Enter-to-launch support with high-visibility focus styling, plus an enlarged 10-foot Couch layout for TV screens.
- **PCSX2 Integration**: Auto-detects Flatpak, AppImage, system, and Windows installations with safe execution argument isolation.
- **Controller Guidance**: Interactive gamepad setup guide for PCSX2 with one-click PCSX2 launcher and official documentation references.
- **Hardware Diagnostics**: Out-of-process C++ probe (`ps2home-diag`) checking CPU instruction sets (SSE4.1, AVX2, AES) and Vulkan GPU compatibility without GUI crash risk.
- **Settings Backup & Restore**: One-click JSON backup export and schema-validated restore for user configuration and library data.

## What it is (and is not)

- It **is** a game library, one-click launcher, setup assistant, and diagnostics tool.
- It is **not** an emulator. PS2 games do not run natively; PCSX2 does the work.
- It does **not** include, download, or link to games, BIOS files, or PCSX2 binaries. You must supply your own legally obtained game images and a BIOS dump from your own console.

*PS2 Home is not affiliated with or endorsed by Sony Interactive Entertainment or the PCSX2 team. "PlayStation" and "PS2" are trademarks of Sony.*

## Prerequisites (for building)

- [.NET SDK 10](https://dotnet.microsoft.com/download) (check with `dotnet --list-sdks`)
- Git
- CMake & C++ compiler (GCC/Clang or MSVC) for the native helper tool
- Linux (x64) or Windows 10/11 (x64)

## Build, test, run

```bash
# Build .NET Solution
dotnet build PS2Home.sln

# Run test suite
dotnet test PS2Home.sln

# Build native diagnostics helper (optional)
cmake -S native/PS2Home.Native -B native/PS2Home.Native/build
cmake --build native/PS2Home.Native/build

# Launch App
dotnet run --project src/PS2Home.App
```

## Repository layout

| Path | Purpose |
|---|---|
| `src/PS2Home.App` | Avalonia desktop UI (MVVM) |
| `src/PS2Home.Core` | Models and application logic (no UI or OS dependencies) |
| `src/PS2Home.Infrastructure` | Files, persistence, cover art, backups, process runner |
| `native/PS2Home.Native` | Standalone C++ hardware capability probe CLI |
| `tests/PS2Home.Tests` | Automated unit tests (xUnit) |
| `docs/` | Architecture, native helper, input/navigation, troubleshooting |
| `prompts/` | Phase-by-phase prompts for development roadmap |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Issues and PRs must not request or share BIOS files, game images, or piracy links.

## License

[MIT](LICENSE)
