using Microsoft.EntityFrameworkCore;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.DataAccess.Data;
using WKFTournamentIS.Helpers;
using WKFTournamentIS.Services.Interfaces;

namespace WKFTournamentIS.Services.Implementation
{
    class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;

        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                                     .Select(u => new User
                                     {
                                            Id = u.Id,
                                            Username = u.Username,
                                            PasswordHash = u.PasswordHash,
                                            Role = u.Role,
                                            PreferredTheme = u.PreferredTheme
                                     })
                                     .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return null;
            }

            bool isPasswordValid = PasswordHelper.VerifyPassword(password, user.PasswordHash);

            return isPasswordValid ? user : null;
        }
    }
}
