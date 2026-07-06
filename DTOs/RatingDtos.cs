using System.ComponentModel.DataAnnotations;

namespace NeighborHelp.DTOs
{
    public class CreateRatingDto
    {
        [Required]
        public string RatedUserId { get; set; } = "";

        [Required]
        public int RequestId { get; set; }

        [Required, Range(1, 5)]
        public int Score { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        [StringLength(2000)]
        public string? ReviewContent { get; set; }
    }

    public class RatingResponseDto
    {
        public int RatingId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
        public string RaterName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string? ReviewContent { get; set; }
    }
}
