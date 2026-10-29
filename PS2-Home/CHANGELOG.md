# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-10-29

### Added
- **Game Library**:
  - Recursive filesystem scanning for PlayStation 2 game disc images (`.iso`).
  - Search filtering by game title or directory path with real-time updates.
  - Missing and moved file tracking with visual badges.
  - Safe, atomic JSON library persistence with corruption recovery backup.
- **PCSX2 Launch Engine**:
  - Safe process invocation using `ProcessStartInfo.ArgumentList` and `UseShellExecute = false` to prevent shell injection vulnerabilities.
  - Option terminator `--` support for paths containing leading hyphens or spaces.
  - Modern PCSX2 Qt CLI flags (`-batch`, `-fullscreen`, `-nogui`).
  - Active session monitoring with fast failure detection (<3s exit) and stderr diagnostics.
  - Rolling launch log (`launch.log`) capped at 1 MB with user path redaction (`~`).
- **Setup Assistant**:
  - Automatic detection of PCSX2 installations on Linux (Flatpak `net.pcsx2.PCSX2`, PATH, `~/.local/bin`, `~/Applications/*.AppImage`) and Windows (`Program Files`, `LOCALAPPDATA`, PATH).
  - Custom binary and AppImage path validator with version readout.
  - Educational guidance on legally dumping PS2 BIOS from physical consoles.
- **Hardware Diagnostics (C++ Helper)**:
  - Standalone out-of-process C++23 capability probe (`ps2home-diag`) protecting the UI from GPU driver crashes.
  - CPU instruction set verification (`SSE4.1`, `AVX2`, `AES-NI`).
  - Dynamic runtime loading of Vulkan drivers (`dlopen` on Linux, `LoadLibrary` on Windows).
  - Formatted Diagnostics card in UI with capability chips and raw JSON inspector.
- **Controller Guidance**:
  - Interactive gamepad setup guide for DualShock 4, DualSense, Xbox, and generic controllers.
  - One-click button to open PCSX2's main configuration GUI without running a game.
  - Links to official PCSX2 controller documentation.
  - Protection against modifying PCSX2 internal config files directly.
- **User Interface & Quality of Life**:
  - Couch Mode: 10-foot UI toggle for TV and living room play.
  - Keyboard navigation: Full arrow key navigation across game cards, Enter to launch, and Escape to clear search.
  - High-visibility focus indicators (`#00D4AA`) for keyboard and accessible navigation.
  - Recently Played tracker and quick-launch row on the Library screen.
  - Custom Cover Art: User-selected local images (PNG, JPG, WebP, BMP up to 10MB) stored in config directory with fallback disc placeholders.
  - Backup & Restore: Export and restore settings and game metadata to JSON files with schema validation.
- **About & Legal**:
  - About view with application metadata, version `1.0.0`, and MIT license info.
  - Explicit non-affiliation trademark disclaimer and anti-piracy statement.
  - Complete `THIRD-PARTY-NOTICES.md` acknowledging open-source components.
- **CI / CD & Packaging**:
  - Multi-platform CI workflow testing on Ubuntu and Windows.
  - Self-contained build artifact generation.
  - Release workflow building `.tar.gz` and `.zip` packages with SHA-256 checksums.
  - Packaging documentation in `docs/packaging.md`.
