using System.ComponentModel.DataAnnotations;

namespace mist.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Login jest wymagany")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}