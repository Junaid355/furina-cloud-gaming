// d:\cloud\tests\ScreenCaptureTests.cs
using FurinaHost.Services;
using Xunit;

namespace FurinaTests;

public class ScreenCaptureTests
{
    [Fact]
    public void ScreenCapture_Instance_IsNotNull()
    {
        var cap = ScreenCapture.Instance;
        Assert.NotNull(cap);
        Assert.True(cap.TargetFps > 0);
        Assert.True(cap.Quality > 0);
    }

    [Fact]
    public void ScreenCapture_CanConfigureSettings()
    {
        var cap = ScreenCapture.Instance;
        cap.TargetFps = 30;
        cap.Quality = 75;
        cap.TargetWidth = 1280;
        cap.TargetHeight = 720;

        Assert.Equal(30, cap.TargetFps);
        Assert.Equal(75, cap.Quality);
        Assert.Equal(1280, cap.TargetWidth);
        Assert.Equal(720, cap.TargetHeight);
    }
}
