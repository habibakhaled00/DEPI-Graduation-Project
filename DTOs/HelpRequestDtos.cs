using NeighborHelp.Models;

namespace NeighborHelp.DTOs
{
    public class CreateHelpRequestDto
    {
        public int CategoryId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class UpdateHelpRequestDto
    {
        public int CategoryId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public RequestStatus Status { get; set; }
    }

    public class HelpRequestResponseDto
    {
        public int RequestId { get; set; }
        public string UserId { get; set; } = "";
        public string RequesterName { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VolunteerCount { get; set; }
        public VolunteerStatus? CurrentUserVolunteerStatus { get; set; }
    }
}
