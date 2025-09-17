using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCWebInvite.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required, ForeignKey(nameof(Table))]
        public int FK_TableId { get; set; }
        public Table? Table { get; set; }

        [Required, ForeignKey(nameof(Guest))]
        public int FK_GuestId { get; set; }
        public Guest? Guest { get; set; }

        [Required]
        public DateTime StartTime { get; set; } //Utc
        [Required]
        public DateTime EndTime { get; set; }//Utc
        [Required]
        public int PartySize { get; set; }
    }
}
