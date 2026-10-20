Read AGENTS.md first and follow it strictly.

**Phase 4: game launching.**

First read https://pcsx2.net/docs/advanced/cli/ and use only options documented there. Typical form is the PCSX2 command, then options such as batch and fullscreen, then `--` and the game path; confirm the exact flags from the docs rather than from memory.

Tasks:
1. Core: a pure, well-tested `LaunchRequest` -> argument list builder (`Pcsx2ArgumentBuilder`) that composes: installation leading args, documented options (fullscreen, batch mode, etc.), then the game path as a single argument. No string concatenation of a command line.
2. Infrastructure: `IGameLauncher` using `ProcessStartInfo` with `ArgumentList`, `UseShellExecute = false`, redirected output where useful, working directory set sensibly. Handle Flatpak (note filesystem permission needs for game folders and surface a clear hint if launch fails because of them).
3. Track the running process: Launching/Running/Exited states in the UI; disable duplicate launches of the same game; react to exit; detect fast failures (non-zero exit shortly after start) and show the exit code plus captured stderr summary.
4. Log launches (command, arguments, exit code, timestamps) to a log file in the config directory with simple size-based rotation. Add a Diagnostics view that shows recent log lines and an "Export logs" action. Redact the user's home path if you include logs in bug reports.
5. Per-game preferences (e.g. fullscreen on/off) stored in settings, keyed by the game id; a Play button on each card and a game details panel.
6. Tests: argument builder (spaces, quotes, non-ASCII, leading dashes in file names handled by the `--` separator), launcher with a fake process runner, failure paths.

Acceptance: a known-working, legally obtained image launches from the app on Linux; names/paths with spaces work; clear feedback when PCSX2 cannot launch the game; tests pass. State honestly if you could not test with a real PCSX2 install.

Report in the format required by AGENTS.md.
