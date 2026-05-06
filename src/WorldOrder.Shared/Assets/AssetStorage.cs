using System.Reflection;

namespace WorldOrder.Assets;

public static class AssetStorage
{
    private const string AssetPrefix = "GameAssets/PostApocalypse";

    public static Stream? OpenPostApocalypseAsset(string relativePath)
    {
        var normalized = Normalize(relativePath);
        var fromDisk = OpenFromDisk(normalized);
        if (fromDisk is not null) return fromDisk;

        var fromAndroidAssets = OpenFromAndroidAssets(normalized);
        if (fromAndroidAssets is not null) return fromAndroidAssets;

        return OpenEmbedded(normalized);
    }

    private static string Normalize(string relativePath)
    {
        return relativePath.Replace('\\', '/').TrimStart('/');
    }

    private static Stream? OpenFromDisk(string relativePath)
    {
        foreach (var root in CandidateRoots())
        {
            var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(path)) return File.OpenRead(path);
        }
        return null;
    }

    private static IEnumerable<string> CandidateRoots()
    {
        var baseDir = AppContext.BaseDirectory;
        var cwd = Directory.GetCurrentDirectory();
        foreach (var start in new[] { baseDir, cwd })
        {
            if (string.IsNullOrWhiteSpace(start)) continue;
            var directory = new DirectoryInfo(start);
            for (var i = 0; i < 8 && directory is not null; i++, directory = directory.Parent)
            {
                yield return Path.Combine(directory.FullName, AssetPrefix.Replace('/', Path.DirectorySeparatorChar));
            }
        }
    }

    private static Stream? OpenFromAndroidAssets(string relativePath)
    {
        try
        {
            var applicationType = Type.GetType("Android.App.Application, Mono.Android");
            var context = applicationType?.GetProperty("Context", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            var assets = context?.GetType().GetProperty("Assets")?.GetValue(context);
            var openMethod = assets?.GetType().GetMethod("Open", new[] { typeof(string) });
            return openMethod?.Invoke(assets, new object[] { $"{AssetPrefix}/{relativePath}" }) as Stream;
        }
        catch
        {
            return null;
        }
    }

    private static Stream? OpenEmbedded(string relativePath)
    {
        var suffix = $"{AssetPrefix}/{relativePath}";
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (var name in assembly.GetManifestResourceNames())
                {
                    if (!name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) continue;
                    return assembly.GetManifestResourceStream(name);
                }
            }
            catch
            {
                // Some runtime-generated assemblies cannot be queried for resources.
            }
        }
        return null;
    }
}
