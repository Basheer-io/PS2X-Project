Read AGENTS.md first and follow it strictly.

**Phase 2: real game library.** Replace sample data with real scanning and persistence. No launching yet.

Tasks:
1. In Core, define `GameEntry` (id, title, full path, file size, added/last-scanned timestamps, missing flag) and interfaces: `IGameScanner`, `IGameLibrary` (reuse from Phase 1), `ISettingsStore`.
2. Decide game identity. Start with normalized full path; document the decision in docs/architecture.md and keep the design open to adding size+hash or disc serial later. Do not hash large files synchronously.
3. Implement an async, cancellable scanner in Infrastructure. Support `.iso` first; make the extension list a single easy-to-extend constant (note other formats like `.chd`, `.cso`, `.bin/.cue` as future work only if verified against PCSX2 docs). Recurse into subfolders; skip inaccessible folders and report them without crashing; avoid duplicates; do not follow symlink loops.
4. Persist settings and the library as JSON under `AppPaths.GetConfigDirectory()`. Write atomically (temp file then replace). Tolerate a missing or corrupt file by backing it up and starting fresh with a clear message.
5. UI: add/remove game folders in Settings, a Rescan button with progress and cancel, search, and clear reporting for missing, moved or inaccessible files. UI must never freeze during scanning.
6. Derive a display title from the file name; keep it simple and editable later.
7. Tests with temp directories: nested folders, duplicates, spaces and non-ASCII names, unreadable directories (where the OS allows), corrupt JSON recovery, persistence round-trip.

Constraints: no launching; no network; no cover-art; no SQLite.

Acceptance: real ISOs (use empty dummy files named *.iso for testing; never real game data) appear in the library; the library survives restart; missing/moved/inaccessible items are reported clearly; tests pass.

Report in the format required by AGENTS.md.
