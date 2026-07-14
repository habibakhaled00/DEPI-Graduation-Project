using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NeighborHelp.Controllers.Mvc
{
    [Route("Rate")]
    [Authorize]
    public class RatingsMvcController : Controller
    {
        [HttpGet("{requestId}")]
        public IActionResult Index(int requestId)
        {
            ViewBag.RequestId = requestId;
            return View("~/Views/Rate/Index.cshtml");
        }
    }
}
