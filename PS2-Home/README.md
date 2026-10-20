# PS2 Home

An open-source, cross-platform launcher and setup assistant for PlayStation 2 game images, for Linux and Windows. It uses your own existing [PCSX2](https://pcsx2.net/) installation as the compatibility engine.

> **Status:** early development (Phase 0). Not usable yet.

## What it is (and is not)

- It **is** a game library, one-click launcher, setup assistant and diagnostics tool.
- It is **not** an emulator. PS2 games do not run natively; PCSX2 does the work.
- It does **not** include, download or link to games, BIOS files or PCSX2 binaries. You must supply your own legally obtained game images and a BIOS dump from your own console.

*PS2 Home is not affiliated with or endorsed by Sony Interactive Entertainment or the PCSX2 team. "PlayStation" and "PS2" are trademarks of Sony.*

## Prerequisites (for building)

- [.NET SDK 10](https://dotnet.microsoft.com/download) (check with `dotnet --list-sdks`)
- Git
- Fedora/Linux or Windows 10/11 x64
- (Later, Phase 5 only) a C++ toolchain such as `g++`/CMake

## Build, test, run

```bash
dotnet restore PS2Home.sln
dotnet build PS2Home.sln
dotnet test PS2Home.sln
dotnet run --project src/PS2Home.App
```

## Repository layout

| Path | Purpose |
|---|---|
| `src/PS2Home.App` | Avalonia desktop UI |
| `src/PS2Home.Core` | Models and application logic (no UI or OS dependencies) |
| `src/PS2Home.Infrastructure` | Files, persistence, process launching |
| `tests/PS2Home.Tests` | Automated tests |
| `docs/` | Architecture, roadmap, troubleshooting |
| `prompts/` | Phase-by-phase prompts for AI coding agents |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Issues and PRs must not request or share BIOS files, game images or piracy links.

## License

[MIT](LICENSE)
