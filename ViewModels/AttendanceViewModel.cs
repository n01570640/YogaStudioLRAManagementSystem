using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using YogaStudioLRAManagementSystem.Models;

namespace YogaStudioLRAManagementSystem.ViewModels
{
    public class AttendanceViewModel
    {
        //shared - used by both Staff and Manager/Admin
        public IEnumerable<Attendance> Records { get; set; } = new List<Attendance>();
        public double TotalHours { get; set; }

        //staff filters by month/year
        [Display(Name = "Month")]
        public int SelectedMonth { get; set; } = DateTime.Today.Month;

        [Display(Name = "Year")]
        public int SelectedYear { get; set; } = DateTime.Today.Year;

        //Manager/Admin filters by date range, employee, status
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Display(Name = "Employee")]
        public int? SelectedEmployeeId { get; set; }

        [Display(Name = "Status")]
        public string? SelectedStatus { get; set; }

        //Staff dropdowns
        public List<SelectListItem> MonthOptions { get; set; } = Enumerable.Range(1, 12)
            .Select(m => new SelectListItem
            {
                Value = m.ToString(),
                Text = new DateTime(2000, m, 1).ToString("MMMM")
            }).ToList();

        public List<SelectListItem> YearOptions { get; set; } = Enumerable.Range(
            DateTime.Today.Year - 2, 3)
            .Select(y => new SelectListItem
            {
                Value = y.ToString(),
                Text = y.ToString()
            }).ToList();

        //Manager/Admin dropdowns, populated by controller
        public List<SelectListItem> EmployeeOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}
