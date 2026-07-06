using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeighborHelp.Models
{
    public class AdminLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = "";

        [StringLength(1000)]
        public string? Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string AdminId { get; set; } = "";
        [ForeignKey("AdminId")]
        public virtual ApplicationUser? Admin { get; set; }
    }
}
