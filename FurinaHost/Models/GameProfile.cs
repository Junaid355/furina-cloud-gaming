// d:\cloud\FurinaHost\Models\GameProfile.cs
namespace FurinaHost.Models;

public class GameProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string ExecutablePath { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public bool IsInstalled { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public StreamSettings RecommendedStreamSettings { get; set; } = new();
    public List<TouchControlElement> Controls { get; set; } = new();
    public Dictionary<string, string> InputMappings { get; set; } = new();
    public bool Verified { get; set; }
    public string VerificationNotes { get; set; } = string.Empty;
}

public class StreamSettings
{
    public int TargetFps { get; set; } = 60;
    public int TargetWidth { get; set; } = 1920;
    public int TargetHeight { get; set; } = 1080;
    public int BitrateKbps { get; set; } = 15000;
    public int Quality { get; set; } = 80; // 1-100 JPEG/WebP or WebRTC QP
    public bool EnableAudio { get; set; } = true;
    public string Codec { get; set; } = "H264";
}

public class TouchControlElement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "button"; // "joystick", "button", "camera_region", "slider", "toggle"
    public string Label { get; set; } = string.Empty;
    public string SubLabel { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double X { get; set; } // 0.0 to 1.0 (normalized)
    public double Y { get; set; } // 0.0 to 1.0 (normalized)
    public double Width { get; set; } // normalized or px ratio
    public double Height { get; set; }
    public double Opacity { get; set; } = 0.85;
    public string PrimaryKey { get; set; } = string.Empty; // e.g. "Space", "KeyE", "KeyQ", "MouseLeft"
    public string SecondaryKey { get; set; } = string.Empty;
    public string ActionMode { get; set; } = "tap"; // "tap", "hold", "toggle", "repeat"
    public int RepeatIntervalMs { get; set; } = 100;
    public double Sensitivity { get; set; } = 1.0;
    public double Deadzone { get; set; } = 0.15;
    public string ColorScheme { get; set; } = "cyan"; // "cyan", "blue", "violet", "silver", "gold"
}
