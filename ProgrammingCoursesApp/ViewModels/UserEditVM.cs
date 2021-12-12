using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProgrammingCoursesApp.ViewModels
{
    public class UserEditVM
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string IdentityRole { get; set; }
        public List<string> IdentityRoles => new List<string>() { "Admin", "CourseCreator", "User" };
    }
}
