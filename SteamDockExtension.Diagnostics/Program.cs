using SteamDockExtension.Services;

var steamPath = SteamInstallationLocator.FindSteamPath();
var steamId = steamPath is null ? null : SteamInstallationLocator.FindMostRecentSteamId(steamPath);
var games = SteamLibraryService.GetInstalledGames(steamPath);

Console.WriteLine($"Steam path: {steamPath ?? "(not found)"}");
Console.WriteLine($"SteamID64: {steamId ?? "(not found)"}");
Console.WriteLine($"Installed games: {games.Count}");

foreach (var game in games.Take(10))
{
    Console.WriteLine($"{game.AppId}\t{game.Name}\t{game.InstallDirectory}");
}
