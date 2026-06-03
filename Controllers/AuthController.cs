using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YogaStudioLRAManagementSystem.Constants;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.Models;
using YogaStudioLRAManagementSystem.ViewModels;

namespace YogaStudioLRAManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        //dbcontext passed in from program.cs
        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        ///  GET: /Auth/Login 
        ///  Redirects to home if user is already logged in
        /// </summary>
        /// <returns>Empty login form</returns>
        [HttpGet]
        public IActionResult Login()
        {
            //If already logged in redirect to home
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// POST: /Auth/Login
        /// Validates credentials, creates claims, signs in user
        /// Redirects to ChangePassword if MustChangePassword flag is true
        /// </summary>
        /// <param name="user">User entered login data</param>
        /// <returns>Redirects if authenticated otherwise returns user form with entered data</returns>
        [HttpPost] //Login form
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginUser )
        {
            if (ModelState.IsValid)
            {
                //find user in db by username
                var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == loginUser.UserName);

                //verify user exists and hash password match
                if (dbUser != null && dbUser.VerifyPassword(loginUser.Password))
                {
                    //block deactivated users
                    if (!dbUser.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "This account has been deactivated. Contact your administrator.");
                        return View(loginUser);
                    }

                    //build claims for cookie 
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, dbUser.UserName),
                        new Claim(ClaimTypes.NameIdentifier, dbUser.UserId.ToString()), //Claim only accepts string value
                        new Claim(ClaimTypes.Role, dbUser.Role), //role comes from DB users table
                        new Claim("EmployeeId", dbUser.EmployeeId.ToString())
					};

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    //sign in with presistent cookie based on RememberMe
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                         new AuthenticationProperties { IsPersistent = loginUser.RememberMe });
                    
                    //generate JWT for AttendanceAPI calls
                    var jwtKey = _configuration["Jwt:Key"]!; //read shared secret key from appsetting.json
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)); //convert key string to bytes
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); //sign with HMAC SHA256

                    var jwtToken = new JwtSecurityToken(
                        claims: claims, //same claims already built above
                        expires: DateTime.Now.AddHours(8), //token expires in 8 hrs
                        signingCredentials: creds
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken); //serialize token to string

                    //store in session so views can access it
                    HttpContext.Session.SetString("jwt", tokenString);
                    
                    //Force password change on first login
                    if (dbUser.MustChangePassword)
                    {
                        return RedirectToAction("ChangePassword");
                    }
                    return RedirectToAction("Index", "Home"); //redirects to Home for now - needs to change
                }

                //Invalid credentials without revealing which field was wrong
                ModelState.AddModelError(string.Empty, "Invalid username or password");
               
            }
            return View(loginUser);
        }

        /// <summary>
        /// GET: /Auth/Register
        /// Only accessible by ADMIN 
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Registeration form with Employee and Role dropdowns</returns>
        [HttpGet] //Register Form
        [Authorize(Roles = UserRoles.ADMIN)]
        public async Task<IActionResult> Register()
        {
            var registerUser = new RegisterViewModel();
            await PopulateDropdowns(registerUser);
            return View(registerUser);
        }

        /// <summary>
        /// POST: /Auth/Register
        /// Only accessible by ADMIN
        /// Creates a new User account linked to an existing Employee
        /// flow -> Admin creates a new employee ->
        /// Admin registers the users with EmployeeId with temp password ->
        /// User prompted to change password on first login
        /// </summary>
        /// <param name="registerUser">Admin entered register user data</param>
        /// <returns>Success message if validated otherwise Registeration form with retained user entery</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles =UserRoles.ADMIN)]
        public async Task<IActionResult> Register(RegisterViewModel registerUser)
        {
            if (ModelState.IsValid)
            {
                //check if username already exists
                bool userNameTaken = await _context.Users.AnyAsync(u => u.UserName == registerUser.UserName);

                if (userNameTaken) //if true, error username taken - return registration form
                {
                    ModelState.AddModelError("UserName", "Username is already taken.");
                    await PopulateDropdowns(registerUser); //repopulate
                    return View(registerUser);
                }

                //check if email already exists
                bool emailTaken = await _context.Users.AnyAsync(u => u.EmailAddress == registerUser.EmailAddress);

                if (emailTaken) //if true, error email taken - return registration form
                {
                    ModelState.AddModelError("EmailAddress", "An account with this email already exists.");
                    await PopulateDropdowns(registerUser); //repopulate
                    return View(registerUser);
                }

                //check if employee already has an account
                bool employeeHasAccount = await _context.Users.AnyAsync(u => u.EmployeeId == registerUser.EmployeeId);

                if (employeeHasAccount) //if true, error employee account already exists - return form
                {
                    ModelState.AddModelError("EmployeeId", "This employee already has an account.");
                    await PopulateDropdowns(registerUser); //repopulate
                    return View(registerUser);
                }

                //build the new User - MustChangePassword is true by default
                var user = new User
                {
                    UserName = registerUser.UserName,
                    EmailAddress = registerUser.EmailAddress,
                    Role = registerUser.Role,
                    EmployeeId = registerUser.EmployeeId
                };

                //Hash the password before saving
                user.SetPassword(registerUser.Password);

                //save the user to db table
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Account created for {registerUser.UserName}. They will be prompted to change their password on first login.";
                return RedirectToAction("Register");
            }

            //validation failed - repopulate dropdowns before returning view
            await PopulateDropdowns(registerUser);
            return View(registerUser);
        }

        /// <summary>
        /// GET: /Auth/ChangePassword
        /// Accessible by any authenticated user
        /// </summary>
        /// <returns>Empty change password form</returns>
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// POST: /Auth/ChangePassword
        /// verifies current password, hashes and saves new password
        /// clears MustChangePassword flag on success
        /// </summary>
        /// <param name="registerUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                //get current logged in user's ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //redirect to sign in if not logged in
                if (userIdClaim == null)
                {
                    return RedirectToAction("Login");
                }

                int userId = int.Parse(userIdClaim);
                var dbUser = await _context.Users.FindAsync(userId);

                if (dbUser == null)
                {
                    return RedirectToAction("Login");
                }

                //verify current password is correct
                if (!dbUser.VerifyPassword(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                //Hash and save new password
                dbUser.SetPassword(model.NewPassword);

                //clear the change password flag
                dbUser.MustChangePassword = false;

                await _context.SaveChangesAsync(); //save changes

                TempData["Success"] = "Password changes successfully!";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
       
        /// <summary>
        /// POST: /Auth/Logout
        /// Signes out user and clear session cookie
        /// </summary>
        /// <returns>Successful signout redrects to logine</returns>
        [HttpPost] //Logout functionality
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        /// <summary>
        /// GET: /Auth/AccessDenied
        /// Shows access denied if user is not authenticated/authorized to access a page
        /// </summary>
        /// <returns>Access denied page</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// helper function - Populates Employee and Role dropdowns for the register form
        /// Only shows employees who don't have a user account yet
        /// </summary>
        /// <param name="registerUser">Admin entered registeration form data</param>
        /// <returns>Select list of employees and roles</returns>
        private async Task PopulateDropdowns(RegisterViewModel registerUser)
        {
            //only employees without an existing account
            var employeesWithoutAccounts = await _context.Employees
                .Where(e => !_context.Users.Any(u => u.EmployeeId == e.EmployeeId)).ToListAsync();

            registerUser.EmployeeOptions = employeesWithoutAccounts
                .Select(e=> new SelectListItem
                    {
                        Value= e.EmployeeId.ToString(),
                        Text = $"{e.FirstName} {e.LastName}"
                    }).ToList();

            //Role options from constants
            registerUser.RoleOptions = new List<SelectListItem>
            {
                new SelectListItem{Value = UserRoles.ADMIN, Text = "Admin"},
                new SelectListItem{Value = UserRoles.MANAGER, Text ="Manager" },
                new SelectListItem{Value = UserRoles.STAFF, Text = "Staff" }
            };

        }
    }
}