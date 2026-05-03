using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace WorldOrder;

[Activity(
    Label = "World Order",
    MainLauncher = true,
    Icon = "@drawable/icon",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.Landscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AndroidGameActivity
{
    private Game1? _game;
    private View? _view;

    protected override void OnCreate(Bundle? bundle)
    {
        base.OnCreate(bundle);
        Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
        _game = new Game1();
        _view = (View)_game.Services.GetService(typeof(View));
        SetContentView(_view);
        HideSystemUi();
        _game.Run();
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus) HideSystemUi();
    }

    private void HideSystemUi()
    {
        if (_view == null) return;
        _view.SystemUiVisibility = (StatusBarVisibility)(
            SystemUiFlags.ImmersiveSticky |
            SystemUiFlags.Fullscreen |
            SystemUiFlags.HideNavigation |
            SystemUiFlags.LayoutFullscreen |
            SystemUiFlags.LayoutHideNavigation |
            SystemUiFlags.LayoutStable);
    }
}
