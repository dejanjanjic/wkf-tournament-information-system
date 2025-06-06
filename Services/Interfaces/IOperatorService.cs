// WKFTournamentIS/Services/Interfaces/IOperatorService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface IOperatorService
    {
        Task<IEnumerable<User>> GetAllOperatorsAsync();
        Task<IEnumerable<User>> SearchOperatorsAsync(string searchTerm);
        Task<User> CreateOperatorAsync(string username, string password);
        Task<bool> UpdateOperatorPasswordAsync(int operatorId, string newPassword);
        Task<bool> DeleteOperatorAsync(int operatorId);
        Task<bool> OperatorExistsAsync(string username);
    }
}