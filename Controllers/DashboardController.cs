using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YogaStudioLRAManagementSystem.Constants;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.ViewModels;

namespace YogaStudioLRAManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /Dashboard/Index
        /// Landing page after login - content differs by role
        /// Staff - Vacation balance, sick leave balance and hours logged this month
        /// Manager - Pending leave request, currently on leave, today's attendance record
        /// Admn - total employees, active users, pending requests, today;s attendance record
        /// </summary>
        /// <returns>Role-based Dashboard</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();
            var today = DateTime.Today;

            //STAFF
            if (User.IsInRole(UserRoles.STAFF))
            {
                var employeeId = GetEmployeeId();
                if (employeeId == null)
                    return RedirectToAction("Login", "Auth");

                //leave balances from employee record
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

                if (employee != null)
                {
                    viewModel.VacationBalance = employee.VacationBalance;
                    viewModel.SickLeaveBalance = employee.SickLeaveBalance;
                }

                //upcoming approved leave - future dates only
                viewModel.UpcomingApprovedLeave = await _context.LeaveRequests
                    .Include(lr => lr.LeaveType)
                    .Where(lr =>
                        lr.EmployeeId == employeeId &&
                        lr.Status == LeaveRequestStatus.APPROVED &&
                        lr.StartDate >= today)
                    .OrderBy(lr => lr.StartDate)
                    .ToListAsync();

                //pending requests awaiting manager review
                viewModel.MyPendingRequests = await _context.LeaveRequests
                    .Include(lr => lr.LeaveType)
                    .Where(lr =>
                        lr.EmployeeId == employeeId &&
                        lr.Status == LeaveRequestStatus.PENDING)
                    .OrderBy(lr => lr.StartDate)
                    .ToListAsync();

                //total hours logged this month
                viewModel.HoursThisMonth = await _context.Attendances
                    .Where(a =>
                        a.EmployeeId == employeeId &&
                        a.Date.Month == today.Month &&
                        a.Date.Year == today.Year)
                    .SumAsync(a => a.TotalHours ?? 0);
            }

            //MANAGER
            else if (User.IsInRole(UserRoles.MANAGER))
            {
                //team approved leave currently ongoing
                viewModel.TeamApprovedLeave = await _context.LeaveRequests
                    .Include(lr => lr.Employee)
                    .Include(lr => lr.LeaveType)
                    .Where(lr =>
                        lr.Status == LeaveRequestStatus.APPROVED &&
                        lr.StartDate <= today &&
                        lr.EndDate >= today)
                    .OrderBy(lr => lr.EndDate)
                    .ToListAsync();

                //all pending requests awaiting action
                viewModel.TeamPendingRequests = await _context.LeaveRequests
                    .Include(lr => lr.Employee)
                    .Include(lr => lr.LeaveType)
                    .Where(lr => lr.Status == LeaveRequestStatus.PENDING)
                    .OrderBy(lr => lr.StartDate)
                    .ToListAsync();

                //today's attendance - all records
                viewModel.TodayAttendance = await _context.Attendances
                    .Include(a => a.Employee)
                    .Where(a => a.Date == today)
                    .OrderBy(a => a.Employee!.FirstName)
                    .ToListAsync();

                //today's attendance counts for the chart
                viewModel.TodayPresent = viewModel.TodayAttendance
                    .Count(a => a.Status == AttendanceStatus.PRESENT);
                viewModel.TodayAbsent = viewModel.TodayAttendance
                    .Count(a => a.Status == AttendanceStatus.ABSENT);
                viewModel.TodayOnLeave = viewModel.TodayAttendance
                    .Count(a => a.Status == AttendanceStatus.ON_LEAVE);
            }

            //ADMIN
            else if (User.IsInRole(UserRoles.ADMIN))
            {
                //count approved leave requests by type this month
                var firstOfMonth = new DateTime(today.Year, today.Month, 1);

                //start with every leave type set to 0, then fill in actual counts
                var allLeaveTypes = await _context.LeaveTypes
                    .OrderBy(lt => lt.Name)
                    .ToListAsync();

                var countsThisMonth = await _context.LeaveRequests
                    .Include(lr => lr.LeaveType)
                    .Where(lr =>
                        lr.Status == LeaveRequestStatus.APPROVED &&
                        lr.StartDate >= firstOfMonth)
                    .GroupBy(lr => lr.LeaveType!.Name)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Name, x => x.Count);

                // merge - every leave type appears, zero if none approved this month
                viewModel.LeaveTypeCounts = allLeaveTypes
                    .ToDictionary(lt => lt.Name, lt => countsThisMonth.ContainsKey(lt.Name) ? countsThisMonth[lt.Name] : 0);
                viewModel.TotalEmployees = await _context.Employees.CountAsync();

                viewModel.TotalActiveUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .CountAsync();

                viewModel.TotalPendingRequests = await _context.LeaveRequests
                    .Where(lr => lr.Status == LeaveRequestStatus.PENDING)
                    .CountAsync();

                //today's attendance counts by status
                var todayAttendance = await _context.Attendances
                    .Where(a => a.Date == today)
                    .ToListAsync();

                viewModel.TodayPresent = todayAttendance.Count(a => a.Status == AttendanceStatus.PRESENT);
                viewModel.TodayAbsent = todayAttendance.Count(a => a.Status == AttendanceStatus.ABSENT);
                viewModel.TodayOnLeave = todayAttendance.Count(a => a.Status == AttendanceStatus.ON_LEAVE);
            }

            return View(viewModel);
        }

        /// <summary>
        /// Reads the EmployeeId from the logged in user's cookie claims
        /// </summary>
        private int? GetEmployeeId()
        {
            var claim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(claim, out var id))
                return id;
            return null;
        }
    }
}