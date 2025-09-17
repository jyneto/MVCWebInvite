using MVCWebInvite.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

//namespace MVCWebInvite.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    public class MenuController : Controller
//    {
//        private readonly IHttpClientFactory _clientFactory;
//        private readonly ILogger<MenuController> _logger;
//        public MenuController(IHttpClientFactory clientFactory, ILogger<MenuController> logger)
//        {
//            _clientFactory = clientFactory;
//            _logger = logger;
//        }
//        private HttpClient CreateAuthorizedClient()
//        {
//            var token = HttpContext.Session.GetString("JWToken");
//            _logger.LogInformation("CreateAuthorizedClient called. Token: {HasToken}", string.IsNullOrEmpty(token) ? "null or empty" : "token");

//            if(string.IsNullOrWhiteSpace(token))
//                throw new InvalidOperationException("JWT token is missing in session.");

//            if(JwtUtils.IsJwtExpired(token))
//                throw new InvalidOperationException("JWT token has expired.");

//            var client = _clientFactory.CreateClient("MenuAPI");
//            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
//            return client;

//        }

//        public IActionResult Index()
//        {
//            var client = CreateAuthorizedClient();
//            var response = client.GetAsync("menu").Result;
//            if (!response.IsSuccessStatusCode)
//                return View("Error");

//            var menuItems = response.Content.ReadFromJsonAsync<List<Menu>>().Result;
//            return View(menuItems);
//        }

//        [HttpGet]
//        public IActionResult Create()
//        {
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Menu menuModel)
//        {
//            if (!ModelState.IsValid) return View(menuModel);
//            var client = CreateAuthorizedClient();
//            var response = await client.PostAsJsonAsync("menu", menuModel);
//            if (!response.IsSuccessStatusCode)
//            {
//                ModelState.AddModelError("", "Failed to create menu item.");
//                return View(menuModel);
//            }
//            ModelState.AddModelError("", "Menu item created successfully.");
//            return RedirectToAction("Login","Account", new {area = "Admin"});
//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            try
//            {
//                var client = CreateAuthorizedClient();
//                var response = await client.GetAsync($"menu/{id}");

//                if (!response.IsSuccessStatusCode)
//                {
//                    var body = await response.Content.ReadAsStringAsync();
//                    TempData["ErrorMessage"] = $"Failed to fetch menu item. {(int)response.StatusCode} {response.ReasonPhrase}" +
//                                               (string.IsNullOrWhiteSpace(body) ? "" : $" | API: {body}");

//                    // 401/403? skicka till login
//                    if ((int)response.StatusCode == 401 || (int)response.StatusCode == 403)
//                        return RedirectToAction("Login", "Admin");

//                    return RedirectToAction("Login", "Account", new { area = "Admin" });
//                }

//                var item = await response.Content.ReadFromJsonAsync<Menu>();
//                if (item == null)
//                {
//                    TempData["ErrorMessage"] = "Menu item not found.";
//            return RedirectToAction("Login","Account", new {area = "Admin"});
//                }

//                return View(item);
//            }
//            catch (InvalidOperationException ex)
//            {
//                TempData["ErrorMessage"] = ex.Message;
//                return RedirectToAction("Login", "Account", new { area = "Admin" });
//            }
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(Menu menuModel)
//        {
//            if (!ModelState.IsValid) return View(menuModel);
//            var client = CreateAuthorizedClient();

//            var updateDto = new Menu
//            {
//                Id = menuModel.Id,
//                Name = menuModel.Name,
//                Description = menuModel.Description,
//                Price = menuModel.Price,
//            };

//            var response = await client.PutAsJsonAsync($"menu/{menuModel.Id}", updateDto);

//            if (!response.IsSuccessStatusCode)
//            {
//                ModelState.AddModelError("", "Failed to update menu item.");
//                return View(menuModel);
//            }
//            TempData["SuccessMessage"] = "Menu item updated successfully.";
//            return RedirectToAction("Login", "Account", new { area = "Admin" });
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int id)
//        {

