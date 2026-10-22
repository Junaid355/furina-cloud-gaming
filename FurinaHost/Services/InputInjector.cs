// d:\cloud\FurinaHost\Services\InputInjector.cs
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FurinaHost.Models;

namespace FurinaHost.Services;

public static class InputInjector
{
    #region Win32 P/Invoke Definitions

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;

    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;
    private const uint KEYEVENTF_SCANCODE = 0x0008;

    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

    #endregion

    // Active state tracker for safety release
    private static readonly ConcurrentDictionary<ushort, bool> _activeKeys = new();
    private static readonly ConcurrentDictionary<int, bool> _activeMouseButtons = new();
    private static readonly object _inputLock = new();

    // Map common key names to Virtual-Key codes
    private static readonly Dictionary<string, ushort> KeyNameToVk = new(StringComparer.OrdinalIgnoreCase)
    {
        { "KeyW", 0x57 }, { "KeyA", 0x41 }, { "KeyS", 0x53 }, { "KeyD", 0x44 },
        { "KeyQ", 0x51 }, { "KeyE", 0x45 }, { "KeyR", 0x52 }, { "KeyF", 0x46 },
        { "KeyT", 0x54 }, { "KeyG", 0x47 }, { "KeyZ", 0x5A }, { "KeyX", 0x58 },
        { "KeyC", 0x43 }, { "KeyV", 0x56 }, { "KeyB", 0x42 }, { "KeyM", 0x4D },
        { "Digit1", 0x31 }, { "Digit2", 0x32 }, { "Digit3", 0x33 }, { "Digit4", 0x34 },
        { "Digit5", 0x35 }, { "Digit6", 0x36 }, { "Digit7", 0x37 }, { "Digit8", 0x38 },
        { "Space", 0x20 },
        { "ShiftLeft", 0xA0 }, { "ShiftRight", 0xA1 }, { "Shift", 0x10 },
        { "ControlLeft", 0xA2 }, { "ControlRight", 0xA3 }, { "Control", 0x11 },
        { "AltLeft", 0x12 }, { "AltRight", 0x12 }, { "Alt", 0x12 },
        { "Tab", 0x09 },
        { "Escape", 0x1B },
        { "Enter", 0x0D },
        { "Backspace", 0x08 },
        { "ArrowUp", 0x26 }, { "ArrowDown", 0x28 }, { "ArrowLeft", 0x25 }, { "ArrowRight", 0x27 }
    };

    public static void ProcessInput(InputMessage msg)
    {
        try
        {
            switch (msg.Type)
            {
                case "key_down":
                    HandleKeyDown(msg.Key);
                    break;
                case "key_up":
                    HandleKeyUp(msg.Key);
                    break;
                case "mouse_delta":
                    HandleMouseDelta(msg.DeltaX, msg.DeltaY);
                    break;
                case "mouse_move":
                    HandleMouseMove(msg.X, msg.Y);
                    break;
                case "mouse_down":
                    HandleMouseButton(msg.Button, true);
                    break;
                case "mouse_up":
                    HandleMouseButton(msg.Button, false);
                    break;
                case "mouse_wheel":
                    HandleMouseWheel(msg.WheelDelta);
                    break;
                case "joystick_vector":
                    HandleJoystickVector(msg.DeltaX, msg.DeltaY);
                    break;
                case "emergency_stop":
                    ReleaseAllInputs();
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Input processing error: {ex.Message}");
        }
    }

    public static void HandleKeyDown(string keyName)
    {
        if (keyName.Equals("MouseLeft", StringComparison.OrdinalIgnoreCase))
        {
            HandleMouseButton(0, true);
            return;
        }
        if (keyName.Equals("MouseRight", StringComparison.OrdinalIgnoreCase))
        {
            HandleMouseButton(2, true);
            return;
        }
        if (keyName.Equals("MouseMiddle", StringComparison.OrdinalIgnoreCase))
        {
            HandleMouseButton(1, true);
            return;
        }

        if (KeyNameToVk.TryGetValue(keyName, out var vk))
        {
            SendKey(vk, isDown: true);
            _activeKeys[vk] = true;
        }
    }

    public static void HandleKeyUp(string keyName)
    {
        if (keyName.Equals("MouseLeft", StringComparison.OrdinalIgnoreCase))
        {
            HandleMouseButton(0, false);
            return;
        }
        if (keyName.Equals("MouseRight", StringComparison.OrdinalIgnoreCase))
        {
            HandleMouseButton(2, false);
            return;
        }
        if (keyName.Equals("MouseMiddle", StringComparison.OrdinalIgnoreCase))
        {
            HandleMouseButton(1, false);
            return;
        }

        if (KeyNameToVk.TryGetValue(keyName, out var vk))
        {
            SendKey(vk, isDown: false);
            _activeKeys.TryRemove(vk, out _);
        }
    }

    public static void SendKey(ushort vk, bool isDown)
    {
        ushort scanCode = (ushort)MapVirtualKey(vk, 0); // MAPVK_VK_TO_VSC

        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = vk,
                    wScan = scanCode,
                    dwFlags = (isDown ? 0 : KEYEVENTF_KEYUP),
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        lock (_inputLock)
        {
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }
    }

    public static void HandleMouseDelta(double dx, double dy)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = (int)Math.Round(dx),
                    dy = (int)Math.Round(dy),
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        lock (_inputLock)
        {
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }
    }

