using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeighborHelp.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Score { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string RaterId { get; set; } = "";
        [ForeignKey("RaterId")]
        public virtual ApplicationUser? Rater { get; set; }

        [Required]
        public string RatedUserId { get; set; } = "";
        [ForeignKey("RatedUserId")]
        public virtual ApplicationUser? RatedUser { get; set; }

        public int RequestId { get; set; }
        [ForeignKey("RequestId")]
        public virtual HelpRequest? HelpRequest { get; set; }

        public virtual Review? Review { get; set; }
    }
}
