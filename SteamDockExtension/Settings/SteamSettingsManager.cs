using Microsoft.CommandPalette.Extensions.Toolkit;
using SteamDockExtension.Services;

namespace SteamDockExtension.Settings;

internal sealed class SteamSettingsManager : JsonSettingsManager
{
    private const string Namespace = "steamDock";
    private readonly SteamCredentialStore _credentialStore = new();

    private readonly TextSetting _apiKey = CreateTextSetting(
        Key("apiKey"),
        "Steam Web API key",
        "Enter a key to store it securely. Leave blank to keep the current key, or enter CLEAR to remove it.",
        string.Empty);

    private readonly TextSetting _steamId = CreateTextSetting(
        Key("steamId"),
        "SteamID64",
        "Optional. Leave blank to use the most recently signed-in local Steam account.",
        string.Empty);

    private readonly TextSetting _steamPath = CreateTextSetting(
        Key("steamPath"),
        "Steam installation folder",
        "Optional override, for example C:\\Program Files (x86)\\Steam.",
        string.Empty);

    private readonly TextSetting _maximumFriends = CreateTextSetting(
        Key("maximumFriends"),
        "Maximum online friends in Dock",
        "Limits the number of friend buttons shown in the Dock band.",
        "8");

    private readonly TextSetting _refreshMinutes = CreateTextSetting(
        Key("refreshMinutes"),
        "Friends refresh interval in minutes",
        "Allowed range: 1 to 60 minutes.",
        "5");

    public event EventHandler? Changed;

    public SteamSettingsManager()
    {
        var directory = Utilities.BaseSettingsPath("SteamDockExtension");
        Directory.CreateDirectory(directory);
        FilePath = Path.Combine(directory, "settings.json");

        Settings.Add(_apiKey);
        Settings.Add(_steamId);
        Settings.Add(_steamPath);
        Settings.Add(_maximumFriends);
        Settings.Add(_refreshMinutes);

        LoadSettings();
        MigrateLegacyApiKey();
        Settings.SettingsChanged += (_, _) => HandleSettingsChanged();
    }

    public string ApiKey
    {
        get
        {
            var environmentApiKey = Environment.GetEnvironmentVariable("STEAM_WEB_API_KEY");
            return !string.IsNullOrWhiteSpace(environmentApiKey)
                ? environmentApiKey.Trim()
                : _credentialStore.ReadApiKey() ?? string.Empty;
        }
    }

    public string? ConfiguredSteamPath =>
        string.IsNullOrWhiteSpace(_steamPath.Value) ? null : _steamPath.Value.Trim();

    public string? SteamId
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_steamId.Value))
            {
                return _steamId.Value.Trim();
            }

            var path = SteamInstallationLocator.FindSteamPath(ConfiguredSteamPath);
            return path is null ? null : SteamInstallationLocator.FindMostRecentSteamId(path);
        }
    }

    public int MaximumFriends => ParseBounded(_maximumFriends.Value, defaultValue: 8, minimum: 1, maximum: 30);

    public int RefreshMinutes => ParseBounded(_refreshMinutes.Value, defaultValue: 5, minimum: 1, maximum: 60);

    private static string Key(string name) => $"{Namespace}.{name}";

    private static TextSetting CreateTextSetting(
        string key,
        string title,
        string description,
        string defaultValue) =>
        new(key, description, title, defaultValue)
        {
            // Toolkit 0.11 renders Input.Text.Description as the visible label.
            Placeholder = description,
        };

    private void MigrateLegacyApiKey()
    {
        var legacyApiKey = _apiKey.Value?.Trim();
        if (string.IsNullOrEmpty(legacyApiKey) || !_credentialStore.TryWriteApiKey(legacyApiKey))
        {
            return;
        }

        _apiKey.Value = string.Empty;
        SaveSettings();
    }

    private void HandleSettingsChanged()
    {
        var apiKeyInput = _apiKey.Value?.Trim();
        if (string.Equals(apiKeyInput, "CLEAR", StringComparison.OrdinalIgnoreCase))
        {
            _credentialStore.DeleteApiKey();
            _apiKey.Value = string.Empty;
        }
        else if (!string.IsNullOrEmpty(apiKeyInput) && _credentialStore.TryWriteApiKey(apiKeyInput))
        {
            _apiKey.Value = string.Empty;
        }

        SaveSettings();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static int ParseBounded(string? value, int defaultValue, int minimum, int maximum) =>
        int.TryParse(value, out var parsed) ? Math.Clamp(parsed, minimum, maximum) : defaultValue;
}
