using System.ComponentModel.DataAnnotations;
using NeighborHelp.Models;

namespace NeighborHelp.ViewModels
{
    public class CreateHelpRequestViewModel
    {
        [Required, StringLength(100, MinimumLength = 5)]
        public string Title { get; set; } = "";

        [Required, StringLength(2000, MinimumLength = 10)]
        public string Description { get; set; } = "";

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class HelpRequestDetailsViewModel
    {
        public HelpRequest HelpRequest { get; set; } = null!;
        public bool IsOwner { get; set; }
        public bool HasVolunteered { get; set; }
        public bool CanRate { get; set; }
        public double RequesterRating { get; set; }
        public int VolunteerCount { get; set; }
        public List<Rating> RequestRatings { get; set; } = new();
    }

    public class HelpRequestListViewModel
    {
        public List<HelpRequest> Requests { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public RequestStatus? Status { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
    }
}
