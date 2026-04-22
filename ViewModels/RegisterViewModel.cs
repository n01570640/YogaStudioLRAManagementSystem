using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace YogaStudioLRAManagementSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email format.")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 100 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;

        //Admin selects role from dropdown
        [Required(ErrorMessage = "Please select a role.")]
        [Display(Name = "Role")]
        public string Role { get; set; } = null!;

        //Admin selects employee from dropdown
        [Required(ErrorMessage = "Has the Employee been registered?")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        //populated in controller for the dropdown
        public List<SelectListItem> EmployeeOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> RoleOptions { get; set; } = new List<SelectListItem>();
    }
}
