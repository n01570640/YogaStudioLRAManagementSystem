using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.Models;

namespace YogaStudioLRAManagementSystem.Controllers
{
    [Authorize] // all actions require login
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employee
        // INDEX - Displays a list of all employees
        // Eager loads StudioRole so we can show the role name in the table
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.StudioRole) // eager load so we can show role name
                .ToListAsync();
            return View(employees);
        }

        // GET: Employee/Details/5
        // DETAILS - Displays a single employee's full info
        // Returns 404 if the id is null or employee doesn't exist
        [Authorize(Roles = "Admin,Manager, Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.StudioRole)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // GET: Employee/Create
        // CREATE (GET) - Loads the empty form for adding a new employee
        // Populates the StudioRole dropdown via ViewBag
        // Admin only — this is Step 1 of the onboarding flow
        // (Admin creates Employee first, then links a User account after)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // load studio roles for dropdown
            ViewBag.StudioRoles = new SelectList(
                _context.StudioRoles, "StudioRoleId", "RoleName");
            return View();
        }

        // POST: Employee/Create
        // CREATE (POST) - Saves the new employee to the database
        // Removes navigation properties from ModelState so validation
        // doesn't fail on fields that aren't part of the form
        // Reloads the dropdown if validation fails
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Employee employee)
        {
            // remove navigation properties from validation
            ModelState.Remove("StudioRole");
            ModelState.Remove("LeaveRequests");
            ModelState.Remove("Attendances");

            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // reload dropdown if validation fails
            ViewBag.StudioRoles = new SelectList(
                _context.StudioRoles, "StudioRoleId", "RoleName", employee.StudioRoleId);
            return View(employee);
        }

        // GET: Employee/Edit/5
        // EDIT (GET) - Loads the form pre-filled with existing employee data
        // Passes the current StudioRoleId to pre-select the dropdown
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            ViewBag.StudioRoles = new SelectList(
                _context.StudioRoles, "StudioRoleId", "RoleName", employee.StudioRoleId);
            return View(employee);
        }

        // POST: Employee/Edit/5
        // EDIT (POST) - Saves the updated employee to the database
        // Checks that the route id matches the form's EmployeeId
        // Handles concurrency in case the record was deleted mid-edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmployeeId) return NotFound();

            ModelState.Remove("StudioRole");
            ModelState.Remove("LeaveRequests");
            ModelState.Remove("Attendances");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employees.Any(e => e.EmployeeId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StudioRoles = new SelectList(
                _context.StudioRoles, "StudioRoleId", "RoleName", employee.StudioRoleId);
            return View(employee);
        }

        // GET: Employee/Delete/5
        // DELETE (GET) - Shows the confirmation page before deleting
        // Includes StudioRole so the confirmation page can show role name
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.StudioRole)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employee/Delete/5
        // DELETE (POST) - Removes the employee from the database
        // SAFETY CHECK: blocks deletion if a User account is linked
        // to this employee — Admin must remove the User account first
        // This protects the Employee → User FK relationship
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.StudioRole)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            // prevent deletion if a User account is linked
            var hasLinkedUser = await _context.Users
                .AnyAsync(u => u.EmployeeId == id);

            if (hasLinkedUser)
            {
                TempData["ErrorMessage"] = "Cannot delete this employee — they have a linked user account. Remove the user account first.";
                return View(employee);
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}