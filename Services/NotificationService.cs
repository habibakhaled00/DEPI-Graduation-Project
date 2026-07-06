using Microsoft.AspNetCore.SignalR;
using NeighborHelp.Data;
using NeighborHelp.Hubs;
using NeighborHelp.Models;

namespace NeighborHelp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext db, IHubContext<NotificationHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        public async Task CreateAsync(string userId, string title, string body,
            NotificationType type, string? relatedUrl = null)
        {
            var notif = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Type = type,
                RelatedUrl = relatedUrl
            };

            _db.Notifications.Add(notif);
            await _db.SaveChangesAsync();

            // push to user in real time via signalR
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    notif.NotificationId, notif.Title, notif.Body,
                    Type = notif.Type.ToString(),
                    notif.RelatedUrl, notif.CreatedAt
                });
            }
            catch
            {
                // user might not be connected, thats fine
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await Task.FromResult(
                _db.Notifications.Count(n => n.UserId == userId && !n.IsRead));
        }
    }
}
