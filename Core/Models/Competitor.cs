using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFTournamentIS.Core.Models
{
    public class Competitor
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }
        public int? ClubId { get; set; }

        [ForeignKey("ClubId")]
        public virtual Club Club { get; set; }
        public virtual ICollection<CompetitorInCategoryInTournament> TournamentCategoryEntries { get; set; } = new HashSet<CompetitorInCategoryInTournament>();
        public string FullNameAndClub => $"{FirstName} {LastName} ({Club?.Name ?? "Nema kluba"})";

    }
}
