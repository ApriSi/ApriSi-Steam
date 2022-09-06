using ApriSiSteam.BL.Models;
using Steamworks;

namespace ApriSiSteam.BL;

public static class Steam
{
    public static void RunSteam() => Steamworks.SteamClient.Init(218);

    public static string GetClientSteamId() => Steamworks.SteamClient.SteamId.ToString();

    public static string GetClientName() => Steamworks.SteamClient.Name;
    public static bool IsGameInstalled(string gameId) => SteamApps.IsAppInstalled(uint.Parse(gameId));

    public static List<SteamFriend> GetFriends()
    {
        var friends = new List<SteamFriend>(SteamFriends.GetFriends().Select(friend => new SteamFriend()
        {
            Name = friend.Name, 
            SteamId = friend.Id.ToString(),
            Avatar = @"https://avatars.akamai.steamstatic.com/3da502bfc03b9f9d54c29467309e5229dcbf2b27_full.jpg"
        }).ToList());

        return friends;
    }
}