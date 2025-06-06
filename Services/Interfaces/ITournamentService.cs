using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface ITournamentService
    {
        Task<Tournament> GetTournamentByIdAsync(int id);
        Task<IEnumerable<Tournament>> GetAllTournamentsAsync();
        Task<IEnumerable<Tournament>> SearchTournamentsAsync(string searchTerm);
        Task<Tournament> CreateTournamentAsync(Tournament tournament);
        Task<bool> UpdateTournamentAsync(Tournament tournament);
        Task<bool> DeleteTournamentAsync(int id);
    }
}
