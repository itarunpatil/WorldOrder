using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace WorldOrder.Android;

[Activity(
    Label = "World Order",
    MainLauncher = true,
    Exported = true,
    Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
    ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.Landscape)]
public sealed class MainActivity : AndroidGameActivity
{
    private GameRoot? _game;

    protected override void OnCreate(Bundle? bundle)
    {
        base.OnCreate(bundle);
        RequestedOrientation = ScreenOrientation.Landscape;
        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
        Window?.SetSoftInputMode(SoftInput.AdjustPan);
        if (Build.VERSION.SdkInt >= BuildVersionCodes.P && Window is not null)
        {
            Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
        }
        ApplyImmersiveFullscreen();
        _game = new GameRoot();
        var view = (View)_game.Services.GetService(typeof(View))!;
        view.Focusable = true;
        view.FocusableInTouchMode = true;
        view.RequestFocus();
        SetContentView(view);
        _game.Run();
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus) ApplyImmersiveFullscreen();
    }

    private void ApplyImmersiveFullscreen()
    {
        Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        var decorView = Window?.DecorView;
        if (decorView is null) return;
        var flags = SystemUiFlags.ImmersiveSticky
            | SystemUiFlags.Fullscreen
            | SystemUiFlags.HideNavigation
            | SystemUiFlags.LayoutFullscreen
            | SystemUiFlags.LayoutHideNavigation
            | SystemUiFlags.LayoutStable;
        decorView.SystemUiVisibility = (StatusBarVisibility)flags;
    }
}
