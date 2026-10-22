// d:\cloud\tests\GameManagerTests.cs
using FurinaHost.Models;
using FurinaHost.Services;
using Xunit;

namespace FurinaTests;

public class GameManagerTests
{
    [Fact]
    public void GetAllGames_ContainsRequiredDefaultProfiles()
    {
        var games = GameManager.GetAllGames();

        Assert.NotNull(games);
        Assert.True(games.Count >= 8, "Expected at least 8 game profiles");

        var requiredIds = new[] { "roblox", "gtav", "genshin", "wuwa", "minecraft_java", "minecraft_bedrock", "hsr", "zzz" };
        foreach (var id in requiredIds)
        {
            var game = games.FirstOrDefault(g => g.Id == id);
            Assert.NotNull(game);
            Assert.False(string.IsNullOrWhiteSpace(game.Name));
            Assert.NotEmpty(game.Controls);
        }
    }

    [Fact]
    public void RobloxProfile_ContainsMovementJoystickAndCameraRegion()
    {
        var game = GameManager.GetGameById("roblox");
        Assert.NotNull(game);

        var joystick = game.Controls.FirstOrDefault(c => c.Type == "joystick");
        var camera = game.Controls.FirstOrDefault(c => c.Type == "camera_region");
        var jump = game.Controls.FirstOrDefault(c => c.PrimaryKey == "Space");

        Assert.NotNull(joystick);
        Assert.NotNull(camera);
        Assert.NotNull(jump);
    }

    [Fact]
    public void GtaVProfile_ContainsDrivingAndOnFootActions()
    {
        var game = GameManager.GetGameById("gtav");
        Assert.NotNull(game);

        var vehicleBtn = game.Controls.FirstOrDefault(c => c.PrimaryKey == "KeyF");
        var handbrakeBtn = game.Controls.FirstOrDefault(c => c.Label.Contains("HANDBRAKE"));

        Assert.NotNull(vehicleBtn);
        Assert.NotNull(handbrakeBtn);
    }

    [Fact]
    public void AddCustomGame_AddsGameToLibrary()
    {
        var customId = $"test_custom_{Guid.NewGuid():N}";
        var newGame = new GameProfile
        {
            Id = customId,
            Name = "Unit Test Game",
            Category = "Test",
            ExecutablePath = @"C:\Windows\notepad.exe"
        };

        var added = GameManager.AddCustomGame(newGame);
        Assert.True(added);

        var retrieved = GameManager.GetGameById(customId);
        Assert.NotNull(retrieved);
        Assert.Equal("Unit Test Game", retrieved.Name);
    }
}
