using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MVCWebInvite.Models;
using MVCWebInvite.Utils;
using MVCWebInvite.ViewModels.Account;
using System.Reflection;

namespace MVCWebInvite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IHttpClientFactory clientFactory, ILogger<AccountController> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var auth = _clientFactory.CreateClient("AuthAPI");
            var resp = await auth.PostAsJsonAsync("login", new { username = vm.Username, password = vm.Password });

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed. Status: {StatusCode}, Response: {Error}", resp.StatusCode, error);
                ModelState.AddModelError("", "Invalid username or password.");
                return View(vm);
            }

            var payload = await resp.Content.ReadFromJsonAsync<ViewModels.Account.LoginResponseDto>();
            var tokenString = payload?.Token;

            if (string.IsNullOrWhiteSpace(tokenString))
            {
                _logger.LogError("Login response did not contain a token.");
                ModelState.AddModelError("", "Authentication failed. Please try again.");
                return View(vm);
            }

            HttpContext.Session.SetString("JWToken", tokenString);
            //TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }


        [HttpGet]
        public IActionResult SessionCheck()
        {
            var token = HttpContext.Session.GetString("JWToken");
            ViewBag.HasToken = !string.IsNullOrEmpty(token);
            ViewBag.TokenPreview = token != null && token.Length > 25 ? token.Substring(0, 25) + "..." : token;
            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWToken");
            TempData.Remove("SuccessMessage");
            TempData["SuccessMessage"] = "You’ve been logged out successfully.";
            return RedirectToAction("Index", "Home", new { area = "" });


        }
    }
}
