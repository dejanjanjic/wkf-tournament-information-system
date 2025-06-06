using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface ICategoryInTournamentService
    {
        Task<CategoryInTournament> AssignCategoryToTournamentAsync(int tournamentId, int categoryId);
        Task<bool> RemoveCategoryFromTournamentAsync(int tournamentId, int categoryId);
        Task<bool> IsCategoryAssignedToTournamentAsync(int tournamentId, int categoryId);
        Task<CategoryInTournament> GetCategoryInTournamentByTournamentAndCategoryAsync(int tournamentId, int categoryId);
    }
}