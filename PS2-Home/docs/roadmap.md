# Roadmap

A phase is done only when its acceptance checks pass. Each phase has a ready-to-paste prompt in `prompts/`.

| Phase | Goal | Prompt |
|---|---|---|
| 0 | Foundation: solution, Avalonia shell, tests, CI | `prompts/phase-0-foundation.md` (base code already included) |
| 1 | Dark dashboard, navigation, sample-data library cards, UI states | `prompts/phase-1-ui-shell.md` |
| 2 | Real game library: folder scanning, persistence, search | `prompts/phase-2-library.md` |
| 3 | PCSX2 detection and setup assistant | `prompts/phase-3-pcsx2-setup.md` |
| 4 | Game launching | `prompts/phase-4-launching.md` |
| 5 | Optional C++ native diagnostics helper | `prompts/phase-5-native-helper.md` |
| 6 | Controller guidance, recents, cover art, keyboard/couch use | `prompts/phase-6-quality-of-life.md` |
| 7 | Testing, packaging, release | `prompts/phase-7-release.md` |

Suggested order note: Phases 2 and 3 can be swapped or interleaved; launching (4) needs both.

## v1.0 definition of done
- Runs on Fedora/Linux and Windows x64
- Scans folders, remembers the library
- Launches a game through the configured PCSX2
- Handles spaces/unusual characters in paths
- Fullscreen option where PCSX2 supports it
- Explains setup failures; provides logs and troubleshooting
- Persists settings; tests cover core behavior
- Distributes no games or BIOS files
- Clear install, setup and build docs

Phase 5 (native helper) is optional for v1.0.
