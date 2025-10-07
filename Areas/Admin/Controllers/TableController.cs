using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MVCWebInvite.Models;
using MVCWebInvite.Utils;
using System.Security.Cryptography;

namespace MVCWebInvite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TableController : Controller
    {

        //private readonly IHttpClientFactory _clientFactory;
        private readonly IAuthorizedClientProvider _authorizedClientProvider;
        private readonly ILogger<TableController> _logger;

        private const string Resource = "tables"; // matcha din WebAPI-route
        public TableController(IAuthorizedClientProvider authorizedClientProvider, ILogger<TableController> logger)
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
                var api = _authorizedClientProvider.GetClient ();
                var resp = await api.GetAsync(Resource);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to load guests. {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    return View(new List<Table>());
                }
                var items = await resp.Content.ReadFromJsonAsync<List<Table>>() ?? new();
                return View(items);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Guest/Index: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Guest/Index HTTP error"); TempData["ErrorMessage"] = "Network error when loading guests."; return View(new List<Table>()); }
            catch (Exception ex) { _logger.LogError(ex, "Guest/Index error"); TempData["ErrorMessage"] = "Unexpected error."; return View(new List<Table>()); }
        }

        // CREATE
        [HttpGet]
        public IActionResult Create() => View(new Table());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Table model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.PostAsJsonAsync(Resource, model);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Failed to create table. {(int)resp.StatusCode} {resp.ReasonPhrase}");
                    return View(model);
                }
                TempData["SuccessMessage"] = "Table created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Table/Create: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Table/Create HTTP error"); ModelState.AddModelError("", "Network error."); return View(model); }
            catch (Exception ex) { _logger.LogError(ex, "Table/Create error"); ModelState.AddModelError("", "Unexpected error."); return View(model); }
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
                    TempData["ErrorMessage"] = $"Failed to fetch table. {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    return RedirectToAction(nameof(Index));
                }
                var item = await resp.Content.ReadFromJsonAsync<Table>();
                if (item == null) { TempData["ErrorMessage"] = "Table not found."; return RedirectToAction(nameof(Index)); }
                return View(item);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Table/Edit GET: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Table/Edit GET HTTP error"); TempData["ErrorMessage"] = "Network error."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) { _logger.LogError(ex, "Table/Edit GET error"); TempData["ErrorMessage"] = "Unexpected error."; return RedirectToAction(nameof(Index)); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Table model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.PutAsJsonAsync($"{Resource}/{model.Id}", model);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Failed to update table. {(int)resp.StatusCode} {resp.ReasonPhrase}");
                    return View(model);
                }
                TempData["SuccessMessage"] = "Table updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Table/Edit POST: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Table/Edit POST HTTP error"); ModelState.AddModelError("", "Network error."); return View(model); }
            catch (Exception ex) { _logger.LogError(ex, "Table/Edit POST error"); ModelState.AddModelError("", "Unexpected error."); return View(model); }
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

                if (resp.IsSuccessStatusCode) 
                {
                    TempData["SuccessMessage"] = "Table deleted";
                }
                else 
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to delete table. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                
                }
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Table/Delete error");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
