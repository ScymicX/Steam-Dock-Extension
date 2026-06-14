using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SteamDockExtension.Pages;

namespace SteamDockExtension.Commands;

internal sealed partial class OpenSteamLibraryCommand : InvokableCommand
{
    public OpenSteamLibraryCommand(IconInfo icon)
    {
        Id = "com.steamdock.open-library";
        Name = "Steam Library";
        Icon = icon;
    }

    public override CommandResult Invoke()
    {
        return CommandResult.GoToPage(new GoToPageArgs
        {
            PageId = SteamLibraryPage.PageId,
            NavigationMode = NavigationMode.Push,
        });
    }
}
