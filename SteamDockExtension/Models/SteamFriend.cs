namespace SteamDockExtension.Models;

internal sealed record SteamFriend(
    string SteamId,
    string PersonaName,
    int PersonaState,
    string ProfileUrl,
    string? AvatarMedium,
    string? GameName)
{
    public bool IsInGame => !string.IsNullOrWhiteSpace(GameName);

    public string StatusText => IsInGame
        ? $"Playing {GameName}"
        : PersonaState switch
        {
            1 => "Online",
            2 => "Busy",
            3 => "Away",
            4 => "Snooze",
            5 => "Looking to trade",
            6 => "Looking to play",
            _ => "Offline",
        };
}
