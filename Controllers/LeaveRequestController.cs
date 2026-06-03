using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.Models;
using YogaStudioLRAManagementSystem.Constants;
using YogaStudioLRAManagementSystem.ViewModels;
using System.Linq.Expressions;

namespace YogaStudioLRAManagementSystem.Controllers
{
    [Authorize]  //you must be logged in to complete any actions heres
    public class LeaveRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? statusFilter, int? employeeIdFilter)
        {
            ModelState.Remove("Employee");
            ModelState.Remove("LeaveType");
            //TempData["Success"] = null;
            //TempData["Error"] = null;

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return Unauthorized();
            var role = user.Role;
            var employeeId = user.EmployeeId;

            var myRequests = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();

            var teamRequests = role == UserRoles.MANAGER
                ? await _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Include(l => l.LeaveType)
                    .Where(l => l.EmployeeId != employeeId)
                    .OrderByDescending(l => l.StartDate)
                    .ToListAsync()
                : new List<LeaveRequest>();

            //since admin should be able to see all the requests plus filter thru them, we create a separate query:
            var adminQuery = _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .AsQueryable();

            if (role == UserRoles.ADMIN)
            {
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    adminQuery = adminQuery.Where(l => l.Status == statusFilter);
                }

                if (employeeIdFilter.HasValue)
                {
                    adminQuery = adminQuery.Where(l => l.EmployeeId == employeeIdFilter.Value);
                }
            }

            var allRequests = role == UserRoles.ADMIN
                ? await adminQuery.OrderByDescending(l => l.StartDate).ToListAsync()
                : new List<LeaveRequest>();

            var model = new LeaveRequestViewModel
            {
                MyRequests = myRequests,
                TeamRequests = teamRequests,
                AllRequests = allRequests,
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? requestId)
        {
            if (requestId == null) return NotFound();

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.RequestId == requestId);

            if (leaveRequest == null) return NotFound();
           
            return View(leaveRequest);
        }
               

        [HttpGet] 
        [Authorize(Roles = "Staff, Manager")] //admin shouldnt be able to submit requests
        public IActionResult Create()
        {
            TempData["Error"] = null;
            TempData["Success"] = null;

            ViewBag.LeaveTypeId = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Manager")]
        public async Task<IActionResult> Create(LeaveRequest formRequest)
        {
            ModelState.Remove("Employee");
            ModelState.Remove("LeaveType");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return Unauthorized();

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(lt => lt.LeaveTypeId == formRequest.LeaveTypeId);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId);

            if (leaveType == null) ModelState.AddModelError("LeaveTypeId", "Invalid leave type.");

            if (employee == null) return Unauthorized();
            //------------------------------------------------------------------------------------------------------
            //MAJOR VALIDATION FOLLOWS:
            //------------------------------------------------------------------------------------------------------

            if (leaveType != null && leaveType.AffectsBalance)
            {
                if (await HasBlockingPending(employee.EmployeeId))
                {
                    TempData["Error"] = "You already have a pending leave request that affects your balance.";
                    return RedirectToAction(nameof(Index));
                }
            }
            //passing 0 cause there is no request id at the moment since this req doesnt exist yet:
            if (await HasOverlap(employee.EmployeeId, 0, formRequest.StartDate, formRequest.EndDate))
                ModelState.AddModelError("", "This leave request overlaps with an existing request.");

            //------------------------------------------------------------------------------------------------------
            //DATE VALIDATION:
            //------------------------------------------------------------------------------------------------------
            ValidateDates(formRequest, leaveType);

            var lrLength = (formRequest.EndDate.Date - formRequest.StartDate.Date).Days + 1;

            if (leaveType != null && leaveType.AffectsBalance &&
                HasInsufficientBalance(employee, formRequest, lrLength))
            {
                ModelState.AddModelError("", "Insufficient balance for this leave request.");
            }

            //------------------------------------------------------------------------------------------------------
            //NOW SAVE IF EVERYTHING IS FINE:
            //------------------------------------------------------------------------------------------------------
            if (ModelState.IsValid)
            {
                TempData["Error"] = null;

                formRequest.EmployeeId = user.EmployeeId;
                formRequest.Status = LeaveRequestStatus.PENDING;
                
                _context.Add(formRequest);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Leave request submitted!";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.LeaveTypeId = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
             return View(formRequest);
               
        }

        [HttpGet]
        [Authorize(Roles = "Staff, Manager")]
        public async Task<IActionResult> Edit(int? requestId)
        {
            if (requestId == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return Unauthorized();

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.RequestId == requestId);

            if (leaveRequest == null) return NotFound();

            //Ownership check:
            if (leaveRequest.EmployeeId != user.EmployeeId)
            {
                TempData["Error"] = "You cannot edit a leave request on the behalf of another employee.";
                return Forbid();
            }
            //Only pending edit-able:
            if (leaveRequest.Status != LeaveRequestStatus.PENDING)
            {
                TempData["Error"] = "You cannot edit non-pending request.";
                return RedirectToAction("Details", new { requestId = leaveRequest.RequestId });
            }

            ViewBag.LeaveTypeId = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");

            return View(leaveRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Manager")]
        public async Task<IActionResult> Edit(int requestId, LeaveRequest formRequest)
        {
            ModelState.Remove("Employee");
            ModelState.Remove("LeaveType");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return Unauthorized();

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(lt => lt.LeaveTypeId == formRequest.LeaveTypeId);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId);

            if (leaveType == null) ModelState.AddModelError("LeaveTypeId", "Invalid leave type.");

            if (employee == null) return Unauthorized();

            var existingRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.RequestId == requestId);

            if (existingRequest == null) return NotFound();

            //Ownership check:
            if (existingRequest.EmployeeId != user.EmployeeId)
            {
                TempData["Error"] = "You cannot edit a leave request on the behalf of another employee.";
                return Forbid();
            }

            //Only pending requests should be edit-able:
            if (existingRequest.Status != LeaveRequestStatus.PENDING)
            {
                TempData["Error"] = "Only pending requests can be editted.";
                return RedirectToAction("Details", new { requestId = existingRequest.RequestId });
            }
            //------------------------------------------------------------------------------------------------------
            //MAJOR VALIDATION
            //------------------------------------------------------------------------------------------------------
            //the blocking rule only matters if the changes being applied to the current request affect the balance:
            //i.e. we are changing the leave type to something that affects balance
            if (leaveType != null && leaveType.AffectsBalance)
            {
                if (await HasBlockingPending(employee.EmployeeId, existingRequest.RequestId))
                {
                    TempData["Error"] =
                        "You already have another pending leave request that affects your balance.";
                    return RedirectToAction(nameof(Index));
                }
            }
            if (await HasOverlap(employee.EmployeeId, requestId,
                    formRequest.StartDate, formRequest.EndDate))
            {
                ModelState.AddModelError("", "This leave request overlaps with an existing approved request.");
            }
            //------------------------------------------------------------------------------------------------------
            //DATE VALIDATION:
            //------------------------------------------------------------------------------------------------------
            ValidateDates(formRequest, leaveType);

            var lrLength = (formRequest.EndDate.Date - formRequest.StartDate.Date).Days + 1;

            //------------------------------------------------------------------------------------------------------
            // BALANCE VALIDATION:
            //------------------------------------------------------------------------------------------------------
            if (leaveType != null && leaveType.AffectsBalance &&
                HasInsufficientBalance(employee, formRequest, lrLength))
            {
                ModelState.AddModelError("", "Insufficient balance for this leave request.");
            }
            //------------------------------------------------------------------------------------------------------
            //UPDATE IF EVERYTHING IS FINE:
            //------------------------------------------------------------------------------------------------------
            if (ModelState.IsValid)
            {
                TempData["Error"] = null;

                existingRequest.StartDate = formRequest.StartDate;
                existingRequest.EndDate = formRequest.EndDate;
                existingRequest.Reason = formRequest.Reason;
                existingRequest.LeaveTypeId = formRequest.LeaveTypeId;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Leave request edited successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LeaveTypeId = new SelectList(_context.LeaveTypes, "LeaveTypeId", "Name");
            return View(formRequest);
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Review(int requestId)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.RequestId == requestId); 

            if (request == null) return NotFound();

            return View(request);
        }

        //allow managers/admin to approve requests:
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int requestId)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != LeaveRequestStatus.PENDING)
            {
                TempData["Error"] = "Only pending requests can be approved.";
                return RedirectToAction(nameof(Index));
            }

            if (request.EmployeeId == int.Parse(User.FindFirstValue("EmployeeId")))
            {
                TempData["Error"] = "Managers cannot approve their own requests.";
                return RedirectToAction(nameof(Index));
            }

            //------------------------------------------------------------------------------------------------------
            // FINAL SAFETY CHECKS (state may have changed since submission):
            //------------------------------------------------------------------------------------------------------

            var lrLength = (request.EndDate.Date - request.StartDate.Date).Days + 1;

            if (await HasOverlap(request.EmployeeId, request.RequestId,
                request.StartDate, request.EndDate))
            {
                TempData["Error"] = "This request now overlaps with another approved request.";
                return RedirectToAction(nameof(Index));
            }

            if (request.LeaveType.AffectsBalance &&
                HasInsufficientBalance(request.Employee, request, lrLength))
            {
                TempData["Error"] = "Insufficient balance at approval time.";
                return RedirectToAction(nameof(Index));
            }

            //------------------------------------------------------------------------------------------------------
            //now deduct from the leave balance:
            if (request.LeaveType.AffectsBalance)
            {
                if (request.LeaveTypeId == 1)
                    request.Employee.VacationBalance -= lrLength;

                if (request.LeaveTypeId == 2)
                    request.Employee.SickLeaveBalance -= lrLength;
            }

            request.Status = LeaveRequestStatus.APPROVED;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request approved!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int requestId)
        {
            var request = await _context.LeaveRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != LeaveRequestStatus.PENDING)
            {
                TempData["Error"] = "Only pending requests can be denied.";
                return RedirectToAction(nameof(Index));
            }

            if (request.EmployeeId == int.Parse(User.FindFirstValue("EmployeeId")))
            {
                TempData["Error"] = "Managers cannot deny their own requests.";
                return RedirectToAction(nameof(Index));
            }

            request.Status = LeaveRequestStatus.DENIED;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request rejected.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Staff, Manager")] //should be a self-service as well as edit
        public async Task<IActionResult> Delete(int? requestId)
        {
            if (requestId == null) return NotFound();

            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.RequestId == requestId);

            if (leaveRequest == null) return NotFound();

            return View(leaveRequest);
        }

        [Authorize(Roles = "Staff, Manager")] 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int requestId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return Unauthorized();

            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.RequestId == requestId);

            if (leaveRequest == null)
            {
                TempData["Error"] = "Request not found.";
                return NotFound();
            }                

            if (leaveRequest.EmployeeId != user.EmployeeId)
            {
                TempData["Error"] = "You cannot delete a leave request on the behalf of another employee.";
                return Forbid();
            }

            if (leaveRequest.Status != LeaveRequestStatus.PENDING)
            {
                TempData["Error"] = "Only pending requests can be deleted.";
                return RedirectToAction(nameof(Index));
            }
            //------------------------------------------------------------------------------------------------------
            //DELETE IF EVERYTHING IS FINE:
            //------------------------------------------------------------------------------------------------------
            if (ModelState.IsValid)
            {
                _context.LeaveRequests.Remove(leaveRequest);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Successfully deleted a leave request!";
                
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "We could not delete your leave request.";
            return RedirectToAction(nameof(Index));
        }
        //------------------------------------------------------------------------------------------------------
        //Helpers for validation:
        //------------------------------------------------------------------------------------------------------
        private void ValidateDates(LeaveRequest request, LeaveType? leaveType)
        {
            var today = DateTime.Today;

            if (request.StartDate <= today)
                ModelState.AddModelError("StartDate", "Start date must be after today.");

            if (request.EndDate <= today)
                ModelState.AddModelError("EndDate", "End date must be after today.");

            if (request.EndDate < request.StartDate)
                ModelState.AddModelError("EndDate", "End date cannot be before start date.");

            var length = (request.EndDate.Date - request.StartDate.Date).Days + 1;

            if (leaveType != null && length > leaveType.MaxDays)
                ModelState.AddModelError("EndDate", "Exceeds maximum # of days for indicated leave type.");
        }
        //regarding overlapping dates --> it truly only matters if the fetched lrs are APPROVED
        //--> you cannot have overlapping dates if the date for one approved lr is overlapping with a pending one 
        private async Task<bool> HasOverlap(int employeeId, int requestId, DateTime start, DateTime end)
        {
            return await _context.LeaveRequests.AnyAsync(lr =>
                lr.EmployeeId == employeeId &&
                lr.RequestId != requestId &&
                lr.Status == LeaveRequestStatus.APPROVED &&
                lr.StartDate <= end &&
                lr.EndDate >= start
            );
        }
        //you cannot create another request that affects balance if there is another one that is PENDING:
        private async Task<bool> HasBlockingPending(int employeeId, int excludeRequestId = 0)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .AnyAsync(lr =>
                    lr.EmployeeId == employeeId &&
                    lr.RequestId != excludeRequestId &&
                    lr.Status == LeaveRequestStatus.PENDING &&
                    lr.LeaveType.AffectsBalance
                );
        }
        //in specific cases, there must be sufficient balance:
        private bool HasInsufficientBalance(Employee employee, LeaveRequest request, int length)
        {
            if (request.LeaveTypeId == 1)
                return employee.VacationBalance < length;

            if (request.LeaveTypeId == 2)
                return employee.SickLeaveBalance < length;

            return false;
        }
    }
}