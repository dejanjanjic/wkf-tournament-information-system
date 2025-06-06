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
    public class ClubService : IClubService
    {
        private readonly ApplicationDbContext _context;

        public ClubService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Club> CreateClubAsync(Club club)
        {
            if (club == null)
            {
                throw new ArgumentNullException(nameof(club));
            }

            await _context.Clubs.AddAsync(club);
            await _context.SaveChangesAsync();
            return club;
        }

        public async Task<bool> DeleteClubAsync(int id)
        {
            var clubToDelete = await _context.Clubs.FindAsync(id);
            if (clubToDelete == null)
            {
                return false;
            }

            var hasCompetitors = await _context.Competitors.AnyAsync(c => c.ClubId == id);
            if (hasCompetitors)
            {
                throw new InvalidOperationException("Nije moguće obrisati klub koji ima prijavljene takmičare.");
            }
        

            _context.Clubs.Remove(clubToDelete);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Club>> GetAllClubsAsync()
        {
            return await _context.Clubs.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Club> GetClubByIdAsync(int id)
        {
            return await _context.Clubs.FindAsync(id);
        }

        public async Task<IEnumerable<Club>> SearchClubsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllClubsAsync();
            }

            var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

            return await _context.Clubs
                .Where(c => c.Name.ToLower().Contains(lowerCaseSearchTerm))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> UpdateClubAsync(Club club)
        {
            if (club == null)
            {
                throw new ArgumentNullException(nameof(club));
            }

            var existingClub = await _context.Clubs.FindAsync(club.Id);
            if (existingClub == null)
            {
                return false;
            }

            _context.Entry(existingClub).CurrentValues.SetValues(club);

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClubExists(club.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<bool> ClubExists(int id)
        {
            return await _context.Clubs.AnyAsync(e => e.Id == id);
        }
    }
}