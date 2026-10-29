# Architecture

PS2 Home is a launcher. PCSX2 (user-installed) runs the games.

## Layers
```
PS2Home.App  ──►  PS2Home.Core  ◄──  PS2Home.Infrastructure
   (UI)        (models/logic)        (files, JSON, processes, OS)
```
- `Core` defines interfaces (e.g. `IGameScanner`, `IPcsx2Locator`, `IGameLauncher`, `ISettingsStore`, `ILibraryStore`) and pure logic (e.g. launch-argument building, game identity).
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
| PCSX2 install types | Windows portable/installed exe, Linux AppImage, Linux Flatpak (`flatpak run net.pcsx2.PCSX2`), distro/native binary. Model "how to launch PCSX2" as a command + leading arguments |
| Process launching | `ArgumentList` only; capture exit code and stderr; log everything |
| Native helper | Phase 5, optional; app must work without it |
| Packaging targets (Phase 7) | Linux: AppImage and/or Flatpak (+ RPM for Fedora if wanted); Windows: zip first, installer later |

## Non-goals
Emulating PS2 hardware, bundling PCSX2, downloading games/BIOS, anything piracy related.
