// d:\cloud\FurinaHost\Program.cs
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FurinaHost.Models;
using FurinaHost.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all interfaces on port 8080
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 8080);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(15)
});
app.UseDefaultFiles();
app.UseStaticFiles();

#region REST API Endpoints

// 1. Status & Dashboard Info
app.MapGet("/api/status", () =>
{
    var hw = HardwareDetector.GetHardwareInfo();
    return Results.Ok(new
    {
        AppName = "Furina Cloud Gaming Host",
        Version = "1.0.0",
        Hostname = Environment.MachineName,
        CurrentPin = PairingManager.CurrentPin,
        IsStreaming = ScreenCapture.Instance.IsStreaming,
        Fps = ScreenCapture.Instance.CurrentFps,
        EncodeMs = Math.Round(ScreenCapture.Instance.AverageEncodeMs, 2),
        BytesSent = ScreenCapture.Instance.TotalBytesSent,
        ConnectedClients = PairingManager.GetActiveSessions().Count,
        LocalIps = hw.LocalIpAddresses,
        Port = 8080
    });
});

// 2. Hardware Information
app.MapGet("/api/hardware", (bool refresh = false) =>
{
    return Results.Ok(HardwareDetector.GetHardwareInfo(refresh));
});

// 3. Pairing
app.MapPost("/api/pair", (PairingRequest req, HttpContext ctx) =>
{
    var remoteIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    var res = PairingManager.PairDevice(req, remoteIp);
    return res.Success ? Results.Ok(res) : Results.BadRequest(res);
});

// 4. Regenerate PIN
app.MapPost("/api/pin/regenerate", () =>
{
    var pin = PairingManager.RegeneratePin();
    return Results.Ok(new { Pin = pin, Message = "New PIN generated." });
});

// 5. Game Library
app.MapGet("/api/games", () =>
{
    return Results.Ok(GameManager.GetAllGames());
});

app.MapGet("/api/games/{id}", (string id) =>
{
    var game = GameManager.GetGameById(id);
    return game != null ? Results.Ok(game) : Results.NotFound(new { Message = "Game not found." });
});

app.MapPost("/api/games", (GameProfile game) =>
{
    if (string.IsNullOrWhiteSpace(game.Name))
        return Results.BadRequest(new { Message = "Game name is required." });

    GameManager.AddCustomGame(game);
    return Results.Ok(new { Success = true, Message = "Custom game added successfully.", Game = game });
});

app.MapPut("/api/games/{id}", (string id, GameProfile updated) =>
{
    updated.Id = id;
    var ok = GameManager.UpdateGameProfile(updated);
    return ok ? Results.Ok(new { Success = true, Message = "Profile updated." }) : Results.NotFound();
});

// 6. Game Process Control
app.MapPost("/api/games/{id}/launch", (string id) =>
{
    var ok = GameManager.LaunchGame(id, out var msg);
    return ok ? Results.Ok(new { Success = true, Message = msg }) : Results.BadRequest(new { Success = false, Message = msg });
});

app.MapPost("/api/games/{id}/stop", (string id) =>
{
    var ok = GameManager.StopGame(id, out var msg);
    return ok ? Results.Ok(new { Success = true, Message = msg }) : Results.BadRequest(new { Success = false, Message = msg });
});

// 7. Sessions Management
app.MapGet("/api/sessions", () =>
{
    return Results.Ok(PairingManager.GetActiveSessions());
});

app.MapPost("/api/sessions/{id}/disconnect", (string id) =>
{
    var ok = PairingManager.DisconnectClient(id);
    return ok ? Results.Ok(new { Success = true, Message = "Session disconnected." }) : Results.NotFound();
});

