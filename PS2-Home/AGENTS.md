# Instructions for AI coding agents (PS2 Home)

Read this file first in every session. (Claude Code users: copy or symlink it as `CLAUDE.md`.)

## Project
PS2 Home is an open-source (MIT) Avalonia/.NET 10 launcher and setup assistant for PS2 game images on Linux and Windows. It launches a user-installed PCSX2. It is NOT an emulator.

## Architecture
- `src/PS2Home.Core`: models and logic. No UI, no OS-specific APIs.
- `src/PS2Home.Infrastructure`: file system, persistence, process launching, platform differences behind small interfaces.
- `src/PS2Home.App`: Avalonia UI only. Keep view code thin.
- `tests/PS2Home.Tests`: xUnit tests for Core and Infrastructure.
- `native/` does not exist until Phase 5.

## Hard rules
1. Work on ONE phase or feature per session. Do not build ahead.
2. Never download, bundle or reference sources for games, BIOS files or PCSX2 binaries. Do not commit any such files.
3. Launch processes with `ProcessStartInfo.ArgumentList`. Never build shell command strings. Never use `shell=true`/`UseShellExecute=true` for launching PCSX2.
4. No hard-coded user paths. Use `AppPaths` / platform config directories.
5. Handle paths with spaces and non-ASCII characters. Test them.
6. No placeholder projects or speculative abstractions. Add a project only when it has real code.
7. Add or update tests for new Core logic.
8. Never claim a build or test passed unless you ran it and saw the result. If the SDK or a package is missing, report the exact blocker.
9. Verify PCSX2 CLI behavior against https://pcsx2.net/docs/advanced/cli/ instead of guessing flags.
10. Before adding a NuGet package: justify it, check its license, add it to THIRD-PARTY-NOTICES.md.
11. Update README/docs when behavior or setup changes.

## Done-report format
End every session with: files changed, commands run (with real results), tests run (with real results), known issues, and the next recommended step.
