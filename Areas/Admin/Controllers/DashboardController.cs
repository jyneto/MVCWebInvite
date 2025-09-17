using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;
using MVCWebInvite.ViewModels.Admin;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MVCWebInvite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardController> _logger;
        public DashboardController(IHttpClientFactory httpClientFactory, ILogger<DashboardController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        private HttpClient CreateAutorizationedClient(string clientName, string token) 
        {
            var client = _httpClientFactory.CreateClient(clientName);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
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
                var guestClient = CreateAutorizationedClient("GuestAPI", token);
                var bookingClient = CreateAutorizationedClient("BookingAPI", token);
                var tableClient = CreateAutorizationedClient("TableAPI", token);
                var menuClient = CreateAutorizationedClient("MenuAPI", token);

                var guests = await guestClient.GetFromJsonAsync<List<Guest>>("guests") ?? new();
                var bookings = await bookingClient.GetFromJsonAsync<List<Booking>>("bookings") ?? new();
                var tables = await tableClient.GetFromJsonAsync<List<Table>>("tables") ?? new();
                var menuItems = await menuClient.GetFromJsonAsync<List<Menu>>("menu") ?? new();

                vm.TotalGuests = guests.Count;
                vm.TotalBookings = bookings.Count;
                vm.AvailableTables = tables.Count;
                vm.MenuItems = menuItems.Count;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while loading dashboard data");
                TempData["ErrorMessage"] = "Failed when loading dashboard data. Please try again later.";
            }
            catch(NotSupportedException ex)
            {
                _logger.LogError(ex, "The content type is not supported");
                TempData["ErrorMessage"] = "Invalid server response";

            }
            catch(System.Text.Json.JsonException ex)
            {  
                _logger.LogError(ex, "Failed to parse error JSON response");
                TempData["ErrorMessage"] = "Could not read server response";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard data");
                TempData["ErrorMessage"] = "An unexpected error occurred when loading dasboard.";
            }
          
            return View(vm); 
        }
    }
}
