# PS2 Home

PS2 Home is a free, open-source desktop library and launcher for your
PlayStation 2 games. It helps you organize legally obtained game images,
find your installed [PCSX2](https://pcsx2.net/) emulator, launch games, and
diagnose common setup problems.

PS2 Home is a launcher, not an emulator. PCSX2 performs the emulation.

> **Project status:** Early development. Linux and Windows desktop builds,
> PCSX2 detection, game launching, library scanning, diagnostics, controller
> guidance, couch mode, cover art, and settings backup are implemented.

## Important legal notice

PS2 Home does **not** provide, download, host, or link to:

- Commercial PlayStation 2 ROMs, ISOs, CHD files, or other game images
- PlayStation 2 BIOS files
- PCSX2 binaries

Use only games and BIOS dumps you are legally entitled to use. Dump a BIOS
from your own console and obtain game backups according to the laws that
apply to you. PS2 Home is independent and is not affiliated with Sony
Interactive Entertainment or the PCSX2 team.

## Features

- Recursive PS2 game-library scanning
- Search, filtering, missing-file tracking, and recently played games
- Launch through a configured PCSX2 executable, AppImage, or Linux Flatpak
- Automatic PCSX2 discovery on Linux and Windows
- Direct **Open PCSX2** action from Settings
- Fullscreen and batch launch arguments
- Local cover art and enlarged couch/TV mode
- Keyboard navigation and controller setup guidance
- CPU/Vulkan diagnostics through the optional `ps2home-diag` helper
- JSON settings/library backup and restore
- Self-contained Linux and Windows release packages

## Supported operating systems

| Operating system | Support | Recommended package |
|---|---|---|
| Windows 10/11, x64 | Supported | `ps2home-win-x64-*.zip` |
| Ubuntu/Debian Linux, x64 | Supported | `ps2home-linux-x64-*.tar.gz` |
| Fedora Linux, x64 | Supported | `ps2home-linux-x64-*.tar.gz` |
| Arch Linux and derivatives, x64 | Supported | `ps2home-linux-x64-*.tar.gz` |
| Other modern x64 Linux distributions | Usually supported | `ps2home-linux-x64-*.tar.gz` |
| macOS | Not currently supported | No macOS build is published |
| ARM Linux/Windows | Not currently supported | Build support is not provided |
| WSL | Not recommended | Use a native Linux desktop session |

PS2 Home release packages include the .NET runtime, so end users do not
normally need to install .NET. A desktop session is required on Linux.

## Installation

### Windows 10 or 11 (x64)

#### Install from a release ZIP

1. Download the latest `ps2home-win-x64-*.zip` from the repository Releases
   page.
2. Optionally verify the SHA-256 checksum shown in the release.
3. Extract the ZIP to a normal folder, for example:

   ```text
   C:\Tools\PS2Home
   ```

4. Open the extracted folder and run `PS2Home.App.exe`.
5. If Windows SmartScreen appears, verify that the archive came from the
   official repository release before choosing **More info** and **Run anyway**.
6. Install PCSX2 separately from its official website.
7. In PS2 Home, open **Settings**, scan for PCSX2, or select
   `pcsx2-qt.exe` manually.
8. Add the folder containing your legally obtained game images.
9. Return to **Library**, scan, select a game, and choose **Launch**.

The app is portable. It does not require administrator privileges or a
registry installer. User settings are stored under:

```text
%APPDATA%\PS2Home
```

#### Build and run from source on Windows

Install:

- Git
- .NET SDK 10 (the launcher can install it if missing)
- Visual Studio 2022 Build Tools or Visual Studio with the C++ workload
- CMake

Then run in PowerShell from the `PS2-Home` directory:

```powershell
dotnet restore PS2Home.sln
dotnet build PS2Home.sln
dotnet test PS2Home.sln
.\launch-ps2home.ps1
```

The repository also includes `launch-ps2home.bat` for Windows Command Prompt
and `launch-ps2home.ps1` for PowerShell. These launchers find the project
relative to their own location, so they can be started from any working
directory without manually typing the `dotnet run` command. If .NET 10 is
missing, the PowerShell launcher asks for consent and installs the SDK for the
current user using Microsoft's official installer.

To create a portable Windows package:

```powershell
.\scripts\package-windows.ps1 -Version 1.0.0
```

The ZIP and checksum are created in `dist\`.

### Linux (x64)

#### Install from a release tarball

1. Download the latest `ps2home-linux-x64-*.tar.gz` from Releases.
2. Verify the SHA-256 checksum if desired.
3. Extract it:

   ```bash
   tar -xzf ps2home-linux-x64-*.tar.gz
   cd ps2home-linux-x64
   ```

4. Start the app:

   ```bash
   ./PS2Home.App
   ```

If needed, make the files executable:

```bash
chmod +x PS2Home.App ps2home-diag
```

The app stores user settings under:

```text
${XDG_CONFIG_HOME:-$HOME/.config}/PS2Home
```

#### Ubuntu/Debian

Install the common desktop libraries used by Avalonia:

```bash
sudo apt update
sudo apt install libx11-6 libx11-xcb1 libxcb1 libfontconfig1 \
  libfreetype6 libice6 libsm6 libxext6 libxrender1 libxrandr2 \
  libxi6 libglib2.0-0
```

Extract and run the release tarball as shown above. PCSX2 may be installed
as a native package, AppImage, or Flatpak.

#### Fedora

Install the common desktop libraries:

```bash
sudo dnf install libX11 libX11-xcb libxcb fontconfig freetype \
  libICE libSM libXext libXrender libXrandr libXi glib2
```

Extract and run the release tarball as shown above.

#### Arch Linux

Install the common desktop libraries:

```bash
sudo pacman -Syu
sudo pacman -S libx11 libx11-xcb libxcb fontconfig freetype2 \
  libice libsm libxext libxrender libxrandr libxi glib2
```

Extract and run the release tarball as shown above.

#### Build and run from source on Linux

Install:

- Git
- .NET SDK 10
- CMake
- GCC or Clang and a standard C++ toolchain

Example Ubuntu/Debian setup:

```bash
sudo apt update
sudo apt install git dotnet-sdk-10.0 cmake build-essential
```

Build and run:

```bash
git clone https://github.com/Basheer-io/PS2X-Project.git
cd PS2X-Project/PS2-Home
dotnet restore PS2Home.sln
dotnet build PS2Home.sln
dotnet test PS2Home.sln
./launch-ps2home.sh
```

If .NET 10 is missing, `launch-ps2home.sh` asks for consent and installs the
SDK for the current user using Microsoft's official installer. You can also
run `./install-dotnet-linux.sh` directly.

Build the optional native diagnostics helper:

```bash
cmake -S native/PS2Home.Native -B native/PS2Home.Native/build \
  -DCMAKE_BUILD_TYPE=Release
cmake --build native/PS2Home.Native/build --config Release
```

Create a self-contained Linux package:

```bash
./scripts/build-release.sh 1.0.0
```

The tarball and checksum are created in `dist/`.

### macOS

There is currently no supported macOS build. Do not expect the Linux
tarball to run on macOS because it contains Linux-native runtime components.
macOS support would require a separate Avalonia runtime target, packaging
workflow, and PCSX2 integration testing.

## Install and configure PCSX2

### Native execution limitation

PS2 Home cannot run PS2 games natively on a normal Windows or Linux PC. PS2
games target PS2-specific processors and graphics hardware, so the launcher
starts them through the user-installed PCSX2 emulator. Native execution
requires a real PS2 console or a game-specific native port. See
[native-execution.md](docs/native-execution.md) for the details.

PCSX2 must be installed separately from its official source:

<https://pcsx2.net/downloads/>

Start PS2 Home, then:

1. Open **Settings**.
2. Click **Scan for PCSX2**.
3. Select the detected installation.
4. If it is not found, enter the executable path and click **Test Binary**.
5. Click **Open PCSX2** to confirm that the emulator starts.

Common executable locations:

| Platform | Examples |
|---|---|
| Linux native | `/usr/bin/pcsx2-qt`, `/usr/local/bin/pcsx2-qt` |
| Linux AppImage | `~/Applications/PCSX2.AppImage` |
| Linux Flatpak | `flatpak run net.pcsx2.PCSX2` |
| Windows | `C:\Program Files\PCSX2\pcsx2-qt.exe` |

PCSX2 also requires a genuine BIOS dump from your own PlayStation 2 console.
Use the official PCSX2 BIOS documentation:

<https://pcsx2.net/docs/setup/bios/>

## Add games

1. Place your legally obtained game images in a folder.
2. Open **Settings** in PS2 Home.
3. Add the folder under **PS2 Game Directories**.
4. Save the settings.
5. Open **Library** and choose **Rescan Library**.
6. Select a game and choose **Launch**.

The current scanner focuses on `.iso` files. Other formats, including
`.chd`, `.cso`, and `.bin/.cue`, may require additional support before they
appear in the library.

## Linux Flatpak notes

If PCSX2 is installed with Flatpak and your games are outside your home
directory, grant PCSX2 access to the game directory:

```bash
flatpak override --user \
  --filesystem="/path/to/your/games:ro" \
  net.pcsx2.PCSX2
```

Use the smallest directory permission that works. PS2 Home itself is
distributed as a portable desktop app, not as a Flatpak, so it can launch
the host Flatpak command directly.

## Troubleshooting

### The app does not start on Linux

- Confirm that you are using an x64 Linux desktop session.
- Run `chmod +x PS2Home.App`.
- Install the desktop libraries listed for your distribution.
- Start the app from a terminal and read the reported error.
- Do not run the Linux build inside WSL unless a working graphical desktop
  bridge is configured.

### PCSX2 is not detected

- Open **Settings** and click **Scan for PCSX2**.
- Test the executable manually.
- On Linux, confirm that an AppImage is executable:

  ```bash
  chmod +x ~/Applications/PCSX2.AppImage
  ```

- On Windows, select the actual `pcsx2-qt.exe` file, not its containing
  folder.

### A game exits immediately

- Confirm that PCSX2 has a valid BIOS configured.
- Try opening PCSX2 directly with **Open PCSX2**.
- Check the game path and file permissions.
- Open **Diagnostics** and export the launch log.
- Review the launch log in the PS2 Home configuration directory.

### PCSX2 cannot read a Flatpak game path

Grant read access to the directory with the `flatpak override` command above.
External drives and network mounts may need a separate permission.

### The native diagnostics helper is unavailable

The helper is optional. The main launcher remains usable without it. For a
source build, install CMake and a C++ compiler, then build
`native/PS2Home.Native`.

## Build commands

Run these commands from `PS2-Home`:

```bash
dotnet restore PS2Home.sln
dotnet build PS2Home.sln
dotnet test PS2Home.sln
./launch-ps2home.sh
```

If .NET 10 is missing, the Linux launcher asks for consent and installs the
SDK for the current user using Microsoft's official installer. You can also
run `./install-dotnet-linux.sh` directly.

The solution targets `.NET 10`. Release packages are self-contained and use
the following runtime identifiers:

- `linux-x64`
- `win-x64`

## Repository layout

| Path | Purpose |
|---|---|
| `src/PS2Home.App` | Avalonia desktop UI |
| `src/PS2Home.Core` | Platform-neutral models and interfaces |
| `src/PS2Home.Infrastructure` | Scanning, persistence, PCSX2 discovery, launching, logs |
| `native/PS2Home.Native` | Optional C++ CPU/Vulkan diagnostics helper |
| `tests/PS2Home.Tests` | xUnit automated tests |
| `docs/` | Architecture, input, packaging, and troubleshooting documentation |
| `scripts/` | Linux and Windows release packaging scripts |

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening an issue or pull
request. Do not post ROMs, BIOS files, piracy links, or copyrighted
downloads in issues, pull requests, or discussions.

## Security

Read [SECURITY.md](SECURITY.md) for private vulnerability reporting.

## License

PS2 Home is licensed under the [MIT License](LICENSE).
