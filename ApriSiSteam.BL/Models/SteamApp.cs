namespace ApriSiSteam.BL.Models;

public class SteamApp
{
    public string Appid { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public List<string> Categories = new();
}