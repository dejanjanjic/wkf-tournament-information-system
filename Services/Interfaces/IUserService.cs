using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> UpdateUserThemeAsync(int userId, string themeName);
    }
}
