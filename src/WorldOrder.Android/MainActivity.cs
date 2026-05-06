using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace WorldOrder.Android;

[Activity(
    Label = "World Order",
    MainLauncher = true,
    Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
    ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize,
    ScreenOrientation = ScreenOrientation.Landscape)]
public sealed class MainActivity : AndroidGameActivity
{
    private GameRoot? _game;

    protected override void OnCreate(Bundle? bundle)
    {
        base.OnCreate(bundle);
        _game = new GameRoot();
        var view = (View)_game.Services.GetService(typeof(View))!;
        SetContentView(view);
        _game.Run();
    }
}
