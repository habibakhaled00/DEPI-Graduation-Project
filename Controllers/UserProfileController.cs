using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;
using NeighborHelp.DTOs;
using NeighborHelp.Models;
using System.Security.Claims;

namespace NeighborHelp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _user;

        public UserProfileController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _db = context;
            _user = userManager;
        }

        private string? CurrentUID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var user = await _user.FindByIdAsync(CurrentUID!);
            if (user == null) return NotFound();
            return Ok(await BuildProfileDto(user, true));
        }

        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(string userId)
        {
            var user = await _user.FindByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(await BuildProfileDto(user, userId == CurrentUID));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var user = await _user.FindByIdAsync(CurrentUID!);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
            if (dto.Bio != null) user.Bio = dto.Bio;
            if (dto.Address != null) user.Address = dto.Address;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;

            await _user.UpdateAsync(user);
            return Ok(new { message = "Profile updated." });
        }

        private async Task<UserProfileDto> BuildProfileDto(ApplicationUser user, bool isOwn)
        {
            var avgRating = await _db.Ratings
                .Where(r => r.RatedUserId == user.Id)
                .AverageAsync(r => (double?)r.Score) ?? 0;

            return new UserProfileDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = isOwn ? user.Email! : "",
                Bio = user.Bio,
                Address = user.Address,
                JoinedDate = user.JoinedDate,
                AverageRating = Math.Round(avgRating, 1),
                RequestsPosted = await _db.HelpRequests.CountAsync(h => h.UserId == user.Id),
                PeopleHelped = await _db.VolunteerRequests
                    .CountAsync(v => v.UserId == user.Id && v.Status == VolunteerStatus.Accepted)
            };
        }
    }
}
