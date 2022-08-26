using ApriSiSteam.Models;
using Steamworks;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ApriSiSteam.Repositories;
public interface IOwnedGamesRepository
{
    Task<OwnedGames> GetOwnedGamesAsync(SteamId steamUserId);
}
