using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCWebInvite.ViewModels.Admin
{
    public class BookingFormViewModel
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int GuestId { get; set; }
        public DateTime StartTime { get; set; }
        public int PartySize { get; set; }

        public IEnumerable<SelectListItem> TableOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> GuestOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
