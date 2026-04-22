using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("YS_LEAVE_TYPES")]
    public class LeaveType
    {
        [Key]
        [Column("LEAVE_TYPE_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage ="Leave name is required.")]
        [Column("LEAVE_NAME")]
        [StringLength(50, ErrorMessage = "Leave name cannot exceed 50 characters.")]
        public string Name { get; set; }
               
        [Column("IS_PAID")]
        public bool IsPaid { get; set; } //true -> is paid, false -> not paid

        [Column("AFFECTS_BALANCE")]
        public bool AffectsBalance { get; set; } //to keep track if to check/deduct leave balance of an employee
        [Column("MIN_DAYS")]
        public int MinDays { get; set; } = 1;

        [Column("MAX_DAYS")]
        public int MaxDays { get; set; }

        //navigation property
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>(); //one leaveType has many leave requests relationship

    }
}
