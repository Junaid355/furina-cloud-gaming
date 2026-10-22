// d:\cloud\FurinaHost\Models\HardwareInfo.cs
namespace FurinaHost.Models;

public class HardwareInfo
{
    public string OsVersion { get; set; } = string.Empty;
    public string OsArchitecture { get; set; } = string.Empty;
    public string CpuName { get; set; } = string.Empty;
    public int CpuCores { get; set; }
    public double TotalRamGb { get; set; }
    public double FreeRamGb { get; set; }
    public List<GpuInfo> Gpus { get; set; } = new();
    public List<DisplayInfo> Displays { get; set; } = new();
    public List<string> AudioDevices { get; set; } = new();
    public List<string> LocalIpAddresses { get; set; } = new();
    public string Hostname { get; set; } = Environment.MachineName;
}

public class GpuInfo
{
    public string Name { get; set; } = string.Empty;
    public string DriverVersion { get; set; } = string.Empty;
    public string VideoProcessor { get; set; } = string.Empty;
    public string AdapterRamMb { get; set; } = string.Empty;
}

public class DisplayInfo
{
    public string DeviceName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int RefreshRate { get; set; }
    public bool IsPrimary { get; set; }
}
