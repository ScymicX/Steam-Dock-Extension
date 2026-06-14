using SteamDockExtension.Models;

namespace SteamDockExtension.Services;

internal static class SteamLibraryService
{
    public static IReadOnlyList<SteamGame> GetInstalledGames(string? configuredSteamPath)
    {
        var steamPath = SteamInstallationLocator.FindSteamPath(configuredSteamPath);
        if (steamPath is null)
        {
            return [];
        }

        var libraries = FindLibraryPaths(steamPath);
        var games = new Dictionary<string, SteamGame>(StringComparer.Ordinal);

        foreach (var library in libraries)
        {
            var steamApps = Path.Combine(library, "steamapps");
            if (!Directory.Exists(steamApps))
            {
                continue;
            }

            foreach (var manifestPath in Directory.EnumerateFiles(steamApps, "appmanifest_*.acf", SearchOption.TopDirectoryOnly))
            {
                var game = TryReadGame(manifestPath, library, steamPath);
                if (game is not null && game.AppId != "228980")
                {
                    games[game.AppId] = game;
                }
            }
        }

        return games.Values
            .OrderBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private static HashSet<string> FindLibraryPaths(string steamPath)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            steamPath,
        };

        var libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryFile))
        {
            return paths;
        }

        try
        {
            var libraries = VdfParser.ParseFile(libraryFile).GetObject("libraryfolders");
            if (libraries is null)
            {
                return paths;
            }

            foreach (var pair in libraries.Values)
            {
                var path = pair.Value.Object?.GetString("path") ?? pair.Value.Text;
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    paths.Add(Path.GetFullPath(path));
                }
            }
        }
        catch
        {
            // The default library remains usable if this optional file is malformed.
        }

        return paths;
    }

    private static SteamGame? TryReadGame(string manifestPath, string libraryPath, string steamPath)
    {
        try
        {
            var state = VdfParser.ParseFile(manifestPath).GetObject("AppState");
            var appId = state?.GetString("appid");
            var name = state?.GetString("name");
            var installDir = state?.GetString("installdir");
            if (string.IsNullOrWhiteSpace(appId) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(installDir))
            {
                return null;
            }

            _ = long.TryParse(state?.GetString("SizeOnDisk"), out var size);
            DateTimeOffset? lastPlayed = null;
            if (long.TryParse(state?.GetString("LastPlayed"), out var unixTime) && unixTime > 0)
            {
                lastPlayed = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime();
            }

            var artwork = FindArtwork(steamPath, appId);
            return new SteamGame(
                appId,
                name,
                Path.Combine(libraryPath, "steamapps", "common", installDir),
                size,
                lastPlayed,
                artwork.Icon,
                artwork.Hero);
        }
        catch
        {
            return null;
        }
    }

    private static (string? Icon, string? Hero) FindArtwork(string steamPath, string appId)
    {
        var cachePath = Path.Combine(steamPath, "appcache", "librarycache", appId);
        if (!Directory.Exists(cachePath))
        {
            return (null, null);
        }

        try
        {
            var files = Directory.EnumerateFiles(cachePath, "*", SearchOption.AllDirectories).ToArray();
            var cover = FindByName(files, "library_600x900.jpg") ??
                        FindByName(files, "library_capsule.jpg");
            var icon = cover ??
                       FindByName(files, "header.jpg") ??
                       FindByName(files, "logo.png");
            var hero = cover ??
                       FindByName(files, "library_hero.jpg") ??
                       FindByName(files, "header.jpg") ??
                       icon;
            return (icon, hero);
        }
        catch
        {
            return (null, null);
        }
    }

    private static string? FindByName(IEnumerable<string> files, string fileName) =>
        files.FirstOrDefault(path => string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase));
}
