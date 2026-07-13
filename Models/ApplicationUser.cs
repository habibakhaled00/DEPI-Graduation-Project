using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NeighborHelp.Models
{
    // extended user profile - Member 1 (Auth)
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = "";

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(300)]
        public string? ProfilePictureUrl { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public string FullName => FirstName + " " + LastName;
    }
}
