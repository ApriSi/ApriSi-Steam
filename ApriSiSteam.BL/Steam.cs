using System.Diagnostics;
using System.Xml;
using ApriSiSteam.BL.Models;
using Steamworks;
using SteamClient = ApriSiSteam.BL.Models.SteamClient;

namespace ApriSiSteam.BL;

public static class Steam
{
    public static void RunSteam() => Steamworks.SteamClient.Init(218);

    public static string GetClientSteamId() => Steamworks.SteamClient.SteamId.ToString();

    public static string GetClientName() => Steamworks.SteamClient.Name;
    public static bool IsGameInstalled(string gameId) => SteamApps.IsAppInstalled(uint.Parse(gameId));

    public static List<SteamFriend> GetFriends()
    {
        List<SteamFriend> friends = new List<SteamFriend>();

        foreach (var steamFriend in SteamFriends.GetFriends())
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load($"https://steamcommunity.com/profiles/{steamFriend.Id}?xml=1");
            var image = xmlDocument.GetElementsByTagName("avatarFull")[0]!.InnerText;

            var friend = new SteamFriend()
            {
                Name = steamFriend.Name,
                SteamId = steamFriend.Id.ToString(),
                Avatar = image
            };
            friends.Add(friend);
        }

        return friends;
    }
}