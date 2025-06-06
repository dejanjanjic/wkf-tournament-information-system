using System.Collections.Generic;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface IClubService
    {
        Task<Club> GetClubByIdAsync(int id);
        Task<IEnumerable<Club>> GetAllClubsAsync();
        Task<IEnumerable<Club>> SearchClubsAsync(string searchTerm);
        Task<Club> CreateClubAsync(Club club);
        Task<bool> UpdateClubAsync(Club club);
        Task<bool> DeleteClubAsync(int id);
    }
}