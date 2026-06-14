namespace SteamDockExtension.Models;

internal sealed record SteamGame(
    string AppId,
    string Name,
    string InstallDirectory,
    long SizeOnDisk,
    DateTimeOffset? LastPlayed,
    string? IconPath,
    string? HeroImagePath);
