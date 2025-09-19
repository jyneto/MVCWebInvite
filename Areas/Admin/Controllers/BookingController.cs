using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;
using MVCWebInvite.Utils;


namespace MVCWebInvite.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class BookingController : Controller
    {


        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<BookingController> _logger;

        private const string Resource = "bookings"; // matcha din WebAPI-route

        public BookingController(IHttpClientFactory clientFactory, ILogger<BookingController> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        private HttpClient CreateAuthorizedClient()
        {
            var token = HttpContext.Session.GetString("JWToken");
            _logger.LogInformation("CreateAuthorizedClient called. Token: {HasToken}", string.IsNullOrEmpty(token) ? "null or empty" : "token");

            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("JWT token is missing in session.");
            if (JwtUtils.IsJwtExpired(token))
                throw new InvalidOperationException("JWT token has expired.");

            var client = _clientFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private IActionResult RedirectLogin(string? message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                TempData["ErrorMessage"] = message;
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            try
            {
                var api = CreateAuthorizedClient();
                var resp = await api.GetAsync(Resource);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to load guests. {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    return View(new List<Booking>());
                }
                var items = await resp.Content.ReadFromJsonAsync<List<Booking>>() ?? new();
                return View(items);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Index: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Index HTTP error"); TempData["ErrorMessage"] = "Network error when loading guests."; return View(new List<Booking>()); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Index error"); TempData["ErrorMessage"] = "Unexpected error."; return View(new List<Booking>()); }
        }

        // CREATE
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var api = CreateAuthorizedClient();
                var resp = await api.PostAsJsonAsync(Resource, model);
                if (!resp.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", $"Failed to create guest. {(int)resp.StatusCode} {resp.ReasonPhrase}");
                    return View(model);
                }
                TempData["SuccessMessage"] = "Guest created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Create: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Create HTTP error"); ModelState.AddModelError("", "Network error."); return View(model); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Create error"); ModelState.AddModelError("", "Unexpected error."); return View(model); }
        }

        // EDIT
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var api = CreateAuthorizedClient();
                var resp = await api.GetAsync($"{Resource}/{id}");
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to fetch guest. {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    return RedirectToAction(nameof(Index));
                }
                var item = await resp.Content.ReadFromJsonAsync<Booking>();
                if (item == null) { TempData["ErrorMessage"] = "Guest not found."; return RedirectToAction(nameof(Index)); }
                return View(item);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Edit GET: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Edit GET HTTP error"); TempData["ErrorMessage"] = "Network error."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Edit GET error"); TempData["ErrorMessage"] = "Unexpected error."; return RedirectToAction(nameof(Index)); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Booking model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var api = CreateAuthorizedClient();
                var resp = await api.PutAsJsonAsync($"{Resource}/{model.Id}", model);
                if (!resp.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", $"Failed to update guest. {(int)resp.StatusCode} {resp.ReasonPhrase}");
                    return View(model);
                }
                TempData["SuccessMessage"] = "Guest updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Edit POST: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Edit POST HTTP error"); ModelState.AddModelError("", "Network error."); return View(model); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Edit POST error"); ModelState.AddModelError("", "Unexpected error."); return View(model); }
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var api = CreateAuthorizedClient();
                var resp = await api.DeleteAsync($"{Resource}/{id}");
                if (!resp.IsSuccessStatusCode)
                    TempData["ErrorMessage"] = $"Failed to delete guest. {(int)resp.StatusCode} {resp.ReasonPhrase}";
                else
                    TempData["SuccessMessage"] = "Guest deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Delete: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Delete HTTP error"); TempData["ErrorMessage"] = "Network error."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Delete error"); TempData["ErrorMessage"] = "Unexpected error."; return RedirectToAction(nameof(Index)); }
        }
    }
}
