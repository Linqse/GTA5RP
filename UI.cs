using PoeShared.Native;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    private bool ToggleLogs = false;
    
    private bool _port;
    private bool Port
    {
        get => _port;
        set => RaiseAndSetIfChanged(ref _port, value);
    }

    private bool _building;

    private bool Building
    {
        get => _building;
        set => RaiseAndSetIfChanged(ref _building, value);
    }

    private bool _mines;
    private bool Mines
    {
        get => _mines;
        set => RaiseAndSetIfChanged(ref _mines, value);
    }
    
    private bool _fish;
    private bool Fish
    {
        get => _fish;
        set => RaiseAndSetIfChanged(ref _fish, value);
    }
    
    private bool _ferma;
    private bool Ferma
    {
        get => _ferma;
        set => RaiseAndSetIfChanged(ref _ferma, value);
    }

    private bool UseFood { get; set; } = true;
    private bool UseMood { get; set; } = true;
    private bool UseCaptcha { get; set; } = true;

    private IWebUIAuraOverlay MainOverlay => AuraTree.Aura.Overlays.Items.OfType<IWebUIAuraOverlay>().First();
    private IHotkeyIsActiveTrigger Hotkey => AuraTree.Aura.Triggers.Items.OfType<IHotkeyIsActiveTrigger>().First();
    
    private void LockUnlock()
    {
        MainOverlay.IsLocked = !MainOverlay.IsLocked;
    }
    private void OverClose()
    {
        Hotkey.TriggerValue = false;
        if (Win.ActiveWindow.Handle != IntPtr.Zero)
        {
            UnsafeNative.ActivateWindow(Win.ActiveWindow.Handle);
        }

    }

}