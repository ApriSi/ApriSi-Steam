using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using System.Xml;
using ApriSiSteam.BL.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;
using Steamworks.Ugc;

namespace ApriSiSteam.BL.Repositories;

public class SteamAppRepository
{
    public static int CurrentLoadedGames;
    public static int GamesToLoad;

    private static List<SteamApp> GetOwnedGames(string steamId)
    {
        CurrentLoadedGames = 0;
        var htmlNode = Scraper.Scrape($"https://steamdb.info/calculator/{steamId}/");
        var games = htmlNode.SelectNodes("//tr[@class='app']");
        
        var apps = new List<SteamApp>();
        GamesToLoad = games.Count;

        if (!File.Exists("ClientGames.json"))
        {
            foreach (var game in games)
            {
                WebClient webClient = new WebClient();
                var json = webClient.DownloadString($"https://steamspy.com/api.php?request=appdetails&appid={game.Attributes["data-appid"].Value}");

                apps.Add(JsonConvert.DeserializeObject<SteamApp>(json)!);
                CurrentLoadedGames++;
            }
        }
        
        return apps;
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

    public static void CreateFriendGamesJson()
    {
        if (File.Exists("FriendGames.json")) return;
        var friendGames = new Dictionary<string, List<string>>();
        var friends = Steam.GetFriends();

        foreach (var friend in friends)
        {
            friendGames.Add(friend.SteamId, new List<string>());

            var xmlDocument = new XmlDocument();
            xmlDocument.Load($"https://steamcommunity.com/profiles/{friend.SteamId}/games?xml=1");
            var apps = xmlDocument.GetElementsByTagName("game");

            GamesToLoad = apps.Count;
            foreach (XmlNode app in apps)
            {
                var appId = app["appID"]!.InnerText;
                friendGames[friend.SteamId].Add(appId);
            }
        }

        File.WriteAllText(@"FriendGames.json", JsonConvert.SerializeObject(friendGames));
    }
    
    public static IEnumerable<string> GetTags()
    {
        var ownedGames = ReadOwnedGames();

        var tags = new List<string>();
        foreach (var app in ownedGames)
        {
            if (app.tags!.ToString() == "[]") continue;

            foreach (var tag in (app.tags as JObject)!)
                if (!tags.Contains(tag.Key))
                    tags.Add(tag.Key);
        }

        return tags; 
    }
}