    public static void HandleMouseMove(double normalizedX, double normalizedY)
    {
        // 0.0 - 1.0 mapped to 0 - 65535
        int absX = (int)Math.Clamp(normalizedX * 65535, 0, 65535);
        int absY = (int)Math.Clamp(normalizedY * 65535, 0, 65535);

        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = absX,
                    dy = absY,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        lock (_inputLock)
        {
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }
    }

    public static void HandleMouseButton(int button, bool isDown)
    {
        uint flag = 0;
        switch (button)
        {
            case 0: // Left
                flag = isDown ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP;
                break;
            case 1: // Middle
                flag = isDown ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP;
                break;
            case 2: // Right
                flag = isDown ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP;
                break;
        }

        if (flag != 0)
        {
            var input = new INPUT
            {
                type = INPUT_MOUSE,
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = flag,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            lock (_inputLock)
            {
                SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
            }

            if (isDown)
                _activeMouseButtons[button] = true;
            else
                _activeMouseButtons.TryRemove(button, out _);
        }
    }

    public static void HandleMouseWheel(int delta)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = (uint)delta,
                    dwFlags = MOUSEEVENTF_WHEEL,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        lock (_inputLock)
        {
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }
    }

    // Joystick 2D vector to WASD keys
    private static bool _joyW, _joyA, _joyS, _joyD;
    public static void HandleJoystickVector(double x, double y)
    {
        const double deadzone = 0.20;
        bool wantW = y < -deadzone;
        bool wantS = y > deadzone;
        bool wantA = x < -deadzone;
        bool wantD = x > deadzone;

        ushort vkW = 0x57;
        ushort vkA = 0x41;
        ushort vkS = 0x53;
        ushort vkD = 0x44;

        if (wantW != _joyW)
        {
            SendKey(vkW, wantW);
            _joyW = wantW;
            if (wantW) _activeKeys[vkW] = true; else _activeKeys.TryRemove(vkW, out _);
        }
        if (wantS != _joyS)
        {
            SendKey(vkS, wantS);
            _joyS = wantS;
            if (wantS) _activeKeys[vkS] = true; else _activeKeys.TryRemove(vkS, out _);
        }
        if (wantA != _joyA)
        {
            SendKey(vkA, wantA);
            _joyA = wantA;
            if (wantA) _activeKeys[vkA] = true; else _activeKeys.TryRemove(vkA, out _);
        }
        if (wantD != _joyD)
        {
            SendKey(vkD, wantD);
            _joyD = wantD;
            if (wantD) _activeKeys[vkD] = true; else _activeKeys.TryRemove(vkD, out _);
        }
    }

    /// <summary>
    /// Safety release: Releases all held keys and mouse buttons.
    /// Fires on disconnect, stream pause, or emergency stop.
    /// </summary>
    public static void ReleaseAllInputs()
    {
        lock (_inputLock)
        {
            // Release all active keys
            foreach (var vk in _activeKeys.Keys.ToArray())
            {
                SendKey(vk, isDown: false);
                _activeKeys.TryRemove(vk, out _);
            }

            // Release joystick keys if held
            if (_joyW) { SendKey(0x57, false); _joyW = false; }
            if (_joyA) { SendKey(0x41, false); _joyA = false; }
            if (_joyS) { SendKey(0x53, false); _joyS = false; }
            if (_joyD) { SendKey(0x44, false); _joyD = false; }

            // Release all active mouse buttons
            foreach (var btn in _activeMouseButtons.Keys.ToArray())
            {
                HandleMouseButton(btn, false);
                _activeMouseButtons.TryRemove(btn, out _);
            }
        }
        Debug.WriteLine("InputInjector: All inputs released successfully.");
    }
}
