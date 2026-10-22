# Furina Cloud Gaming — Architecture & Technical Specifications

## 1. System Overview

Furina Cloud Gaming is a low-latency remote game streaming system designed to stream Windows PC gameplay directly to mobile devices (iPhone, iPad, and Android) with per-game optimized touch control overlays.

```
┌────────────────────────────────────────────────────────────────────────┐
│                        WINDOWS PC HOST (FurinaHost)                    │
│                                                                        │
│   ┌─────────────────────┐   ┌───────────────────┐  ┌───────────────┐   │
│   │   ScreenCapture     │   │   GameManager     │  │ Hardware      │   │
│   │   (GDI/DXGI Engine) │   │   & Process Watch │  │ Detection     │   │
│   └──────────┬──────────┘   └─────────┬─────────┘  └───────┬───────┘   │
│              │ (Frames)               │ (Process control)  │           │
│   ┌──────────▼────────────────────────▼────────────────────▼───────┐   │
│   │          ASP.NET Core Kestrel Host (Port 8080)                 │   │
│   │  - REST API (/api/status, /api/games, /api/pair, /api/hardware)│   │
│   │  - Binary WebSocket Stream Pipeline (/ws/stream)               │   │
│   │  - Low-Latency JSON WebSocket Input Pipeline (/ws/input)       │   │
│   └──────────────────────────────┬─────────────────────────────────┘   │
│                                  │ (Win32 SendInput)                   │
│                     ┌────────────▼───────────┐                         │
│                     │      InputInjector     │                         │
│                     │  (Keyboard, Mouse, KB) │                         │
│                     └────────────────────────┘                         │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │ LAN / Wi-Fi (WebSocket + HTTP)
┌──────────────────────────────────▼─────────────────────────────────────┐
│                 MOBILE CLIENTS (iPhone, iPad, Android)                 │
│                                                                        │
│   ┌────────────────────────────────────────────────────────────────┐   │
│   │                   Furina Web App & PWA Core                    │   │
│   │   - Canvas Rendering (createImageBitmap / WebGL Blit)          │   │
│   │   - Dual-touch event dispatch (Dynamic Joysticks + Swipes)     │   │
│   │   - Responsive HUD & Safe Area Aware Styling                   │   │
│   │   - In-Place Visual Layout Editor                              │   │
│   └──────────────────────┬─────────────────────────┬───────────────┘   │
│                          │                         │                   │
│   ┌──────────────────────▼───────┐ ┌───────────────▼───────────────┐   │
│   │     Native iOS Wrapper       │ │    Native Android Wrapper     │   │
│   │  - WKWebView / WebRTC Ready  │ │  - Android WebChromeClient    │   │
│   │  - Taptic Engine Haptics     │ │  - Immersive Sticky Fullscreen│   │
│   │  - Safari Standalone PWA     │ │  - Vibrator Feedback          │   │
│   └──────────────────────────────┘ └───────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────┘
```

## 2. Technical Stack Choices & Rationale

| Subsystem | Selected Technology | Alternative Evaluated | Why Selected |
| :--- | :--- | :--- | :--- |
| **Host Runtime** | .NET 10 (`net10.0-windows`), C# | Node.js / Electron, C++ | High performance memory handling, native Win32 P/Invoke integration, integrated Kestrel HTTP/WebSocket server, zero external runtime dependency. |
| **Stream Transport** | Binary WebSocket with 8-byte Microsecond Header | WebRTC / RTP | Immediate cross-platform compatibility across iOS Safari, Chrome, and Android without STUN/TURN complex signaling negotiation overhead on local LAN. |
| **Frame Encoding** | Hardware Accelerated JPEG / SIMD GDI Capture | H.264 NvEnc / AMF / VAAPI | Instant zero-frame pipeline buffering. H.264 requires B-frame buffering or encoder license constraints; JPEG memory streams decode in hardware via `createImageBitmap` on iOS/Android in < 2ms. |
| **Input Pipeline** | JSON over WebSocket to Win32 `SendInput` | Raw UDP | Guarantees packet delivery without dropped key-up events, preventing character walking forever; achieves sub-10ms latency on local 5GHz Wi-Fi. |
| **Mobile Client** | Responsive HTML5/ES6 PWA + Native Swift/Kotlin Wrappers | Pure React Native / Flutter | Zero App Store friction for immediate testing via iOS Safari ("Add to Home Screen"), while native Swift and Kotlin wrappers provide hardware haptics and immersive fullscreen. |
| **Input Injection** | Windows `SendInput` API with hardware scancodes | Virtual XInput / ViGEmBus | Universal compatibility across all Windows games (Roblox, GTA V, Genshin, Wuthering Waves) without requiring unsigned kernel drivers. |

## 3. Communication Protocols

### Stream Channel (`/ws/stream`)
Binary protocol with an 8-byte binary header:
- `Bytes 0-7`: 64-bit Big-Endian Unix Epoch timestamp in milliseconds.
- `Bytes 8+`: Raw encoded image frame bytes.
- This allows client-side RTT and transport latency calculation with millisecond precision.

### Input Channel (`/ws/input`)
JSON text protocol over dedicated persistent WebSocket:
```json
{
  "Type": "keyboard" | "mouse" | "mouse_relative" | "joystick" | "ping",
  "Token": "session_security_token",
  "KeyCode": 65,
  "IsDown": true,
  "DeltaX": 12,
  "DeltaY": -5,
  "JoystickX": 0.85,
  "JoystickY": -0.52
}
```

### Heartbeat & Emergency Release
If client fails to send a packet or heartbeat within 5 seconds, or if the socket disconnects, the host executes `InputInjector.ReleaseAllInputs()`, releasing all physical keys, mouse buttons, and reset virtual axes.
