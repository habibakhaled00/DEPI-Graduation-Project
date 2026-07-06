using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeighborHelp.Models
{
    public enum RequestStatus
    {
        Open,
        Pending,
        Accepted,
        Completed,
        Cancelled
    }

    public class HelpRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public string UserId { get; set; } = ""; // FK to ApplicationUser (Identity)

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = "";

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = "";

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<VolunteerRequest> VolunteerRequests { get; set; }
            = new List<VolunteerRequest>();
    }
}
