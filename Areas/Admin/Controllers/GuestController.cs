using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;
using MVCWebInvite.Utils;
using MVCWebInvite.ViewModels.Admin;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MVCWebInvite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GuestController : Controller
    {
        //private readonly IHttpClientFactory _clientFactory;
        private readonly IAuthorizedClientProvider _authorizedClientProvider;
        private readonly ILogger<GuestController> _logger;

        private const string Resource = "guest"; // matcha din WebAPI-route

        //public GuestController(IHttpClientFactory clientFactory, ILogger<GuestController> logger)
        //{
        //    _clientFactory = clientFactory;
        //    _logger = logger;
        //}

        //private HttpClient CreateAuthorizedClient()
        //{
        //    var token = HttpContext.Session.GetString("JWToken");
        //    _logger.LogInformation("CreateAuthorizedClient called. Token: {HasToken}", string.IsNullOrEmpty(token) ? "null or empty" : "token");

        //    if (string.IsNullOrWhiteSpace(token))
        //        throw new InvalidOperationException("JWT token is missing in session.");
        //    if (JwtUtils.IsJwtExpired(token))
        //        throw new InvalidOperationException("JWT token has expired.");

        //    var client = _clientFactory.CreateClient("BackendAPI");
        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //    return client;
        //}
        public GuestController(IAuthorizedClientProvider authorizedClientProvider, ILogger<GuestController> logger)
        {
            _authorizedClientProvider = authorizedClientProvider;
            _logger = logger;
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
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.GetAsync(Resource);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to load guests. {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    return View(new List<Guest>());
                }
                var items = await resp.Content.ReadFromJsonAsync<List<Guest>>() ?? new();
                return View(items);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Index: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Index HTTP error"); TempData["ErrorMessage"] = "Network error when loading guests."; return View(new List<Guest>()); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Index error"); TempData["ErrorMessage"] = "Unexpected error."; return View(new List<Guest>()); }
        }

        // CREATE
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var tables = await GetAvailableTables();
            var viewModel = new GuestFormViewModel
            {
                Guest = new Guest(),
                AvailableTables = tables
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GuestFormViewModel vModel)
        {
            if (!ModelState.IsValid)
            {
                vModel.AvailableTables = await GetAvailableTables();
                return View(vModel);
            }
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.PostAsJsonAsync(Resource, vModel.Guest);
                if (!resp.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", $"Failed to create guest. {(int)resp.StatusCode} {resp.ReasonPhrase}");
                    vModel.AvailableTables = await GetAvailableTables();
                    return View(vModel);
                }
                TempData["SuccessMessage"] = "Guest created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Create: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Create HTTP error"); ModelState.AddModelError("", "Network error."); return View(vModel); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Create error"); ModelState.AddModelError("", "Unexpected error."); return View(vModel); }
        }

        // EDIT
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.GetAsync($"{Resource}/{id}");
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to fetch guest. {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    return RedirectToAction(nameof(Index));
                }
                var item = await resp.Content.ReadFromJsonAsync<Guest>();
                if (item == null) { TempData["ErrorMessage"] = "Guest not found."; return RedirectToAction(nameof(Index)); }

                var tables = await GetAvailableTables();
                var viewModel = new GuestFormViewModel()
                {
                    Guest = item,
                    AvailableTables = tables
                };
                return View(viewModel);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Edit GET: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Edit GET HTTP error"); TempData["ErrorMessage"] = "Network error."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Edit GET error"); TempData["ErrorMessage"] = "Unexpected error."; return RedirectToAction(nameof(Index)); }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GuestFormViewModel vModel)
        {
            if (!ModelState.IsValid) return View(vModel);
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.PutAsJsonAsync($"{Resource}/{vModel.Guest.Id}", vModel.Guest);
                if (!resp.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", $"Failed to update guest. {(int)resp.StatusCode} {resp.ReasonPhrase}");
                    return View(vModel);
                }
                TempData["SuccessMessage"] = "Guest updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Edit POST: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Edit POST HTTP error"); ModelState.AddModelError("", "Network error."); return View(vModel); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Edit POST error"); ModelState.AddModelError("", "Unexpected error."); return View(vModel); }
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var api = _authorizedClientProvider.GetClient();
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

        public async Task<IActionResult> SubmitRSVP(string guestName, bool IsAttending)
        {
            var response = new RsvpResponse { GuestName = guestName, IsAttending = IsAttending };
            var client = _authorizedClientProvider.GetClient(); 
            await client.PostAsJsonAsync("api/rsvp",response);
            return View("ThankYou");
        }
        public async Task<IActionResult> RsvpForm()
        {
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.GetAsync(Resource);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to load guests. {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    return View(new List<Guest>());
                }
                var items = await resp.Content.ReadFromJsonAsync<List<Guest>>() ?? new();
                return View(items);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Index: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Index HTTP error"); TempData["ErrorMessage"] = "Network error when loading guests."; return View(new List<Guest>()); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Index error"); TempData["ErrorMessage"] = "Unexpected error."; return View(new List<Guest>()); }
        }
        private async Task<List<Table>> GetAvailableTables()
        {
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.GetAsync("tables");
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load tables. Status: {Status} {Reason}", (int)resp.StatusCode, resp.ReasonPhrase);
                    return new List<Table>();
                }
                var items = await resp.Content.ReadFromJsonAsync<List<Table>>() ?? new();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tables");
                return new List<Table>();
            }
        }

    }
}
