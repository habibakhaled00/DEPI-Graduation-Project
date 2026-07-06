using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeighborHelp.Models
{
    public enum VolunteerStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public class VolunteerRequest
    {
        [Key]
        public int VolunteerId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public string UserId { get; set; } = ""; // The volunteer

        [Required]
        public VolunteerStatus Status { get; set; } = VolunteerStatus.Pending;

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("RequestId")]
        public virtual HelpRequest? HelpRequest { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
