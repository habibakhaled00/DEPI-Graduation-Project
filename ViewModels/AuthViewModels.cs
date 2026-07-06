using System.ComponentModel.DataAnnotations;

namespace NeighborHelp.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Token { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = "";
    }
}
