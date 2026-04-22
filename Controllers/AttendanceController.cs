using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YogaStudioLRAManagementSystem.Constants;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.Models;
using YogaStudioLRAManagementSystem.ViewModels;

namespace YogaStudioLRAManagementSystem.Controllers
{
    [Authorize] //all actions require login
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /Attendance/ClockIn
        /// Dedicated clock in/out screen - Staff only
        /// Reads today's attendance record and passes current state to view
        /// View handles rendering the correct button based on record state
        /// </summary>
        [HttpGet]
        [Authorize(Roles = UserRoles.STAFF)]
        public async Task<IActionResult> ClockIn()
        {
            var employeeId = GetEmployeeId();
            if (employeeId == null)
                return RedirectToAction("Login", "Auth");

            //get today's attendance record if it exists and null if employee hasn't visited page yet
            var todayRecord = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == DateTime.Today);

            // pass JWT to view for fetch calls to AttendanceAPI
            ViewBag.JwtToken = HttpContext.Session.GetString("jwt");

            return View(todayRecord); //null = not clocked in yet
        }

        /// <summary>
        /// GET: /Attendance/Index
        /// Staff - sees own attendance filtered by month and year
        /// Manager/Admin - sees all employee attendance filtered by employee, date range, status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? month, int? year, int? employeeId, DateTime? startDate, DateTime? endDate, string? status)
        {
            var viewModel = new AttendanceViewModel();

            if (User.IsInRole(UserRoles.STAFF))
            {
                //Staff sees only their own records filtered by month/year
                var staffEmployeeId = GetEmployeeId();
                if (staffEmployeeId == null)
                    return RedirectToAction("Login", "Auth");

                viewModel.SelectedMonth = month ?? DateTime.Today.Month;
                viewModel.SelectedYear = year ?? DateTime.Today.Year;

                viewModel.Records = await _context.Attendances
                    .Where(a =>
                        a.EmployeeId == staffEmployeeId &&
                        a.Date.Month == viewModel.SelectedMonth &&
                        a.Date.Year == viewModel.SelectedYear)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();
            }
            else
            {
                // backfill today's record for Staff only
                var staffEmployees = await _context.Employees
                    .Where(e => _context.Users.Any(u =>
                        u.EmployeeId == e.EmployeeId &&
                        u.Role == UserRoles.STAFF))
                    .ToListAsync();

                foreach (var employee in staffEmployees)
                {
                    var exists = await _context.Attendances.AnyAsync(a =>
                        a.EmployeeId == employee.EmployeeId &&
                        a.Date == DateTime.Today);

                    if (exists) continue;

                    var isOnLeave = await _context.LeaveRequests.AnyAsync(lr =>
                        lr.EmployeeId == employee.EmployeeId &&
                        lr.Status == LeaveRequestStatus.APPROVED &&
                        lr.StartDate <= DateTime.Today &&
                        lr.EndDate >= DateTime.Today);

                    _context.Attendances.Add(new Attendance
                    {
                        EmployeeId = employee.EmployeeId,
                        Date = DateTime.Today,
                        Status = isOnLeave ? AttendanceStatus.ON_LEAVE : AttendanceStatus.ABSENT
                    });
                }

                await _context.SaveChangesAsync();

                // build query - Staff only
                var query = _context.Attendances
                    .Include(a => a.Employee)
                    .Where(a => _context.Users.Any(u =>
                        u.EmployeeId == a.EmployeeId &&
                        u.Role == UserRoles.STAFF))
                    .AsQueryable();

                // only apply filters if provided
                if (employeeId.HasValue)
                    query = query.Where(a => a.EmployeeId == employeeId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(a => a.Status == status);

                if (startDate.HasValue)
                    query = query.Where(a => a.Date >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.Date <= endDate.Value);

                viewModel.Records = await query
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                // retain selected filter values for the view
                viewModel.SelectedEmployeeId = employeeId;
                viewModel.SelectedStatus = status;
                if (startDate.HasValue) viewModel.StartDate = startDate.Value;
                if (endDate.HasValue) viewModel.EndDate = endDate.Value;

                // populate dropdowns - retain selected values
                viewModel.EmployeeOptions = await _context.Employees
                    .Where(e => _context.Users.Any(u =>
                        u.EmployeeId == e.EmployeeId &&
                        u.Role == UserRoles.STAFF))
                    .Select(e => new SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = $"{e.FirstName} {e.LastName}",
                        Selected = e.EmployeeId == employeeId
                    }).ToListAsync();

                viewModel.StatusOptions = new List<SelectListItem>
                {
                    new() { Value = AttendanceStatus.PRESENT, Text = AttendanceStatus.PRESENT, Selected = status == AttendanceStatus.PRESENT },
                    new() { Value = AttendanceStatus.ABSENT, Text = AttendanceStatus.ABSENT, Selected = status == AttendanceStatus.ABSENT },
                    new() { Value = AttendanceStatus.ON_LEAVE, Text = AttendanceStatus.ON_LEAVE, Selected = status == AttendanceStatus.ON_LEAVE }
                };
            }

            viewModel.TotalHours = viewModel.Records.Sum(a => a.TotalHours ?? 0);

            return View(viewModel);
        }

        /// <summary>
        /// GET: /Attendance/Edit/5
        /// Manager and Admin only - loads edit form for an attendance record
        /// </summary>
        [HttpGet]
        [Authorize(Roles = $"{UserRoles.MANAGER},{UserRoles.ADMIN}")]
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AttendanceId == id);

            if (record == null)
                return NotFound();

            return View(record);
        }

        /// <summary>
        /// POST: /Attendance/Edit/5
        /// Manager and Admin only - saves corrected ClockIn and ClockOut
        /// Recalculates TotalHours automatically from corrected times
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRoles.MANAGER},{UserRoles.ADMIN}")]
        public async Task<IActionResult> Edit(int attendanceId, TimeSpan? clockIn, TimeSpan? clockOut)
        {
            var record = await _context.Attendances.FindAsync(attendanceId);
            if (record == null)
                return NotFound();

            //validate clockOut is after clockIn if both are provided
            if (clockIn.HasValue && clockOut.HasValue && clockOut <= clockIn)
            {
                ModelState.AddModelError(string.Empty, "Clock out time must be after clock in time.");
                record = await _context.Attendances
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);
                return View(record);
            }

            //keep the date, just updating the time portion
            record.ClockIn = clockIn.HasValue ? record.Date.Date + clockIn.Value : null;
            record.ClockOut = clockOut.HasValue ? record.Date.Date + clockOut.Value : null;

            //recalculate total hours if both times are set
            if (record.ClockIn.HasValue && record.ClockOut.HasValue)
                record.TotalHours = Math.Round((record.ClockOut.Value - record.ClockIn.Value).TotalHours, 2);
            else
                record.TotalHours = null; //reset if either time is cleared

            //update status based on clock in
            if (clockIn.HasValue)
                record.Status = AttendanceStatus.PRESENT;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Attendance record updated successfully.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Reads the EmployeeId from the logged in user's cookie claims
        /// Returns null if claim is missing or cannot be parsed
        /// </summary>
        private int? GetEmployeeId()
        {
            var claim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(claim, out var id))
            {
                return id;
            }
            return null;
        }
    }
}