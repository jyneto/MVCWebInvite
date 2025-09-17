using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.ViewModels.Admin;

namespace MVCWebInvite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        //[HttpGet]
        //public IActionResult Index()
        //{
        //    var token = HttpContext.Session.GetString("JWToken");
        //    if (string.IsNullOrWhiteSpace(token))
        //    {
        //        return RedirectToAction("Login", "Account", new {area ="Admin"});
        //    }
        //    ViewBag.Token = token;
        //    return View();
        //}

        [HttpGet]
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var vm = new DashboardViewModel
            {
                TotalGuests = 0,
                TotalBookings = 0,
                AvailableTables = 0,
                MenuItems = 0
            };

            return View(vm); // ← VIKTIGT
        }
    }
}
