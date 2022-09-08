using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using System.Xml;
using ApriSiSteam.BL.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ApriSiSteam.BL.Repositories;

public class SteamAppRepository
{
    public static int CurrentLoadedGames;
    public static int GamesToLoad;

    private static List<SteamApp> GetOwnedGames(string steamId, bool isFriend = false)
    {
        CurrentLoadedGames = 0;
        var steamApps = new List<SteamApp>();


        var htmlNode = Scraper.Scrape($"https://steamdb.info/calculator/{steamId}/", false);

        var games = htmlNode.SelectNodes("//tr[@class='app']");



        GamesToLoad = games.Count;
        foreach (HtmlNode app in games)
        {
            var appId = app.Attributes["data-appid"].Value;

            if (!File.Exists("ClientGames.json"))
            {
                var apps = new List<SteamApp>();
                foreach (var game in games)
                {
                    WebClient webClient = new WebClient();
                    var json = webClient.DownloadString($"https://steamspy.com/api.php?request=appdetails&appid={game.Attributes["data-appid"].Value}");

                    apps.Add(JsonConvert.DeserializeObject<SteamApp>(json)!);
                    CurrentLoadedGames++;
                }
                File.WriteAllText(@"ClientGames.json", JsonConvert.SerializeObject(apps));
            }

        }

        var clientGames = File.ReadAllText("ClientGames.json");

        return JsonConvert.DeserializeObject<List<SteamApp>>(clientGames)!;
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
    
    public static List<string> GetTags()
    {
        var responseTags = Scraper.Scrape("https://store.steampowered.com/search/");
        var steamTags = responseTags.SelectNodes("//div[@class='tab_filter_control_row ']");
        var sortedTags = steamTags.OrderBy(o => o.Attributes["data-loc"].Value).ToList();
        
        return sortedTags.Select(tag => tag.Attributes["data-loc"].Value).ToList();
    }
}