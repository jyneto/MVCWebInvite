using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;
using MVCWebInvite.Utils;
using MVCWebInvite.ViewModels;
using MVCWebInvite.ViewModels.Menu;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MVCWebInvite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthorizedClientProvider _authorizedClientProvider;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IAuthorizedClientProvider authorizedClientProvider)
        {
            _authorizedClientProvider = authorizedClientProvider;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new HomePageVm();

            try
            {
                var apiClient = _authorizedClientProvider.GetAnonClient();
                var items = await apiClient.GetAsync("menu");
                if (items.IsSuccessStatusCode)
                {
                    vm.Menu = await items.Content.ReadFromJsonAsync<List<MenuItemVm>>() ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get menu from API");
                TempData["ErrorMessage"] = $"Failed to load menu items. {ex.Message}";
            }

            vm.RsvpForm = new RsvpFormVm();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRSVP(RsvpFormVm rsvpForm)
        {
            var vm = new HomePageVm { RsvpForm = rsvpForm };

            try
            {
                var apiClient = _authorizedClientProvider.GetAnonClient();
                var items = await apiClient.GetAsync("menu");
                if (items.IsSuccessStatusCode)
                {
                    vm.Menu = await items.Content.ReadFromJsonAsync<List<MenuItemVm>>() ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Menu reload failed");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix the form errors.";
                return View("Index", vm);
            }

            try
            {
                var apiClient = _authorizedClientProvider.GetAnonClient();
                var response = await apiClient.PostAsJsonAsync("guest", rsvpForm);
                if (response.IsSuccessStatusCode)
                {
                    TempData["RsvpSuccess"] = true;
                    TempData["IsAttending"] = rsvpForm.IsAttending;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = $"RSVP failed: {(int)response.StatusCode} {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RSVP submission error");
                TempData["ErrorMessage"] = $"RSVP failed: {ex.Message}";
            }

            return View("Index", vm);
        }


        [HttpGet]
        public IActionResult ThnakYou()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


         
    }
}
