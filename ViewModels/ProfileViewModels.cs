using System.ComponentModel.DataAnnotations;

namespace NeighborHelp.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime JoinedDate { get; set; }
        public double AverageRating { get; set; }
        public int TotalHelped { get; set; }
        public int TotalRequests { get; set; }
        public bool IsOwnProfile { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
