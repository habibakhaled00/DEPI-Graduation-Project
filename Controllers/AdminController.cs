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
                CompletedRequests = await _db.HelpRequests.CountAsync(h => h.Status == RequestStatus.Accepted),
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
                        Completed = g.Count(h => h.Status == RequestStatus.Accepted)
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

        // ── Admin: Delete any help request ───────────────────────
        [HttpDelete("requests/{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _db.HelpRequests.FindAsync(id);
            if (request == null) return NotFound();

            _db.HelpRequests.Remove(request);

            _db.AdminLogs.Add(new AdminLog
            {
                Action = "Deleted Request",
                Details = $"Request #{id}: {request.Title}",
                AdminId = CurrentUID!
            });
            await _db.SaveChangesAsync();
            return Ok(new { message = "Request deleted." });
        }

        // ── Admin: List all help requests ────────────────────────
        [HttpGet("requests")]
        public async Task<IActionResult> GetAllRequests([FromQuery] string? search)
        {
            var query = _db.HelpRequests
                .Include(h => h.User)
                .Include(h => h.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(h =>
                    h.Title.ToLower().Contains(term) ||
                    h.User!.FirstName.ToLower().Contains(term) ||
                    h.User!.LastName.ToLower().Contains(term));
            }

            var items = await query
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new
                {
                    h.RequestId,
                    h.Title,
                    h.Status,
                    h.CreatedAt,
                    CategoryName = h.Category!.Name,
                    RequesterName = h.User!.FirstName + " " + h.User.LastName,
                    RequesterEmail = h.User.Email
                })
                .ToListAsync();

            return Ok(items);
        }

        // ── Admin: Get chat messages for any request ─────────────
        [HttpGet("chats/{requestId}")]
        public async Task<IActionResult> GetChatMessages(int requestId)
        {
            var request = await _db.HelpRequests
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.RequestId == requestId);

            if (request == null) return NotFound();

            var messages = await _db.ChatMessages
                .Where(m => m.RequestId == requestId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.MessageId,
                    m.SenderId,
                    m.SenderName,
                    m.Content,
                    m.SentAt
                })
                .ToListAsync();

            return Ok(new
            {
                requestId,
                requestTitle = request.Title,
                requesterName = request.User!.FirstName + " " + request.User.LastName,
                messages
            });
        }

        // ── Admin: List all requests that have chat messages ─────
        [HttpGet("chats")]
        public async Task<IActionResult> GetChatRooms()
        {
            var rooms = await _db.ChatMessages
                .GroupBy(m => m.RequestId)
                .Select(g => new
                {
                    RequestId = g.Key,
                    MessageCount = g.Count(),
                    LastMessage = g.Max(m => m.SentAt)
                })
                .ToListAsync();

            var requestIds = rooms.Select(r => r.RequestId).ToList();
            var requests = await _db.HelpRequests
                .Include(h => h.User)
                .Where(h => requestIds.Contains(h.RequestId))
                .ToDictionaryAsync(h => h.RequestId);

            var result = rooms.Select(r => new
            {
                r.RequestId,
                r.MessageCount,
                r.LastMessage,
                Title = requests.ContainsKey(r.RequestId) ? requests[r.RequestId].Title : "Deleted",
                RequesterName = requests.ContainsKey(r.RequestId)
                    ? requests[r.RequestId].User!.FirstName + " " + requests[r.RequestId].User!.LastName
                    : "Unknown"
            }).OrderByDescending(r => r.LastMessage);

            return Ok(result);
        }
    }
}
