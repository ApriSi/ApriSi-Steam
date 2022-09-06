using System.Diagnostics;
using ApriSiSteam.BL.Models;

namespace ApriSiSteam.BL.Repositories;

public class SteamFriendRepository
{
    public static List<SteamFriend> GetFriends()
    {
        var steamFriends = Steam.GetFriends();
        foreach (var friend in steamFriends)
        {
            Debug.WriteLine(friend.Avatar);
        }
        return steamFriends.Select(steamFriend => new SteamFriend() { SteamId = steamFriend.SteamId!.ToString(), Name = steamFriend.Name, Avatar = steamFriend.Avatar }).ToList();
    }
}