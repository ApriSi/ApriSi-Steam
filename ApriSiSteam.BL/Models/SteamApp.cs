namespace ApriSiSteam.BL.Models;

public class SteamApp
{
    public int appid { get; set; }
    public string? name { get; set; }
    public string? developer { get; set; }
    public string? publisher { get; set; }
    public string? score_rank { get; set; }
    public int positive { get; set; }
    public int negative { get; set; }
    public int userscore { get; set; }
    public string? owners { get; set; }
    public int average_forever { get; set; }
    public int average_2weeks { get; set; }
    public int median_forever { get; set; }
    public int median_2weeks { get; set; }
    public string? price { get; set; }
    public string? initialprice { get; set; }
    public string? discount { get; set; }
    public int ccu { get; set; }
    public string? languages { get; set; }
    public string? genre { get; set; }
    public object? tags { get; set; }
}