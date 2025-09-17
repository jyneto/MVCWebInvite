using System.ComponentModel.DataAnnotations;

namespace MVCWebInvite.Models
{
    public class Guest
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string? FullName { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }
        [Phone]
        public string? Phone { get; set; }
        public bool IsAttending { get; set; }
   
        public string? Allergies { get; set; }
    }
}
