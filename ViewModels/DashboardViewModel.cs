using YogaStudioLRAManagementSystem.Models;

namespace YogaStudioLRAManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        //staff
        public IEnumerable<LeaveRequest> UpcomingApprovedLeave { get; set; } = new List<LeaveRequest>();
        public IEnumerable<LeaveRequest> MyPendingRequests { get; set; } = new List<LeaveRequest>();
        public int VacationBalance { get; set; }
        public int SickLeaveBalance { get; set; }
        public double HoursThisMonth { get; set; }

        //manager
        public IEnumerable<LeaveRequest> TeamApprovedLeave { get; set; } = new List<LeaveRequest>();
        public IEnumerable<LeaveRequest> TeamPendingRequests { get; set; } = new List<LeaveRequest>();
        public IEnumerable<Attendance> TodayAttendance { get; set; } = new List<Attendance>();

        //admin
        public int TotalEmployees { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalPendingRequests { get; set; }
        public int TodayPresent { get; set; }
        public int TodayAbsent { get; set; }
        public int TodayOnLeave { get; set; }
        public Dictionary<string, int> LeaveTypeCounts { get; set; } = new(); //dic to map each leavetype name to its approved count this month
    }
}
