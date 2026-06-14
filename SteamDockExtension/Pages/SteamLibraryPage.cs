using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Globalization;
using SteamDockExtension.Models;
using SteamDockExtension.Services;
using SteamDockExtension.Settings;

namespace SteamDockExtension.Pages;

internal sealed partial class SteamLibraryPage : ListPage
{
    public const string PageId = "com.steamdock.library";

    private readonly SteamSettingsManager _settings;
    private IListItem[] _items = [];

    public SteamLibraryPage(SteamSettingsManager settings)
    {
        _settings = settings;

        Id = PageId;
        Name = "Steam Library";
        Title = "Steam Library";
        PlaceholderText = "Search installed Steam games";
        ShowDetails = true;
        Icon = SteamIcons.GetSteamIcon(settings.ConfiguredSteamPath);

        Reload();
        _settings.Changed += OnSettingsChanged;
    }

    public override IListItem[] GetItems() => _items;

    public void Reload()
    {
        var games = SteamLibraryService.GetInstalledGames(_settings.ConfiguredSteamPath);
        _items = games.Select(game => (IListItem)new SteamGameListItem(game)).ToArray();

        EmptyContent = _items.Length == 0
            ? new CommandItem(new NoOpCommand())
            {
                Title = "No installed Steam games found",
                Subtitle = "Check the Steam installation folder in extension settings.",
            }
            : null;

        RaiseItemsChanged(_items.Length);
    }

    public ICommandItem? FindCommand(string id) =>
        _items.FirstOrDefault(item => string.Equals(item.Command?.Id, id, StringComparison.Ordinal));

    private void OnSettingsChanged(object? sender, EventArgs e) => Reload();
}

internal sealed partial class SteamGameListItem : ListItem
{
    public SteamGameListItem(SteamGame game)
        : base(CreateLaunchCommand(game))
    {
        Title = game.Name;
        Subtitle = string.Empty;
        Icon = string.IsNullOrWhiteSpace(game.IconPath)
            ? new IconInfo("\uE7FC")
            : new IconInfo(game.IconPath);
        Details = CreateDetails(game);
    }

    private static OpenUrlCommand CreateLaunchCommand(SteamGame game) => new($"steam://rungameid/{game.AppId}")
    {
        Id = $"com.steamdock.game.{game.AppId}",
        Name = $"Play {game.Name}",
        Result = CommandResult.Dismiss(),
    };

    private static Details CreateDetails(SteamGame game)
    {
        var storeCommand = new OpenUrlCommand($"https://store.steampowered.com/app/{game.AppId}/")
        {
            Id = $"com.steamdock.game.{game.AppId}.details-store",
            Name = "Open Steam store page",
            Icon = new IconInfo("\uE719"),
            Result = CommandResult.Dismiss(),
        };

        var metadata = new List<IDetailsElement>();
        if (game.LastPlayed is not null)
        {
            metadata.Add(new DetailsElement
            {
                Key = "Last played",
                Data = new DetailsLink
                {
                    Text = game.LastPlayed.Value.ToString("f", CultureInfo.CurrentCulture),
                },
            });
        }

        metadata.Add(new DetailsElement
        {
            Key = string.Empty,
            Data = new DetailsCommands { Commands = [storeCommand] },
        });

        return new Details
        {
            Body = CreateDetailsBody(game),
            Metadata = metadata.ToArray(),
            Size = ContentSize.Small,
        };
    }

    private static string CreateDetailsBody(SteamGame game)
    {
        if (!string.IsNullOrWhiteSpace(game.HeroImagePath) && File.Exists(game.HeroImagePath))
        {
            var imageUri = new Uri(game.HeroImagePath, UriKind.Absolute).AbsoluteUri;
            return $"![Steam library artwork]({imageUri}" +
                   "?--x-cmdpal-maxwidth=220&--x-cmdpal-maxheight=220&--x-cmdpal-fit=fit)";
        }

        return string.Empty;
    }
}

internal static class SteamIcons
{
    public static IconInfo GetSteamIcon(string? configuredSteamPath)
    {
        return IconHelpers.FromRelativePath("Assets\\CommandPaletteIcon.png");
    }
}
