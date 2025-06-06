// WKFTournamentIS/Services/Implementation/CompetitorService.cs
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
    public class CompetitorService : ICompetitorService
    {
        private readonly ApplicationDbContext _context;

        public CompetitorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Competitor> GetCompetitorByIdAsync(int id)
        {
            return await _context.Competitors
                                 .Include(c => c.Club)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Competitor>> GetAllCompetitorsAsync()
        {
            return await _context.Competitors
                                 .Include(c => c.Club)
                                 .OrderBy(c => c.LastName)
                                 .ThenBy(c => c.FirstName)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Competitor>> SearchCompetitorsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllCompetitorsAsync();
            }

            var lowerCaseSearchTerm = searchTerm.Trim().ToLower();
            return await _context.Competitors
                .Include(c => c.Club)
                .Where(c => c.FirstName.ToLower().Contains(lowerCaseSearchTerm) ||
                            c.LastName.ToLower().Contains(lowerCaseSearchTerm) ||
                            (c.Club != null && c.Club.Name.ToLower().Contains(lowerCaseSearchTerm)))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<Competitor> CreateCompetitorAsync(Competitor competitor)
        {
            if (competitor == null)
            {
                throw new ArgumentNullException(nameof(competitor));
            }
            if (string.IsNullOrWhiteSpace(competitor.FirstName))
                throw new ArgumentException("Ime takmičara je obavezno.", nameof(competitor.FirstName));
            if (string.IsNullOrWhiteSpace(competitor.LastName))
                throw new ArgumentException("Prezime takmičara je obavezno.", nameof(competitor.LastName));
            if (competitor.DateOfBirth == default(DateTime) || competitor.DateOfBirth > DateTime.Now.AddYears(-3))
                throw new ArgumentException("Datum rođenja nije validan.", nameof(competitor.DateOfBirth));
            if (competitor.ClubId == null || competitor.ClubId <= 0)
                throw new ArgumentException("Izbor kluba je obavezan.", nameof(competitor.ClubId));


            if (await CompetitorExistsAsync(competitor.FirstName, competitor.LastName, competitor.DateOfBirth))
            {
                throw new InvalidOperationException($"Takmičar '{competitor.FirstName} {competitor.LastName}' rođen {competitor.DateOfBirth:dd.MM.yyyy} već postoji.");
            }

            await _context.Competitors.AddAsync(competitor);
            await _context.SaveChangesAsync();
            return competitor;
        }

        public async Task<bool> UpdateCompetitorAsync(Competitor competitor)
        {
            if (competitor == null)
            {
                throw new ArgumentNullException(nameof(competitor));
            }
            if (string.IsNullOrWhiteSpace(competitor.FirstName))
                throw new ArgumentException("Ime takmičara je obavezno.", nameof(competitor.FirstName));
            if (string.IsNullOrWhiteSpace(competitor.LastName))
                throw new ArgumentException("Prezime takmičara je obavezno.", nameof(competitor.LastName));
            if (competitor.DateOfBirth == default(DateTime) || competitor.DateOfBirth > DateTime.Now.AddYears(-3))
                throw new ArgumentException("Datum rođenja nije validan.", nameof(competitor.DateOfBirth));
            if (competitor.ClubId == null || competitor.ClubId <= 0) 
                throw new ArgumentException("Izbor kluba je obavezan.", nameof(competitor.ClubId));

            var existingCompetitor = await _context.Competitors.FindAsync(competitor.Id);
            if (existingCompetitor == null)
            {
                return false; // Takmičar nije pronađen
            }

            // Provera da li novi podaci (ime, prezime, datum rođenja) već postoje za nekog drugog takmičara
            if (await CompetitorExistsAsync(competitor.FirstName, competitor.LastName, competitor.DateOfBirth, competitor.Id))
            {
                throw new InvalidOperationException($"Takmičar sa imenom '{competitor.FirstName} {competitor.LastName}' i datumom rođenja {competitor.DateOfBirth:dd.MM.yyyy} već postoji.");
            }

            existingCompetitor.FirstName = competitor.FirstName.Trim();
            existingCompetitor.LastName = competitor.LastName.Trim();
            existingCompetitor.DateOfBirth = competitor.DateOfBirth;
            existingCompetitor.ClubId = competitor.ClubId;

            _context.Entry(existingCompetitor).State = EntityState.Modified;

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Competitors.AnyAsync(e => e.Id == competitor.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteCompetitorAsync(int id)
        {
            var competitorToDelete = await _context.Competitors.FindAsync(id);
            if (competitorToDelete == null)
            {
                return false;
            }

            _context.Competitors.Remove(competitorToDelete);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CompetitorExistsAsync(string firstName, string lastName, DateTime dateOfBirth, int? currentId = null)
        {
            var query = _context.Competitors
                .Where(c => c.FirstName.ToLower() == firstName.Trim().ToLower() &&
                            c.LastName.ToLower() == lastName.Trim().ToLower() &&
                            c.DateOfBirth.Date == dateOfBirth.Date);

            if (currentId.HasValue)
            {
                query = query.Where(c => c.Id != currentId.Value);
            }
            return await query.AnyAsync();
        }
    }
}
