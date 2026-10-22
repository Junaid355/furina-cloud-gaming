// d:\cloud\FurinaHost\Services\PairingManager.cs
using System.Collections.Concurrent;
using System.Security.Cryptography;
using FurinaHost.Models;

namespace FurinaHost.Services;

public class PairingManager
{
    private static string _currentPin = GeneratePin();
    private static readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
    private static readonly ConcurrentDictionary<string, string> _pairedTokens = new(); // Token -> ClientId
    private static readonly object _lock = new();

    public static string CurrentPin
    {
        get
        {
            lock (_lock) return _currentPin;
        }
    }

    public static string RegeneratePin()
    {
        lock (_lock)
        {
            _currentPin = GeneratePin();
            return _currentPin;
        }
    }

    public static PairingResponse PairDevice(PairingRequest req, string remoteIp)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(req.Pin) || req.Pin != _currentPin)
            {
                return new PairingResponse
                {
                    Success = false,
                    Message = "Invalid PIN. Check the PIN shown on your Windows PC screen."
                };
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var session = new SessionInfo
            {
                ClientId = Guid.NewGuid().ToString("N")[..8],
                DeviceName = string.IsNullOrWhiteSpace(req.DeviceName) ? "Mobile Device" : req.DeviceName,
                DeviceType = string.IsNullOrWhiteSpace(req.DeviceType) ? "Mobile" : req.DeviceType,
                RemoteIp = remoteIp,
                Token = token,
                IsPaired = true,
                ConnectedAt = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow
            };

            _sessions[session.ClientId] = session;
            _pairedTokens[token] = session.ClientId;

            return new PairingResponse
            {
                Success = true,
                Token = token,
                Hostname = Environment.MachineName,
                Message = "Pairing successful. Connected to Furina Cloud Host."
            };
        }
    }

    public static bool ValidateToken(string? token, out SessionInfo? session)
    {
        session = null;
        if (string.IsNullOrEmpty(token)) return false;

        if (_pairedTokens.TryGetValue(token, out var clientId) && _sessions.TryGetValue(clientId, out var s))
        {
            s.LastHeartbeat = DateTime.UtcNow;
            session = s;
            return true;
        }

        return false;
    }

    public static List<SessionInfo> GetActiveSessions()
    {
        // Cleanup stale sessions (> 30s no heartbeat)
        var now = DateTime.UtcNow;
        foreach (var kvp in _sessions)
        {
            if ((now - kvp.Value.LastHeartbeat).TotalSeconds > 30)
            {
                DisconnectClient(kvp.Key);
            }
        }
        return _sessions.Values.ToList();
    }

    public static bool DisconnectClient(string clientId)
    {
        if (_sessions.TryRemove(clientId, out var s))
        {
            _pairedTokens.TryRemove(s.Token, out _);
            // Safety release all inputs if this was an active controller
            InputInjector.ReleaseAllInputs();
            return true;
        }
        return false;
    }

    private static string GeneratePin()
    {
        return RandomNumberGenerator.GetInt32(1000, 10000).ToString();
    }
}
