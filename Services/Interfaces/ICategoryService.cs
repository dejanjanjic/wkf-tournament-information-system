using System.Collections.Generic;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> GetCategoryByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsByNameAsync(string name, int? currentId = null);
        Task<IEnumerable<Category>> GetCategoriesByTournamentIdAsync(int tournamentId);
        Task<IEnumerable<Category>> GetCategoriesNotAssignedToTournamentAsync(int tournamentId);
    }
}
