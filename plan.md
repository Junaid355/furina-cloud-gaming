# Furina Cloud Gaming — Architecture & Implementation Plan

## System Architecture

Furina Cloud Gaming enables ultra-low-latency PC game streaming from a Windows host to iPhone, iPad, and Android mobile devices with game-specific customizable touch controls.

```
+------------------------------------------------------------------------+
|                            WINDOWS PC HOST                             |
|                                                                        |
|  +------------------------------------------------------------------+  |
|  | Hardware & Environment Engine                                    |  |
|  | - GPU detection (AMD AMF / DXGI / DirectX)                       |  |
|  | - Displays, resolutions, refresh rates, audio devices            |  |
|  +------------------------------------------------------------------+  |
|  | Game Discovery & Process Manager                                 |  |
|  | - Auto-detects Roblox, GTA V, Genshin, WuWa, Steam, HoYoPlay     |  |
|  | - Process lifecycle (start, focus, stop, watchdog)              |  |
|  +------------------------------------------------------------------+  |
|  | Real-Time Capture & Streaming Pipeline                            |  |
|  | - DXGI Desktop Duplication / Win32 GDI fallback capture          |  |
|  | - Audio Loopback Capture (WASAPI system audio)                   |  |
|  | - WebRTC Video & Audio Streamer (H.264 / VP8 / Opus)             |  |
|  | - Ultra-low latency WebSocket binary fallback stream             |  |
|  +------------------------------------------------------------------+  |
|  | Low-Latency Input Adapter                                        |  |
|  | - Win32 SendInput (Keyboard scancodes, mouse move/click/wheel)  |  |
|  | - Virtual joystick vector math to WASD / analog simulation       |  |
|  | - Relative mouse-look engine for 3D camera control               |  |
|  | - Disconnect auto-release safety guard (anti-stuck keys)         |  |
|  +------------------------------------------------------------------+  |
|  | Host Management Server & Pairing Gateway                         |  |
|  | - Fastify/Express HTTP + WebSocket & WebRTC Signaling           |  |
|  | - Device pairing PIN authorization & session token auth          |  |
|  | - Host desktop dashboard GUI + System tray launcher              |  |
+------------------------------------------------------------------------+
                               |    ^
               WebRTC / WS     |    |  Input DataChannel / WS
             (Video & Audio)   v    |  (Touch vectors, buttons, keys)
+------------------------------------------------------------------------+
|                   MOBILE CLIENT (iOS, iPadOS, Android)                 |
|                                                                        |
|  +------------------------------------------------------------------+  |
|  | Mobile Streaming Viewport                                        |  |
|  | - Hardware accelerated video decoding (<20ms frame latency)      |  |
|  | - Aspect ratio scaling, safe-area inset preservation             |  |
|  | - Fullscreen landscape lock with Dynamic Island / notch support  |  |
|  +------------------------------------------------------------------+  |
|  | Game-Specific Touch Control Engine                               |  |
|  | - Floating / Fixed Virtual Dynamic Joystick                      |  |
|  | - Multi-touch camera drag & swipe region                         |  |
|  | - Pressure/tap action buttons with haptic feedback               |  |
|  | - Contextual layouts (On-foot vs Vehicle for GTA V)              |  |
|  | - Experience-specific profiles (Roblox, Genshin, WuWa, GTA)     |  |
|  +------------------------------------------------------------------+  |
|  | Visual Control Editor                                            |  |
|  | - On-screen drag, resize, opacity, sensitivity slider            |  |
|  | - Binding picker (keys, mouse clicks, mouse look, macros)       |  |
|  | - Normalized coordinates (0.0 - 1.0) for cross-device sizing     |  |
|  | - Import / Export JSON profiles & Reset to defaults              |  |
|  +------------------------------------------------------------------+  |
|  | Furina Luxury Gaming UI                                          |  |
|  | - Dark midnight-blue / deep violet / silver / cyan aesthetics    |  |
|  | - Game Library, PC Status, Latency/FPS overlay, Quick Connect    |  |
|  +------------------------------------------------------------------+  |
|  | Native Packaging Targets                                         |  |
|  | - iOS/iPadOS: Mobile WebApp / PWA + Xcode Swift Container        |  |
|  | - Android: Mobile WebApp / PWA + Android Studio Kotlin Container |  |
+------------------------------------------------------------------------+
```

