using ApriSiSteam.BL.Models;

namespace ApriSiSteam.BL.Repositories;

public static class SteamClientRepository
{
    public static SteamClient GetSteamClientInformation()
    {
        var steamClient = new SteamClient();
        var clientHtmlNode = Scraper.Scrape($"https://steamcommunity.com/profiles/{Steam.GetClientSteamId()}");

        steamClient.Level = clientHtmlNode.SelectSingleNode("//div[@class='persona_name persona_level']//div//span").InnerText;
        steamClient.Avatar = clientHtmlNode.SelectSingleNode("//div[@class='playerAvatarAutoSizeInner']//img").Attributes["src"].Value;

        if(clientHtmlNode.SelectSingleNode("//div[@class='profile_avatar_frame']//img") is not null)
            steamClient.AvatarFrame = clientHtmlNode.SelectSingleNode("//div[@class='profile_avatar_frame']//img")
            .Attributes["src"].Value;

        return steamClient;
    }
}