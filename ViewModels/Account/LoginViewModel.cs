using System.ComponentModel.DataAnnotations;

namespace MVCWebInvite.ViewModels.Account
{
    public class LoginViewModel
    {
        //    [Required,EmailAddress]
        //    public string Email { get; set; } = "";
        //    [Required, DataType(DataType.Password)]
        //    public string Password { get; set; } = "";
        //    public bool RememberMe { get; set; }
        //    public string? ReturnUrl { get; set; }
        [Required]
        public string? Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }

}

