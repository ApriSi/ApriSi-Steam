using System.Diagnostics;
using ApriSiSteam.BL.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApriSiSteam.BL.Repositories;

public class SteamFriendRepository
{
    public static List<SteamFriend> GetFriends()
    {
        var steamFriends = Steam.GetFriends();
        return steamFriends.Select(steamFriend => new SteamFriend() { SteamId = steamFriend.SteamId!.ToString(), Name = steamFriend.Name, Avatar = steamFriend.Avatar}).ToList();
    }

    public static List<string> GetFriendGames(string? steamId)
    {
        var games = File.ReadAllText("FriendGames.json");
        var friends = JsonConvert.DeserializeObject<JObject>(games);
        var friendGames = friends[steamId].Select(game => game.ToString()).ToList();

        return friendGames;
    }
}