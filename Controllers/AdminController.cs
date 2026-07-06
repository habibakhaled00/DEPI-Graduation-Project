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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _user;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _db = context;
            _user = userManager;
        }

        private string? CurrentUID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            var stats = new DashboardData
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalRequests = await _db.HelpRequests.CountAsync(),
                OpenRequests = await _db.HelpRequests.CountAsync(h => h.Status == RequestStatus.Open),
                CompletedRequests = await _db.HelpRequests.CountAsync(h => h.Status == RequestStatus.Completed),
                TotalMessages = await _db.ChatMessages.CountAsync(),
                TotalRatings = await _db.Ratings.CountAsync(),
                AverageRating = Math.Round(
                    await _db.Ratings.AverageAsync(r => (double?)r.Score) ?? 0, 1),

                CategoryStats = await _db.HelpRequests
                    .GroupBy(h => h.Category!.Name)
                    .Select(g => new CategoryStatDto { Name = g.Key, Count = g.Count() })
                    .ToListAsync(),

                MonthlyStats = await _db.HelpRequests
                    .Where(h => h.CreatedAt >= sixMonthsAgo)
                    .GroupBy(h => new { h.CreatedAt.Year, h.CreatedAt.Month })
                    .Select(g => new MonthlyStatDto
                    {
                        Month = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                        Requests = g.Count(),
                        Completed = g.Count(h => h.Status == RequestStatus.Completed)
                    })
                    .OrderBy(s => s.Month)
                    .ToListAsync()
            };

            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var term = search.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    u.Email!.ToLower().Contains(term));
            }

            var users = await query.OrderByDescending(u => u.JoinedDate).ToListAsync();

            var result = new List<AdminUserDto>();
            foreach (var u in users)
            {
                var roles = await _user.GetRolesAsync(u);
                result.Add(new AdminUserDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    JoinedDate = u.JoinedDate,
                    IsActive = u.IsActive,
                    Roles = roles.ToList()
                });
            }

            return Ok(result);
        }

        [HttpPut("users/{userId}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _user.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _user.UpdateAsync(user);

            _db.AdminLogs.Add(new AdminLog
            {
                Action = user.IsActive ? "Activated User" : "Deactivated User",
                Details = $"User: {user.Email}",
                AdminId = CurrentUID!
            });
            await _db.SaveChangesAsync();

            return Ok(new { isActive = user.IsActive });
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _db.AdminLogs
                .Include(a => a.Admin)
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .Select(a => new {
                    a.LogId, a.Action, a.Details,
                    AdminName = a.Admin!.FullName,
                    a.CreatedAt
                })
                .ToListAsync();
            return Ok(logs);
        }
    }
}
