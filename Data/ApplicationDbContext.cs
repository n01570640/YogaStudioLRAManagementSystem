using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YogaStudioLRAManagementSystem.Constants;
using YogaStudioLRAManagementSystem.Models;

namespace YogaStudioLRAManagementSystem.Data
{
    //Class ApplicationDBContext inherits from DBContext
    //DbContext = database connection
    public class ApplicationDbContext : DbContext
    {
        //Dependency injection passed from program.cs - constructor receives db config options from Program.cs
        //DbContextOptions = settings for the db connection
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        //Dbsets - each re[resents a table in the database
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<StudioRole> StudioRoles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Admin Employee
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    EmployeeId = 999,
                    FirstName = "Studio",
                    LastName = "Owner",
                    HireDate = new DateTime(2024, 1, 1),
                    VacationBalance = 0,
                    SickLeaveBalance = 0,
                    StudioRoleId = 1 // Instructor
                }
            );

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 999,
                    UserName = "admin",
                    EmailAddress = "admin@yogastudio.com",
                    Password = new PasswordHasher<User>().HashPassword(null!, "Admin123"),
                    Role = UserRoles.ADMIN,
                    EmployeeId = 999,
                    MustChangePassword = false
                }
            );

            // StudioRoles Seed Data - Static tables only - seeded once and are not editable by users
            modelBuilder.Entity<StudioRole>().HasData(
                new StudioRole
                {
                    StudioRoleId = 1,
                    RoleName = "Instructor",
                    RequiresCertification = true
                },
                new StudioRole
                {
                    StudioRoleId = 2,
                    RoleName = "Receptionist",
                    RequiresCertification = false
                },
                new StudioRole
                {
                    StudioRoleId = 3,
                    RoleName = "Cleaner",
                    RequiresCertification = false
                }
            );

            // LeaveTypes Seed Data — Static tables only - seeded once and are not editable by users
            // AffectsBalance = true  → deducts from VacationBalance or SickLeaveBalance on approval
            // AffectsBalance = false → tracked in LeaveRequests history but no balance deduction
            // IsPaid = true          → paid leave
            // IsPaid = false         → unpaid leave
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType
                {
                    LeaveTypeId = 1,
                    Name = "Vacation",
                    IsPaid = true,
                    AffectsBalance = true,  //deducts from VacationBalance
                    MinDays = 1,
                    MaxDays = 14
                },
                new LeaveType
                {
                    LeaveTypeId = 2,
                    Name = "Sick - Paid",
                    IsPaid = true,
                    AffectsBalance = true,  //deducts from SickLeaveBalance
                    MinDays = 1,
                    MaxDays = 3             //ontario standard: 3 paid sick days
                },
                new LeaveType
                {
                    LeaveTypeId = 3,
                    Name = "Sick - Unpaid",
                    IsPaid = false,
                    AffectsBalance = false, //no balance deduction - manager approval only
                    MinDays = 1,
                    MaxDays = 30
                },
                new LeaveType
                {
                    LeaveTypeId = 4,
                    Name = "Parental Leave",
                    IsPaid = true,
                    AffectsBalance = false, //does not affect balance - tracked in history only
                    MinDays = 1,
                    MaxDays = 365
                },
                new LeaveType
                {
                    LeaveTypeId = 5,
                    Name = "Bereavement",
                    IsPaid = true,
                    AffectsBalance = false, //does not affect balance - tracked in history only
                    MinDays = 1,
                    MaxDays = 5
                },
                new LeaveType
                {
                    LeaveTypeId = 6,
                    Name = "Certification",
                    IsPaid = false,
                    AffectsBalance = false, //does not affect balance - tracked in history only
                    MinDays = 1,
                    MaxDays = 7
                }
            );
        }
    }
}
