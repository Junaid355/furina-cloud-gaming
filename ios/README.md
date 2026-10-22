# Furina Cloud Gaming — iOS & iPadOS Client

## Overview
The iOS and iPadOS client provides native, zero-compromise low-latency game streaming from your Windows PC with customized touch overlays.

### Two Delivery Methods:
1. **Direct WebApp / PWA (Instant, Zero Sideloading Required)**:
   - Open Safari on iPhone or iPad.
   - Navigate to `http://<YOUR_PC_IP>:8080`.
   - Tap the Share button in Safari -> **"Add to Home Screen"**.
   - Launch "Furina Cloud" from your home screen. It runs in full-screen standalone mode without any browser URL bar, with full WebRTC hardware video decoding, touch events, and haptic feedback.

2. **Native Xcode Project (`FurinaCloud.xcodeproj`)**:
   - Open `ios/FurinaCloud.xcodeproj` in Xcode on macOS.
   - Select your connected iPhone or iPad.
   - Choose your free Apple ID Team in **Signing & Capabilities**.
   - Build and Run (`Cmd + R`).
   - Sideloading via AltStore, Sideloadly, or TrollStore is also fully supported using the built `.ipa` file.

## Features:
- Native Taptic Engine haptics.
- Safe Area Inset compensation for iPhone Dynamic Island and gesture home bar.
- Low-latency touch event pipeline with sub-5ms input transmission.
- Hardware-accelerated canvas/video decoding.
