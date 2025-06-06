using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WKFTournamentIS.Core.Enums;

namespace WKFTournamentIS.Core.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        [Required]
        public DateTime BeginningDateTime { get; set; }

        [Required]
        public DateTime EndingDateTime { get; set; }

        [Required]
        public string Location { get; set; }

        public virtual ICollection<CategoryInTournament> TournamentCategories { get; set; } = new HashSet<CategoryInTournament>();

    }
}