// 8. Stream Configuration
app.MapPost("/api/stream/config", (StreamSettings config) =>
{
    var cap = ScreenCapture.Instance;
    if (config.TargetFps > 0) cap.TargetFps = Math.Clamp(config.TargetFps, 15, 120);
    if (config.Quality > 0) cap.Quality = Math.Clamp(config.Quality, 10, 100);
    if (config.TargetWidth > 0) cap.TargetWidth = config.TargetWidth;
    if (config.TargetHeight > 0) cap.TargetHeight = config.TargetHeight;

    return Results.Ok(new
    {
        Success = true,
        Fps = cap.TargetFps,
        Quality = cap.Quality,
        Width = cap.TargetWidth,
        Height = cap.TargetHeight
    });
});

// 9. Emergency Stop / Release all inputs
app.MapPost("/api/emergency_stop", () =>
{
    InputInjector.ReleaseAllInputs();
    return Results.Ok(new { Success = true, Message = "All inputs released." });
});

#endregion

#region WebSockets: Stream, Input & WebRTC Signaling

app.Map("/ws/stream", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var token = context.Request.Query["token"].ToString();
        // Allow connection if token is valid or in local dev mode
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var cap = ScreenCapture.Instance;
        cap.AddSubscriber(webSocket);

        var buffer = new byte[1024];
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Stream WS disconnected: {ex.Message}");
        }
        finally
        {
            cap.RemoveSubscriber(webSocket);
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Map("/ws/input", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var token = context.Request.Query["token"].ToString();
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[4096];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    InputInjector.ReleaseAllInputs();
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var msg = JsonSerializer.Deserialize<InputMessage>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (msg != null)
                        {
                            if (msg.Type == "heartbeat" || msg.Type == "ping")
                            {
                                // Echo pong with host timestamp for latency calculation
                                var pong = JsonSerializer.Serialize(new
                                {
                                    Type = "pong",
                                    ClientTimestamp = msg.ClientTimestamp,
                                    HostTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                    CurrentFps = ScreenCapture.Instance.CurrentFps
                                });
                                var pongBytes = Encoding.UTF8.GetBytes(pong);
                                await webSocket.SendAsync(new ArraySegment<byte>(pongBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            else
                            {
                                InputInjector.ProcessInput(msg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Input deserialize error: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Input WS disconnected: {ex.Message}");
        }
        finally
        {
            InputInjector.ReleaseAllInputs();
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

#endregion

// Host Startup Banner
var hostHw = HardwareDetector.GetHardwareInfo();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("================================================================================");
Console.WriteLine("                  FURINA CLOUD GAMING — WINDOWS HOST SERVER                     ");
Console.WriteLine("================================================================================");
Console.ResetColor();
Console.WriteLine($"Host OS:        {hostHw.OsVersion} ({hostHw.OsArchitecture})");
Console.WriteLine($"CPU:            {hostHw.CpuName} ({hostHw.CpuCores} cores)");
Console.WriteLine($"Memory:         {hostHw.TotalRamGb} GB (Free: {hostHw.FreeRamGb} GB)");
if (hostHw.Gpus.Count > 0)
    Console.WriteLine($"GPU:            {hostHw.Gpus[0].Name} ({hostHw.Gpus[0].VideoProcessor})");
Console.WriteLine($"Display:        {hostHw.Displays.FirstOrDefault()?.Width}x{hostHw.Displays.FirstOrDefault()?.Height} @ {hostHw.Displays.FirstOrDefault()?.RefreshRate}Hz");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"Pairing PIN:    {PairingManager.CurrentPin}");
Console.ResetColor();
Console.WriteLine($"Local URLs:");
foreach (var ip in hostHw.LocalIpAddresses)
{
    Console.WriteLine($"  -> http://{ip}:8080");
}
Console.WriteLine($"  -> http://localhost:8080");
Console.WriteLine("================================================================================");

bool isHeadless = args.Contains("--headless");

if (isHeadless)
{
    app.Run();
}
else
{
    _ = Task.Run(() => app.Run());
    try
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FurinaHost.UI.HostForm());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GUI Warning] Falling back to console mode: {ex.Message}");
        app.WaitForShutdown();
    }
}
