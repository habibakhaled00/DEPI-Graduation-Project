using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NeighborHelp.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // No custom client-callable methods needed yet.
        // The server pushes notifications to specific users via IHubContext<NotificationHub>
        // from VolunteerController (e.g. "a volunteer applied", "you were accepted").
    }
}
