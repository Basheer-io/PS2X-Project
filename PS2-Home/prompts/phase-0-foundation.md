Read AGENTS.md first and follow it strictly.

**Phase 0: verify and finish the project foundation.** The repo already contains a solution, an Avalonia shell, a test project, CI and docs, but none of it has been built yet.

Tasks:
1. Run `dotnet --list-sdks`. Confirm .NET 10 is available. If not, report the exact blocker and stop.
2. Run `dotnet restore`, `dotnet build PS2Home.sln`, and `dotnet test PS2Home.sln`.
3. Fix only what prevents these from succeeding (for example: Avalonia or xunit package versions that do not resolve; check the latest stable versions compatible with net10.0 and update the csproj files and THIRD-PARTY-NOTICES.md). Make the smallest changes needed.
4. Launch the app with `dotnet run --project src/PS2Home.App`. Confirm a dark window titled "PS2 Home" opens with Library, Settings, Diagnostics navigation that switches the page text. If no display is available, say so instead of claiming it ran.
5. Verify the README build/run instructions match reality and fix them if not.
6. Run `git init` if needed and make sure `.gitignore` excludes bin/obj and any game/BIOS file types.

Constraints: do not implement scanning or launching; do not add packages beyond fixing versions; do not add a native project; do not claim success without real command output.

Report: files changed, exact commands and results, blockers, and the next step (Phase 1).
