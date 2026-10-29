# PS2 Home 🎮

**Your PlayStation 2 games, in one tidy place.**

Remember when playing a game meant picking it up and pressing a button? PS2 Home is a small, friendly app that tries to bring a bit of that feeling back to your PC. Point it at the folder where your games live, pick one, press **Play**. If something's missing, it tells you what and how to fix it, in plain language.

It's free, open source, and built for **Linux and Windows**.

> 🚧 **Where things stand:** PS2 Home is in its very early days. The project foundation is being laid right now, so there's nothing to play with yet. If you're here early, welcome! Come back soon, or follow along and help shape it.

---

## What PS2 Home is

- 📚 **A game library.** Scan your folders, search your collection, and have it all still there next time you open the app.
- ▶️ **A one-click launcher.** Select a game and play it, with preferences like fullscreen remembered per game.
- 🧭 **A setup helper.** It helps you find PCSX2, checks what's missing, and explains the fix without the jargon.
- 🔍 **A friendly troubleshooter.** When something goes wrong, you get a clear message and useful logs instead of silence.

## What PS2 Home is *not*

Being upfront matters, so here it is:

- **It is not an emulator.** PS2 games don't run natively on a PC. [PCSX2](https://pcsx2.net/) does the heavy lifting, and PS2 Home just makes it nicer to use. You'll need PCSX2 installed on your own.
- **It does not provide games or a BIOS.** You'll need your own legally obtained game images and a BIOS dump from your own PS2 console. PS2 Home will never download, bundle, or point you to either of those.
- **It can't promise every game will run.** Compatibility depends on PCSX2, the game itself, and your computer.

*PS2 Home is an independent community project. It isn't affiliated with or endorsed by Sony Interactive Entertainment or the PCSX2 team. "PlayStation" and "PS2" are trademarks of Sony.*

---

## Planned first-time experience

Once it's ready, getting started should feel something like this:

1. Install PCSX2 (you choose and download it yourself).
2. Open PS2 Home. It looks for PCSX2 and, if it can't find it, asks you to point to it.
3. Add the folder where your game images live.
4. Pick a game and press **Play**.

If anything gets in the way, like a missing BIOS or a game that won't start, PS2 Home will explain what happened and what to try next.

---

## Building it yourself

Want to try the early code, or help out? Here's what you need.

**You'll need**
- [.NET SDK 10](https://dotnet.microsoft.com/download). Check yours with `dotnet --list-sdks`.
- Git
- Fedora or another Linux distribution, or Windows 10/11 (64-bit)

**Build, test, and run**

```bash
dotnet restore PS2Home.sln
dotnet build PS2Home.sln
dotnet test PS2Home.sln
dotnet run --project src/PS2Home.App
```

If something doesn't build, don't worry. This is an early project, and a clear bug report helps a lot (see below).

## How the project is organized

| Folder | What lives there |
|---|---|
| `src/PS2Home.App` | The desktop interface, built with [Avalonia](https://avaloniaui.net/) |
| `src/PS2Home.Core` | The app's logic and models, with no UI or OS-specific code |
| `src/PS2Home.Infrastructure` | Files, saved settings, and starting PCSX2 |
| `tests/PS2Home.Tests` | Automated tests |
| `docs/` | Architecture notes, roadmap, and troubleshooting |
| `prompts/` | Step-by-step prompts for building the project with an AI coding assistant |

Curious about what's coming next? The [roadmap](docs/roadmap.md) walks through each phase.

## Want to help?

Contributions of all sizes are welcome, whether that's code, testing on your setup, improving docs, or just reporting what confused you. Please read [CONTRIBUTING.md](CONTRIBUTING.md) first.

A few simple house rules:

- Be kind. We follow a [Code of Conduct](CODE_OF_CONDUCT.md).
- **No piracy, please.** Don't ask for or share game images, BIOS files, or links to them in issues, pull requests, or discussions. It keeps the project safe and the community welcoming.
- Found a security problem? See [SECURITY.md](SECURITY.md) to report it privately.

This project is also developed with AI coding assistants. If you contribute that way, you're still responsible for reviewing and testing what you submit.

## Having trouble?

Check the [troubleshooting guide](docs/troubleshooting.md). If that doesn't help, open an issue and include your operating system, your PCSX2 version, and how you installed it (AppImage, Flatpak, or Windows build). Those details save a lot of guesswork.

---

[![Architecture diagram of basheer-io/ps2x-project](https://gitdiagram.com/basheer-io/ps2x-project/diagram.png)](https://gitdiagram.com/basheer-io/ps2x-project?utm_source=readme&utm_medium=picture)

*Made for people who still love their PS2 libraries.*

## Process has just started still underdeveloped .
