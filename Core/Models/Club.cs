using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WKFTournamentIS.Core.Models
{
    public class Club
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public virtual ICollection<Competitor> Competitors { get; set; } = new HashSet<Competitor>();
    }
}