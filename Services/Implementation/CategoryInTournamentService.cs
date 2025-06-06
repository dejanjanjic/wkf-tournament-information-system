using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.DataAccess.Data;
using WKFTournamentIS.Services.Interfaces;

namespace WKFTournamentIS.Services.Implementation
{
    public class CategoryInTournamentService : ICategoryInTournamentService
    {
        private readonly ApplicationDbContext _context;

        public CategoryInTournamentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsCategoryAssignedToTournamentAsync(int tournamentId, int categoryId)
        {
            return await _context.CategoriesInTournaments
                .AnyAsync(cit => cit.TournamentId == tournamentId && cit.CategoryId == categoryId);
        }

        public async Task<CategoryInTournament> AssignCategoryToTournamentAsync(int tournamentId, int categoryId)
        {
            var tournamentExists = await _context.Tournaments.AnyAsync(t => t.Id == tournamentId);
            if (!tournamentExists)
            {
                throw new ArgumentException($"Turnir sa ID-jem {tournamentId} nije pronađen.", nameof(tournamentId));
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                throw new ArgumentException($"Kategorija sa ID-jem {categoryId} nije pronađena.", nameof(categoryId));
            }

            if (await IsCategoryAssignedToTournamentAsync(tournamentId, categoryId))
            {
                throw new InvalidOperationException("Ova kategorija je već dodeljena odabranom turniru.");
            }

            var newAssignment = new CategoryInTournament
            {
                TournamentId = tournamentId,
                CategoryId = categoryId
            };

            await _context.CategoriesInTournaments.AddAsync(newAssignment);
            await _context.SaveChangesAsync();

            return newAssignment;
        }

        public async Task<bool> RemoveCategoryFromTournamentAsync(int tournamentId, int categoryId)
        {
            var assignmentToRemove = await _context.CategoriesInTournaments
                .FirstOrDefaultAsync(cit => cit.TournamentId == tournamentId && cit.CategoryId == categoryId);

            if (assignmentToRemove == null)
            {
                return false;
            }

            _context.CategoriesInTournaments.Remove(assignmentToRemove);

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
        public async Task<CategoryInTournament> GetCategoryInTournamentByTournamentAndCategoryAsync(int tournamentId, int categoryId)
        {
            return await _context.CategoriesInTournaments
                                 .Include(cit => cit.Category)
                                 .Include(cit => cit.Tournament)
                                 .FirstOrDefaultAsync(cit => cit.TournamentId == tournamentId && cit.CategoryId == categoryId);
        }
    }
}