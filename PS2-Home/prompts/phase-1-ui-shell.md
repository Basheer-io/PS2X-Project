Read AGENTS.md first and follow it strictly.

**Phase 1: interface shell with sample data only.** No real scanning or launching.

Goal: a dark gaming dashboard that feels like a usable app, not a demo window.

Tasks:
1. Introduce MVVM using `CommunityToolkit.Mvvm` (justify and add to THIRD-PARTY-NOTICES.md; MIT). Keep ViewModels in the App project unless they are UI-free, in which case use Core.
2. Replace the code-behind navigation with a `MainViewModel` and a navigation model for Library, Settings, Diagnostics. Compiled bindings stay on.
3. Library page: a responsive grid of game cards (title, placeholder art tile, file name) bound to a sample-data service behind an `IGameLibrary` interface defined in Core, with a `SampleGameLibrary` implementation. Include a search box that filters the sample list.
4. Implement and visibly demonstrate empty, loading and error states (a debug switch or sample toggles is fine).
5. Shared styling: define colors, spacing and text styles as resources in one place; hover/focus/selected states for cards and nav; sensible keyboard focus and tab order; minimum window size that stays usable.
6. Add unit tests for the filtering logic and view models where practical.

Constraints: no real file access; no PCSX2 interaction; no images downloaded; keep it simple.

Acceptance: navigation works; UI remains usable from about 800x500 to large windows; build and tests pass (run them and show output).

Report in the format required by AGENTS.md.
