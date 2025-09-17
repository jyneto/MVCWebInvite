using System.ComponentModel.DataAnnotations;

namespace MVCWebInvite.Models
{
    public class Table
    {
        public int Id { get; set; }
        [Required]
        public int TableNumber { get; set; }
        [Required]
        public int Capacity { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public List<Guest> Guests { get; set; } = new List<Guest>();
    }
}
