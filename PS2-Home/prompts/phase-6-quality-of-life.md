Read AGENTS.md first and follow it strictly.

**Phase 6: controller guidance and quality of life.**

Tasks:
1. Controller guidance page: plain-language steps for configuring a controller in PCSX2, with a button to open PCSX2 itself (via the existing launcher infrastructure) and links to the official docs. Do not attempt to write PCSX2's config files unless you first verify their format and I approve.
2. Recently played and last-played timestamps, persisted; a "Recent" row on the Library page.
3. Cover art: user-supplied images only. Let the user pick an image per game; copy it into the config directory (resized/limited size); show a graceful fallback tile when missing or corrupt. Do not download artwork or scrape websites in this phase.
4. Keyboard navigation everywhere (arrow keys across cards, Enter to play, Escape to go back) and visible focus; optional larger "couch" layout toggle. Gamepad input to the UI is out of scope unless trivial with Avalonia and no new heavy dependency; if not, document it as future work.
5. Settings backup/restore to a user-chosen JSON file, with validation on import.
6. Tests for new logic (recents ordering, backup import validation, cover-art fallback).

Acceptance: core navigation and settings usable without a mouse; missing art or controller details never break the library or launcher; tests pass.

Report in the format required by AGENTS.md.
