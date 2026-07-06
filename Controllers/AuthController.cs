using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NeighborHelp.DTOs;
using NeighborHelp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NeighborHelp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _user = userManager;
            _signin = signInManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var result = await _user.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            await _user.AddToRoleAsync(user, "User");
            await _signin.SignInAsync(user, isPersistent: false);

            var token = await GenerateJwtToken(user);
            return Ok(new { token, userId = user.Id, name = user.FullName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _user.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                return Unauthorized(new { message = "Invalid email or password." });

            var result = await _signin.PasswordSignInAsync(
                dto.Email, dto.Password, isPersistent: false, lockoutOnFailure: false);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid email or password." });

            var token = await GenerateJwtToken(user);
            var roles = await _user.GetRolesAsync(user);
            return Ok(new { token, userId = user.Id, name = user.FullName, roles });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signin.SignOutAsync();
            return Ok(new { message = "Logged out." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _user.FindByEmailAsync(dto.Email);
            if (user == null) return Ok(new { message = "If the email exists, a reset link has been sent." });

            var token = await _user.GeneratePasswordResetTokenAsync(user);
            // in production you'd email this. for demo we return it
            var resetUrl = $"/Account/ResetPassword?email={dto.Email}&token={Uri.EscapeDataString(token)}";

            return Ok(new { message = "Reset link generated.", resetUrl });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _user.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Invalid request.");

            var result = await _user.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Password reset successfully." });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _user.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"], _config["Jwt:Audience"],
                claims, expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
