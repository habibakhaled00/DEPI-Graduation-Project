using NeighborHelp.Models;

namespace NeighborHelp.Services
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string title, string body,
            NotificationType type, string? relatedUrl = null);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
