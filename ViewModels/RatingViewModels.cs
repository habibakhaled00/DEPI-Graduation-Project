using System.ComponentModel.DataAnnotations;

namespace NeighborHelp.ViewModels
{
    public class CreateRatingViewModel
    {
        [Required]
        public string RatedUserId { get; set; } = "";

        public string RatedUserName { get; set; } = "";

        [Required]
        public int HelpRequestId { get; set; }

        [Required, Range(1, 5)]
        public int Score { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        [StringLength(2000)]
        public string? ReviewContent { get; set; }
    }
}