## Implementation Phases

### Phase 1: Environment & Foundational Host Engine
- Project directory organization in `d:\cloud\`:
  - `host/`: Windows Host service (.NET / Node.js) with Win32 Input injection, game detection, system info, and streaming server.
  - `client/`: Mobile client application (HTML5/TypeScript/CSS3 PWA) optimized for iOS Safari and Android Chrome, responsive across screen sizes.
  - `ios/`: Native iOS Xcode wrapper project configuration.
  - `android/`: Native Android project configuration.
  - `profiles/`: Default game control layouts (Roblox, GTA V, Genshin, WuWa, Minecraft, HSR, ZZZ).

### Phase 2: Windows Host Core Implementation
- Implement hardware detection: OS, GPU, resolutions, displays, audio.
- Implement game discovery: Auto-scan default install locations for Roblox, HoYoPlay (Genshin, HSR, ZZZ), Wuthering Waves, Epic Games, Steam, Rockstar Games, XboxGames.
- Implement process management: Launch game executable or URI scheme, track process ID, stop process.
- Implement Win32 input injection via native C# / PowerShell / Node ffi:
  - `SendInput` keyboard events with hardware scan codes.
  - `SendInput` mouse movement (relative delta and absolute).
  - Smooth joystick interpolation.
  - Fail-safe release mechanism when client disconnects or pauses.
- Implement desktop capture & streaming pipeline:
  - Low-latency screen streaming with WebRTC / Canvas WebSocket stream.
  - WASAPI loopback audio capture.
  - Configurable bitrate, FPS (30/60), and resolution (720p/1080p).

### Phase 3: Mobile Touch Overlay & Control System
- Normalized coordinate system (0.0 to 1.0) invariant to device DPI and aspect ratio.
- Virtual joystick with deadzone, dynamic touch centering, and return springs.
- Smooth camera look area with customizable sensitivity and acceleration.
- Multi-touch handling: simultaneous joystick run + camera pan + jump + attack.
- Tactile haptic feedback integration using standard Vibration API.
- Safe area inset handling for iPhone Dynamic Island and gesture home indicator.

### Phase 4: Game Profiles
1. **Roblox Profile**: Left floating joystick, right camera swipe area, jump, sprint, interact, first-person mouse lock toggle.
2. **GTA V Profile**: Driving mode (steering, gas, brake, handbrake, vehicle exit, horn) and On-foot mode (WASD joystick, aim, shoot, jump, sprint, enter vehicle).
3. **Genshin Impact Profile**: Movement joystick, attack, elemental skill, burst, jump, dash/sprint, character switch 1-4, pickup/interact.
4. **Wuthering Waves Profile**: Movement joystick, normal attack, dodge/dash, jump, resonance skill, resonance liberation, echo skill, switch 1-3.
5. **Minecraft (Java/Bedrock)**: D-pad / joystick, jump, sneak, inventory, destroy, place, camera swipe.

### Phase 5: Visual Controller Editor
- Interactive visual overlay editor:
  - Select any button or joystick on screen.
  - Drag handles to move, slider to resize, slider to adjust opacity.
  - Key mapping selector (A-Z, Space, Shift, Ctrl, Mouse Left/Right/Middle, etc.).
  - Save to localStorage and host sync.
  - Profile export/import as JSON.

### Phase 6: Connection, Pairing & Security
- Local Network: Auto-generated 4-digit pairing PIN on host GUI.
- Session tokens with HMAC-SHA256 authentication.
- Rate limiting and client disconnect controls.
- WebRTC STUN/TURN integration for remote access.

### Phase 7: Packaging, Release & Documentation
- Windows single-click run script and installer.
- PWA manifest for iOS "Add to Home Screen" (fullscreen, no Safari address bar, full WebRTC & touch support).
- iOS Xcode project configuration and Android Studio project structure.
- Verification and automated testing suite.
