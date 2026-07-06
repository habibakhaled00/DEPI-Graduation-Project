using System.ComponentModel.DataAnnotations;

namespace NeighborHelp.DTOs
{
    public class RegisterDto
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = "";

        [Required, StringLength(50)]
        public string LastName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6)]
        public string Password { get; set; } = "";
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }

    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Token { get; set; } = "";

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = "";
    }

    public class UpdateProfileDto
    {
        [StringLength(50)]
        public string? FirstName { get; set; }
        [StringLength(50)]
        public string? LastName { get; set; }
        [StringLength(500)]
        public string? Bio { get; set; }
        [StringLength(200)]
        public string? Address { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
    }

    public class UserProfileDto
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public DateTime JoinedDate { get; set; }
        public double AverageRating { get; set; }
        public int RequestsPosted { get; set; }
        public int PeopleHelped { get; set; }
    }
}
