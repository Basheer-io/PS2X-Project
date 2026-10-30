# Native execution and the PS2 Home launcher

## Can a PS2 game image run natively on a normal PC?

No. A PlayStation 2 game is compiled for the PS2's Emotion Engine, I/O
Processor, vector units, and graphics hardware. A standard Windows or Linux
PC does not provide those processors or their memory and graphics behavior.

PS2 Home therefore does **not** claim to provide native PS2 execution. It is a
desktop library and launcher that starts a user-installed PCSX2 process with
the selected game image. PCSX2 emulates the PS2 hardware; PS2 Home handles
library scanning, configuration, safe process launching, and diagnostics.

Native execution is only possible on:

- an actual PlayStation 2 console; or
- a game-specific native port that was separately developed for Windows or
  Linux.

PS2 Home cannot convert a PS2 ISO, CHD, or ELF into a native PC executable.

## Using the launcher

1. Install PCSX2 from its official source.
2. Dump a BIOS from your own console and configure it in PCSX2.
3. Open **Settings** in PS2 Home and scan for PCSX2, or select its executable.
4. Add the folder containing your legally obtained game images.
5. Rescan the **Library**, select a game, and press **Launch**.

The launcher passes the game path as a separate process argument, so spaces,
Unicode characters, and leading dashes in paths are preserved without shell
command construction.
