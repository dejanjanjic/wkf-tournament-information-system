using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Enums;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.DataAccess.Data;
using WKFTournamentIS.Helpers;
using WKFTournamentIS.Services.Interfaces;

namespace WKFTournamentIS.Services.Implementation
{
    public class OperatorService : IOperatorService
    {
        private readonly ApplicationDbContext _context;

        public OperatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllOperatorsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Operator)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> SearchOperatorsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllOperatorsAsync();
            }

            var lowerCaseSearchTerm = searchTerm.Trim().ToLower();
            return await _context.Users
                .Where(u => u.Role == UserRole.Operator && u.Username.ToLower().Contains(lowerCaseSearchTerm))
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<User> CreateOperatorAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Korisničko ime i lozinka su obavezni.");
            }

            if (await OperatorExistsAsync(username))
            {
                throw new InvalidOperationException($"Operator sa korisničkim imenom '{username}' već postoji.");
            }

            var newOperator = new User
            {
                Username = username.Trim(),
                PasswordHash = PasswordHelper.HashPassword(password),
                Role = UserRole.Operator
            };

            await _context.Users.AddAsync(newOperator);
            await _context.SaveChangesAsync();

            return newOperator;
        }

        public async Task<bool> UpdateOperatorPasswordAsync(int operatorId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentException("Nova lozinka je obavezna.");
            }

            var operatorToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == operatorId && u.Role == UserRole.Operator);
            if (operatorToUpdate == null)
            {
                return false;
            }

            operatorToUpdate.PasswordHash = PasswordHelper.HashPassword(newPassword);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteOperatorAsync(int operatorId)
        {
            var operatorToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Id == operatorId && u.Role == UserRole.Operator);
            if (operatorToDelete == null)
            {
                return false;
            }

            _context.Users.Remove(operatorToDelete);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> OperatorExistsAsync(string username)
        {
            var lowerCaseUsername = username.Trim().ToLower();
            return await _context.Users.AnyAsync(u => u.Username.ToLower() == lowerCaseUsername);
        }
    }
}