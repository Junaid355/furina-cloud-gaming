// d:\cloud\tests\PairingManagerTests.cs
using FurinaHost.Models;
using FurinaHost.Services;
using Xunit;

namespace FurinaTests;

public class PairingManagerTests
{
    [Fact]
    public void CurrentPin_IsFourDigits()
    {
        var pin = PairingManager.CurrentPin;
        Assert.NotNull(pin);
        Assert.Equal(4, pin.Length);
        Assert.True(int.TryParse(pin, out _));
    }

    [Fact]
    public void RegeneratePin_GeneratesNewFourDigitPin()
    {
        var newPin = PairingManager.RegeneratePin();
        Assert.NotNull(newPin);
        Assert.Equal(4, newPin.Length);
    }

    [Fact]
    public void PairDevice_WithValidPin_SucceedsAndReturnsToken()
    {
        var pin = PairingManager.CurrentPin;
        var req = new PairingRequest
        {
            Pin = pin,
            DeviceName = "Test iPhone 15 Pro",
            DeviceType = "iPhone"
        };

        var res = PairingManager.PairDevice(req, "192.168.1.50");
        Assert.True(res.Success);
        Assert.False(string.IsNullOrWhiteSpace(res.Token));

        // Validate token
        var isValid = PairingManager.ValidateToken(res.Token, out var session);
        Assert.True(isValid);
        Assert.NotNull(session);
        Assert.Equal("Test iPhone 15 Pro", session.DeviceName);
    }

    [Fact]
    public void PairDevice_WithInvalidPin_Fails()
    {
        var req = new PairingRequest
        {
            Pin = "0000",
            DeviceName = "Hacker Phone",
            DeviceType = "Unknown"
        };

        // Ensure "0000" isn't the active pin
        if (PairingManager.CurrentPin == "0000")
            PairingManager.RegeneratePin();

        var res = PairingManager.PairDevice(req, "192.168.1.99");
        Assert.False(res.Success);
        Assert.Empty(res.Token);
    }
}
