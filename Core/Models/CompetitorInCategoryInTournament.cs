using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace WKFTournamentIS.Core.Models
{
    public class CompetitorInCategoryInTournament
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryInTournamentId { get; set; }
        [ForeignKey("CategoryInTournamentId")]
        public virtual CategoryInTournament CategoryInTournament { get; set; }

        [Required]
        public int CompetitorId { get; set; }
        [ForeignKey("CompetitorId")]
        public virtual Competitor Competitor { get; set; }

        public int? Placement { get; set; }
    }
}