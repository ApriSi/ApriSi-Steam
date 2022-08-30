using ApriSiSteam.Models;
using Newtonsoft.Json.Linq;
using Steamworks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ApriSiSteam.Repositories
{
    public class OwnedGamesRepository
    {
        public static async Task<OwnedGames> GetOwnedGamesAsync(SteamId steamUserId)
        {
            using var client = new HttpClient();
            var response = await client.GetFromJsonAsync<Dictionary<string, OwnedGames>>($"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={Token.GetKey()}&steamid={steamUserId}&include_appinfo=true");
            if (response is null)
                return (OwnedGames)Enumerable.Empty<OwnedGames>();

            return response["response"];
        }
    }
}
