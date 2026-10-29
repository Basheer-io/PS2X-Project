# Input, Navigation, and Couch Mode

PS2 Home provides first-class keyboard navigation, an enlarged 10-foot Couch Mode for living room and TV setups, and an interactive Controller Setup Assistant for PCSX2.

## 1. Keyboard Navigation

The entire launcher can be driven via standard keyboard shortcuts without requiring a mouse:

| Key | Context | Action |
|---|---|---|
| **Arrow Keys (Up/Down/Left/Right)** | Library Game Grid | Navigate between game cards with clear visible focus rings. |
| **Enter** | Focused Game Card | Launch the highlighted game through PCSX2. |
| **Escape** | Library / Search Box | Clears active search filter or exits focus. |
| **Tab / Shift+Tab** | General Navigation | Cycles through interactive UI elements with visible high-contrast focus rings (`#00D4AA`). |

## 2. Couch Mode (10-Foot UI)

For television and couch play, PS2 Home provides an optional **Couch Mode**:
- **Enlarged Game Cards**: Expands card dimensions from 260×350px to 340×430px for high readability from across the room.
- **Larger Typography & Badges**: Increases titles, badges, and timestamps.
- **Prominent Cover Displays**: Expands artwork display area.
- **Persistence**: Couch Mode can be toggled via the `🛋️ Couch Mode` button in the Library header or saved permanently in **Settings → General & Layout**.

## 3. Controller Guidance & PCSX2 Integration

PS2 Home features a dedicated **Controllers** guidance page (`ControllersView`):
- Plain-language instructions for connecting gamepads (DualShock 4, DualSense, Xbox One/Series, Nintendo Switch Pro, and generic XInput/DirectInput controllers).
- Direct **🚀 Open PCSX2** launch button to open PCSX2's main interface.
- Official documentation links for configuring Controller Port 1, automatic button mappings, rumble motors, and analog deadzones.
- **Configuration Safety**: PS2 Home strictly avoids writing to PCSX2's `.ini` or controller configuration files directly. This avoids profile corruption, preserves custom user deadzones, and maintains compatibility across PCSX2 2.x releases.

## 4. Gamepad Input in UI (Future Work)

- **Current Status**: While PCSX2 has built-in controller drivers and natively supports controllers during gameplay, gamepad navigation within the Avalonia desktop UI is currently handled via keyboard navigation (or system controller-to-keyboard mappers like Steam Input / `antimicrox`).
- **Future Architecture**: Avalonia does not currently bundle a cross-platform gamepad input backend out-of-the-box. Adding raw SDL2 or Silk.NET dependencies would introduce heavy C runtime dependencies. Direct UI gamepad navigation is tracked as future work for a subsequent phase or optional plugin.
