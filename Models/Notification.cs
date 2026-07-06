using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeighborHelp.Models
{
    public enum NotificationType
    {
        NewVolunteer, VolunteerAccepted, VolunteerRejected,
        NewMessage, RequestCompleted, NewRating, System
    }

    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(500)]
        public string Body { get; set; } = "";

        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(300)]
        public string? RelatedUrl { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
