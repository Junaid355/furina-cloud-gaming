// d:\cloud\FurinaHost\Models\InputMessage.cs
namespace FurinaHost.Models;

public class InputMessage
{
    public string Type { get; set; } = string.Empty; // "key_down", "key_up", "mouse_move", "mouse_delta", "mouse_down", "mouse_up", "mouse_wheel", "joystick_vector", "emergency_stop", "heartbeat"
    public string Key { get; set; } = string.Empty; // "KeyW", "Space", "ShiftLeft", "MouseLeft", etc.
    public int KeyCode { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double DeltaX { get; set; }
    public double DeltaY { get; set; }
    public int Button { get; set; } // 0: Left, 1: Middle, 2: Right
    public int WheelDelta { get; set; }
    public long ClientTimestamp { get; set; }
    public string SessionToken { get; set; } = string.Empty;
}

public class SessionInfo
{
    public string ClientId { get; set; } = Guid.NewGuid().ToString();
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = "Mobile"; // "iPhone", "iPad", "Android", "Browser"
    public string RemoteIp { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public string Token { get; set; } = string.Empty;
    public bool IsPaired { get; set; }
    public double LatencyMs { get; set; }
    public double Fps { get; set; }
    public long BytesTransferred { get; set; }
}

public class PairingRequest
{
    public string Pin { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
}

public class PairingResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Hostname { get; set; } = Environment.MachineName;
}
