using MVCWebInvite.Models;

namespace MVCWebInvite.ViewModels.Admin
{
    public class GuestFormViewModel
    {
        public Guest Guest { get; set; }
        public List<Table> AvailableTables { get; set; } = new();
        //public List<AvailableTableDTO> AvailableTables { get; set; } = new ();

    }
}
