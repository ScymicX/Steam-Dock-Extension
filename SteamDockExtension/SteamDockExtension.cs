using System.Runtime.InteropServices;
using Microsoft.CommandPalette.Extensions;

namespace SteamDockExtension;

[Guid("389FA345-561B-4898-A7D6-11528EC27454")]
public sealed partial class SteamDockExtension : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;
    private readonly SteamCommandsProvider _provider = new();

    public SteamDockExtension(ManualResetEvent extensionDisposedEvent)
    {
        _extensionDisposedEvent = extensionDisposedEvent;
    }

    public object? GetProvider(ProviderType providerType) => providerType switch
    {
        ProviderType.Commands => _provider,
        _ => null,
    };

    public void Dispose()
    {
        _provider.Dispose();
        _extensionDisposedEvent.Set();
    }
}
