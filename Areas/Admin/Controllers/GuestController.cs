//using Microsoft.AspNetCore.Mvc;
//using MVCWebInvite.Models;
//using MVCWebInvite.Utils;
//using System.Threading.Tasks;
//using System.Net.Http.Json;
//namespace MVCWebInvite.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    public class GuestController : Controller
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly ILogger<GuestController> _logger;
//        private const string Resource = "guests";
//        public GuestController(IHttpClientFactory clientFactory, ILogger<GuestController> logger)
//        {
//            _httpClientFactory = clientFactory;
//            _logger = logger;
//        }

//        public HttpClient CreateAuthorizedClient()
//        {
//            var token = HttpContext.Session.GetString("JWToken");
//            _logger.LogInformation("CreateAuthorizedClient called. Token: {HasToken}", string.IsNullOrEmpty(token) ? "null or empty" : "token");
//            if (string.IsNullOrWhiteSpace(token))
//                throw new InvalidOperationException("JWT token is missing in session.");

//            if (JwtUtils.IsJwtExpired(token))
//                throw new InvalidOperationException("JWT token has expired.");
//            //var client = _clientFactory.CreateClient("MenuAPI");
//            var client = _httpClientFactory.CreateClient("BackendAPI");


//            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
//            return client;
//        }
//        public IActionResult RedirectLogin(string? message = null)
//        {
//            if (!string.IsNullOrWhiteSpace(message))
//                TempData["ErrorMessage"] = message;
//            return RedirectToAction("Login", "Account", new { area = "" });
//        }


//        [HttpGet]
//        public async Task<IActionResult> Index()
//        {
//            try
//            {
//                var apiClient = CreateAuthorizedClient();
//                var response = await apiClient.GetAsync("guests");
//                if (!response.IsSuccessStatusCode)
//                {
//                    TempData["ErrorMessage"] = $"Failed to load guests. {(int)response.StatusCode} {response.ReasonPhrase}";
//                    return View(new List<Guest>());
//                }
//                var items = await response.Content.ReadFromJsonAsync<List<Guest>>() ?? new();
//                return View(items);
//            }
//            catch (InvalidOperationException ex)
//            {
//                _logger.LogWarning("Token problem on Menu/Index");
//                return RedirectLogin(ex.Message);
//            }
//            catch (HttpRequestException ex)
//            {
//                _logger.LogError(ex, "Unexpected error in Menu/Index");
//                TempData["ErrorMessage"] = "An unexpected error occurred while loading menu items.";
//                return View(new List<Guest>());
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unexpected error in Menu/Index");
//                TempData["ErrorMessage"] = "An unexpected error occurred while loading menu items.";
//                return View(new List<Guest>());
//            }

//        }

//        [HttpGet]
//        public IActionResult Create()
//        {
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Guest guest)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(guest);
//            }
//            try
//            {
//                var apiClient = CreateAuthorizedClient();
//                var response = await apiClient.PostAsJsonAsync("guest", guest);
//                if (response.IsSuccessStatusCode)
//                {
//                    TempData["SuccessMessage"] = "Guest created successfully.";
//                    return RedirectToAction(nameof(Index));
//                }
//                else
//                {
//                    TempData["ErrorMessage"] = $"Failed to create guest. {(int)response.StatusCode} {response.ReasonPhrase}";
//                    return View(guest);
//                }
//            }
//            catch (InvalidOperationException ex)
//            {
//                _logger.LogWarning("Token problem on Guest/Create");
//                return RedirectLogin(ex.Message);
//            }
//            catch (HttpRequestException ex)
//            {
//                _logger.LogError(ex, "Unexpected error in Guest/Create");
//                TempData["ErrorMessage"] = "An unexpected error occurred while creating the guest.";
//                return View(guest);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unexpected error in Guest/Create");
//                TempData["ErrorMessage"] = "An unexpected error occurred while creating the guest.";
//                return View(guest);
//            }
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;
using MVCWebInvite.Utils;
using System.Net.Http.Json;

namespace MVCWebInvite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GuestController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<GuestController> _logger;

        private const string Resource = "guest"; // matcha din WebAPI-route

        public GuestController(IHttpClientFactory clientFactory, ILogger<GuestController> logger)
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
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guest model)
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
                var item = await resp.Content.ReadFromJsonAsync<Guest>();
                if (item == null) { TempData["ErrorMessage"] = "Guest not found."; return RedirectToAction(nameof(Index)); }
                return View(item);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Edit GET: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Edit GET HTTP error"); TempData["ErrorMessage"] = "Network error."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Edit GET error"); TempData["ErrorMessage"] = "Unexpected error."; return RedirectToAction(nameof(Index)); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guest model)
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
