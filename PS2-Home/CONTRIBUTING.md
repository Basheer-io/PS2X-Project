# Contributing to PS2 Home

Thanks for helping! AI-assisted contributions are welcome, but you are responsible for what you submit.

## Ground rules
- **No piracy.** Do not post, request or link to game images, BIOS files or other copyrighted material in issues, PRs or discussions. Such content will be removed.
- Be respectful. See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
- Never commit BIOS files, game images, secrets or user-specific paths.

## Workflow
1. Open or find an issue; for larger work, agree on scope first.
2. Fork, branch from `main`, keep the change to one coherent feature.
3. `dotnet build PS2Home.sln` and `dotnet test PS2Home.sln` must pass locally.
4. Update docs when behavior or setup changes.
5. Open a PR using the template.

## Code conventions
- C# / .NET 10, nullable enabled. Prefer clear, testable code over cleverness.
- `Core` has no UI or OS-specific dependencies. Platform differences go behind small interfaces in `Infrastructure`.
- Launch processes with `ProcessStartInfo.ArgumentList`, never by concatenating shell strings.
- Add tests for core logic. Do not claim a build or test passed unless you ran it.
- Avoid unnecessary packages; review the license of any new dependency and note it in `THIRD-PARTY-NOTICES.md`.

## Sign-off
Commits should include a `Signed-off-by:` line (`git commit -s`), certifying the [Developer Certificate of Origin](https://developercertificate.org/).
