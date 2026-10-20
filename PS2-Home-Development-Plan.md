# PS2 Home — Development Plan

> A cross-platform PlayStation 2 gaming hub for Linux and Windows.
>
> **Core goal:** Make managing and launching legally owned PS2 game images straightforward. PS2 Home is a launcher and configuration assistant, not a new PS2 emulator. An existing PCSX2 installation will provide the game-compatibility engine.

## 1. Product Vision

PS2 Home should make playing PS2 games on a PC feel closer to using a console. Users should be able to open their library, select a game, press **Play**, and get helpful guidance if something is missing.

### Core features

- **Game library:** Scan selected folders for supported game-image files, show games in a library, search and organize the collection, and remember paths between launches.
- **One-click launching:** Launch a selected game through a configured PCSX2 installation, with fullscreen and per-game preferences where supported.
- **Setup assistant:** Help users locate PCSX2, check prerequisites, identify missing configuration, and explain fixes in plain language.
- **Controller-friendly setup:** Provide clear guidance for controller configuration and save relevant preferences.
- **Diagnostics:** Report common setup and launch problems, with useful logs and actionable troubleshooting steps.
- **Cross-platform builds:** Support Fedora/Linux and Windows, with a shared application codebase wherever possible.

### Possible later features

- Per-game profiles and launch options
- Cover-art management
- Controller-only/couch interface
- Library statistics and recently played games
- Backup and restore of PS2 Home settings
- Additional platform-specific diagnostics

These are roadmap ideas, not promises that every feature will work for every game or controller.

## 2. Technical Stack

| Area | Technology | Responsibility |
|---|---|---|
| Desktop UI | C# + Avalonia UI | Shared Linux and Windows interface |
| Main application | C# / .NET 10 | Library, settings, process launching, diagnostics and configuration |
| Native helper (later) | C++ | Native system diagnostics where platform APIs make it useful |
| PS2 compatibility engine | Existing PCSX2 installation | Run PS2 games |
| Initial persistence | JSON configuration files | Store settings and lightweight library metadata |
| Possible future persistence | SQLite | Use if library data becomes complex enough to justify it |
| Testing | .NET test project | Test core logic and prevent regressions |
| CI/releases | GitHub Actions | Build/test automation and release artifacts |

### Why this stack?

C# and Avalonia provide a practical path to a shared desktop UI for Linux and Windows. C# is well-suited to the launcher and application logic. C++ will be added only when native integration offers a real benefit; it should not be included merely to use two languages.

PS2 Home will initially detect or let the user select their PCSX2 executable. It will not silently download or bundle third-party executables. The project must also respect the licensing and redistribution rules of all dependencies.

## 3. Development Roadmap

Work milestone by milestone. A phase is complete only when its acceptance checks pass.

### Phase 0 — Project foundation

**Tasks**
- Create the Git repository and solution.
- Establish coding conventions and a basic architecture.
- Create the Avalonia application and minimal navigation.
- Add README, architecture notes, roadmap and contribution guidance.
- Add a test project.

**Acceptance checks**
- The app builds and opens on Fedora KDE.
- The repository contains clear build and run instructions.
- Tests run successfully.

### Phase 1 — Interface shell

**Tasks**
- Design a dark gaming dashboard.
- Add navigation for Library, Settings and Diagnostics.
- Create library cards and empty/loading/error states using sample data.
- Establish reusable UI styling and accessibility basics.

**Acceptance checks**
- The interface feels like a usable application, not a demo window.
- Navigation works and the UI remains usable at common desktop window sizes.
- No real game scanning or launching is required yet.

### Phase 2 — Real game library

**Tasks**
- Let users select and manage game folders.
- Scan for supported game-image files, initially prioritizing ISO files.
- Avoid duplicate entries and handle inaccessible folders gracefully.
- Add searching, rescanning and library persistence.
- Make file scanning responsive without freezing the UI.

**Acceptance checks**
- Real files appear in the library.
- The library remains available after restarting the app.
- Missing, moved or inaccessible files are reported clearly.

### Phase 3 — PCSX2 detection and setup assistant

**Tasks**
- Detect common PCSX2 executable locations where practical.
- Allow the user to browse for the executable if detection fails.
- Check that the configured executable can be started or queried safely.
- Explain required setup, including the need for a compatible BIOS dump obtained from the user's own PS2 console.
- Provide clear links/instructions instead of silently downloading executables or firmware.

**Acceptance checks**
- A user can configure their PCSX2 executable.
- Missing dependencies or setup requirements result in helpful instructions.
- Paths with spaces and non-ASCII characters are handled correctly.

### Phase 4 — Game launching

**Tasks**
- Launch the selected game with documented PCSX2 options.
- Prefer safe process APIs and argument lists over manually constructed shell commands.
- Add fullscreen launching where supported.
- Detect failed starts and report useful errors.
- Track the launched process while it is running and respond when it exits.
- Store sensible per-game launch preferences.

**Acceptance checks**
- A known-working, legally obtained game image launches from the app on Linux.
- Filenames and paths containing spaces work correctly.
- The user receives clear feedback if PCSX2 cannot launch the game.

### Phase 5 — C++ native diagnostics helper

**Tasks**
- Define a small, documented interface between C# and the native helper.
- Add C++ only for information that benefits from native/platform-specific access.
- Return structured results and errors.
- Handle missing libraries, unsupported platforms and helper crashes gracefully.

**Acceptance checks**
- The C# application can call the helper and interpret its output.
- If the helper is unavailable or fails, the main app continues working.
- The native module is tested and built separately for each supported platform.

### Phase 6 — Controller and quality-of-life improvements

**Tasks**
- Add controller setup guidance and links to relevant PCSX2 settings.
- Improve recent games and saved per-game settings.
- Add cover-art support with graceful fallback when artwork is missing.
- Improve keyboard navigation and couch-use friendliness.

