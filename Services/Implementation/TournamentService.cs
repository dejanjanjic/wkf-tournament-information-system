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
    public class TournamentService : ITournamentService
    {
        private readonly ApplicationDbContext _context;

        public TournamentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Tournament> CreateTournamentAsync(Tournament tournament)
        {
            if (tournament == null)
            {
                throw new ArgumentNullException(nameof(tournament));
            }

            if (tournament.EndingDateTime <= tournament.BeginningDateTime)
            {
                throw new ArgumentException("Datum završetka mora biti nakon datuma početka.");
            }

            await _context.Tournaments.AddAsync(tournament);
            await _context.SaveChangesAsync();
            return tournament;
        }

        public async Task<bool> DeleteTournamentAsync(int id)
        {
            var tournamentToDelete = await _context.Tournaments.FindAsync(id);
            if (tournamentToDelete == null)
            {
                return false;
            }

            _context.Tournaments.Remove(tournamentToDelete);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Tournament>> GetAllTournamentsAsync()
        {
            return await _context.Tournaments.OrderByDescending(t => t.BeginningDateTime).ToListAsync();
        }

        public async Task<Tournament> GetTournamentByIdAsync(int id)
        {
            return await _context.Tournaments.FindAsync(id);
        }

        public async Task<IEnumerable<Tournament>> SearchTournamentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllTournamentsAsync();
            }

            var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

            return await _context.Tournaments
                .Where(t => t.Name.ToLower().Contains(lowerCaseSearchTerm) ||
                            t.Location.ToLower().Contains(lowerCaseSearchTerm))
                .OrderByDescending(t => t.BeginningDateTime)
                .ToListAsync();
        }

        public async Task<bool> UpdateTournamentAsync(Tournament tournament)
        {
            if (tournament == null)
            {
                throw new ArgumentNullException(nameof(tournament));
            }

            if (tournament.EndingDateTime <= tournament.BeginningDateTime)
            {
                throw new ArgumentException("Datum završetka mora biti nakon datuma početka.");
            }

            var existingTournament = await _context.Tournaments.FindAsync(tournament.Id);
            if (existingTournament == null)
            {
                return false;
            }

            _context.Entry(existingTournament).CurrentValues.SetValues(tournament);


            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TournamentExists(tournament.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<bool> TournamentExists(int id)
        {
            return await _context.Tournaments.AnyAsync(e => e.Id == id);
        }
    }
}