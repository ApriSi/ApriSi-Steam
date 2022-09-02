using System.Diagnostics;
using System.Net;
using ApriSiSteam.BL.Models;
using Newtonsoft.Json;
using Steamworks;

namespace ApriSiSteam.BL.Repositories;

public class SteamAppRepository
{
    private static List<SteamApp> GetOwnedGames(string steamId)
    {
        var steamApps = new List<SteamApp>();

        /*var categoryRequests = new List<RequestItem>()
        {
            "//a[@class='game_area_details_specs_ctn']",
            "//img[@class='game_header_image_full']"
        };*/

        var requests = new List<RequestItem>()
        {
            new()
            {
                Request = "//tr[@class='app']",
                Attribute = "data-appid"
            }
        };

        var cookies = new List<Cookie>() { new Cookie("birthtime", "312850801") };
        var response = Scraper.ScrapeHtmlNodes($"https://steamdb.info/calculator/{steamId}/?cc=eu", requests, null);

        foreach (var app in response)
        {
            var steamApp = new SteamApp()
            {
                Appid = app.Attributes["data-appid"].Value,
                Name = app.SelectSingleNode("td[@class='text-left']").SelectSingleNode("a").InnerText,
                //Image = steamAppNode.SelectSingleNode("td[@class='applogo']").SelectSingleNode("a").SelectSingleNode("img").Attributes["src"].Value,
                //Categories = categories
            };

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