using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;


namespace MVCWebInvite.Areas.Admin.Controllers
{
    public class BookingController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
