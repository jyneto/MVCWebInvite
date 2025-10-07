using MVCWebInvite.Models;
using MVCWebInvite.ViewModels.Menu;
using System.Collections.Generic;
namespace MVCWebInvite.ViewModels
{
    public class HomePageVm
    {
        public List<MenuItemVm> Menu { get; set; } = new();
        public RsvpFormVm RsvpForm { get; set; } = new();
    }
}
