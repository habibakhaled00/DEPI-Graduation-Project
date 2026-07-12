using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NeighborHelp.Controllers.Mvc
{
    [Route("Account")]
    public class AccountController : Controller
    {
        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return Redirect("/Admin/Dashboard");
                return Redirect("/HelpRequests");
            }
            return View();
        }

        [HttpGet("Register")]
        public IActionResult Register() => View();

        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword() => View();

        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword(string email, string token)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpGet("Profile")]
        [HttpGet("Profile/{userId?}")]
        [Authorize]
        public IActionResult Profile(string? userId)
        {
            ViewBag.UserId = userId;
            return View();
        }

        [HttpGet("Settings")]
        [Authorize]
        public IActionResult Settings() => View();
    }
}
