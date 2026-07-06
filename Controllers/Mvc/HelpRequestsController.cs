using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;

namespace NeighborHelp.Controllers.Mvc
{
    // MVC controller responsible for serving Razor Views under /HelpRequests/*.
    // Actual data CRUD goes through NeighborHelp.Controllers.HelpRequestsApiController (api/HelpRequests)
    // via JS/fetch calls from these views. Lives in its own namespace, and the API controller
    // is named differently (HelpRequestsApiController), so the two don't collide on the
    // route-derived "controller" name used by MVC action/link matching.
    [Route("HelpRequests")]
    public class HelpRequestsController : Controller
    {
        private readonly AppDbContext _context;

        public HelpRequestsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/")]
        [HttpGet("")]
        [HttpGet("Index")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        [HttpGet("Create")]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        [HttpGet("Details/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.HelpRequests.FindAsync(id);
            if (request == null) return NotFound();

            ViewBag.RequestId = id;
            return View();
        }

        [HttpGet("ManageVolunteers/{id:int}")]
        [Authorize]
        public async Task<IActionResult> ManageVolunteers(int id)
        {
            var request = await _context.HelpRequests.FindAsync(id);
            if (request == null) return NotFound();

            ViewBag.RequestId = id;
            ViewBag.RequestTitle = request.Title;
            return View();
        }
    }
}
