using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YogaStudioLRAManagementSystem.Constants;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("YS_USERS")]
    public class User
    {
        [Key]
        [Column("USER_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //to ensure db generates the value
        public int UserId { get; set; }

        [Column("USER_NAME")]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50,MinimumLength = 3, ErrorMessage ="Username is required.")]
        public string UserName { get; set; }

        [Column("HASH_PASSWORD")]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 100 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Column("MUST_CHANGE_PASSWORD")] 
        public bool MustChangePassword { get; set; } = true; //Admin sets inital password, flag to keep track if first sign in - must change password

        [Column("EMAIL_ID")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage ="Please provide a valid email format.")]
        [StringLength(100)]
        public string EmailAddress { get; set; }

        [Column("PROFILE_ROLE")]
        [Required(ErrorMessage = "Roles is required.")]
        [StringLength(10)]
        [RegularExpression(UserRoles.VALIDATION_PATTERN, ErrorMessage = "Invalid role.")]
        public string Role { get; set; } = UserRoles.STAFF; //default to Staff role [Least privileged role]

        [Column("EMPLOYEE_ID")]
        [Required(ErrorMessage ="Has the Employee been registered?")]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!; //null-forgiving operator - right now this is empty but it will not be empty when it matters

        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; } = true; //default true, soft delete sets to false

        /// <summary>
        /// Hashes password enterd by the user on registration
        /// and set it to PasswordHash - then hashed password
        /// is saved to DB table
        /// </summary>
        /// <param name="password">user enter password to be hashed</param>
        public void SetPassword(string password)
        {
            var hasher = new PasswordHasher<User>();
            Password = hasher.HashPassword(this, password);
        }

        /// <summary>
        /// Verify entered password against stored hash on login
        /// </summary>
        /// <param name="password">User entered password on sign in</param>
        /// <returns>True for match and False if unmatched</returns>
        public bool VerifyPassword(string password)
        {
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(this, Password, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
