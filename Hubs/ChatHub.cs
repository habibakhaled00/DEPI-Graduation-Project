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

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        private static string GroupName(int requestId) => $"request-{requestId}";

        // Called by client after connecting to join the room for a specific request
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
            var userName = Context.User?.Identity?.Name ?? "User";

            if (userId == null || string.IsNullOrWhiteSpace(message)) return;

            var chatMessage = new ChatMessage
            {
                RequestId = requestId,
                SenderId = userId,
                SenderName = userName,
                Content = message,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Group(GroupName(requestId)).SendAsync("ReceiveMessage", new
            {
                senderId = userId,
                senderName = userName,
                content = message,
                sentAt = chatMessage.SentAt
            });
        }

        public async Task LeaveRequestRoom(int requestId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(requestId));
        }
    }
}
