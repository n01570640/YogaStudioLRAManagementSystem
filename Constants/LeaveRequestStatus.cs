/*
 Static data for leave reuest STATUS defined here
 */
namespace YogaStudioLRAManagementSystem.Constants
{
    public static class LeaveRequestStatus
    {
        public const string PENDING = "Pending";
        public const string APPROVED = "Approved";
        public const string DENIED = "Denied";

        //pattern used in the LeaveRequest Model REGEX to validate
        public const string VALIDATION_PATTERN = PENDING + "|" + APPROVED + "|" + DENIED;

    }
}
