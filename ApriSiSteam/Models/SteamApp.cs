using System.Collections.Generic;

namespace ApriSiSteam.Models
{
    public class SteamApp
    {
        public int Appid { get; set; }
        public List<string>? AppCategories { get; set; }
        public List<string>? UserDefinedCategories { get; set; }
        public string ImgPath { get; set; }
        public string Description { get; set; }
    }
}
