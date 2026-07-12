using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;
using NeighborHelp.Models;
using System.Security.Claims;

namespace NeighborHelp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _notificationHub;

        // Tracks active users in chat rooms: ConnectionId -> (UserId, RequestId)
        private static readonly Dictionary<string, (string UserId, int RequestId)> _activeUsers = new();

        public ChatHub(AppDbContext context, IHubContext<NotificationHub> notificationHub)
        {
            _context = context;
            _notificationHub = notificationHub;
        }

        private static string GroupName(int requestId) => $"request-{requestId}";

        public async Task JoinRequestRoom(int requestId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            var request = await _context.HelpRequests.FindAsync(requestId);
            if (request == null) return;

            bool isOwner = request.UserId == userId;
            bool isAcceptedVolunteer = await _context.VolunteerRequests.AnyAsync(v =>
                v.RequestId == requestId && v.UserId == userId && v.Status == VolunteerStatus.Accepted);

            if (!isOwner && !isAcceptedVolunteer)
            {
                await Clients.Caller.SendAsync("Unauthorized");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(requestId));

            // Track user as active in this room
            lock (_activeUsers)
            {
                _activeUsers[Context.ConnectionId] = (userId, requestId);
            }

            // Send chat history on join
            var history = await _context.ChatMessages
                .Where(m => m.RequestId == requestId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    senderId = m.SenderId,
                    senderName = m.SenderName,
                    content = m.Content,
                    sentAt = m.SentAt
                })
                .ToListAsync();

            await Clients.Caller.SendAsync("LoadHistory", history);
        }

        public async Task SendMessage(int requestId, string message)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || string.IsNullOrWhiteSpace(message)) return;

            var senderUser = await _context.Users.FindAsync(userId);
            var senderDisplayName = senderUser != null ? $"{senderUser.FirstName} {senderUser.LastName}" : "A neighbor";

            var chatMessage = new ChatMessage
            {
                RequestId = requestId,
                SenderId = userId,
                SenderName = senderDisplayName,
                Content = message,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Broadcast the message to all clients in the group
            await Clients.Group(GroupName(requestId)).SendAsync("ReceiveMessage", new
            {
                senderId = userId,
                senderName = senderDisplayName,
                content = message,
                sentAt = chatMessage.SentAt
            });

            // Find the receiver of the message
            var request = await _context.HelpRequests
                .Include(h => h.VolunteerRequests)
                .FirstOrDefaultAsync(h => h.RequestId == requestId);

            if (request == null) return;

            var volunteer = request.VolunteerRequests
                .FirstOrDefault(v => v.Status == VolunteerStatus.Accepted);

            string? receiverId = null;
            if (userId == request.UserId)
            {
                receiverId = volunteer?.UserId;
            }
            else if (volunteer != null && userId == volunteer.UserId)
            {
                receiverId = request.UserId;
            }

            if (string.IsNullOrEmpty(receiverId)) return;

            // Check if the receiver is active in this chat room
            bool isReceiverActive = false;
            lock (_activeUsers)
            {
                isReceiverActive = _activeUsers.Values.Any(u => u.UserId == receiverId && u.RequestId == requestId);
            }

            // Save and send notification if the receiver is offline or not actively viewing this room
            if (!isReceiverActive)
            {
                var bodyText = message.Length > 100 ? message.Substring(0, 100) + "..." : message;
                var notif = new Notification
                {
                    UserId = receiverId,
                    Title = $"New Message from {senderDisplayName}",
                    Body = bodyText,
                    Type = NotificationType.NewMessage,
                    RelatedUrl = $"/Chat/{requestId}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notif);
                await _context.SaveChangesAsync();

                // Send live badge notification via the global NotificationHub
                await _notificationHub.Clients.User(receiverId)
                    .SendAsync("ReceiveNotification", notif.Body);
            }
        }

        public async Task LeaveRequestRoom(int requestId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(requestId));
            lock (_activeUsers)
            {
                _activeUsers.Remove(Context.ConnectionId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_activeUsers)
            {
                _activeUsers.Remove(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
