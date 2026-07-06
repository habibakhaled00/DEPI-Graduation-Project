using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeighborHelp.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RatingId { get; set; }
        [ForeignKey("RatingId")]
        public virtual Rating? Rating { get; set; }
    }
}
