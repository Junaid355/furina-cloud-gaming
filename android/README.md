# Furina Cloud Gaming — Android Client

## Overview
The Android client provides high-performance, low-latency PC game streaming with custom game overlays for Android smartphones and tablets.

### Two Delivery Methods:
1. **Direct WebApp / PWA (Instant, No Installation Needed)**:
   - Open Chrome on your Android phone or tablet.
   - Navigate to `http://<YOUR_PC_IP>:8080`.
   - Tap Chrome menu (3 dots) -> **"Add to Home screen"** or **"Install app"**.
   - Launch "Furina Cloud" from your app drawer. It launches in immersive fullscreen landscape mode with zero browser address bar, hardware decoding, and haptics.

2. **Native Android Studio Project**:
   - Open the `android/` directory in Android Studio.
   - Connect your Android phone via USB debugging.
   - Click **Run** (`Shift + F10`) or run `./gradlew assembleDebug` to build the APK (`app/build/outputs/apk/debug/app-debug.apk`).
   - Sideload directly to any Android device running Android 8.0 (API 26) through Android 15+.

## Features:
- Immersive Sticky Fullscreen (system navigation and status bars hidden).
- Hardware-accelerated WebView with full WebRTC audio/video permissions.
- Native Android `Vibrator` bridge for tactile haptic feedback.
- Automatic reconnect after network interruptions.
