# Game Library & Profile Compatibility Matrix

## 1. Automated Host Game Discovery

`GameManager.cs` scans common game installation locations, including registry paths, `Program Files`, `Program Files (x86)`, local `AppData`, and custom user game drives (`D:\`).

### Discovered Games on Host Machine

| Game | Status on Host | Detected Path | Profile Type |
| :--- | :--- | :--- | :--- |
| **Roblox** | **Installed & Verified** | `C:\Program Files (x86)\Roblox\RobloxPlayerLauncher.exe` | Mobile Roblox (Left Joystick, Right Look, Jump, Interact) |
| **Genshin Impact** | **Installed & Verified** | `C:\Program Files\HoYoPlay\launcher.exe` | Genshin Mobile (Attack, Elemental Skill, Burst, Sprint, Swap) |
| **Wuthering Waves** | **Installed & Verified** | `D:\Wuthering Waves\launcher.exe` | WuWa Mobile (Normal Attack, Dodge, Resonance Skill, Liberation) |
| **Minecraft Bedrock** | **Installed & Verified** | `C:\Program Files\WindowsApps\...` or Standard Path | D-Pad / Touch Crosshair |
| **Grand Theft Auto V** | **Configured** | Configurable via Custom Game Adder | Driving & On-Foot Hybrid Layout |
| **Honkai: Star Rail** | **Configured** | Configurable via Custom Game Adder | Turn-based RPG Controls (Technique, Ult, Basic, Skill) |
| **Zenless Zone Zero** | **Configured** | Configurable via Custom Game Adder | Action Combat Layout (Attack, Special, Dodge, Chain) |

## 2. Default Control Profiles Detail

### 1. Roblox Mobile Profile
- **Left Joystick**: Dynamic analog floating touch joystick mapping to PC `W`, `A`, `S`, `D` with analog deadzone filtering.
- **Right Swipe Surface**: Camera rotation via relative mouse movement emulation (`SendInput MOUSEEVENTF_MOVE`).
- **Jump Button**: `Space` key scancode.
- **Interact Button**: `E` key scancode.
- **Sprint Button**: `Left Shift` toggle or hold.
- **Chat / Menu**: `Escape` or `/` key.

### 2. Grand Theft Auto V Profile
- **Left Joystick**: Steer / Walk (`W`, `A`, `S`, `D`).
- **Right Look Area**: Mouse camera control.
- **Accelerate / Brake**: `W` / `S` or `RT` / `LT`.
- **Handbrake**: `Space`.
- **Enter/Exit Vehicle**: `F`.
- **Shoot / Aim**: Left click / Right click.
- **Cover / Reload**: `Q` / `R`.

### 3. Genshin Impact Mobile Profile
- **Left Joystick**: Character movement (`W`, `A`, `S`, `D`).
- **Right Drag Region**: Camera perspective.
- **Normal Attack**: Mouse Left Click.
- **Elemental Skill**: `E` key.
- **Elemental Burst**: `Q` key.
- **Jump**: `Space`.
- **Sprint / Dash**: Mouse Right Click or `Left Shift`.
- **Character Switchers**: 1, 2, 3, 4 quick tap buttons on right column.
- **Interact / Pick Up**: `F` key.

### 4. Wuthering Waves Mobile Profile
- **Left Joystick**: Movement (`W`, `A`, `S`, `D`).
- **Right Drag Region**: Camera orbit.
- **Normal Attack**: Left Click.
- **Resonance Skill**: `E`.
- **Resonance Liberation (Ult)**: `R` / `Q`.
- **Dodge / Sprint**: `Left Shift` / Right Click.
- **Jump**: `Space`.
- **Echo Skill**: `Q`.
- **Character Switch**: Quick-select buttons for teammate 1 & 2.

## 3. Adding Custom Games

Users can add any executable directly from the mobile UI or host panel:
1. Tap **"+ Add Custom Game"** in the Library tab.
2. Enter the Game Name (e.g. `Hades II`, `Elden Ring`).
3. Provide the Executable Path (e.g. `C:\Games\HadesII\Hades2.exe`).
4. Select the base control template (Default Universal, Roblox, GTA V, Genshin, etc.).
5. The game is saved to `games.json` and immediately playable.
