namespace WorldOrder.Core;

public static class StoragePaths
{
    public static string SaveRoot
    {
        get
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(folder)) folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            if (string.IsNullOrWhiteSpace(folder)) folder = AppContext.BaseDirectory;
            return Path.Combine(folder, "WorldOrder", "Saves");
        }
    }

    public static string AssetRoot
    {
        get
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "GameAssets", "PostApocalypse");
        }
    }
}
