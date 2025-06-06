using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.DataAccess.Data;
using WKFTournamentIS.Services.Interfaces;

namespace WKFTournamentIS.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllCategoriesAsync();
            }

            var lowerCaseSearchTerm = searchTerm.Trim().ToLower();
            return await _context.Categories
                .Where(c => c.Name.ToLower().Contains(lowerCaseSearchTerm))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                throw new ArgumentException("Naziv kategorije je obavezan.", nameof(category.Name));
            }

            if (await CategoryExistsByNameAsync(category.Name))
            {
                throw new InvalidOperationException($"Kategorija sa nazivom '{category.Name}' već postoji.");
            }

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                throw new ArgumentException("Naziv kategorije je obavezan.", nameof(category.Name));
            }

            var existingCategory = await _context.Categories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                return false;
            }

            if (await CategoryExistsByNameAsync(category.Name, category.Id))
            {
                throw new InvalidOperationException($"Kategorija sa nazivom '{category.Name}' već postoji.");
            }

            existingCategory.Name = category.Name.Trim();
            _context.Entry(existingCategory).State = EntityState.Modified;


            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Categories.AnyAsync(e => e.Id == category.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var categoryToDelete = await _context.Categories.FindAsync(id);
            if (categoryToDelete == null)
            {
                return false;
            }

            _context.Categories.Remove(categoryToDelete);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CategoryExistsByNameAsync(string name, int? currentId = null)
        {
            var query = _context.Categories.Where(c => c.Name.ToLower() == name.Trim().ToLower());
            if (currentId.HasValue)
            {
                query = query.Where(c => c.Id != currentId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesByTournamentIdAsync(int tournamentId)
        {
            if (!_context.Tournaments.Any(t => t.Id == tournamentId))
            {
                return new List<Category>();
            }

            return await _context.CategoriesInTournaments
                .Where(cit => cit.TournamentId == tournamentId)
                .Select(cit => cit.Category)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesNotAssignedToTournamentAsync(int tournamentId)
        {
            if (!_context.Tournaments.Any(t => t.Id == tournamentId))
            {
                return new List<Category>();
            }

            var assignedCategoryIds = await _context.CategoriesInTournaments
                .Where(cit => cit.TournamentId == tournamentId)
                .Select(cit => cit.CategoryId)
                .Distinct()
                .ToListAsync();

            return await _context.Categories
                .Where(c => !assignedCategoryIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
