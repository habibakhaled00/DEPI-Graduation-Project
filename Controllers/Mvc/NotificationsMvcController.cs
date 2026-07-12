using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NeighborHelp.Controllers.Mvc
{
    [Route("Notifications")]
    [Authorize]
    public class NotificationsMvcController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Views/Notifications/Index.cshtml");
    }
}
