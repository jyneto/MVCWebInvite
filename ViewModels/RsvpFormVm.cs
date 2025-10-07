using System.ComponentModel.DataAnnotations;

namespace MVCWebInvite.ViewModels
{
    public class RsvpFormVm
    {
        [Required,Display(Name ="Your name")]
        public string FullName { get; set; } = "";
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Phone]
        public string? Phone { get; set; }
        public string? Allergies { get; set; }
        public bool IsAttending { get; set; }
        
    }
}
