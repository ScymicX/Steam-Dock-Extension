using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;

namespace SteamDockExtension;

public static class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] != "-RegisterProcessAsComServer")
        {
            return;
        }

        using ManualResetEvent extensionDisposedEvent = new(false);
        ComServer server = new();
        var extension = new SteamDockExtension(extensionDisposedEvent);

        server.RegisterClass<SteamDockExtension, IExtension>(() => extension);
        server.Start();
        extensionDisposedEvent.WaitOne();
        server.Stop();
        server.UnsafeDispose();
    }
}
