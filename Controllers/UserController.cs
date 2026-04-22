using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YogaStudioLRAManagementSystem.Constants;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.ViewModels;

namespace YogaStudioLRAManagementSystem.Controllers
{
    [Authorize(Roles = UserRoles.ADMIN)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /User/Index
        /// Admin only - displays all users with employee name, status and action buttons
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Employee)
                .Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    EmailAddress = u.EmailAddress,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    EmployeeName = $"{u.Employee.FirstName} {u.Employee.LastName}"
                })
                .ToListAsync();

            return View(users);
        }

        /// <summary>
        /// GET: /User/Edit/5
        /// Admin only - loads edit form pre-populated with existing user data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dbUser = await _context.Users.FindAsync(id);
            if (dbUser == null)
                return NotFound();

            var viewModel = new UserViewModel
            {
                UserId = dbUser.UserId,
                UserName = dbUser.UserName,
                EmailAddress = dbUser.EmailAddress,
                Role = dbUser.Role,
                IsActive = dbUser.IsActive
            };

            PopulateRoleOptions(viewModel);
            return View(viewModel);
        }

        /// <summary>
        /// POST: /User/Edit
        /// Admin only - saves updated username, email and role
        /// Checks for duplicate username and email before saving
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dbUser = await _context.Users.FindAsync(model.UserId);
                if (dbUser == null)
                    return NotFound();

                //check if username is taken by another user
                bool userNameTaken = await _context.Users
                    .AnyAsync(u => u.UserName == model.UserName && u.UserId != model.UserId);

                if (userNameTaken)
                {
                    ModelState.AddModelError("UserName", "Username is already taken.");
                    PopulateRoleOptions(model);
                    return View(model);
                }

                //check if email is taken by another user
                bool emailTaken = await _context.Users
                    .AnyAsync(u => u.EmailAddress == model.EmailAddress && u.UserId != model.UserId);

                if (emailTaken)
                {
                    ModelState.AddModelError("EmailAddress", "Email is already in use.");
                    PopulateRoleOptions(model);
                    return View(model);
                }

                dbUser.UserName = model.UserName;
                dbUser.EmailAddress = model.EmailAddress;
                dbUser.Role = model.Role;

                await _context.SaveChangesAsync();

                TempData["Success"] = $"User {model.UserName} updated successfully.";
                return RedirectToAction("Index");
            }

            //validation failed then repopulate dropdown before returning view
            PopulateRoleOptions(model);
            return View(model);
        }

        /// <summary>
        /// GET: /User/Deactivate/5
        /// Admin only - shows deactivation confirmation page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Deactivate(int userId)
        {
            var dbUser = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (dbUser == null)
                return NotFound();

            var viewModel = new UserViewModel
            {
                UserId = dbUser.UserId,
                UserName = dbUser.UserName,
                EmailAddress = dbUser.EmailAddress,
                Role = dbUser.Role,
                IsActive = dbUser.IsActive,
                EmployeeName = $"{dbUser.Employee.FirstName} {dbUser.Employee.LastName}"
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST: /User/DeactivateConfirmed
        /// Admin only - soft deletes a user by setting IsActive to false
        /// Cannot deactivate own account
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateConfirmed(int userId)
        {
            var dbUser = await _context.Users.FindAsync(userId);
            if (dbUser == null)
                return NotFound();

            //prevent admin from deactivating their own account
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != null && int.Parse(currentUserId) == userId)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction("Index");
            }

            dbUser.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{dbUser.UserName} has been deactivated.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /User/Reactivate
        /// Admin only - reactivates a previously deactivated user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int userId)
        {
            var dbUser = await _context.Users.FindAsync(userId);
            if (dbUser == null)
                return NotFound();

            dbUser.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{dbUser.UserName} has been reactivated.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// GET: /User/ResetPassword/5
        /// Admin only - shows reset password confirmation page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var dbUser = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (dbUser == null)
                return NotFound();

            var viewModel = new UserViewModel
            {
                UserId = dbUser.UserId,
                UserName = dbUser.UserName,
                EmailAddress = dbUser.EmailAddress,
                Role = dbUser.Role,
                IsActive = dbUser.IsActive,
                EmployeeName = $"{dbUser.Employee.FirstName} {dbUser.Employee.LastName}"
            };

            return View(viewModel);
        }


        /// <summary>
        /// POST: /User/ResetPasswordConfirmed
        /// Admin only - resets a user's password to a temp value
        /// Forces password change on next login via MustChangePassword flag
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordConfirmed(int userId)
        {
            var dbUser = await _context.Users.FindAsync(userId);
            if (dbUser == null)
                return NotFound();

            // generate temp password
            var tempPassword = $"Temp@{Guid.NewGuid().ToString("N")[..8]}!";

            dbUser.SetPassword(tempPassword);
            dbUser.MustChangePassword = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Password reset for {dbUser.UserName}. Temporary password: {tempPassword}";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Helper - populates role dropdown using UserRoles constants
        /// Called on GET Edit and on failed POST Edit validation
        /// </summary>
        private void PopulateRoleOptions(UserViewModel model)
        {
            model.RoleOptions = new List<SelectListItem>
            {
                new() { Value = UserRoles.ADMIN,   Text = UserRoles.ADMIN },
                new() { Value = UserRoles.MANAGER, Text = UserRoles.MANAGER },
                new() { Value = UserRoles.STAFF,   Text = UserRoles.STAFF }
            };
        }
    }
}
