using ApriSiSteam.BL.Models;

namespace ApriSiSteam.BL.Repositories;

public static class SteamClientRepository
{
    public static SteamClient GetSteamClientInformation()
    {
        var steamClient = new SteamClient();
        var clientHtmlNode = Scraper.Scrape($"https://steamdb.info/calculator/{Steam.GetClientSteamId()}/", false);

        steamClient.Level = clientHtmlNode.SelectSingleNode("//ul[@class='player-info']//span").InnerText;
        steamClient.Avatar = clientHtmlNode.SelectSingleNode("//img[@class='avatar']").Attributes["src"].Value;

        return steamClient;
    }
}