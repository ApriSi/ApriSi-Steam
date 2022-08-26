namespace ApriSiSteam.Models;
public class Game
{
    public int? Appid { get; set; }
    public string? Name { get; set; }
    public int? Playtime_forever { get; set; }
    public string? Img_icon_url { get; set; }
    public bool? Has_community_visible_stats { get; set; }
    public int? Playtime_windows_forever { get; set; }
    public int? Playtime_mac_forever { get; set; }
    public int? Playtime_linux_forever { get; set; }
    public int? Rtime_last_played { get; set; }
    public int? Playtime_2weeks { get; set; }
}