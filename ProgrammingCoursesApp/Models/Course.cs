using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProgrammingCoursesApp.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, StringLength(256, MinimumLength = 1)]
        public string Name { get; set; }

        [Required, StringLength(256, MinimumLength = 1)]
        public string Description { get; set; }
        public bool IsOpened { get; set; }
        
        public IdentityUser User { get; set; }
        public ICollection<Topic> Topics { get; set; }
    }
}
