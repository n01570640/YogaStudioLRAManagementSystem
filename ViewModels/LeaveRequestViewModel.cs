using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YogaStudioLRAManagementSystem.Data;
using YogaStudioLRAManagementSystem.Models;

namespace YogaStudioLRAManagementSystem.ViewModels
{
    public class LeaveRequestViewModel
    {
        public IEnumerable<LeaveRequest> MyRequests { get; set; } = new List<LeaveRequest>();
        public IEnumerable<LeaveRequest> TeamRequests { get; set; } = new List<LeaveRequest>();

        public IEnumerable<LeaveRequest> AllRequests { get; set; } = new List<LeaveRequest>();

        //public string Role { get; set; }
    }
}
