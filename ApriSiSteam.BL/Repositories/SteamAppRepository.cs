using System;
using System.Diagnostics;
using System.Net;
using ApriSiSteam.BL.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Steamworks;
using SteamClient = Steamworks.SteamClient;

namespace ApriSiSteam.BL.Repositories;

public class SteamAppRepository
{
    private static List<SteamApp> GetOwnedGames(string steamId)
    {
        var steamApps = new List<SteamApp>();
        
        var responseApps = Scraper.Scrape($"https://steamdb.info/calculator/{steamId}/?cc=eu", false);

        var apps = responseApps.SelectNodes("//tr[@class='app']");

        var categories = new List<string>();

        foreach (var app in apps)
        {
        
            var appid = app.Attributes["data-appid"].Value;
            var name = app.SelectSingleNode("//td[@class='text-left']").InnerText;
            Debug.WriteLine(name);

            var responseCategories = Scraper.Scrape($"https://store.steampowered.com/app/{appid}", true);

            var categoryCollection = responseCategories.SelectNodes("//a[@class='game_area_details_specs_ctn']//div[@class='label']");
            
            if(categoryCollection is not null)
                foreach (var category in categoryCollection)
                {   
                    if (category is not null)
                    {
                        categories.Add(category.InnerText);
                    }
                }
            


            var steamApp = new SteamApp()
            {
                Appid = appid,
                Name = name,
                Image = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/header.jpg",
                Categories = categories
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