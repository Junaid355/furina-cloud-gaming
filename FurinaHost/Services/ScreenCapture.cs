// d:\cloud\FurinaHost\Services\ScreenCapture.cs
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using FurinaHost.Models;

namespace FurinaHost.Services;

public class ScreenCapture : IDisposable
{
    private static ScreenCapture? _instance;
    private static readonly object _instanceLock = new();

    private readonly List<WebSocket> _subscribers = new();
    private readonly object _subLock = new();
    private CancellationTokenSource? _cts;
    private Task? _captureTask;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    public int TargetFps { get; set; } = 60;
    public int Quality { get; set; } = 80;
    public int TargetWidth { get; set; } = 1920;
    public int TargetHeight { get; set; } = 1080;
    public bool IsStreaming => _captureTask != null && !_captureTask.IsCompleted;

    public double CurrentFps { get; private set; }
    public double AverageEncodeMs { get; private set; }
    public long TotalBytesSent { get; private set; }
    private long _totalFrames = 0;

    private readonly ImageCodecInfo _jpegEncoder;
    private readonly EncoderParameters _encoderParams;

    public static ScreenCapture Instance
    {
        get
        {
            lock (_instanceLock)
            {
                return _instance ??= new ScreenCapture();
            }
        }
    }

    private ScreenCapture()
    {
        _jpegEncoder = GetEncoder(ImageFormat.Jpeg);
        _encoderParams = new EncoderParameters(1);
        _encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)Quality);
    }

    public void StartStream()
    {
        lock (_instanceLock)
        {
            if (IsStreaming) return;

            Console.WriteLine("[ScreenCapture] Starting screen capture background loop...");
            _cts = new CancellationTokenSource();
            _captureTask = Task.Run(() => CaptureLoop(_cts.Token));
        }
    }

    public void StopStream()
    {
        lock (_instanceLock)
        {
            _cts?.Cancel();
            _captureTask = null;
            Debug.WriteLine("ScreenCapture: Stream stopped.");
        }
    }

    public void AddSubscriber(WebSocket socket)
    {
        lock (_subLock)
        {
            if (!_subscribers.Contains(socket))
            {
                _subscribers.Add(socket);
                Console.WriteLine($"[ScreenCapture] Added subscriber. Total subscribers: {_subscribers.Count}");
                StartStream();
            }
        }
    }

    public void RemoveSubscriber(WebSocket socket)
    {
        lock (_subLock)
        {
            _subscribers.Remove(socket);
            if (_subscribers.Count == 0)
            {
                // Can keep streaming or pause
            }
        }
    }

    private async Task CaptureLoop(CancellationToken ct)
    {
        int frameCount = 0;
        var fpsStopwatch = Stopwatch.StartNew();
        var frameInterval = TimeSpan.FromMilliseconds(1000.0 / TargetFps);
        var encodeTimer = new Stopwatch();

        while (!ct.IsCancellationRequested)
        {
            var loopStart = Stopwatch.GetTimestamp();

            try
            {
                byte[]? frameData = null;
                encodeTimer.Restart();

                // Determine Display Bounds
                int screenW = GetSystemMetrics(SM_CXSCREEN);
                int screenH = GetSystemMetrics(SM_CYSCREEN);
                if (screenW <= 0) screenW = Screen.PrimaryScreen?.Bounds.Width ?? 1920;
                if (screenH <= 0) screenH = Screen.PrimaryScreen?.Bounds.Height ?? 1080;

                using (var bitmap = new Bitmap(screenW, screenH, PixelFormat.Format32bppPArgb))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        try
                        {
                            g.CopyFromScreen(0, 0, 0, 0, new Size(screenW, screenH), CopyPixelOperation.SourceCopy);
                        }
                        catch (Exception winEx)
                        {
                            // Desktop locked or headless fallback frame
                            g.Clear(Color.FromArgb(8, 12, 22));
                            using var brushCyan = new SolidBrush(Color.FromArgb(77, 232, 224));
                            using var brushSilver = new SolidBrush(Color.FromArgb(226, 232, 240));
                            using var fontTitle = new Font(FontFamily.GenericSansSerif, 28, FontStyle.Bold);
                            using var fontSub = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Regular);

                            g.DrawString("FURINA CLOUD GAMING — STREAMING HOST ACTIVE", fontTitle, brushCyan, 60, 60);
                            g.DrawString($"Screen: {screenW}x{screenH} | Time: {DateTime.Now:HH:mm:ss.fff}", fontSub, brushSilver, 60, 120);
                            g.DrawString($"Desktop buffer: {winEx.Message}", fontSub, brushSilver, 60, 160);
                        }
                    }

                    // Scaling if configured
                    Bitmap outputBmp = bitmap;
                    bool scaled = false;
                    if (TargetWidth > 0 && TargetHeight > 0 && (screenW != TargetWidth || screenH != TargetHeight))
                    {
                        outputBmp = new Bitmap(TargetWidth, TargetHeight, PixelFormat.Format32bppPArgb);
                        using (var gScale = Graphics.FromImage(outputBmp))
                        {
                            gScale.InterpolationMode = InterpolationMode.Bilinear;
                            gScale.DrawImage(bitmap, 0, 0, TargetWidth, TargetHeight);
                        }
                        scaled = true;
                    }

                    using var ms = new MemoryStream();
                    _encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)Quality);
                    outputBmp.Save(ms, _jpegEncoder, _encoderParams);
                    frameData = ms.ToArray();

                    if (scaled)
                    {
                        outputBmp.Dispose();
                    }
                }

                encodeTimer.Stop();
                AverageEncodeMs = (AverageEncodeMs * 0.9) + (encodeTimer.Elapsed.TotalMilliseconds * 0.1);

                if (frameData != null && frameData.Length > 0)
                {
                    await BroadcastFrame(frameData, ct);
                    frameCount++;
                }

                if (fpsStopwatch.ElapsedMilliseconds >= 1000)
                {
                    CurrentFps = Math.Round((frameCount * 1000.0) / fpsStopwatch.ElapsedMilliseconds, 1);
                    frameCount = 0;
                    fpsStopwatch.Restart();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScreenCapture] Capture loop error: {ex.Message} {ex.StackTrace}");
            }

            // Frame Rate Pacing
            var elapsed = Stopwatch.GetElapsedTime(loopStart);
            var delay = frameInterval - elapsed;
            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, ct);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task BroadcastFrame(byte[] frameData, CancellationToken ct)
    {
        List<WebSocket> deadSockets = new();
        List<WebSocket> activeList;

        lock (_subLock)
        {
            activeList = _subscribers.ToList();
        }

        if (activeList.Count == 0) return;

        // Prefix timestamp header: 8 bytes Unix timestamp in ms
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        byte[] packet = new byte[frameData.Length + 8];
        BitConverter.TryWriteBytes(packet.AsSpan(0, 8), now);
        Buffer.BlockCopy(frameData, 0, packet, 8, frameData.Length);

        TotalBytesSent += packet.Length * activeList.Count;

        if (_totalFrames++ % 60 == 0)
        {
            Console.WriteLine($"[ScreenCapture] Broadcasting frame {_totalFrames} ({packet.Length} bytes) to {activeList.Count} subscriber(s)");
        }

        foreach (var ws in activeList)
        {
            if (ws.State == WebSocketState.Open)
            {
                try
                {
                    await ws.SendAsync(new ArraySegment<byte>(packet), WebSocketMessageType.Binary, true, ct);
                }
                catch
                {
                    deadSockets.Add(ws);
                }
            }
            else
            {
                deadSockets.Add(ws);
            }
        }

        if (deadSockets.Count > 0)
        {
            lock (_subLock)
            {
                foreach (var dead in deadSockets)
                {
                    _subscribers.Remove(dead);
                }
            }
        }
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageDecoders();
        foreach (var codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return codecs[0];
    }

    public void Dispose()
    {
        StopStream();
        _cts?.Dispose();
    }
}
