# Troubleshooting

This guide grows with the app. Add an entry whenever a new failure mode is handled.

## Build
- **`dotnet` not found / wrong SDK:** install .NET SDK 10 and run `dotnet --list-sdks`.
- **Avalonia package not found for net10.0:** check the latest stable Avalonia 11.x release and update the versions in `src/PS2Home.App/PS2Home.App.csproj`.

## Runtime (to be expanded as features land)
- **PCSX2 not found:** set its path in Settings. Flatpak installs use the `flatpak` command, not a file path.
- **No BIOS:** PCSX2 needs a BIOS dump from your own PS2 console. See https://pcsx2.net/docs/setup/bios/. PS2 Home cannot provide one.
- **Game does not start:** open Diagnostics, export logs, and check the PCSX2 exit code and message.
