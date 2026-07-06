using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;
using System.Security.Claims;

namespace NeighborHelp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NotificationsApiController(AppDbContext context) => _db = context;

        private string? CurrentUID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifs = await _db.Notifications
                .Where(n => n.UserId == CurrentUID)
                .OrderByDescending(n => n.CreatedAt)
                .Take(30)
                .Select(n => new {
                    n.NotificationId, n.Title, n.Body,
                    Type = n.Type.ToString(), n.IsRead,
                    n.CreatedAt, n.RelatedUrl
                })
                .ToListAsync();
            return Ok(notifs);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _db.Notifications
                .CountAsync(n => n.UserId == CurrentUID && !n.IsRead);
            return Ok(new { count });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n == null || n.UserId != CurrentUID) return NotFound();
            n.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var unread = await _db.Notifications
                .Where(n => n.UserId == CurrentUID && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok(new { marked = unread.Count });
        }
    }
}
