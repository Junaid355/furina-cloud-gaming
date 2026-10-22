// d:\cloud\FurinaHost\Services\GameManager.cs
using System.Diagnostics;
using System.Text.Json;
using FurinaHost.Models;

namespace FurinaHost.Services;

public class GameManager
{
    private static readonly string ProfilesDir = Path.Combine(AppContext.BaseDirectory, "profiles");
    private static readonly string CustomGamesFile = Path.Combine(AppContext.BaseDirectory, "custom_games.json");
    private static readonly List<GameProfile> _games = new();
    private static readonly ConcurrentProcessTracker _processTracker = new();
    private static readonly object _lock = new();

    static GameManager()
    {
        InitializeGameCatalog();
    }

    public static List<GameProfile> GetAllGames()
    {
        lock (_lock)
        {
            ScanInstalledStatus();
            return _games.ToList();
        }
    }

    public static GameProfile? GetGameById(string id)
    {
        lock (_lock)
        {
            return _games.FirstOrDefault(g => g.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static bool AddCustomGame(GameProfile game)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(game.Id))
                game.Id = Guid.NewGuid().ToString();

            game.IsInstalled = File.Exists(game.ExecutablePath) || Directory.Exists(game.ExecutablePath);
            _games.RemoveAll(g => g.Id == game.Id);
            _games.Add(game);
            SaveCustomGames();
            return true;
        }
    }

    public static bool UpdateGameProfile(GameProfile updated)
    {
        lock (_lock)
        {
            var idx = _games.FindIndex(g => g.Id == updated.Id);
            if (idx >= 0)
            {
                _games[idx] = updated;
                SaveCustomGames();
                return true;
            }
            return false;
        }
    }

