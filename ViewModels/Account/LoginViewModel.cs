using System.ComponentModel.DataAnnotations;

namespace MVCWebInvite.ViewModels.Account
{
    public class LoginViewModel
    {
       
        [Required]
        public string? Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }

}

