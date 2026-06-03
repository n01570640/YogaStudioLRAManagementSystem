/*
 Static data for user roles defined here
 */
namespace YogaStudioLRAManagementSystem.Constants
{
    public static class UserRoles
    {
        public const string ADMIN = "Admin";
        public const string MANAGER = "Manager";
        public const string STAFF = "Staff";

        //pattern used in the Users Model REGEX to validate
        public const string VALIDATION_PATTERN = ADMIN + "|" + MANAGER + "|" + STAFF;
    }
}
