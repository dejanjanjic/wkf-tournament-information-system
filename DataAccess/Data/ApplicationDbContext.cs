using Microsoft.EntityFrameworkCore;
using WKFTournamentIS.Core.Enums;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Helpers;

namespace WKFTournamentIS.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Competitor> Competitors { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<CategoryInTournament> CategoriesInTournaments { get; set; }
        public DbSet<CompetitorInCategoryInTournament> CompetitorRegistrations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            string adminPasswordHash = PasswordHelper.HashPassword("admin");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = adminPasswordHash,
                    Role = UserRole.Administrator
                }
            );

            modelBuilder.Entity<CategoryInTournament>()
               .HasOne(cit => cit.Tournament)
               .WithMany(t => t.TournamentCategories)
               .HasForeignKey(cit => cit.TournamentId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoryInTournament>()
                .HasOne(cit => cit.Category)
                .WithMany(c => c.TournamentLinks)
                .HasForeignKey(cit => cit.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CategoryInTournament>()
                .HasIndex(cit => new { cit.TournamentId, cit.CategoryId })
                .IsUnique();

            modelBuilder.Entity<CompetitorInCategoryInTournament>(entity =>
            {
                entity.HasOne(cit => cit.CategoryInTournament)
                      .WithMany(ct => ct.RegisteredCompetitors)
                      .HasForeignKey(cit => cit.CategoryInTournamentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cit => cit.Competitor)
                      .WithMany(c => c.TournamentCategoryEntries)
                      .HasForeignKey(cit => cit.CompetitorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(cit => new { cit.CategoryInTournamentId, cit.CompetitorId })
                      .IsUnique();
            });

            modelBuilder.Entity<Competitor>()
                .HasOne(c => c.Club)
                .WithMany(cl => cl.Competitors)
                .HasForeignKey(c => c.ClubId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Club>()
                .HasIndex(c => c.Name)
                .IsUnique();
        }
    }
}