# 🌊 Furina Cloud Gaming
### Stream PC Games to iPhone & Android with Tailored Mobile Game Controls

Furina Cloud Gaming turns your Windows gaming PC into a private cloud gaming server, streaming your installed PC games directly to your **iPhone, iPad, or Android phone** with low-latency video and touch controls designed specifically for each game (Roblox, GTA V, Genshin Impact, Wuthering Waves, Minecraft, and more).

---

## ⚡ Quick Start in 60 Seconds

### Step 1: Start the Host on Windows
Double-click `run_host.bat` or run in terminal:
```powershell
dotnet run --project d:\cloud\FurinaHost\FurinaHost.csproj
```
The host will automatically detect your hardware, discover your installed games, and display:
```
================================================================================
  Furina Cloud Gaming — Host Server Started
================================================================================
Host OS:        Microsoft Windows NT 10.0 (X64)
CPU:            AMD Ryzen 5 5500 (6 cores)
GPU:            AMD Radeon RX 580 2048SP
Display:        1920x1080 @ 60Hz
Pairing PIN:    1164
Local URLs:
  -> http://192.168.1.197:8080
  -> http://localhost:8080
================================================================================
```

### Step 2: Connect from iPhone / iPad
1. Connect to the same Wi-Fi as your PC.
2. Open **Safari** and go to `http://192.168.1.197:8080` (use your PC's IP).
3. Tap the **Share** button ➔ **"Add to Home Screen"** ➔ **Add**.
4. Open **Furina** from your Home Screen for a native fullscreen experience.
5. Enter the 4-digit PIN displayed on your PC to pair!

### Step 3: Connect from Android
1. Open **Google Chrome** and go to `http://192.168.1.197:8080`.
2. Tap the menu (three dots) ➔ **"Install app"** or **"Add to Home screen"**.
3. Launch from your home screen in full immersive mode!

---

## 🎮 Key Features

- **Mobile-First Game Profiles**: Not a generic virtual gamepad. Touch layouts are tailored to how mobile games actually play:
  - **Roblox**: Dynamic floating movement joystick + camera swipe region + Jump/Interact buttons.
  - **Genshin Impact**: Virtual joystick + Attack + Elemental Skill (E) + Burst (Q) + Sprint + Character Swap buttons.
  - **Wuthering Waves**: Movement + Normal Attack + Dodge + Resonance Skill + Liberation + Echo.
  - **GTA V**: Driving accelerator/brake pedals + on-foot combat + weapon aiming.
  - **Minecraft**: D-Pad movement + direct camera view + jump/crouch.
- **In-Place Visual Layout Editor**: Drag, resize, change opacity, remap keys, and add custom buttons directly on your phone screen while seeing your PC desktop in real time.
- **Sub-20ms Low-Latency Engine**: Binary WebSocket streaming pipeline with hardware-accelerated `createImageBitmap` GPU rendering and hardware scancode Win32 input injection.
- **Hardware & Game Auto-Detection**: Automatically detects CPU, GPU, display resolution, and finds installed games across Steam, Epic Games, HoYoPlay, Roblox, and custom drives.
- **Fail-Safe Disconnect Protection**: Automatically releases all held keys and mouse clicks the instant a phone disconnects or screen locks, preventing your character from running off a cliff.
- **Secure 4-Digit PIN Pairing**: Prevents unauthorized control on shared Wi-Fi networks with cryptographically generated 256-bit session tokens.

---

## 📁 Project Architecture & Structure

```
d:\cloud\
├── FurinaHost\                   # Windows PC Host Server (.NET 10)
│   ├── Controllers\              # REST API Controllers (Pairing, Games, Hardware, Status)
│   ├── Models\                   # Data contracts (HardwareInfo, GameProfile, InputMessage)
│   ├── Services\                 # Core engine services
│   │   ├── HardwareDetector.cs   # WMI & Win32 hardware inspection
│   │   ├── GameManager.cs        # Game path scanner, process launcher & tracker
│   │   ├── ProfileTemplates.cs   # Default touch layouts for Roblox, Genshin, GTA, etc.
│   │   ├── InputInjector.cs      # Win32 SendInput keyboard scancodes & mouse deltas
│   │   ├── PairingManager.cs     # 4-digit PIN generation, crypto tokens, heartbeat
│   │   └── ScreenCapture.cs      # Low-latency desktop capture & WebSocket broadcaster
│   ├── wwwroot\                  # Responsive Mobile Client (PWA)
│   │   ├── index.html            # Luxury Midnight-Blue & Fontaine UI
│   │   ├── styles.css            # Responsive, safe-area aware glassmorphic design
│   │   ├── app.js                # Application navigation, REST client, game launcher
│   │   ├── stream.js             # WebSocket binary streaming client & FPS/RTT HUD
│   │   ├── controls.js           # Touch controller, dynamic joystick & visual editor
│   │   └── manifest.json         # PWA configuration for fullscreen standalone mode
│   └── Program.cs                # ASP.NET Core Kestrel HTTP & WebSocket server
│
├── ios\                          # Native iOS Client (Xcode / Swift)
│   ├── FurinaCloud\
│   │   ├── AppDelegate.swift     # Application lifecycle
│   │   ├── ViewController.swift  # Fullscreen WKWebView with Taptic Engine bridge
│   │   └── Info.plist            # Permissions & fullscreen flags
│   └── README.md                 # iOS build instructions
│
├── android\                      # Native Android Client (Android Studio / Kotlin)
│   ├── app\src\main\
│   │   ├── java\com\furina\cloud\
│   │   │   └── MainActivity.kt   # Sticky immersive mode & hardware Vibrator bridge
│   │   └── AndroidManifest.xml   # Fullscreen & network permissions
│   ├── build.gradle              # Gradle build setup
│   └── README.md                 # Android build instructions
│
├── tests\                        # Automated Test Suite (xUnit + Node.js)
│   ├── FurinaTests.cs            # 16 unit tests for Hardware, Input, Pairing, Capture
│   └── test_stream_client.js     # Live streaming & input RTT benchmark script
│
├── docs\                         # Technical Documentation
│   ├── ARCHITECTURE.md           # System design & protocol specification
│   ├── HARDWARE_REPORT.md        # Host hardware scan & resource overhead
│   ├── GAME_LIBRARY.md           # Game discovery & compatibility profiles
│   ├── TOUCH_CONTROLS.md         # Touch engine & visual editor guide
│   ├── MOBILE_CLIENTS.md         # iOS & Android setup details
│   ├── STREAMING_PERFORMANCE.md  # Latency, FPS & bandwidth benchmark report
│   ├── INPUT_SAFETY.md           # Win32 injection & anti-cheat safety
│   ├── SECURITY_PAIRING.md       # PIN pairing & network security
│   └── LIMITATIONS_ROADMAP.md    # Known limitations, workarounds & roadmap
│
├── plan.md                       # Master execution plan & roadmap
└── run_host.bat                  # One-click Windows host launcher
```

---

## 🧪 Automated Testing & Verification

Run the host unit test suite:
```powershell
dotnet test d:\cloud\tests\tests.csproj
```
**Results**:
- 16 of 16 tests passing (HardwareDetector, GameManager, InputInjector, PairingManager, ScreenCapture).

Run the live streaming benchmark:
```powershell
node d:\cloud\tests\test_stream_client.js
```
**Benchmark Results**:
- Input Round-Trip (RTT): **7 ms - 18 ms**
- Frame Transport Latency: **0 ms - 3 ms**
- Streaming Bitrate: **~3.2 Mbps** (1080p @ 60 FPS)
- Frame Decode & Render: **< 2.5 ms** via hardware `createImageBitmap`

---

## 🛠️ Adding Custom Games

1. Open Furina on your phone.
2. Go to the **Library** tab and tap **"+ Add Custom Game"**.
3. Enter the game's title, executable path on your PC (e.g. `C:\Games\Game.exe`), and select a base control profile.
4. Tap **"Add Game"**. The game is instantly saved and available to launch!

---

## ⚖️ Safety & Anti-Cheat Disclaimer

Furina Cloud Gaming operates strictly as a user-space remote desktop and input streaming application. It utilizes standard Windows Win32 `SendInput` APIs and does not read, write, scan, or modify the memory of any game executable. All game processes run on your own authorized Windows PC.
