using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SteamDockExtension.Commands;
using SteamDockExtension.Dock;
using SteamDockExtension.Pages;
using SteamDockExtension.Services;
using SteamDockExtension.Settings;

namespace SteamDockExtension;

public sealed partial class SteamCommandsProvider : CommandProvider
{
    public const string ProviderId = "com.steamdock.extension";

    private readonly SteamSettingsManager _settings = new();
    private readonly SteamLibraryPage _libraryPage;
    private readonly SteamFriendsDockBand _friendsBand;
    private readonly WrappedDockItem _libraryBand;
    private readonly ICommandItem[] _commands;

    public SteamCommandsProvider()
    {
        Id = ProviderId;
        DisplayName = "Steam";
        Icon = SteamIcons.GetSteamIcon(_settings.ConfiguredSteamPath);
        Settings = _settings.Settings;

        _libraryPage = new SteamLibraryPage(_settings);
        _libraryBand = new WrappedDockItem(
            new OpenSteamLibraryCommand(Icon),
            "Steam Library")
        {
            Icon = Icon,
        };
        _friendsBand = new SteamFriendsDockBand(_settings);

        _commands =
        [
            new CommandItem(_libraryPage)
            {
                Title = "Steam Library",
                Subtitle = "Search and launch installed games",
                Icon = Icon,
                MoreCommands =
                [
                    new CommandContextItem(new ReloadSteamLibraryCommand(_libraryPage)),
                    new CommandContextItem(_settings.Settings.SettingsPage),
                    new CommandContextItem(new OpenUrlCommand("steam://open/main")
                    {
                        Id = "com.steamdock.open-steam",
                        Name = "Open Steam",
                        Result = CommandResult.Dismiss(),
                    }),
                ],
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands() => _commands;

    public override ICommandItem[] GetDockBands() => [_libraryBand, _friendsBand];

    public override ICommandItem? GetCommandItem(string id)
    {
        if (string.Equals(id, SteamLibraryPage.PageId, StringComparison.Ordinal))
        {
            return _commands[0];
        }

        return _libraryPage.FindCommand(id);
    }

    public override void Dispose()
    {
        _friendsBand.Dispose();
        base.Dispose();
    }
}
