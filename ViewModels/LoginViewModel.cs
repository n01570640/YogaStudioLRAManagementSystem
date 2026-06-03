using System.ComponentModel.DataAnnotations;

namespace YogaStudioLRAManagementSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;


        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
