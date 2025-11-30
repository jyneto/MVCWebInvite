using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCWebInvite.Models;
using MVCWebInvite.Models.ApiDtos;
using MVCWebInvite.Utils;
using MVCWebInvite.ViewModels.Admin;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;


namespace MVCWebInvite.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class BookingController : Controller
    {
        private readonly IAuthorizedClientProvider _authorizedClientProvider;
        private readonly ILogger<BookingController> _logger;

        private const string Resource = "bookings"; // matcha din WebAPI-route

        public BookingController(IAuthorizedClientProvider authorizedClientProvider, ILogger<BookingController> logger)
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

       

        public async Task<IActionResult> Index()
        {
            var api = _authorizedClientProvider.GetClient();
            var bookings = await api.GetFromJsonAsync<List<Booking>>(Resource) ?? new List<Booking>();
            //var tables = await api.GetFromJsonAsync<List<Table>>("tables") ?? new List<Table>();
            //var guests = await api.GetFromJsonAsync<List<Guest>>("guest") ?? new List<Guest>();

            var vm = bookings.Select(b => new BookingListItemVm
            {
                Id = b.Id,
                TableId = b.FK_TableId,
                //TableNumber = tables.FirstOrDefault(t => t.Id == b.FK_TableId)?.TableNumber.ToString(),
                TableNumber = b.TableNumber?.ToString(),
                GuestId = b.FK_GuestId,
                //GuestName = guests.FirstOrDefault(g => g.Id == b.FK_GuestId)?.FullName,
                GuestName = b.GuestName,
                StartTime = b.StartTime.ToLocalTime(),
                EndTime = b.EndTime.ToLocalTime(),
                PartySize = b.PartySize
            }).ToList();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? tableId, int? guestId)
        {
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var tables = await api.GetFromJsonAsync<List<Table>>("tables") ?? new();
                var guests = await api.GetFromJsonAsync<List<Guest>>("guest") ?? new();

                var vm = new BookingFormViewModel
                {
                    TableId = tableId ?? 0,
                    GuestId = guestId ?? 0,
                    StartTime = DateTime.Now,
                    TableOptions = tables.Select(t => new SelectListItem($"Table {t.TableNumber} (cap {t.Capacity})", t.Id.ToString(), t.Id == tableId)),
                    GuestOptions = guests.Select(g => new SelectListItem(g.FullName ?? $"Guest {g.Id}", g.Id.ToString(), g.Id == guestId))
                };
                return View(vm);
            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Booking/Create GET: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Booking/Create GET HTTP error"); TempData["ErrorMessage"] = "Network error when loading form data."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) { _logger.LogError(ex, "Booking/Create GET error"); TempData["ErrorMessage"] = "Unexpected error."; return RedirectToAction(nameof(Index)); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var apiX = _authorizedClientProvider.GetClient();
                var tablesX = await apiX.GetFromJsonAsync<List<Table>>("tables") ?? new();
                var guestsX = await apiX.GetFromJsonAsync<List<Guest>>("guest") ?? new();
                vm.TableOptions = tablesX.Select(t => new SelectListItem($"Table {t.TableNumber} (cap {t.Capacity})", t.Id.ToString(), t.Id == vm.TableId));
                vm.GuestOptions = guestsX.Select(g => new SelectListItem(g.FullName ?? $"Guest {g.Id}", g.Id.ToString(), g.Id == vm.GuestId));
                return View(vm);
            }

            try
            {
                var api = _authorizedClientProvider.GetClient();
                var newBooking = new BookingCreateDto
                {
                    TableId = vm.TableId,
                    GuestId = vm.GuestId,
                    StartTime = vm.StartTime.ToUniversalTime(),
                    PartySize = vm.PartySize
                };
                var resp = await api.PostAsJsonAsync(Resource, newBooking);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Failed to create booking. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}");
                    var tablesX = await api.GetFromJsonAsync<List<Table>>("tables") ?? new();
                    var guestsX = await api.GetFromJsonAsync<List<Guest>>("guest") ?? new();
                    vm.TableOptions = tablesX.Select(t => new SelectListItem($"Table {t.TableNumber} (cap {t.Capacity})", t.Id.ToString(), t.Id == vm.TableId));
                    vm.GuestOptions = guestsX.Select(g => new SelectListItem(g.FullName ?? $"Guest {g.Id}", g.Id.ToString(), g.Id == vm.GuestId));
                    return View(vm);
                }
                TempData["SuccessMessage"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Booking/Create error");
                ModelState.AddModelError("", "Unexpected error.");
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var api = _authorizedClientProvider.GetClient();

                //booking
                var booking = await api.GetFromJsonAsync<Booking>($"{Resource}/{id}");
                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction(nameof(Index));
                }

                //lists
                var guests = await api.GetFromJsonAsync<List<Guest>>("guest") ?? new();
                var tables = await api.GetFromJsonAsync<List<Table>>("tables") ?? new();

                var vm = new BookingFormViewModel
                {
                    Id = booking.Id,
                    TableId = booking.FK_TableId,
                    GuestId = booking.FK_GuestId,
                    StartTime = booking.StartTime.ToLocalTime(),
                    PartySize = booking.PartySize,
                    TableOptions = tables.Select(t => new SelectListItem($"Table {t.TableNumber} (cap {t.Capacity})", t.Id.ToString(), t.Id == booking.FK_TableId)),
                    GuestOptions = guests.Select(g => new SelectListItem(g.FullName ?? $"Guest {g.Id}", g.Id.ToString(), g.Id == booking.FK_GuestId))
                };
                return View(vm);

            }
            catch (InvalidOperationException ex) { _logger.LogWarning("Token issue on Booking/Edit GET: {Msg}", ex.Message); return RedirectLogin(ex.Message); }
            catch (HttpRequestException ex) { _logger.LogError(ex, "Booking/Edit GET HTTP error"); TempData["ErrorMessage"] = "Network error."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) { _logger.LogError(ex, "Booking/Edit GET error"); TempData["ErrorMessage"] = "Unexpected error."; return RedirectToAction(nameof(Index)); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookingFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLists(vm);
                return View(vm);
            }

            try
            {
                var api = _authorizedClientProvider.GetClient();

                var dto = new MVCWebInvite.Models.ApiDtos.BookingUpdateDto
                {
                    Id = vm.Id,
                    TableId = vm.TableId,
                    GuestId = vm.GuestId,
                    StartTime = vm.StartTime.ToUniversalTime(),
                    PartySize = vm.PartySize
                };
                var resp = await api.PutAsJsonAsync($"{Resource}/{vm.Id}", dto);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    if ((int)resp.StatusCode == 400 || (int)resp.StatusCode == 409)
                        ModelState.AddModelError("", body);

                    else
                        ModelState.AddModelError("", $"{(int)resp.StatusCode}");
                    await PopulateLists(vm);
                    return View(vm);

                  
                }
                TempData["SuccessMessage"] = "Booking updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Booking/Edit POST error");
                ModelState.AddModelError("", "Unexpected error.");
                await PopulateLists(vm);
                return View(vm);

            }
        }


        private async Task PopulateLists(BookingFormViewModel vm)
        {
            var api = _authorizedClientProvider.GetClient();
            var tables = await api.GetFromJsonAsync<List<Table>>("tables") ?? new();
            var guests = await api.GetFromJsonAsync<List<Guest>>("guest") ?? new();

            vm.TableOptions = tables.Select(t =>
                new SelectListItem($"Table {t.TableNumber} (cap {t.Capacity})", t.Id.ToString(), t.Id == vm.TableId));

            vm.GuestOptions = guests.Select(g =>
                new SelectListItem(g.FullName ?? $"Guest {g.Id}", g.Id.ToString(), g.Id == vm.GuestId));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var api = _authorizedClientProvider.GetClient();
                var resp = await api.DeleteAsync($"{Resource}/{id}");
                if (resp.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Booking deleted.";
                else if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
                    TempData["ErrorMessage"] = await resp.Content.ReadAsStringAsync();
                else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    TempData["ErrorMessage"] = "Booking not found.";
                else
                    TempData["ErrorMessage"] = $"{(int)resp.StatusCode} {resp.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Guest/Delete error");
                TempData["ErrorMessage"] = "Unexpected error.";
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
