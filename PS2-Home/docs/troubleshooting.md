# Troubleshooting

This guide grows with the app. Add an entry whenever a new failure mode is handled.

## Build
- **`dotnet` not found / wrong SDK:** install .NET SDK 10 and run `dotnet --list-sdks`.
- **Avalonia package not found for net10.0:** check the latest stable Avalonia 11.x release and update the versions in `src/PS2Home.App/PS2Home.App.csproj`.

## Runtime
- **PCSX2 not found:** set its path in Settings or click "Scan for PCSX2". Flatpak installs use the command `flatpak run net.pcsx2.PCSX2`.
- **No BIOS:** PCSX2 needs a genuine BIOS dump from your own PS2 console. See https://pcsx2.net/docs/setup/bios/. PS2 Home cannot provide one.
- **Game does not start / Exits immediately:**
  - Check the error banner in the Library view.
  - Open Diagnostics and click "Export Launch Logs" to view the exact command line and error stream.
  - Review `AppPaths.GetConfigDirectory()/logs/launch.log` directly.
- **Flatpak filesystem permission issue:**
  - If your games reside on a secondary drive or outside `/home/user`, the Flatpak sandbox may block access.
  - Grant read permission to the directory via:
    ```bash
    flatpak override --user --filesystem="/path/to/your/games" net.pcsx2.PCSX2
    ```
- **Permission denied on executable (Linux):** Ensure custom PCSX2 binaries or AppImages have execute permissions set: `chmod +x /path/to/pcsx2.AppImage`.
