using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WKFTournamentIS.Core.Models
{
    public class CategoryInTournament
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        public virtual ICollection<CompetitorInCategoryInTournament> RegisteredCompetitors { get; set; } = new HashSet<CompetitorInCategoryInTournament>();
    }
}