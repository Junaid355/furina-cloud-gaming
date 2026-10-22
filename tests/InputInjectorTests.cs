// d:\cloud\tests\InputInjectorTests.cs
using FurinaHost.Models;
using FurinaHost.Services;
using Xunit;

namespace FurinaTests;

public class InputInjectorTests
{
    [Fact]
    public void ProcessInput_HandlesKeyDownAndKeyUpWithoutExceptions()
    {
        var msgDown = new InputMessage { Type = "key_down", Key = "Space" };
        var msgUp = new InputMessage { Type = "key_up", Key = "Space" };

        var exDown = Record.Exception(() => InputInjector.ProcessInput(msgDown));
        var exUp = Record.Exception(() => InputInjector.ProcessInput(msgUp));

        Assert.Null(exDown);
        Assert.Null(exUp);
    }

    [Fact]
    public void ProcessInput_HandlesJoystickVectorWithoutExceptions()
    {
        var joyForward = new InputMessage { Type = "joystick_vector", DeltaX = 0.0, DeltaY = -0.8 };
        var joyRight = new InputMessage { Type = "joystick_vector", DeltaX = 0.9, DeltaY = 0.0 };
        var joyZero = new InputMessage { Type = "joystick_vector", DeltaX = 0.0, DeltaY = 0.0 };

        Assert.Null(Record.Exception(() => InputInjector.ProcessInput(joyForward)));
        Assert.Null(Record.Exception(() => InputInjector.ProcessInput(joyRight)));
        Assert.Null(Record.Exception(() => InputInjector.ProcessInput(joyZero)));
    }

    [Fact]
    public void ProcessInput_HandlesMouseDeltaWithoutExceptions()
    {
        var mouseMsg = new InputMessage { Type = "mouse_delta", DeltaX = 15.5, DeltaY = -8.2 };
        var ex = Record.Exception(() => InputInjector.ProcessInput(mouseMsg));
        Assert.Null(ex);
    }

    [Fact]
    public void ReleaseAllInputs_ExecutesSafely()
    {
        // Simulate keys being pressed
        InputInjector.ProcessInput(new InputMessage { Type = "key_down", Key = "KeyW" });
        InputInjector.ProcessInput(new InputMessage { Type = "mouse_down", Button = 0 });

        var ex = Record.Exception(() => InputInjector.ReleaseAllInputs());
        Assert.Null(ex);
    }
}
