using ApriSiSteam.Models;
using Newtonsoft.Json.Linq;
using Steamworks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApriSiSteam.Repositories
{
    public class OwnedGamesRepository
    {
        public static async Task<OwnedGames> GetOwnedGamesAsync(SteamId steamUserId)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetFromJsonAsync<Dictionary<string, OwnedGames>>($"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={Token.GetKey()}&steamid={steamUserId}&include_appinfo=true");
                
                return response["response"];
            } catch
            {
                Debug.WriteLine("Invalid Token");
            }
            return null;
        }

        public static async Task<SteamUserSummaries> GetUserSummaries(SteamId steamUserId)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetFromJsonAsync<JsonObject>($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={Token.GetKey()}&steamids={steamUserId}");

                var steamUserSummaries = new SteamUserSummaries()
                {
                    Avatarfull = response["response"]["players"][0]["avatarfull"].ToString()
                };

                return steamUserSummaries;
            } catch
            {
                Debug.WriteLine("Invalid Token");
            }

            return null;
        }
    }
}
