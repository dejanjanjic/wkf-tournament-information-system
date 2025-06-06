using System.Collections.Generic;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface ICompetitorRegistrationService
    {
        Task<IEnumerable<Competitor>> GetRegisteredCompetitorsAsync(int categoryInTournamentId);
        Task<IEnumerable<Competitor>> GetAvailableCompetitorsForCategoryAsync(int categoryInTournamentId);
        Task<IEnumerable<CompetitorInCategoryInTournament>> GetRegistrationsWithDetailsAsync(int categoryInTournamentId);
        Task<CompetitorInCategoryInTournament> RegisterCompetitorAsync(int categoryInTournamentId, int competitorId);

        Task<bool> UnregisterCompetitorAsync(int categoryInTournamentId, int competitorId);
        Task<bool> UnregisterCompetitorByIdAsync(int registrationId);
        Task<bool> IsCompetitorRegisteredAsync(int categoryInTournamentId, int competitorId);
        Task<CompetitorInCategoryInTournament> GetRegistrationByIdAsync(int registrationId);
        Task<bool> UpdatePlacementAsync(int registrationId, int? placement);
    }
}