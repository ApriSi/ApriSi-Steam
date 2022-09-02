using System;
using System.Diagnostics;
using System.Net;
using ApriSiSteam.BL.Models;
using HtmlAgilityPack;
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
            new() { Request = "//a[@class='game_area_details_specs_ctn']" }
        };

        var cookies = new List<Cookie>() { new Cookie("birthtime", "312850801") };

        var requests = new List<RequestItem>()
        {
            new()
            {
                Request = "//tr[@class='app']",
                Attribute = "data-appid"
            }
        };

        var responseApps = Scraper.ScrapeHtmlNodes($"https://steamdb.info/calculator/{steamId}/?cc=eu", requests, null);

        var categories = new List<string>();
        foreach (var app in responseApps[0])
        {
            var appid = app.Attributes["data-appid"].Value;
            
            var responseCategories = Scraper.ScrapeHtmlNodes($"https://store.steampowered.com/app/{appid}", categoryRequests, cookies);
            categories.AddRange(responseCategories[0].Select(category => category.InnerText));
            var steamApp = new SteamApp()
            {
                Appid = appid,
                Name = app.SelectSingleNode("td[@class='text-left']").SelectSingleNode("a").InnerText,
                Image = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/header.jpg",
                Categories = categories
            };
            steamApps.Add(steamApp);
        }*/

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