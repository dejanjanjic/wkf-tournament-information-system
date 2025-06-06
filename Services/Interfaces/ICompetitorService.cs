// WKFTournamentIS/Services/Interfaces/ICompetitorService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface ICompetitorService
    {
        Task<Competitor> GetCompetitorByIdAsync(int id);
        Task<IEnumerable<Competitor>> GetAllCompetitorsAsync();
        Task<IEnumerable<Competitor>> SearchCompetitorsAsync(string searchTerm);
        Task<Competitor> CreateCompetitorAsync(Competitor competitor);
        Task<bool> UpdateCompetitorAsync(Competitor competitor);
        Task<bool> DeleteCompetitorAsync(int id);
        Task<bool> CompetitorExistsAsync(string firstName, string lastName, DateTime dateOfBirth, int? currentId = null);
    }
}
