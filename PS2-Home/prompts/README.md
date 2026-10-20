# Prompts

Paste one phase prompt at a time into your coding agent, opened in the repo root. The agent should read `AGENTS.md` first (the prompts tell it to).

## Loop for every phase
1. Start a fresh agent session. Paste the phase prompt.
2. Let it work; do not add extra features mid-phase.
3. Run `dotnet build PS2Home.sln && dotnet test PS2Home.sln` yourself. Run the app.
4. Review the diff (hard-coded paths, shell strings, needless packages, off-architecture code).
5. `git add -A && git commit -s -m "Phase N: ..."`.
6. Only then move to the next phase.

## Repair prompt (when something breaks)
> Read AGENTS.md. The build/test/run is failing. Here is the exact output: <paste>. Diagnose the root cause, make the smallest fix, re-run the build and tests, and report real results. Do not refactor or add features.

## Review prompt (after each phase)
> Read AGENTS.md. Review the latest commit as a strict reviewer. Check for: hard-coded paths, shell-string process launching, unhandled exceptions on user-facing paths, Windows/Linux assumptions in Core, missing tests, unnecessary packages or abstractions, license/legal issues. List concrete findings with file and line. Do not change code.

## Phases
- `phase-0-foundation.md` (base code already in the repo; use this to verify it)
- `phase-1-ui-shell.md`
- `phase-2-library.md`
- `phase-3-pcsx2-setup.md`
- `phase-4-launching.md`
- `phase-5-native-helper.md`
- `phase-6-quality-of-life.md`
- `phase-7-release.md`
