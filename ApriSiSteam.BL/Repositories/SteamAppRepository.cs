using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Web;
using System.Xml;
using ApriSiSteam.BL.Models;
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

        var xmlDocument = new XmlDocument();
        xmlDocument.Load($"https://steamcommunity.com/profiles/{steamId}/games?xml=1");
        var apps = xmlDocument.GetElementsByTagName("game");

        GamesToLoad = apps.Count;
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
            
                var responseApps = Scraper.Scrape($"https://store.steampowered.com/app/{appId}", steam: true);
                var tags = responseApps.SelectNodes(
                    "//a[@class='game_area_details_specs_ctn']//div[@class='label']");

                if(tags is not null)
                    foreach (var tag in tags)
                        steamApp.Categories.Add(tag.InnerText);
            }

            CurrentLoadedGames++;
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
    
    public static List<string> GetTags()
    {
        var responseTags = Scraper.Scrape("https://store.steampowered.com/search/");
        var steamTags = responseTags.SelectNodes("//div[@class='tab_filter_control_row ']");
        var sortedTags = steamTags.OrderBy(o => o.Attributes["data-loc"].Value).ToList();
        
        return sortedTags.Select(tag => tag.Attributes["data-loc"].Value).ToList();
    }
}