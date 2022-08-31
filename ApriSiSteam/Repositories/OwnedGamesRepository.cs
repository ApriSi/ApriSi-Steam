using ApriSiSteam.Models;
using HtmlAgilityPack;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ApriSiSteam.Repositories
{
    public class OwnedGamesRepository
    {
        public static List<Game> GetOwnedGames(SteamId steamUserId)
        {
            HtmlWeb web = new HtmlWeb();
            using var client = new HttpClient();
            web.UserAgent = "Mozilla/5.0 (X11; CrOS x86_64 14816.131.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";

            Uri uri = new Uri($"https://steamdb.info/calculator/{steamUserId}/?cc=eu");
            HtmlDocument doc = web.Load(uri);

            var apps = doc.DocumentNode.SelectNodes("//tr[@class='app']");

            var ownedGames = new List<Game>();
            foreach (var app in apps)
            {
                var appid = int.Parse(app.Attributes["data-appid"].Value);

                var name = app.SelectSingleNode("td[@class='text-left']").SelectSingleNode("a").InnerText;

                var steamApp = new Game()
                {
                    Appid = appid,
                    Name = name,
                    ImagePath = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/header.jpg"
                };


                ownedGames.Add(steamApp);
            }

            return ownedGames;
        }
    }
}