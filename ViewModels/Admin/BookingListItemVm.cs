namespace MVCWebInvite.ViewModels.Admin
{
    public class BookingListItemVm
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public string? TableNumber { get; set; }
        public int GuestId { get; set; }
        public string? GuestName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PartySize { get; set; }
    }
}