    public static bool LaunchGame(string id, out string message)
    {
        lock (_lock)
        {
            var game = _games.FirstOrDefault(g => g.Id == id);
            if (game == null)
            {
                message = "Game not found in library.";
                return false;
            }

            if (string.IsNullOrEmpty(game.ExecutablePath) && !game.ExecutablePath.StartsWith("minecraft:", StringComparison.OrdinalIgnoreCase))
            {
                message = "Executable path is not configured.";
                return false;
            }

            try
            {
                // URI Scheme check (e.g. minecraft:)
                if (game.ExecutablePath.Contains("://") || game.ExecutablePath.EndsWith(":"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = game.ExecutablePath,
                        UseShellExecute = true
                    });
                    message = $"Launched {game.Name} via URI protocol.";
                    return true;
                }

                if (!File.Exists(game.ExecutablePath))
                {
                    message = $"Executable not found at path: {game.ExecutablePath}";
                    return false;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = game.ExecutablePath,
                    Arguments = game.Arguments,
                    WorkingDirectory = !string.IsNullOrEmpty(game.WorkingDirectory) && Directory.Exists(game.WorkingDirectory)
                        ? game.WorkingDirectory
                        : Path.GetDirectoryName(game.ExecutablePath) ?? "",
                    UseShellExecute = true
                };

                var proc = Process.Start(psi);
                if (proc != null)
                {
                    _processTracker.TrackProcess(game.Id, proc.Id);
                    message = $"Successfully launched {game.Name} (PID: {proc.Id}).";
                    return true;
                }

                message = $"Failed to start process for {game.Name}.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Error launching {game.Name}: {ex.Message}";
                return false;
            }
        }
    }

    public static bool StopGame(string id, out string message)
    {
        if (_processTracker.TryKill(id, out var msg))
        {
            message = msg;
            return true;
        }

        // Also check if process name is running
        var game = GetGameById(id);
        if (game != null && !string.IsNullOrEmpty(game.ProcessName))
        {
            try
            {
                var procs = Process.GetProcessesByName(game.ProcessName.Replace(".exe", ""));
                if (procs.Length > 0)
                {
                    foreach (var p in procs)
                    {
                        p.Kill();
                    }
                    message = $"Terminated {procs.Length} instance(s) of {game.Name}.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = $"Failed to kill process: {ex.Message}";
                return false;
            }
        }

        message = "No active running process found for this game.";
        return false;
    }

    private static void InitializeGameCatalog()
    {
        _games.Clear();

        // 1. Roblox
        _games.Add(new GameProfile
        {
            Id = "roblox",
            Name = "Roblox",
            Category = "Action / Sandbox",
            IconUrl = "/assets/icons/roblox.png",
            ProcessName = "RobloxPlayerBeta.exe",
            VerificationNotes = "DirectX Raw Input mapping verified for desktop client.",
            RecommendedStreamSettings = new StreamSettings { TargetFps = 60, TargetWidth = 1920, TargetHeight = 1080, BitrateKbps = 12000, Quality = 85 },
            Controls = ProfileTemplates.GetRobloxControls()
        });

        // 2. Grand Theft Auto V
        _games.Add(new GameProfile
        {
            Id = "gtav",
            Name = "Grand Theft Auto V",
            Category = "Action / Open World",
            IconUrl = "/assets/icons/gtav.png",
            ProcessName = "GTA5.exe",
            VerificationNotes = "Dual-state layout (On-foot / Driving) with vehicle toggle.",
            RecommendedStreamSettings = new StreamSettings { TargetFps = 60, TargetWidth = 1920, TargetHeight = 1080, BitrateKbps = 18000, Quality = 85 },
            Controls = ProfileTemplates.GetGtaVControls()
        });

        // 3. Genshin Impact
        _games.Add(new GameProfile
        {
            Id = "genshin",
            Name = "Genshin Impact",
            Category = "Action RPG",
            IconUrl = "/assets/icons/genshin.png",
            ProcessName = "GenshinImpact.exe",
            VerificationNotes = "Mobile-inspired layout with Elemental Skill (E), Burst (Q), Sprint (Shift), Jump (Space).",
            RecommendedStreamSettings = new StreamSettings { TargetFps = 60, TargetWidth = 1920, TargetHeight = 1080, BitrateKbps = 16000, Quality = 85 },
            Controls = ProfileTemplates.GetGenshinControls()
        });

        // 4. Wuthering Waves
        _games.Add(new GameProfile
        {
            Id = "wuwa",
            Name = "Wuthering Waves",
            Category = "Action RPG",
            IconUrl = "/assets/icons/wuwa.png",
            ProcessName = "Client-Win64-Shipping.exe",
            VerificationNotes = "Combat layout with Resonance Skill (E), Liberation (R), Echo Skill (Q), Dodge (Shift).",
            RecommendedStreamSettings = new StreamSettings { TargetFps = 60, TargetWidth = 1920, TargetHeight = 1080, BitrateKbps = 16000, Quality = 85 },
            Controls = ProfileTemplates.GetWuWaControls()
        });

        // 5. Minecraft Java Edition
        _games.Add(new GameProfile
        {
            Id = "minecraft_java",
            Name = "Minecraft Java Edition",
            Category = "Sandbox",
            IconUrl = "/assets/icons/minecraft_java.png",
            ProcessName = "javaw.exe",
            VerificationNotes = "Full keyboard + camera drag and left/right click.",
            RecommendedStreamSettings = new StreamSettings { TargetFps = 60, TargetWidth = 1920, TargetHeight = 1080, BitrateKbps = 14000, Quality = 80 },
            Controls = ProfileTemplates.GetMinecraftControls()
        });

        // 6. Minecraft Bedrock Edition
        _games.Add(new GameProfile
        {
            Id = "minecraft_bedrock",
            Name = "Minecraft Bedrock Edition",
            Category = "Sandbox",
            ExecutablePath = "minecraft:",
            IconUrl = "/assets/icons/minecraft_bedrock.png",
            ProcessName = "Minecraft.Windows.exe",
            VerificationNotes = "Windows UWP application, launched via minecraft: URI.",
            RecommendedStreamSettings = new StreamSettings { TargetFps = 60, TargetWidth = 1920, TargetHeight = 1080, BitrateKbps = 14000, Quality = 80 },
            Controls = ProfileTemplates.GetMinecraftControls()
        });

        // 7. Honkai: Star Rail
        _games.Add(new GameProfile
        {
            Id = "hsr",
            Name = "Honkai: Star Rail",
            Category = "Turn-Based RPG",
            IconUrl = "/assets/icons/hsr.png",
            ProcessName = "StarRail.exe",
            VerificationNotes = "Exploration joystick and interaction keys.",
            RecommendedStreamSettings = new StreamSettings { TargetFps = 60, TargetWidth = 1920, TargetHeight = 1080, BitrateKbps = 14000, Quality = 85 },
            Controls = ProfileTemplates.GetHsrControls()
        });

        // 8. Zenless Zone Zero
        _games.Add(new GameProfile
        {
            Id = "zzz",
            Name = "Zenless Zone Zero",
            Category = "Action",
            IconUrl = "/assets/icons/zzz.png",
            ProcessName = "ZenlessZoneZero.exe",
            VerificationNotes = "Fast combat layout with Basic Attack, Special, Dodge, Switch.",
            RecommendedStreamSettings = new StreamSettings { TargetFps = 60, TargetWidth = 1920, TargetHeight = 1080, BitrateKbps = 16000, Quality = 85 },
            Controls = ProfileTemplates.GetZzzControls()
        });

        LoadCustomGames();
        ScanInstalledStatus();
    }

    private static void ScanInstalledStatus()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        foreach (var g in _games)
        {
            switch (g.Id)
            {
                case "roblox":
                    // Check Roblox
                    string robloxProg = Path.Combine(progFilesX86, "Roblox", "Versions");
                    string robloxApp = Path.Combine(localAppData, "Roblox", "Versions");
                    var foundRoblox = SearchExe(robloxProg, "RobloxPlayerBeta.exe") ?? SearchExe(robloxApp, "RobloxPlayerBeta.exe");
                    if (foundRoblox != null)
                    {
                        g.ExecutablePath = foundRoblox;
                        g.IsInstalled = true;
                    }
                    else if (Directory.Exists(Path.Combine(progFilesX86, "Roblox")))
                    {
                        g.IsInstalled = true;
                        g.ExecutablePath = Path.Combine(progFilesX86, "Roblox", "RobloxPlayerLauncher.exe");
                    }
                    break;

                case "gtav":
                    string[] gtaPaths = {
                        Path.Combine(progFiles, "Rockstar Games", "Grand Theft Auto V", "GTA5.exe"),
                        Path.Combine(progFiles, "Epic Games", "GTAV", "GTA5.exe"),
                        @"D:\SteamLibrary\steamapps\common\Grand Theft Auto V\GTA5.exe",
                        @"D:\Games\Grand Theft Auto V\GTA5.exe"
                    };
                    foreach (var p in gtaPaths)
                    {
                        if (File.Exists(p))
                        {
                            g.ExecutablePath = p;
                            g.IsInstalled = true;
                            break;
                        }
                    }
                    break;

                case "genshin":
                    string[] genshinPaths = {
                        Path.Combine(progFiles, "HoYoPlay", "games", "Genshin Impact Game", "GenshinImpact.exe"),
                        Path.Combine(progFiles, "Genshin Impact", "Genshin Impact Game", "GenshinImpact.exe"),
                        @"C:\Program Files\Genshin Impact\launcher.exe",
                        @"D:\Games\Genshin Impact\Genshin Impact Game\GenshinImpact.exe",
                        Path.Combine(progFiles, "HoYoPlay", "launcher.exe")
                    };
                    foreach (var p in genshinPaths)
                    {
                        if (File.Exists(p))
                        {
                            g.ExecutablePath = p;
                            g.IsInstalled = true;
                            break;
                        }
                    }
                    break;

                case "wuwa":
                    string[] wuwaPaths = {
                        @"D:\Wuthering Waves\Wuthering Waves Game\Client\Binaries\Win64\Client-Win64-Shipping.exe",
                        @"D:\Wuthering Waves\launcher.exe",
                        Path.Combine(progFiles, "Wuthering Waves", "launcher.exe"),
                        Path.Combine(progFiles, "Wuthering Waves", "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")
                    };
                    foreach (var p in wuwaPaths)
                    {
                        if (File.Exists(p))
                        {
                            g.ExecutablePath = p;
                            g.IsInstalled = true;
                            break;
                        }
                    }
                    break;

                case "minecraft_bedrock":
                    g.IsInstalled = true; // URI protocol standard on Windows
                    break;

                default:
                    if (!string.IsNullOrEmpty(g.ExecutablePath) && File.Exists(g.ExecutablePath))
                    {
                        g.IsInstalled = true;
                    }
                    break;
            }
        }
    }

    private static string? SearchExe(string dir, string exeName)
    {
        try
        {
            if (!Directory.Exists(dir)) return null;
            var files = Directory.GetFiles(dir, exeName, SearchOption.AllDirectories);
            return files.Length > 0 ? files[0] : null;
        }
        catch
        {
            return null;
        }
    }

    private static void LoadCustomGames()
    {
        try
        {
            if (File.Exists(CustomGamesFile))
            {
                var json = File.ReadAllText(CustomGamesFile);
                var customs = JsonSerializer.Deserialize<List<GameProfile>>(json);
                if (customs != null)
                {
                    foreach (var c in customs)
                    {
                        _games.RemoveAll(x => x.Id == c.Id);
                        _games.Add(c);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load custom games: {ex.Message}");
        }
    }

    private static void SaveCustomGames()
    {
        try
        {
            var customs = _games.Where(g => g.Id.StartsWith("custom_") || !IsDefaultGame(g.Id)).ToList();
            var json = JsonSerializer.Serialize(customs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CustomGamesFile, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save custom games: {ex.Message}");
        }
    }

    private static bool IsDefaultGame(string id) =>
        id is "roblox" or "gtav" or "genshin" or "wuwa" or "minecraft_java" or "minecraft_bedrock" or "hsr" or "zzz";
}

public class ConcurrentProcessTracker
{
    private readonly Dictionary<string, int> _trackedProcesses = new();
    private readonly object _lock = new();

    public void TrackProcess(string gameId, int pid)
    {
        lock (_lock)
        {
            _trackedProcesses[gameId] = pid;
        }
    }

    public bool TryKill(string gameId, out string message)
    {
        lock (_lock)
        {
            if (_trackedProcesses.TryGetValue(gameId, out var pid))
            {
                try
                {
                    var proc = Process.GetProcessById(pid);
                    proc.Kill();
                    _trackedProcesses.Remove(gameId);
                    message = $"Terminated process (PID: {pid}).";
                    return true;
                }
                catch (Exception ex)
                {
                    _trackedProcesses.Remove(gameId);
                    message = $"Process PID {pid} was not running: {ex.Message}";
                    return false;
                }
            }
            message = "No process tracked for this game ID.";
            return false;
        }
    }
}
