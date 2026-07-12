using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;
using System.Security.Claims;

namespace NeighborHelp.Controllers.Mvc
{
    [Route("Chat")]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{requestId:int}")]
        public async Task<IActionResult> Index(int requestId)
        {
            var request = await _context.HelpRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            var CurrentUID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isOwner = request.UserId == CurrentUID;
            bool isAcceptedVolunteer = await _context.VolunteerRequests.AnyAsync(v =>
                v.RequestId == requestId && v.UserId == CurrentUID &&
                v.Status == Models.VolunteerStatus.Accepted);

            if (!isOwner && !isAcceptedVolunteer)
                return Forbid();

            ViewBag.RequestId = requestId;
            ViewBag.RequestTitle = request.Title;
            ViewBag.IsOwner = isOwner;
            return View();
        }
    }
}
