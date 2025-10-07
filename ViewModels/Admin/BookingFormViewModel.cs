using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVCWebInvite.ViewModels.Admin
{
    public class BookingFormViewModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue,ErrorMessage ="Please select a table")]
        public int TableId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a guest")]
        public int GuestId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Range(1,60)]
        public int PartySize { get; set; }

        public IEnumerable<SelectListItem> TableOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> GuestOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
