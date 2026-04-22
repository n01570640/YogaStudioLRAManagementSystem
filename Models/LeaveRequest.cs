using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YogaStudioLRAManagementSystem.Constants;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("YS_LEAVE_REQUESTS")]
    public class LeaveRequest 

    {
        [Key]
        [Column("REQUEST_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Column("START_DATE")]
        [Required(ErrorMessage ="Start date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] //display as 2026-03-28
        public DateTime StartDate { get; set; }

        [Column("END_DATE")]
        [Required(ErrorMessage ="End date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] //display as 2026-03-28
        public DateTime EndDate { get; set; }

        [Column("REQUEST_STATUS")]
        [Required] //no error message needed - system generated
        [StringLength(10)]
        [RegularExpression(LeaveRequestStatus.VALIDATION_PATTERN, ErrorMessage = "Invalid leave request status.")]
        public string Status { get; set; } = LeaveRequestStatus.PENDING; //default to pending - possible Values: PENDING, APPROVED, DENIED

        [Column("REASON")]
        [StringLength(150, MinimumLength = 2, ErrorMessage ="Reason must be between 2 and 150 characters.")]
        public string? Reason { get; set; }

        [Column("EMPLOYEE_ID")]
        //Foreign keys:
        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!; //navigation property

        [Column("LEAVE_TYPE_ID")]
        [Required]
        public int LeaveTypeId { get; set; }

        [ForeignKey("LeaveTypeId")]
        public LeaveType LeaveType { get; set; } = null!; //navigation property
    }
}
