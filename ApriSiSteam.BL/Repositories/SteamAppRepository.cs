using System.Data.SqlTypes;
using System.Diagnostics;
using System.Web;
using System.Xml;
using ApriSiSteam.BL.Models;
using Newtonsoft.Json;

namespace ApriSiSteam.BL.Repositories;

public class SteamAppRepository
{
    private static List<SteamApp> GetOwnedGames(string steamId, bool isFriend = false)
    {
        var steamApps = new List<SteamApp>();

        var xmlDocument = new XmlDocument();
        xmlDocument.Load($"https://steamcommunity.com/profiles/{steamId}/games?xml=1");

        var apps = xmlDocument.GetElementsByTagName("game");
        foreach (XmlNode app in apps)
        {
            var appId = app["appID"]!.InnerText;

            var steamApp = new SteamApp()
            {
                Appid = appId,
            };

            if (!isFriend)
            {
                steamApp.Name = app["name"]!.InnerText;
                steamApp.Image = $"https://cdn.akamai.steamstatic.com/steam/apps/{appId}/header.jpg";
            }

            steamApps.Add(steamApp);
        }

        return steamApps;
    }

    public static void CreateOwnedGamesJson(string steamId)
    {
        if (File.Exists("ClientGames.json")) return;
        var ownedGames = GetOwnedGames(steamId);
        File.WriteAllText(@"ClientGames.json", JsonConvert.SerializeObject(ownedGames));
    }

    public static List<SteamApp>? ReadOwnedGames()
    {
        var clientGames = File.ReadAllText("ClientGames.json");
        var steamApps = JsonConvert.DeserializeObject<List<SteamApp>>(clientGames);

        return steamApps;
    }
}