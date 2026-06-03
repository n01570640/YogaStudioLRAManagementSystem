/*
 Static data for Attendance STATUS defined here
*/
namespace YogaStudioLRAManagementSystem.Constants
{
    public static class AttendanceStatus
    {
        public const string ABSENT = "Absent";
        public const string PRESENT = "Present";
        public const string ON_LEAVE = "On Leave";

        //pattern used in the Attendance Model REGEX to validate
        public const string VALIDATION_PATTERN = ABSENT + "|" + PRESENT + "|" + ON_LEAVE;
    }
}
