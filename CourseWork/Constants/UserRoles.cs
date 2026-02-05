namespace CourseWork.Constants
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Customer = "Customer";

        public const string AdminOrManager = Admin + "," + Manager;
    }
}