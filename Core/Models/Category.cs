using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFTournamentIS.Core.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public virtual ICollection<CategoryInTournament> TournamentLinks { get; set; } = new HashSet<CategoryInTournament>();
    }
}
