# Touch Control System & Visual Layout Editor

## 1. Touch Engine Architecture

The mobile touch system (`controls.js`) is designed specifically to solve the traditional pain points of remote PC gaming:
1. **Normalized Coordinate System**: All button positions and sizes are stored as percentages (`x: 0-100`, `y: 0-100`, `width: 0-100`, `height: 0-100`). Layouts scale seamlessly from an iPhone 13 mini up to a 13-inch iPad Pro or 10-inch Android tablet.
2. **Dynamic Floating Joystick**: Automatically spawns at the exact point of the user's thumb contact on the left third of the screen, eliminating "thumb drift" common in fixed joysticks.
3. **Dedicated Camera Region**: Dedicated multi-touch drag area on the right half of the screen. Computes delta vectors `(dx, dy)` and converts them into hardware relative mouse scancode packets sent over the low-latency input WebSocket.
4. **Safe-Area Awareness**: Incorporates CSS environment variables `env(safe-area-inset-left)`, `env(safe-area-inset-right)`, and `env(safe-area-inset-top)` to ensure no interactive buttons collide with iPhone Dynamic Islands or Android gesture navigation bars.

## 2. In-Place Visual Layout Editor

Users can edit any game's layout directly on their device without opening a configuration file:

### How to Use the Editor
1. In the **Play** tab, tap the **"Edit Controls"** button in the top HUD.
2. An amber status bar will indicate **"EDIT MODE ACTIVE"**.
3. **Move a Control**: Drag any button or joystick with your finger to reposition it.
4. **Select a Control**: Tap any button to open the Property Inspector drawer.
5. **Customize Properties**:
   - Change Button Label (e.g. `Jump`, `Attack`, `E`).
   - Change Mapped Key / Mouse action (`W`, `A`, `S`, `D`, `Space`, `LeftClick`, `RightClick`, etc.).
   - Adjust Size slider (from 35px up to 120px).
   - Adjust Opacity slider (from 20% ghost mode up to 100% solid).
   - Toggle Shape (Circle vs Rounded Rectangle).
6. **Add New Controls**: Tap **"+ Add Button"** to spawn a new customizable button anywhere on screen.
7. **Export & Import**: Layouts can be exported as clean JSON strings or downloaded to a file for sharing with the community.
8. **Save Layout**: Tap **"Done Editing"** to persist the new layout to browser `localStorage` and sync with the host.

## 3. Haptic Feedback Integration

Furina Cloud Gaming supports tactile response for every button press:
- **Mobile Browsers / PWA**: Uses `navigator.vibrate(15)` for micro-haptic pulses on Android and supported mobile browsers.
- **Native iOS Wrapper**: Bridges to UIKit's `UIImpactFeedbackGenerator(style: .medium)` and `UISelectionFeedbackGenerator` for true Apple Taptic Engine feedback.
- **Native Android Wrapper**: Bridges to Android's `Vibrator` service with `VibrationEffect.createPredefined(EFFECT_CLICK)`.
