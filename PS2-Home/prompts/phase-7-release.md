Read AGENTS.md first and follow it strictly.

**Phase 7: testing, packaging and release.**

Tasks:
1. Extend CI: restore, build, test on Linux and Windows; upload test results; publish self-contained or framework-dependent builds as workflow artifacts (decide and document the trade-off).
2. Packaging, documented in docs/packaging.md and scripted where possible:
   - Windows x64: a zip first (portable); installer only if it adds clear value.
   - Linux: choose from AppImage, Flatpak and/or RPM for Fedora. Start with one and document it. If Flatpak, address filesystem permissions and how it launches a separate Flatpak PCSX2 (flatpak-spawn/host permissions) honestly, including limitations.
3. Release workflow: on a version tag, build, test, create a GitHub Release with artifacts and SHA-256 checksums. Use semantic versioning; add a CHANGELOG.md.
4. Docs: README install/upgrade/uninstall, first-run setup walkthrough (PCSX2 + own BIOS + own games), troubleshooting expansions, build-from-source steps, screenshots placeholder.
5. Licensing review: list every dependency and license in THIRD-PARTY-NOTICES.md, confirm no game images, BIOS files or PCSX2 binaries are in the repo or release artifacts, confirm the trademark disclaimer is present in README and the About view. Add an About view with version and license info.
6. Fresh-machine check: follow the README exactly in a clean environment (container, VM or new user account) and record what failed. Report test results honestly; do not claim platforms you did not test.

Acceptance: another person can follow the README, configure their own PCSX2 and launch a game without private instructions; builds and test results are reported honestly; licensing reviewed.

Report in the format required by AGENTS.md.
