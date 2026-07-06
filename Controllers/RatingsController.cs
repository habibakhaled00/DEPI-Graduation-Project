using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;
using NeighborHelp.DTOs;
using NeighborHelp.Hubs;
using NeighborHelp.Models;
using System.Security.Claims;

namespace NeighborHelp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RatingsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _user;
        private readonly IHubContext<NotificationHub> _hubContext;

        public RatingsController(AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> hubContext)
        {
            _db = context;
            _user = userManager;
            _hubContext = hubContext;
        }

        private string? CurrentUID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRatingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // check for duplicate
            var exists = await _db.Ratings.AnyAsync(r =>
                r.RaterId == CurrentUID &&
                r.RatedUserId == dto.RatedUserId &&
                r.RequestId == dto.RequestId);

            if (exists) return BadRequest("You already rated this user for this request.");

            var rating = new Rating
            {
                Score = dto.Score,
                Comment = dto.Comment,
                RaterId = CurrentUID!,
                RatedUserId = dto.RatedUserId,
                RequestId = dto.RequestId
            };

            _db.Ratings.Add(rating);
            await _db.SaveChangesAsync();

            // add review if provided
            if (!string.IsNullOrWhiteSpace(dto.ReviewContent))
            {
                _db.Reviews.Add(new Review
                {
                    Content = dto.ReviewContent,
                    RatingId = rating.RatingId
                });
                await _db.SaveChangesAsync();
            }

            // create notification
            var rater = await _user.FindByIdAsync(CurrentUID!);
            var notif = new Notification
            {
                UserId = dto.RatedUserId,
                Title = "New Rating",
                Body = $"{rater?.FullName} rated you {dto.Score}/5 stars",
                Type = NotificationType.NewRating,
                RelatedUrl = $"/HelpRequests/Details/{dto.RequestId}"
            };
            _db.Notifications.Add(notif);
            await _db.SaveChangesAsync();

            // push via signalR
            await _hubContext.Clients.User(dto.RatedUserId)
                .SendAsync("ReceiveNotification", notif.Body);

            return Ok(new { ratingId = rating.RatingId, message = "Rating submitted." });
        }

        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetForUser(string userId)
        {
            var ratings = await _db.Ratings
                .Where(r => r.RatedUserId == userId)
                .Include(r => r.Rater)
                .Include(r => r.Review)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RatingResponseDto
                {
                    RatingId = r.RatingId,
                    Score = r.Score,
                    Comment = r.Comment,
                    RaterName = r.Rater!.FullName,
                    CreatedAt = r.CreatedAt,
                    ReviewContent = r.Review != null ? r.Review.Content : null
                })
                .ToListAsync();

            var avg = ratings.Any() ? Math.Round(ratings.Average(r => r.Score), 1) : 0;

            return Ok(new { average = avg, count = ratings.Count, ratings });
        }

        [HttpGet("request/{requestId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetForRequest(int requestId)
        {
            var ratings = await _db.Ratings
                .Where(r => r.RequestId == requestId)
                .Include(r => r.Rater)
                .Include(r => r.Review)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RatingResponseDto
                {
                    RatingId = r.RatingId,
                    Score = r.Score,
                    Comment = r.Comment,
                    RaterName = r.Rater!.FullName,
                    CreatedAt = r.CreatedAt,
                    ReviewContent = r.Review != null ? r.Review.Content : null
                })
                .ToListAsync();

            return Ok(ratings);
        }
    }
}
