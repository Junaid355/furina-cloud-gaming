// d:\cloud\tests\HardwareDetectorTests.cs
using FurinaHost.Services;
using Xunit;

namespace FurinaTests;

public class HardwareDetectorTests
{
    [Fact]
    public void GetHardwareInfo_ReturnsValidSystemSpecs()
    {
        var info = HardwareDetector.GetHardwareInfo(refresh: true);

        Assert.NotNull(info);
        Assert.False(string.IsNullOrWhiteSpace(info.Hostname));
        Assert.False(string.IsNullOrWhiteSpace(info.OsVersion));
        Assert.True(info.CpuCores > 0, "CPU cores should be greater than 0");
        Assert.True(info.TotalRamGb > 0, "Total RAM should be greater than 0");
        Assert.NotEmpty(info.LocalIpAddresses);
    }

    [Fact]
    public void GetHardwareInfo_DetectsAtLeastOneDisplay()
    {
        var info = HardwareDetector.GetHardwareInfo();

        Assert.NotNull(info.Displays);
        Assert.NotEmpty(info.Displays);
        Assert.True(info.Displays[0].Width > 0);
        Assert.True(info.Displays[0].Height > 0);
    }
}
