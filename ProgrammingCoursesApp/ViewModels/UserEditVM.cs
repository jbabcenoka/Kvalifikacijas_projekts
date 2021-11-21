using System.Collections.Generic;

namespace ProgrammingCoursesApp.ViewModels
{
    public class UserEditVM
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string OldIdentityRole { get; set; }
        public string IdentityRole { get; set; }
        public List<string> IdentityRoles => new List<string>() { "Admin", "CourseCreator", "User" };
    }
}
