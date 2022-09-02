using ApriSiSteam.BL.Models;

namespace ApriSiSteam.BL.Repositories;

public static class SteamClientRepository
{
    public static SteamClient GetSteamClientInformation()
    {
        var steamClient = new SteamClient();
        var requests = new List<string>()
        {
            "//ul[@class='player-info']//span",
            "//img[@class='avatar']",
        };

        var response = Scraper.GetSingleNodes($"https://steamdb.info/calculator/{Steam.GetClientSteamId()}/?cc=eu",
            requests, null);

        steamClient.Level = response[0].InnerText;
        steamClient.Avatar = response[1].Attributes["src"].Value;

        return steamClient;
    }
}