namespace MVCWebInvite.Models.ApiDtos
{
    public class BookingUpdateDto
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int GuestId { get; set; }
        public DateTime StartTime { get; set; }
        public int PartySize { get; set; }
    }
}
