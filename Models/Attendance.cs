using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YogaStudioLRAManagementSystem.Constants;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("YS_ATTENDANCES")]
    public class Attendance
    {
        [Key]
        [Column("ATTENDANCE_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //to ensure db generates the value
        public int AttendanceId { get; set; }

        [Required]
        [Column("EMPLOYEE_ID")]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!; //null-forgiving operator - right now this is empty but it will not be empty when it matters

        [Required]
        [Column("DATE")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] //display as 2026-03-28
        public DateTime Date {  get; set; }

        [Column("CLOCK_IN")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}")] //display as 09:30 AM, 02:15 PM
        [Display(Name ="Clock In")]
        public DateTime? ClockIn { get; set; } //nullable - not clocked in yet

        [Column("CLOCK_OUT")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}")] //display as 09:30 AM, 02:15 PM
        [Display(Name = "Clock Out")]
        public DateTime? ClockOut { get; set; } //nullable - not clocked out yet

        [Column("TOTAL_HOURS")]
        [Display(Name ="Total Hours")]
        public double? TotalHours { get; set; } //nullable - calculated after clock out

        [Required]
        [Column("STATUS")]
        [StringLength(10)]
        [RegularExpression(AttendanceStatus.VALIDATION_PATTERN, ErrorMessage ="Invalid attendance status.")]
        public string Status { get; set; } = AttendanceStatus.ABSENT; //default: Absent, Present, On Leave
    }
}