//            var client = CreateAuthorizedClient();
//            var response = await client.DeleteAsync($"menu/{id}");
//            if (!response.IsSuccessStatusCode)
//            {
//                TempData["ErrorMessage"] = $"Failed to delete menu item. {(int)response.StatusCode} {response.ReasonPhrase}";
//            }
//            else
//            {
//                TempData["SuccessMessage"] = "Menu item deleted successfully.";
//            }
//            return RedirectToAction("Login", "Account", new { area = "Admin" });
//        }


//    }
//}

namespace MVCWebInvite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<MenuController> _logger;
        public MenuController(IHttpClientFactory clientFactory, ILogger<MenuController> logger)
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
            var client = _clientFactory.CreateClient("MenuAPI");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private IActionResult RedirectLogin(string? message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                TempData["ErrorMessage"] = message;

            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var client = CreateAuthorizedClient();
                var response = client.GetAsync("menu").Result;
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to load menu items. {(int)response.StatusCode} {response.ReasonPhrase}";
                    return View(new List<Menu>());
                }

                var menuItems = await response.Content.ReadFromJsonAsync<List<Menu>>() ?? new List<Menu>();

                return View(menuItems);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Token problem on Menu/Index");
                return RedirectLogin(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Index");
                TempData["ErrorMessage"] = "An unexpected error occurred while loading menu items.";
                return View(new List<Menu>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Index");
                TempData["ErrorMessage"] = "An unexpected error occurred while loading menu items.";
                return View(new List<Menu>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                _ = CreateAuthorizedClient();
                return View();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Token problem on Menu/Create GET");
                return RedirectLogin(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Menu menuModel)
        {
            if (!ModelState.IsValid)
                return View(menuModel);
            try
            {
                var client = CreateAuthorizedClient();

                var response = await client.PostAsJsonAsync("menu", menuModel);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, $"Failed to create menu item. {(int)response.StatusCode} {response.ReasonPhrase}");
                    return View(menuModel);
                }
                TempData["SuccessMessage"] = "Menu item created successfully.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Token problem on Menu/Create POST");
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectLogin(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Create POST");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the menu item.");
                return View(menuModel);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var client = CreateAuthorizedClient();

                var response = await client.GetAsync($"menu/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to fetch menu item. {(int)response.StatusCode} {response.ReasonPhrase}" +
                                               (string.IsNullOrWhiteSpace(body) ? "" : $" | API: {body}");
                    // 401/403? skicka till login
                    if ((int)response.StatusCode == 401 || (int)response.StatusCode == 403)
                        return RedirectToAction("Login", "Account", new { area = "Admin" });
                    return RedirectToAction("Index");
                }

                var item = await response.Content.ReadFromJsonAsync<Menu>();
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Menu item not found.";
                    return RedirectToAction("Index");
                }
                return View(item);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Token problem on Menu/Edit GET");
                return RedirectLogin(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Edit GET");
                TempData["ErrorMessage"] = "An unexpected error occurred while fetching the menu item.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Edit GET");
                TempData["ErrorMessage"] = "An unexpected error occurred while fetching the menu item.";
                return RedirectToAction("Index");
            }
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Menu menuModel)
        {
            if (!ModelState.IsValid) return View(menuModel);
            try
            {
                var client = CreateAuthorizedClient();
                var response = await client.PutAsJsonAsync($"menu/{menuModel.Id}", new Menu

                {
                    Id = menuModel.Id,
                    Name = menuModel.Name,
                    Description = menuModel.Description,
                    Price = menuModel.Price,

                });

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, $"Failed to update menu item. {(int)response.StatusCode} {response.ReasonPhrase}");
                    return View(menuModel);
                }
                TempData["SuccessMessage"] = "Menu item updated successfully.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Token problem on Menu/Edit POST");
                return RedirectLogin(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Edit POST");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the menu item.");
                return View(menuModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Edit POST");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the menu item.");
                return View(menuModel);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = CreateAuthorizedClient();

                var response = await client.DeleteAsync($"menu/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to delete menu item. {(int)response.StatusCode} {response.ReasonPhrase}";
                }
                else
                {
                    TempData["SuccessMessage"] = "Menu item deleted successfully.";

                }
                return RedirectLogin("Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Token problem on Menu/Delete POST");
                return RedirectLogin(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Delete POST");
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the menu item.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Menu/Delete POST");
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the menu item.";
                return RedirectToAction("Index");
            }
        }   
    }
}    