using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;
using MVCWebInvite.Utils;
using MVCWebInvite.ViewModels.Admin;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MVCWebInvite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        //private readonly IHttpClientFactory _http;
        private readonly IAuthorizedClientProvider _authorizedClientProvider;
        private readonly ILogger<DashboardController> _logger;
        private const string Resource = "bookings";

        public DashboardController(IAuthorizedClientProvider authorizedClientProvider, ILogger<DashboardController> logger)
        {
            _authorizedClientProvider = authorizedClientProvider;
            _logger = logger;
        }

        // helper: don't throw on non-200; tell us what failed
        private static async Task<(bool ok, int code, string? err, List<T> data)>
            GetListSafe<T>(HttpClient c, string path)
        {
            var resp = await c.GetAsync(path);
            if (!resp.IsSuccessStatusCode)
                return (false, (int)resp.StatusCode, $"{(int)resp.StatusCode} {resp.ReasonPhrase}", new());
            var data = await resp.Content.ReadFromJsonAsync<List<T>>() ?? new();
            return (true, 200, null, data);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var vm = new DashboardViewModel();

            try
            {
                var api = _authorizedClientProvider.GetClient();

                // adjust paths to YOUR API routes (e.g. "Guests", "Bookings" etc.)
                var g = await GetListSafe<Guest>(api, "guest");
                var b = await GetListSafe<Booking>(api, "bookings");
                var t = await GetListSafe<Table>(api, "tables");
                var m = await GetListSafe<Menu>(api, "menu");

                if (!g.ok || !b.ok || !t.ok || !m.ok)
                {
                    _logger.LogWarning("Dashboard load issues: guests={G} bookings={B} tables={T} menu={M}",
                        g.err, b.err, t.err, m.err);
                    // keep this message local to the dashboard
                    TempData["DashboardError"] = "Failed when loading dashboard data. Please try again later.";
                }

                vm.TotalGuests = g.data.Count;
                vm.TotalBookings = b.data.Count;
                vm.AvailableTables = t.data.Count; // or t.data.Count(x => x.IsAvailable)
                vm.MenuItems = m.data.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while loading dashboard");
                TempData["DashboardError"] = "Failed when loading dashboard data. Please try again later.";
            }

            return View(vm);
        }
    }
}
