# Input Injection, Anti-Cheat Interaction & Safety Disconnect Guard

## 1. Win32 Input Injection Mechanism

Furina Cloud Gaming translates mobile touch actions into native Windows OS input events using the official Microsoft Win32 `SendInput` API.

### Input Types Handled
1. **Keyboard Keys (WASD, Space, E, Q, Shift, etc.)**:
   - Dispatched with physical hardware scan codes (`KEYEVENTF_SCANCODE`).
   - Ensures compatibility with games running in raw input or DirectInput mode that ignore standard virtual key codes.
2. **Relative Mouse Movements (Camera Look / Aiming)**:
   - Dispatched with `MOUSEEVENTF_MOVE`.
   - Supports 360-degree free look in Roblox, GTA V, Genshin Impact, and Minecraft without being bounded by desktop screen borders.
3. **Mouse Clicks (Attack, Aim, Drag)**:
   - Dispatched with `MOUSEEVENTF_LEFTDOWN`, `MOUSEEVENTF_LEFTUP`, `MOUSEEVENTF_RIGHTDOWN`, and `MOUSEEVENTF_RIGHTUP`.
4. **Analog Joystick Emulation**:
   - Converts 2D mobile joystick vectors `(x, y)` into precise 8-direction WASD keyboard pulses with analog deadband filtering (`Deadzone = 0.15`).

## 2. Anti-Cheat Software Compatibility

Modern games employ various anti-cheat solutions. Here is the assessed operational profile for each supported game:

| Game | Anti-Cheat Solution | SendInput Compatibility | Notes / Best Practices |
| :--- | :--- | :--- | :--- |
| **Roblox** | Hyperion (Byfron) | Fully Compatible | Works natively; input events arrive as standard hardware keyboard/mouse interrupts. |
| **Genshin Impact** | HoYoKProtect (mhyprot2) | Fully Compatible | Furina runs as a standard user process and does not hook memory or inject DLLs into the game. |
| **Wuthering Waves** | ACE (Anti-Cheat Expert) | Fully Compatible | Standard input stream accepted without flag. |
| **Grand Theft Auto V** | BattlEye / Rockstar Social | Fully Compatible | Single-player and standard online accept Win32 input without driver hooks. |
| **Minecraft (Bedrock / Java)** | Standard Sandbox | Fully Compatible | DirectInput mouse and keyboard fully responsive. |

> **Important Safety Note**: Furina Cloud Gaming is purely a remote desktop and input streaming tool. It does **not** read, write, scan, or patch the memory of any game executable. All inputs are standard user-level OS input events.

## 3. Disconnection Safety Guard (`ReleaseAllInputs`)

A frequent issue with remote desktop tools is the "stuck key" glitch (e.g. your character keeps running forward after the phone screen turns off).

Furina implements a strict fail-safe guard:
1. **Socket Drop Event**: The instant the `/ws/input` socket disconnects or closes, `InputInjector.ReleaseAllInputs()` is automatically triggered.
2. **Heartbeat Timeout**: If no input packet or ping is received for **5 seconds**, the host automatically assumes a dropped connection and executes `ReleaseAllInputs()`.
3. **Host Safety Button**: The host web dashboard includes an instant **"Release All Inputs"** button that clears all active keys and mouse buttons immediately.
