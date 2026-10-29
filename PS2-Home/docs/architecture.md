# Architecture

PS2 Home is a launcher. PCSX2 (user-installed) runs the games.

## Layers
```
PS2Home.App  ──►  PS2Home.Core  ◄──  PS2Home.Infrastructure
   (UI)        (models/logic)        (files, JSON, processes, OS)
```
- `Core` defines interfaces (`IGameScanner`, `IPcsx2Locator`, `IProcessRunner`, `ISettingsStore`, `ILibraryStore`) and pure logic (game identity, installation modeling).
- `Infrastructure` implements them. Linux/Windows differences live here behind small interfaces.
- `App` wires things together (DI container with `Microsoft.Extensions.DependencyInjection`) and presents state.

## Key decisions
| Topic | Decision |
|---|---|
| Config location | `AppPaths`: XDG config on Linux, `%APPDATA%` on Windows |
| Persistence | Atomic JSON persistence (`File.Move(overwrite: true)`) with corrupted file backup; SQLite only if the library outgrows it |
| Game identity | Normalized absolute path initially (`GameEntry.Id`); designed to accept disc serial or partial hash in later phases without breaking models. Large files are never hashed synchronously |
| Supported formats | `.iso` first; single extensible list constant in `FileSystemGameScanner`. Additional formats (`.chd`, `.cso`, `.bin/.cue`) to be added after CLI testing against PCSX2 |
| PCSX2 versions | Support current Qt-based PCSX2 (2.x). Document that legacy 1.6 is unsupported unless added deliberately |
| PCSX2 install types | Windows portable/installed exe, Linux AppImage, Linux Flatpak (`flatpak run net.pcsx2.PCSX2`), distro/native binary. Model "how to launch PCSX2" as a command + leading arguments (`Pcsx2Installation`) |
| Process launching | `ArgumentList` only; `UseShellExecute = false`; enforce timeouts (e.g. 2s for version query); capture exit code and stderr; log everything |
| Native helper | Phase 5, optional; app must work without it |
| Packaging targets (Phase 7) | Linux: AppImage and/or Flatpak (+ RPM for Fedora if wanted); Windows: zip first, installer later |

## PCSX2 Discovery
`IPcsx2Locator` prioritizes standard package managers and known directories:
1. **Linux:**
   - Flatpak check via `flatpak info net.pcsx2.PCSX2`.
   - PATH search (`pcsx2-qt`, `pcsx2`).
   - User standard folders (`~/.local/bin/`, `~/Applications/*pcsx2*.AppImage`).
2. **Windows:**
   - Program Files (`%ProgramFiles%\PCSX2\pcsx2-qt.exe`, etc.).
   - Local AppData (`%LOCALAPPDATA%\Programs\PCSX2\pcsx2-qt.exe`).
   - PATH lookup.
Full-disk searches are strictly avoided to ensure fast startup and respect user storage.

## Non-goals
Emulating PS2 hardware, bundling PCSX2, downloading games/BIOS, anything piracy related.
