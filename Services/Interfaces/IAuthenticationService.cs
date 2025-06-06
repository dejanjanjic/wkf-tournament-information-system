using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;

namespace WKFTournamentIS.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User?> LoginAsync(string username, string password);
    }
}