**Acceptance checks**
- Core navigation and settings remain usable without a mouse where practical.
- Missing artwork or controller details do not break the library or launcher.

### Phase 7 — Testing, packaging and release

**Tasks**
- Test on Fedora KDE and supported Windows versions.
- Add automated build and test workflows.
- Package the application for Linux and Windows using a documented process.
- Write installation, upgrade, troubleshooting and contribution documentation.
- Publish a versioned GitHub release.

**Acceptance checks**
- Another person can follow the README, configure their own PCSX2 setup and launch a game without needing private instructions.
- Builds and test results are reported honestly.
- Licensing and redistribution requirements have been reviewed.

## 4. Suggested Repository Structure

```text
PS2-Home/
├── README.md
├── LICENSE
├── CONTRIBUTING.md
├── docs/
│   ├── architecture.md
│   ├── roadmap.md
│   └── troubleshooting.md
├── src/
│   ├── PS2Home.App/              # Avalonia desktop UI
│   ├── PS2Home.Core/             # Models and application logic
│   └── PS2Home.Infrastructure/   # Files, persistence, process launching
├── native/                       # Add when the C++ module is justified
│   └── PS2Home.Native/
├── tests/
│   └── PS2Home.Tests/
└── .github/
    └── workflows/                # Automated build and test
```

Do not create placeholder projects solely to make the structure look complete. Add each project when it has a clear responsibility and real code.

## 5. Version 1.0 Definition of Done

- [ ] Runs on Fedora/Linux and Windows x64.
- [ ] Scans selected folders and remembers the game library.
- [ ] Launches a selected game through the configured PCSX2 installation.
- [ ] Handles spaces and unusual characters in paths.
- [ ] Supports fullscreen launch options where PCSX2 supports them.
- [ ] Explains setup failures instead of silently doing nothing.
- [ ] Provides logs and practical troubleshooting instructions.
- [ ] Persists user settings across restarts.
- [ ] Includes tests for important core behavior.
- [ ] Does not distribute commercial game images or proprietary PS2 BIOS files.
- [ ] Includes clear installation, setup and build documentation.

**Important limitation:** PS2 Home will simplify setup and launching, but it does not remove the need for an underlying compatibility engine. PS2 games do not generally run natively on ordinary PCs just by opening an ISO. Compatibility still depends on PCSX2, the game, and the user's system configuration.

## 6. Vibe-Coding Workflow

For each feature, use this loop:

1. **Plan:** Define the feature and acceptance criteria before generating code.
2. **Implement:** Give the coding agent the relevant files, constraints and scope. Avoid asking it to build the entire product in one giant prompt.
3. **Build and test:** Run the application and execute relevant tests on the development machine.
4. **Review:** Inspect the diff for hard-coded paths, fragile shell commands, unnecessary dependencies, unsafe file/process handling and generated code that does not fit the architecture.
5. **Checkpoint:** Commit the working change to Git before adding another feature.
6. **Document:** Update the README or troubleshooting guide when behavior or setup instructions change.

Rules:
- Work on one milestone or coherent feature at a time.
- Never claim a build or test passed unless it was actually run.
- Keep secrets, proprietary BIOS files and game images out of the repository.
- Avoid excessive abstractions and packages.
- Prefer readable, testable code over clever shortcuts.
- Keep Linux and Windows differences behind small, well-defined interfaces.
- Review third-party licenses before redistribution.

## 7. Initial AI Coding Prompt — Phase 0

Copy this prompt into your coding agent after opening the project folder:

> Act as the senior engineer for PS2 Home, an open-source cross-platform desktop application for Linux and Windows.
>
> **Product goal:** Make managing and launching legally owned PlayStation 2 game images straightforward through a modern desktop library. An existing PCSX2 installation provides the emulation engine; this project is a launcher and configuration assistant, not a new PS2 emulator.
>
> **Technology:** C# with .NET 10 and Avalonia UI. Separate UI, core logic and infrastructure cleanly. C++ is planned for a later native diagnostics module; do not add an empty native project yet. Include automated tests for core logic.
>
> **This phase only:**
> 1. Inspect the development environment and available project templates.
> 2. Create a clean solution and minimal application structure.
> 3. Create an Avalonia window with a dark theme and a PS2 Home title.
> 4. Add basic navigation placeholders for Library, Settings and Diagnostics.
> 5. Add a README with prerequisites, build instructions and run instructions.
> 6. Add a minimal test project.
> 7. Ensure the project can be built and launched on the current Linux development machine.
>
> **Constraints:**
> - Do not implement the game scanner or emulator launcher yet.
> - Do not download games, BIOS files or third-party executables.
> - Avoid unnecessary packages and abstractions.
> - Do not hard-code user-specific filesystem paths.
> - Keep the architecture suitable for a future Windows build.
> - Do not claim success unless the build and tests have actually been run.
> - If a required SDK or template is missing, report the exact blocker instead of fabricating a successful build.
>
> At the end, summarize the created files, commands executed, test results and next recommended milestone. Make only the changes needed for Phase 0.

## 8. Reference Links

- [PCSX2 source repository](https://github.com/PCSX2/pcsx2)
- [PCSX2 command-line documentation](https://pcsx2.net/docs/advanced/cli/)
- [PCSX2 BIOS setup documentation](https://pcsx2.net/docs/setup/bios/)
- [Avalonia supported platforms](https://docs.avaloniaui.net/docs/supported-platforms)
- [Microsoft .NET documentation](https://learn.microsoft.com/dotnet/)

---

**First action:** Start Phase 0 on Fedora KDE. Check the installed .NET SDK and C++ compiler, create the initial solution, build it, and run the tests before moving on to interface work.
