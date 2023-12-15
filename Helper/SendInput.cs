using System.Runtime.InteropServices;
using PoeShared.Native;
using System.Windows.Forms;
using EyeAuras.Roxy.Shared;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    [Dependency] public ISendInputController SendInputController { get; init; }
    
    
    [Dependency] public IHotkeyConverter HotkeyConverter { get; init; }
    
    
    
    private static readonly SendInputArgs DefaultSendInputArgs = new()
    {
        InputSimulatorId = "Windows Input"
    };
    private static readonly SendInputArgs DefaultWindowsMessageArgs = new()
    {
        InputSimulatorId = "Windows Message API",
        InputEventType = InputEventType.KeyPress
    };

    
   

    /*private void MouseMove()
    {
        try
        {
            using var control = InputWindowsEx.Rent();
            control.LeftButtonDown(Win.ActiveWindow, new Point(0, 0));
            control.LeftButtonUp(Win.ActiveWindow, new Point(0, 0));
            control.MoveMouseBy(Win.ActiveWindow, -500, +50);
            control.MoveMouseBy(Win.ActiveWindow, +500, -50);
        }
        catch (Exception ex)
        {
            Log.Error($"Mouse move : {ex.Message}");
        }
        
        
    }*/

    private async Task MouseMove()
    {
        var random = new Random();
        var rand = random.Next(-100, 100);
        
        var point = new Point(Win.ActiveWindow.DwmWindowBounds.Width / 2, Win.ActiveWindow.DwmWindowBounds.Height / 2);
        await Secret(new Point(point.X+rand, point.Y+rand));

    }
    private async Task Secret(Point point)
    {
        
        var key = HotkeyConverter.ConvertFromString("MouseLeft");
        
        var send = DefaultWindowsMessageArgs with
        {
            Window = Win.ActiveWindow,
            Gesture = key,
            MouseLocation = point
        };
        await SendInputController.Send(send, CancellationToken.None);
    }

    private async Task SendBackgroundKey(string key, Point point = default)
    {
        var kp = HotkeyConverter.ConvertFromString(key);
        
        var send = DefaultWindowsMessageArgs with
        {
            Window = Win.ActiveWindow,
            Gesture = kp,
            MouseLocation = point == default ? Point.Empty : point
        };
        await SendInputController.Send(send, CancellationToken.None);
    }

    private async Task SendKey(string key, Point point = default, string inputEventType = default, bool fast = false)
    {
        var kp = HotkeyConverter.ConvertFromString(key);
    
        InputEventType eventType = inputEventType switch
        {
            "KeyDown" => InputEventType.KeyDown,
            "KeyUp" => InputEventType.KeyUp,
            _ => InputEventType.KeyPress // Default value
        };

        var sendInputArgs = DefaultSendInputArgs with
        {
            Window = Win.ActiveWindow,
            Gesture = kp,
            InputEventType = eventType,
            MouseLocation = point != default ? point : DefaultSendInputArgs.MouseLocation,
            MinDelay = fast == false ? TimeSpan.FromMilliseconds(25) : TimeSpan.FromMilliseconds(0),
            MaxDelay = fast == false ? TimeSpan.FromMilliseconds(35) : TimeSpan.FromMilliseconds(0),
        };

        await SendInputController.Send(sendInputArgs, CancellationToken.None);
    }

    
}