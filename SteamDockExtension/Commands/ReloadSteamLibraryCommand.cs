using Microsoft.CommandPalette.Extensions.Toolkit;
using SteamDockExtension.Pages;

namespace SteamDockExtension.Commands;

internal sealed partial class ReloadSteamLibraryCommand : InvokableCommand
{
    private readonly SteamLibraryPage _page;

    public ReloadSteamLibraryCommand(SteamLibraryPage page)
    {
        _page = page;
        Id = "com.steamdock.reload-library";
        Name = "Reload Steam library";
        Icon = new IconInfo("\uE72C");
    }

    public override CommandResult Invoke()
    {
        _page.Reload();
        return CommandResult.KeepOpen();
    }
}
