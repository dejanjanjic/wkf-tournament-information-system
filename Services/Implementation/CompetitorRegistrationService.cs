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
    public class CompetitorRegistrationService : ICompetitorRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public CompetitorRegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsCompetitorRegisteredAsync(int categoryInTournamentId, int competitorId)
        {
            return await _context.CompetitorRegistrations
                .AnyAsync(cr => cr.CategoryInTournamentId == categoryInTournamentId && cr.CompetitorId == competitorId);
        }

        public async Task<IEnumerable<Competitor>> GetRegisteredCompetitorsAsync(int categoryInTournamentId)
        {
            var categoryInTournamentExists = await _context.CategoriesInTournaments.AnyAsync(cit => cit.Id == categoryInTournamentId);
            if (!categoryInTournamentExists)
            {
                return new List<Competitor>();
            }

            return await _context.CompetitorRegistrations
                .Where(cr => cr.CategoryInTournamentId == categoryInTournamentId)
                .Include(cr => cr.Competitor)
                .ThenInclude(c => c.Club)
                .Select(cr => cr.Competitor)
                .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Competitor>> GetAvailableCompetitorsForCategoryAsync(int categoryInTournamentId)
        {
            var categoryInTournamentExists = await _context.CategoriesInTournaments.AnyAsync(cit => cit.Id == categoryInTournamentId);
            if (!categoryInTournamentExists)
            {
                return new List<Competitor>();
            }

            var registeredCompetitorIds = await _context.CompetitorRegistrations
                .Where(cr => cr.CategoryInTournamentId == categoryInTournamentId)
                .Select(cr => cr.CompetitorId)
                .Distinct()
                .ToListAsync();

            return await _context.Competitors
                .Include(c => c.Club)
                .Where(c => !registeredCompetitorIds.Contains(c.Id))
                .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompetitorInCategoryInTournament>> GetRegistrationsWithDetailsAsync(int categoryInTournamentId)
        {
            var categoryInTournamentExists = await _context.CategoriesInTournaments.AnyAsync(cit => cit.Id == categoryInTournamentId);
            if (!categoryInTournamentExists)
            {
                return new List<CompetitorInCategoryInTournament>();
            }

            return await _context.CompetitorRegistrations
                .Where(cr => cr.CategoryInTournamentId == categoryInTournamentId)
                .Include(cr => cr.Competitor)
                    .ThenInclude(c => c.Club)
                .OrderBy(cr => cr.Placement.HasValue ? 0 : 1)
                .ThenBy(cr => cr.Placement)
                .ThenBy(cr => cr.Competitor.LastName)
                .ThenBy(cr => cr.Competitor.FirstName)
                .ToListAsync();
        }

        public async Task<CompetitorInCategoryInTournament> RegisterCompetitorAsync(int categoryInTournamentId, int competitorId)
        {
            var categoryInTournamentExists = await _context.CategoriesInTournaments.AnyAsync(cit => cit.Id == categoryInTournamentId);
            if (!categoryInTournamentExists)
            {
                throw new ArgumentException($"Instanca kategorije na turniru sa ID-jem {categoryInTournamentId} nije pronađena.", nameof(categoryInTournamentId));
            }

            var competitorExists = await _context.Competitors.AnyAsync(c => c.Id == competitorId);
            if (!competitorExists)
            {
                throw new ArgumentException($"Takmičar sa ID-jem {competitorId} nije pronađen.", nameof(competitorId));
            }

            if (await IsCompetitorRegisteredAsync(categoryInTournamentId, competitorId))
            {
                throw new InvalidOperationException("Takmičar je već registrovan za ovu kategoriju na ovom turniru.");
            }

            var newRegistration = new CompetitorInCategoryInTournament
            {
                CategoryInTournamentId = categoryInTournamentId,
                CompetitorId = competitorId,
            };

            await _context.CompetitorRegistrations.AddAsync(newRegistration);
            await _context.SaveChangesAsync();

            return newRegistration;
        }

        public async Task<bool> UnregisterCompetitorAsync(int categoryInTournamentId, int competitorId)
        {
            var registrationToRemove = await _context.CompetitorRegistrations
                .FirstOrDefaultAsync(cr => cr.CategoryInTournamentId == categoryInTournamentId && cr.CompetitorId == competitorId);

            if (registrationToRemove == null)
            {
                return false;
            }

            _context.CompetitorRegistrations.Remove(registrationToRemove);
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> UnregisterCompetitorByIdAsync(int registrationId)
        {
            var registrationToRemove = await _context.CompetitorRegistrations.FindAsync(registrationId);

            if (registrationToRemove == null)
            {
                return false;
            }

            _context.CompetitorRegistrations.Remove(registrationToRemove);
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<CompetitorInCategoryInTournament> GetRegistrationByIdAsync(int registrationId)
        {
            return await _context.CompetitorRegistrations
                                 .Include(cr => cr.Competitor)
                                 .Include(cr => cr.CategoryInTournament)
                                     .ThenInclude(cit => cit.Category)
                                 .Include(cr => cr.CategoryInTournament)
                                     .ThenInclude(cit => cit.Tournament)
                                 .FirstOrDefaultAsync(cr => cr.Id == registrationId);
        }

        public async Task<bool> UpdatePlacementAsync(int registrationId, int? placement)
        {
            var registrationToUpdate = await _context.CompetitorRegistrations.FindAsync(registrationId);
            if (registrationToUpdate == null)
            {
                throw new ArgumentException($"Registracija sa ID-jem {registrationId} nije pronađena.", nameof(registrationId));
            }

            if (placement.HasValue && (placement.Value <= 0))
            {
                throw new ArgumentException("Plasman mora biti pozitivan broj.", nameof(placement));
            }

            registrationToUpdate.Placement = placement;
            _context.Entry(registrationToUpdate).State = EntityState.Modified;

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.CompetitorRegistrations.AnyAsync(e => e.Id == registrationId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}