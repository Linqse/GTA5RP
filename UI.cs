using PoeShared.Native;
using KeyboardEventArgs = Microsoft.AspNetCore.Components.Web.KeyboardEventArgs;
using MouseEventArgs = Microsoft.AspNetCore.Components.Web.MouseEventArgs;

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

    private bool UseFood { get; set; } = false;
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
    
    
    private bool keybindermodal = false;
    /*
    private void Togglekeybindermodal()
    {
        keybindermodal = !keybindermodal; // Переключает состояние модального окна
    }
    private void OnKeyClicked(string key)
    {
        keybindermodal = false;
        FishHotkey.Hotkey = HotkeyConverter.ConvertFromString(key);
    }*/

    
    private Func<string, Task> currentOnKeyClicked;
    

    private Task OnKeyClickedForKeybinder(string key)
    {
        keybindermodal = false;
        FishHotkey.Hotkey = HotkeyConverter.ConvertFromString(key);
        return Task.CompletedTask;
    }

    private Task OnKeyClickedForMood(string key)
    {
        keybindermodal = false;
        _config.moodkey = key;
        SaveConfig();
        return Task.CompletedTask;
    } 
    private Task OnKeyClickedForRoad(string key)
    {
        keybindermodal = false;
        _config.roadkey = key;
        SaveConfig();
        return Task.CompletedTask;
    } 

    private void ToggleModal(string modalName)
    {
        keybindermodal = !keybindermodal;
        switch (modalName)
        {
            case "keybinder":
                currentOnKeyClicked = OnKeyClickedForKeybinder;
                break;
            case "mood":
                currentOnKeyClicked = OnKeyClickedForMood;
                break;
            case "road":
                currentOnKeyClicked = OnKeyClickedForRoad;
                break;
            // Добавьте другие случаи по необходимости
        }
    }

    private async Task TestCall()
    {
        TelegramMessage("Test");
    }
    
}