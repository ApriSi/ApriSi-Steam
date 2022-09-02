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

        var requests = new List<string>() { "//tr[@class='app']" };
        var categoryRequests = new List<string>() { "//a[@class='game_area_details_specs_ctn']", "//img[@class='game_header_image_full']" };

        var cookies = new List<Cookie>() { new Cookie("birthtime", "312850801") };

        var response = Scraper.GetMultipleNodes($"https://steamdb.info/calculator/{steamId}/?cc=eu", requests, null);

        /*foreach (var steamAppNode in response)
        {
            var appid = steamAppNode.Attributes["data-appid"].Value;

            var categoryResponse = Scraper.GetMultipleNodes($"https://store.steampowered.com/app/{appid}", categoryRequests, cookies);
            var categories = categoryResponse.Select(category => category.InnerText).ToList();

            var steamApp = new SteamApp()
            {
                Appid = appid,
                Name = steamAppNode.SelectSingleNode("td[@class='text-left']").SelectSingleNode("a").InnerText,
                Image = steamAppNode.SelectSingleNode("td[@class='applogo']").SelectSingleNode("a").SelectSingleNode("img").Attributes["src"].Value,
                Categories = categories
            };

            steamApps.Add(steamApp);
        }*/

        return steamApps;
    }

    public static void CreateOwnedGamesJSON(string steamId)
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