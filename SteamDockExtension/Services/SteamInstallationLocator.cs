using Microsoft.Win32;

namespace SteamDockExtension.Services;

internal static class SteamInstallationLocator
{
    public static string? FindSteamPath(string? configuredPath = null)
    {
        if (IsSteamDirectory(configuredPath))
        {
            return Path.GetFullPath(configuredPath!);
        }

        foreach (var candidate in RegistryCandidates())
        {
            if (IsSteamDirectory(candidate))
            {
                return Path.GetFullPath(candidate!);
            }
        }

        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var defaultPath = Path.Combine(programFilesX86, "Steam");
        return IsSteamDirectory(defaultPath) ? defaultPath : null;
    }

    public static string? FindMostRecentSteamId(string steamPath)
    {
        var loginUsersPath = Path.Combine(steamPath, "config", "loginusers.vdf");
        if (!File.Exists(loginUsersPath))
        {
            return null;
        }

        try
        {
            var users = VdfParser.ParseFile(loginUsersPath).GetObject("users");
            if (users is null)
            {
                return null;
            }

            string? fallback = null;
            foreach (var pair in users.Values)
            {
                if (pair.Value.Object is not { } user)
                {
                    continue;
                }

                fallback ??= pair.Key;
                if (user.GetString("MostRecent") == "1")
                {
                    return pair.Key;
                }
            }

            return fallback;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<string?> RegistryCandidates()
    {
        yield return ReadRegistryValue(Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath");
        yield return ReadRegistryValue(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath");
        yield return ReadRegistryValue(Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath");
    }

    private static string? ReadRegistryValue(RegistryKey root, string keyPath, string valueName)
    {
        try
        {
            using var key = root.OpenSubKey(keyPath);
            return key?.GetValue(valueName) as string;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSteamDirectory(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        File.Exists(Path.Combine(path, "steam.exe")) &&
        Directory.Exists(Path.Combine(path, "steamapps"));
}
