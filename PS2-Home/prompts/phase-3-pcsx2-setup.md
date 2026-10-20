Read AGENTS.md first and follow it strictly.

**Phase 3: PCSX2 detection and setup assistant.** No game launching yet.

First, fetch and read https://pcsx2.net/docs/advanced/cli/ and https://pcsx2.net/docs/setup/bios/ (or ask me to paste them if you have no network). Base behavior on what they say.

Tasks:
1. Core: model "how to run PCSX2" as `Pcsx2Installation` (kind: Executable, AppImage, Flatpak, Unknown; command; leading arguments; display name; optional version). Flatpak means command `flatpak` with leading args `run net.pcsx2.PCSX2`.
2. `IPcsx2Locator` in Core; implementations in Infrastructure per platform behind one interface. Check common locations only where practical (Windows: Program Files and common portable folders; Linux: PATH, `~/Applications`, `~/.local/bin`, Flatpak via `flatpak info net.pcsx2.PCSX2`). Never hard-code a user's home path; never scan the whole disk.
3. Let the user browse for an executable or AppImage if detection fails; persist the choice in settings.
4. Validate safely: confirm the file exists and is executable (Linux), and optionally run a harmless query with a short timeout and `ArgumentList`; handle timeouts and failures without hanging.
5. Setup assistant UI (Settings/Diagnostics): a checklist showing PCSX2 found/not found, version if known, BIOS guidance, and plain-language fixes. Explain that a BIOS dump from the user's own console is required, link to the BIOS docs, and never download or offer BIOS files or PCSX2 binaries.
6. Tests with fake file systems or temp dirs: paths with spaces and non-ASCII characters, missing file, non-executable file, timeouts (use a fake process runner interface).

Acceptance: a user can configure PCSX2; missing pieces produce helpful instructions; unusual paths work; tests pass.

Report in the format required by AGENTS.md.
