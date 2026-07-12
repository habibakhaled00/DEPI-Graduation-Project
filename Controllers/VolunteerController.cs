using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;
using NeighborHelp.Hubs;
using NeighborHelp.Models;
using System.Security.Claims;

namespace NeighborHelp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VolunteerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public VolunteerController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private string? CurrentUID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost("apply/{requestId}")]
        public async Task<IActionResult> Apply(int requestId)
        {
            if (CurrentUID == null) return Unauthorized();

            var request = await _context.HelpRequests.FindAsync(requestId);
            if (request == null) return NotFound("Help request not found.");

            if (request.UserId == CurrentUID)
                return BadRequest("You cannot volunteer for your own request.");

            if (request.Status != RequestStatus.Open && request.Status != RequestStatus.Pending)
                return BadRequest("This request is no longer accepting volunteers.");

            var alreadyApplied = await _context.VolunteerRequests
                .AnyAsync(v => v.RequestId == requestId && v.UserId == CurrentUID);
            if (alreadyApplied) return BadRequest("You already applied for this request.");

            var volunteer = new VolunteerRequest
            {
                RequestId = requestId,
                UserId = CurrentUID,
                Status = VolunteerStatus.Pending,
                AppliedDate = DateTime.UtcNow
            };

            _context.VolunteerRequests.Add(volunteer);

            // Move request to Pending once at least one volunteer applies
            request.Status = RequestStatus.Pending;

            await _context.SaveChangesAsync();

            // Save notification to database
            var notif = new Notification
            {
                UserId = request.UserId,
                Title = "New Volunteer Application",
                Body = $"A new volunteer applied for '{request.Title}'.",
                Type = NotificationType.NewVolunteer,
                RelatedUrl = $"/HelpRequests/ManageVolunteers/{request.RequestId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Notify request owner in real time
            await _hubContext.Clients.User(request.UserId)
                .SendAsync("ReceiveNotification", notif.Body);

            return Ok(new { message = "Successfully applied.", status = volunteer.Status.ToString() });
        }

        [HttpGet("request/{requestId}")]
        public async Task<IActionResult> GetApplicants(int requestId)
        {
            var request = await _context.HelpRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            if (request.UserId != CurrentUID) return Forbid();

            var applicants = await _context.VolunteerRequests
                .Include(v => v.User)
                .Where(v => v.RequestId == requestId)
                .Select(v => new
                {
                    v.VolunteerId,
                    v.UserId,
                    UserName = v.User!.UserName,
                    v.Status,
                    v.AppliedDate
                })
                .ToListAsync();

            return Ok(applicants);
        }

        [HttpGet("my-applications")]
        public async Task<IActionResult> MyApplications()
        {
            var applications = await _context.VolunteerRequests
                .Include(v => v.HelpRequest)
                .Where(v => v.UserId == CurrentUID)
                .OrderByDescending(v => v.AppliedDate)
                .Select(v => new
                {
                    v.VolunteerId,
                    v.RequestId,
                    RequestTitle = v.HelpRequest!.Title,
                    v.Status,
                    v.AppliedDate
                })
                .ToListAsync();

            return Ok(applications);
        }

        [HttpPut("accept/{volunteerId}")]
        public async Task<IActionResult> Accept(int volunteerId)
        {
            var volunteer = await _context.VolunteerRequests
                .Include(v => v.HelpRequest)
                .FirstOrDefaultAsync(v => v.VolunteerId == volunteerId);

            if (volunteer == null || volunteer.HelpRequest == null) return NotFound();
            if (volunteer.HelpRequest.UserId != CurrentUID) return Forbid();

            // Check if there is already an accepted match for this help request
            var hasAcceptedMatch = await _context.VolunteerRequests
                .AnyAsync(v => v.RequestId == volunteer.RequestId && v.Status == VolunteerStatus.Accepted);

            if (hasAcceptedMatch)
                return BadRequest("You already have an accepted volunteer for this request. Please cancel the current match first.");

            volunteer.Status = VolunteerStatus.Accepted;
            volunteer.HelpRequest.Status = RequestStatus.Accepted;

            await _context.SaveChangesAsync();

            // Save notification to database
            var notif = new Notification
            {
                UserId = volunteer.UserId,
                Title = "Volunteer Application Accepted",
                Body = $"You were accepted for '{volunteer.HelpRequest.Title}'! You can now chat with the requester.",
                Type = NotificationType.VolunteerAccepted,
                RelatedUrl = $"/Chat/{volunteer.RequestId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(volunteer.UserId)
                .SendAsync("ReceiveNotification", notif.Body);

            return Ok(new { message = "Volunteer accepted.", requestId = volunteer.RequestId });
        }

        [HttpPut("reject/{volunteerId}")]
        public async Task<IActionResult> Reject(int volunteerId)
        {
            var volunteer = await _context.VolunteerRequests
                .Include(v => v.HelpRequest)
                .FirstOrDefaultAsync(v => v.VolunteerId == volunteerId);

            if (volunteer == null || volunteer.HelpRequest == null) return NotFound();
            if (volunteer.HelpRequest.UserId != CurrentUID) return Forbid();

            volunteer.Status = VolunteerStatus.Rejected;

            // If no other pending/accepted applicants remain, reopen the request
            var hasActiveApplicants = await _context.VolunteerRequests
                .AnyAsync(v => v.RequestId == volunteer.RequestId
                               && v.VolunteerId != volunteerId
                               && v.Status != VolunteerStatus.Rejected);

            if (!hasActiveApplicants)
                volunteer.HelpRequest.Status = RequestStatus.Open;

            await _context.SaveChangesAsync();

            // Save notification to database
            var notif = new Notification
            {
                UserId = volunteer.UserId,
                Title = "Volunteer Application Declined",
                Body = $"Your application for '{volunteer.HelpRequest.Title}' was declined.",
                Type = NotificationType.VolunteerRejected,
                RelatedUrl = $"/HelpRequests/Details/{volunteer.RequestId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(volunteer.UserId)
                .SendAsync("ReceiveNotification", notif.Body);

            return Ok(new { message = "Volunteer rejected." });
        }

        [HttpPut("cancel-match/{requestId}")]
        public async Task<IActionResult> CancelMatch(int requestId)
        {
            if (CurrentUID == null) return Unauthorized();

            var volunteer = await _context.VolunteerRequests
                .Include(v => v.HelpRequest)
                .FirstOrDefaultAsync(v => v.RequestId == requestId && v.Status == VolunteerStatus.Accepted);

            if (volunteer == null || volunteer.HelpRequest == null) 
                return NotFound("No active accepted volunteer match found for this request.");

            if (volunteer.HelpRequest.UserId != CurrentUID) 
                return Forbid();

            // Set volunteer status back to Rejected/Cancelled
            volunteer.Status = VolunteerStatus.Rejected;

            // Re-evaluate help request status: if there are other pending applicants, set to Pending, else Open
            var hasPendingVolunteers = await _context.VolunteerRequests
                .AnyAsync(v => v.RequestId == requestId && v.Status == VolunteerStatus.Pending && v.VolunteerId != volunteer.VolunteerId);

            volunteer.HelpRequest.Status = hasPendingVolunteers ? RequestStatus.Pending : RequestStatus.Open;

            await _context.SaveChangesAsync();

            // Save notification to database for the cancelled volunteer
            var notif = new Notification
            {
                UserId = volunteer.UserId,
                Title = "Help Request Match Cancelled",
                Body = $"The match for '{volunteer.HelpRequest.Title}' was cancelled by the owner.",
                Type = NotificationType.VolunteerRejected,
                RelatedUrl = $"/HelpRequests/Details/{requestId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(volunteer.UserId)
                .SendAsync("ReceiveNotification", notif.Body);

            return Ok(new { message = "Match cancelled." });
        }
    }
}
