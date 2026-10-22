# Mobile Client Implementation Guide: iOS & Android

Furina Cloud Gaming provides dual deployment paths for both iOS and Android:
1. **Immediate Zero-Install PWA (Progressive Web App)**: Runs directly in Safari or Chrome, installable to Home Screen in 5 seconds with zero compilation or Apple Developer account required.
2. **Native Mobile Shells (Swift / Kotlin)**: Full native projects providing immersive fullscreen, hardware Taptic feedback, and background connection persistence.

---

## 1. iPhone & iPad Client Setup

### Option A: iOS Safari PWA (Recommended for Instant Setup)
1. Ensure your iPhone or iPad is connected to the same Wi-Fi network as your PC.
2. Open **Safari** on your iOS device.
3. Navigate to: `http://192.168.1.197:8080` (or your host's local IP).
4. Tap the **Share** button (the square with an arrow pointing upward).
5. Scroll down and tap **"Add to Home Screen"**.
6. Name it **"Furina"** and tap **Add**.
7. Launch Furina from your iOS Home Screen. It runs as a standalone, fullscreen app with no browser URL bar or navigation buttons.

### Option B: Native iOS Xcode Wrapper (`d:\cloud\ios\`)
- **Location**: `d:\cloud\ios\FurinaCloud\`
- **Key Files**:
  - `AppDelegate.swift`: Application lifecycle and window configuration.
  - `ViewController.swift`: Fullscreen `WKWebView` with hardware WebRTC, WebGL, and Taptic feedback bridge.
  - `Info.plist`: Configures `NSLocalNetworkUsageDescription`, `NSMicrophoneUsageDescription`, and `UIRequiresFullScreen`.
- **How to Build**:
  1. Open Xcode on macOS.
  2. Create an iOS App project or import the `d:\cloud\ios\` directory.
  3. Ensure your bundle ID is set and sign with your Apple ID or Team.
  4. Build and run on your connected iPhone or iPad.

---

## 2. Android Client Setup

### Option A: Android Chrome PWA (Instant Setup)
1. Connect your Android phone or tablet to the same Wi-Fi network.
2. Open **Google Chrome**.
3. Browse to `http://192.168.1.197:8080`.
4. Tap the three dots (menu) in the top-right corner.
5. Tap **"Install app"** or **"Add to Home screen"**.
6. Launch Furina from your Android App Drawer in full immersive mode.

### Option B: Native Android Studio Wrapper (`d:\cloud\android\`)
- **Location**: `d:\cloud\android\`
- **Key Files**:
  - `app/src/main/java/com/furina/cloud/MainActivity.kt`: Sets up sticky immersive fullscreen mode (`WindowInsetsControllerCompat.BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE`), disables screen dimming/sleep (`FLAG_KEEP_SCREEN_ON`), and injects Android `Vibrator` feedback.
  - `app/src/main/AndroidManifest.xml`: Permissions for `INTERNET`, `ACCESS_NETWORK_STATE`, and `VIBRATE`.
  - `build.gradle` & `app/build.gradle`: Standard Gradle configuration for SDK 24 to 34.
- **How to Build**:
  1. Open Android Studio.
  2. Open directory `d:\cloud\android`.
  3. Connect your Android phone via USB with USB Debugging enabled.
  4. Click **Run 'app'**.
