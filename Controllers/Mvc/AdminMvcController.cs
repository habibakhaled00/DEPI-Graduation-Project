using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NeighborHelp.Controllers.Mvc
{
    [Route("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminMvcController : Controller
    {
        [HttpGet("")]
        [HttpGet("Dashboard")]
        public IActionResult Dashboard() => View();

        [HttpGet("Users")]
        public IActionResult Users() => View();

        [HttpGet("Reports")]
        public IActionResult Reports() => View();
    }
}
