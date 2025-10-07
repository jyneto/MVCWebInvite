namespace MVCWebInvite.Models
{
    public class RsvpResponse
    {
        public string GuestName { get; set; } = "";
        public bool IsAttending { get; set; }
        public string?  Email { get; internal set; }
        public string? Allergies { get; set; }
    }
}
