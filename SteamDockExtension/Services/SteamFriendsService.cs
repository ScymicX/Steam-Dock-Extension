using System.Net;
using System.Text.Json;
using SteamDockExtension.Models;

namespace SteamDockExtension.Services;

internal static class SteamFriendsService
{
    private static readonly HttpClient HttpClient = CreateHttpClient();

    public static async Task<IReadOnlyList<SteamFriend>> GetOnlineFriendsAsync(
        string apiKey,
        string steamId,
        CancellationToken cancellationToken)
    {
        var friendIds = await GetFriendIdsAsync(apiKey, steamId, cancellationToken).ConfigureAwait(false);
        if (friendIds.Count == 0)
        {
            return [];
        }

        var friends = new List<SteamFriend>();
        foreach (var batch in friendIds.Chunk(100))
        {
            var summaries = await GetPlayerSummariesAsync(apiKey, batch, cancellationToken).ConfigureAwait(false);
            friends.AddRange(summaries);
        }

        return friends
            .Where(friend => friend.PersonaState != 0 || friend.IsInGame)
            .OrderByDescending(friend => friend.IsInGame)
            .ThenBy(friend => friend.PersonaName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private static async Task<IReadOnlyList<string>> GetFriendIdsAsync(
        string apiKey,
        string steamId,
        CancellationToken cancellationToken)
    {
        var url = $"https://api.steampowered.com/ISteamUser/GetFriendList/v1/?" +
                  $"key={Uri.EscapeDataString(apiKey)}&steamid={Uri.EscapeDataString(steamId)}&relationship=friend";
        using var response = await HttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await ThrowFriendListUnauthorizedAsync(apiKey, steamId, cancellationToken).ConfigureAwait(false);
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!document.RootElement.TryGetProperty("friendslist", out var friendList) ||
            !friendList.TryGetProperty("friends", out var friends))
        {
            return [];
        }

        return friends.EnumerateArray()
            .Select(friend => friend.TryGetProperty("steamid", out var id) ? id.GetString() : null)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .ToArray();
    }

    private static async Task ThrowFriendListUnauthorizedAsync(
        string apiKey,
        string steamId,
        CancellationToken cancellationToken)
    {
        var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?" +
                  $"key={Uri.EscapeDataString(apiKey)}&steamids={Uri.EscapeDataString(steamId)}";
        using var response = await HttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "The Steam API key is valid, but this account's Friends List is private. " +
                "In Steam, open Edit Profile > Privacy Settings and set Friends List to Public.");
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "Steam rejected the Web API key. Create or verify it at steamcommunity.com/dev/apikey.");
        }

        throw new InvalidOperationException(
            $"Steam returned 401 for the friend list and the API key could not be verified " +
            $"(HTTP {(int)response.StatusCode}).");
    }

    private static async Task<IReadOnlyList<SteamFriend>> GetPlayerSummariesAsync(
        string apiKey,
        IEnumerable<string> steamIds,
        CancellationToken cancellationToken)
    {
        var ids = string.Join(",", steamIds);
        var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?" +
                  $"key={Uri.EscapeDataString(apiKey)}&steamids={Uri.EscapeDataString(ids)}";
        using var response = await HttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!document.RootElement.TryGetProperty("response", out var responseNode) ||
            !responseNode.TryGetProperty("players", out var players))
        {
            return [];
        }

        var result = new List<SteamFriend>();
        foreach (var player in players.EnumerateArray())
        {
            var id = ReadString(player, "steamid");
            var name = ReadString(player, "personaname");
            var profile = ReadString(player, "profileurl");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(profile))
            {
                continue;
            }

            result.Add(new SteamFriend(
                id,
                name,
                ReadInt32(player, "personastate"),
                profile,
                ReadString(player, "avatarmedium"),
                ReadString(player, "gameextrainfo")));
        }

        return result;
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) ? value.GetString() : null;

    private static int ReadInt32(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var result) ? result : 0;

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15),
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("SteamDockExtension/0.1");
        return client;
    }
}
