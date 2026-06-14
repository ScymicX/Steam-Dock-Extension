using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SteamDockExtension.Models;
using SteamDockExtension.Pages;
using SteamDockExtension.Services;
using SteamDockExtension.Settings;

namespace SteamDockExtension.Dock;

internal sealed partial class SteamFriendsDockBand : WrappedDockItem, IDisposable
{
    public const string BandId = "com.steamdock.online-friends";

    private readonly SteamSettingsManager _settings;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly CancellationTokenSource _cancellation = new();
    private System.Threading.Timer? _timer;

    public SteamFriendsDockBand(SteamSettingsManager settings)
        : base(CreateLoadingItems(), BandId, "Online Steam Friends")
    {
        _settings = settings;
        Icon = SteamIcons.GetSteamIcon(settings.ConfiguredSteamPath);

        _settings.Changed += OnSettingsChanged;
        RestartTimer();
        _ = RefreshAsync();
    }

    public void Dispose()
    {
        _settings.Changed -= OnSettingsChanged;
        _timer?.Dispose();
        _cancellation.Cancel();
    }

    private static IListItem[] CreateLoadingItems() =>
    [
        new ListItem(new NoOpCommand())
        {
            Title = "Loading Steam friends...",
            Icon = new IconInfo("\uE895"),
        },
    ];

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Icon = SteamIcons.GetSteamIcon(_settings.ConfiguredSteamPath);
        RestartTimer();
        _ = RefreshAsync();
    }

    private void RestartTimer()
    {
        _timer?.Dispose();
        _timer = new System.Threading.Timer(
            _ => _ = RefreshAsync(),
            null,
            TimeSpan.FromMinutes(_settings.RefreshMinutes),
            TimeSpan.FromMinutes(_settings.RefreshMinutes));
    }

    private async Task RefreshAsync()
    {
        if (!await _refreshLock.WaitAsync(0).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            var apiKey = _settings.ApiKey;
            var steamId = _settings.SteamId;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Items = CreateConfigurationItems(
                    "Steam API key required",
                    "Add it in extension settings or STEAM_WEB_API_KEY.");
                return;
            }

            if (string.IsNullOrWhiteSpace(steamId))
            {
                Items = CreateConfigurationItems(
                    "Steam account not found",
                    "Set SteamID64 in extension settings.");
                return;
            }

            var friends = await SteamFriendsService
                .GetOnlineFriendsAsync(apiKey, steamId, _cancellation.Token)
                .ConfigureAwait(false);

            Items = friends.Count == 0
                ? CreateConfigurationItems("No friends online", "Steam reports no online friends.")
                : friends.Take(_settings.MaximumFriends).Select(CreateFriendItem).ToArray();
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Items = CreateConfigurationItems("Steam friends unavailable", ex.Message);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private IListItem[] CreateConfigurationItems(string title, string subtitle)
    {
        var registerCommand = new OpenUrlCommand("https://steamcommunity.com/dev/apikey")
        {
            Id = "com.steamdock.open-api-key-page",
            Name = "Open Steam API key page",
            Result = CommandResult.Dismiss(),
        };

        return
        [
            new ListItem(registerCommand)
            {
                Title = title,
                Subtitle = subtitle,
                Icon = SteamIcons.GetSteamIcon(_settings.ConfiguredSteamPath),
                MoreCommands = [new CommandContextItem(_settings.Settings.SettingsPage)],
            },
        ];
    }

    private static IListItem CreateFriendItem(SteamFriend friend)
    {
        var chatCommand = new OpenUrlCommand($"steam://friends/message/{friend.SteamId}")
        {
            Id = $"com.steamdock.friend.{friend.SteamId}",
            Name = $"Chat with {friend.PersonaName}",
            Result = CommandResult.Dismiss(),
        };

        var profileCommand = new OpenUrlCommand(friend.ProfileUrl)
        {
            Id = $"com.steamdock.friend-profile.{friend.SteamId}",
            Name = $"Open {friend.PersonaName}'s profile",
            Result = CommandResult.Dismiss(),
        };

        return new ListItem(chatCommand)
        {
            Title = friend.PersonaName,
            Subtitle = friend.StatusText,
            Icon = string.IsNullOrWhiteSpace(friend.AvatarMedium)
                ? new IconInfo("\uE77B")
                : new IconInfo(friend.AvatarMedium),
            MoreCommands = [new CommandContextItem(profileCommand)],
        };
    }
}
