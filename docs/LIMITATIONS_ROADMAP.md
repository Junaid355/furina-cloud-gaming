# Known Limitations, Practical Workarounds & Future Roadmap

## 1. Current Known Platform Limitations & Workarounds

| Limitation | Impact | Cause | Practical Workaround |
| :--- | :--- | :--- | :--- |
| **Windows Lock Screen / Headless Mode** | GDI desktop capture stops returning frames when PC is locked or display is turned off. | Windows Desktop Window Manager (DWM) pauses compositing when the session is locked or disconnected. | Leave host PC logged in to desktop, or use a cheap HDMI dummy plug ($5 on Amazon) if running PC without a physical monitor. |
| **UAC (User Account Control) Elevation Dialogs** | Mobile input cannot click "Yes" on Windows administrator elevation prompts. | Windows security prevents standard user processes from injecting input into elevated Secure Desktop windows. | Run `FurinaHost` as Administrator (`run_host.bat` -> Run as Administrator) or launch games before switching to mobile. |
| **Exclusive Fullscreen Mode in Older DirectX 9/11 Games** | Some older games capture the mouse exclusively and do not update desktop compositing. | Exclusive Fullscreen bypasses DWM desktop rendering. | Configure games to run in **"Borderless Windowed"** or **"Windowed Fullscreen"** mode in their graphics settings. |
| **iOS Safari Audio Autoplay Policy** | Audio will not play automatically until the user interacts with the page. | Apple Safari security policy blocks automatic sound playback without a user gesture. | Tap the **"Unmute Audio"** button in the mobile HUD once when starting a stream session. |
| **Cellular NAT Traversal** | Cannot connect over cellular 4G/5G without local network routing. | Carrier-Grade NAT (CGNAT) blocks inbound port connections to home routers. | Use **Tailscale** or **ZeroTier** for seamless, zero-config peer-to-peer encrypted mesh networking anywhere in the world. |

---

## 2. Technical Roadmap

### Phase 2: Native NVENC / AMF Hardware Video Pipeline
- Integrate Direct3D 11 Desktop Duplication API (DXGI) directly with FFmpeg / NvEnc hardware libraries for native H.264 / HEVC video encoding.
- Implement low-latency WebRTC data channels for unified video, audio, and input multiplexing.

### Phase 3: Virtual Gamepad Driver (ViGEmClient)
- Integrate the open-source ViGEmBus driver to allow mobile touch controls to emulate native Xbox 360 and DualShock 4 controllers directly in Windows.
- Enables native controller rumble and full 360-degree analog trigger support in racing and sports games.

### Phase 4: Cloud Layout Repository
- Allow users to browse and download community-created touch layouts for hundreds of popular Steam, Epic, and indie PC games.
- Enable one-tap layout sharing via QR codes and cloud sync.
