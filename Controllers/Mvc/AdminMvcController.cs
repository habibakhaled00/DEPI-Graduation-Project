using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;
using NeighborHelp.Models;
using System.Text.Json;

namespace NeighborHelp.Controllers.Mvc
{
    [Route("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminMvcController : Controller
    {
        private readonly AppDbContext _db;

        public AdminMvcController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Compute stats server-side — avoids client-side API auth issues
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            var monthlyStatsDb = await _db.HelpRequests
                .Where(h => h.CreatedAt >= sixMonthsAgo)
                .GroupBy(h => new { h.CreatedAt.Year, h.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    requests = g.Count(),
                    completed = g.Count(h => h.Status == RequestStatus.Accepted)
                })
                .ToListAsync();

            var monthlyStatsFormatted = monthlyStatsDb
                .Select(s => new
                {
                    month = s.Year + "-" + s.Month.ToString("D2"),
                    s.requests,
                    s.completed
                })
                .OrderBy(s => s.month)
                .ToList();

            var statsObj = new
            {
                totalUsers         = await _db.Users.CountAsync(),
                totalRequests      = await _db.HelpRequests.CountAsync(),
                openRequests       = await _db.HelpRequests.CountAsync(h => h.Status == RequestStatus.Open),
                completedRequests  = await _db.HelpRequests.CountAsync(h => h.Status == RequestStatus.Accepted),
                totalMessages      = await _db.ChatMessages.CountAsync(),
                totalRatings       = await _db.Ratings.CountAsync(),
                averageRating      = Math.Round(
                    await _db.Ratings.AverageAsync(r => (double?)r.Score) ?? 0, 1),
                categoryStats = await _db.HelpRequests
                    .GroupBy(h => h.Category!.Name)
                    .Select(g => new { name = g.Key, count = g.Count() })
                    .ToListAsync(),
                monthlyStats = monthlyStatsFormatted
            };

            var logsObj = await _db.AdminLogs
                .Include(a => a.Admin)
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .Select(a => new
                {
                    a.Action,
                    a.Details,
                    adminName  = a.Admin!.FirstName + " " + a.Admin.LastName,
                    a.CreatedAt
                })
                .ToListAsync();

            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            ViewBag.StatsJson = JsonSerializer.Serialize(statsObj, opts);
            ViewBag.LogsJson  = JsonSerializer.Serialize(logsObj,  opts);

            return View("~/Views/Admin/Dashboard.cshtml");
        }

        [HttpGet("Users")]
        public IActionResult Users() => View("~/Views/Admin/Users.cshtml");

        [HttpGet("Reports")]
        public IActionResult Reports() => View("~/Views/Admin/Reports.cshtml");

        [HttpGet("Requests")]
        public IActionResult Requests() => View("~/Views/Admin/Requests.cshtml");

        [HttpGet("Chats")]
        public IActionResult Chats() => View("~/Views/Admin/Chats.cshtml");

        [HttpGet("Chats/{requestId:int}")]
        public IActionResult ChatDetail(int requestId) =>
            View("~/Views/Admin/ChatDetail.cshtml", requestId);
    }
}
