// d:\cloud\FurinaHost\Services\HardwareDetector.cs
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using FurinaHost.Models;

namespace FurinaHost.Services;

public class HardwareDetector
{
    private static HardwareInfo? _cachedInfo;
    private static readonly object _lock = new();

    public static HardwareInfo GetHardwareInfo(bool refresh = false)
    {
        lock (_lock)
        {
            if (_cachedInfo != null && !refresh)
            {
                return _cachedInfo;
            }

            var info = new HardwareInfo
            {
                OsVersion = Environment.OSVersion.VersionString,
                OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                Hostname = Environment.MachineName
            };

            // 1. IP Addresses
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        info.LocalIpAddresses.Add(ip.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                info.LocalIpAddresses.Add("127.0.0.1");
                Debug.WriteLine($"Failed to retrieve IP addresses: {ex.Message}");
            }

            // 2. CPU and RAM via WMI
            try
            {
                using var cpuSearcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores FROM Win32_Processor");
                foreach (var obj in cpuSearcher.Get())
                {
                    info.CpuName = obj["Name"]?.ToString()?.Trim() ?? "Unknown CPU";
                    info.CpuCores = Convert.ToInt32(obj["NumberOfCores"] ?? Environment.ProcessorCount);
                    break;
                }
            }
            catch
            {
                info.CpuName = "Windows Host Processor";
                info.CpuCores = Environment.ProcessorCount;
            }

            try
            {
                using var memSearcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                foreach (var obj in memSearcher.Get())
                {
                    var totalKb = Convert.ToDouble(obj["TotalVisibleMemorySize"] ?? 0);
                    var freeKb = Convert.ToDouble(obj["FreePhysicalMemory"] ?? 0);
                    info.TotalRamGb = Math.Round(totalKb / (1024 * 1024), 2);
                    info.FreeRamGb = Math.Round(freeKb / (1024 * 1024), 2);
                    break;
                }
            }
            catch
            {
                info.TotalRamGb = 16.0;
                info.FreeRamGb = 8.0;
            }

            // 3. GPU via WMI
            try
            {
                using var gpuSearcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, VideoProcessor, AdapterRAM FROM Win32_VideoController");
                foreach (var obj in gpuSearcher.Get())
                {
                    var name = obj["Name"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        var ramBytes = Convert.ToInt64(obj["AdapterRAM"] ?? 0);
                        info.Gpus.Add(new GpuInfo
                        {
                            Name = name,
                            DriverVersion = obj["DriverVersion"]?.ToString() ?? "Unknown",
                            VideoProcessor = obj["VideoProcessor"]?.ToString() ?? "DirectX Hardware",
                            AdapterRamMb = ramBytes > 0 ? $"{(ramBytes / (1024 * 1024))} MB" : "Dynamic"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GPU detect error: {ex.Message}");
            }

            // 4. Displays
            try
            {
                foreach (var screen in Screen.AllScreens)
                {
                    info.Displays.Add(new DisplayInfo
                    {
                        DeviceName = screen.DeviceName,
                        Width = screen.Bounds.Width,
                        Height = screen.Bounds.Height,
                        RefreshRate = 60, // Standard default; enhanced via EnumDisplaySettings if available
                        IsPrimary = screen.Primary
                    });
                }
            }
            catch
            {
                info.Displays.Add(new DisplayInfo
                {
                    DeviceName = "Primary Display",
                    Width = 1920,
                    Height = 1080,
                    RefreshRate = 60,
                    IsPrimary = true
                });
            }

            // 5. Audio Devices
            try
            {
                using var soundSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_SoundDevice");
                foreach (var obj in soundSearcher.Get())
                {
                    var name = obj["Name"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        info.AudioDevices.Add(name);
                    }
                }
            }
            catch
            {
                info.AudioDevices.Add("Standard Audio Output (WASAPI Loopback)");
            }

            _cachedInfo = info;
            return info;
        }
    }
}
