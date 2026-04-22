using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("YS_EMPLOYEES")]
    public class Employee
    {
        [Key]
        [Column("EMPLOYEE_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //to ensure db generates the value
        public int EmployeeId { get; set; }

        [Column("FIRST_NAME")]
        [Required(ErrorMessage = "First Name is required.")]        
        [StringLength(30, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 30 characters.")]
        public string FirstName { get; set; }

        [Column("LAST_NAME")]
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 30 characters.")]
        public string LastName { get; set; }

        [Column("HIRE_DATE")]
        [Required(ErrorMessage = "Hire date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime HireDate { get; set; }

        [Column("CERTIFICATION")]
        [StringLength(150)]
        public string? Certification { get; set; }

        [Column("VACATION_BALANCE")]
        [Required(ErrorMessage ="Leave balance is required.")]        
        public int VacationBalance { get; set; } // in days

        [Column("SICK_LEAVE_BALANCE")]
        [Required(ErrorMessage = "Leave balance is required.")]
        public int SickLeaveBalance { get; set; } // in days

        // Foreign key to StudioRole
        [Column("STUDIO_ROLE_ID")]
        [Required(ErrorMessage = "Please select a studio role.")]       
        public int StudioRoleId { get; set; } //dropdown

        [ForeignKey("StudioRoleId")]
        public StudioRole StudioRole { get; set; } = null!; //emp belongs to one role

        //navigation properpty
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>(); //emp has many requests [relationship]
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>(); //emp has many attendance records [relationship]
    }
}