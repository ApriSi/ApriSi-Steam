using System.Diagnostics;
using ApriSiSteam.BL.Models;

namespace ApriSiSteam.BL.Repositories;

public static class SteamClientRepository
{
    public static SteamClient GetSteamClientInformation()
    {
        var steamClient = new SteamClient();
        var requests = new List<RequestItem>()
        {
            new RequestItem()
            {
                Request = "//ul[@class='player-info']//span"
            },
            new RequestItem() {
                Request = "//img[@class='avatar']"
            }
        };
        var response = Scraper.ScrapeHtmlNodes($"https://steamdb.info/calculator/{Steam.GetClientSteamId()}/", requests, null);

        steamClient.Level = response[0][0].InnerText;
        steamClient.Avatar = response[1][0].Attributes["src"].Value;

        return steamClient;
    }
}