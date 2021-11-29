using System.ComponentModel.DataAnnotations;

namespace ProgrammingCoursesApp.Models
{
    public class ReadTask : Task
    {
        [Required, StringLength(4000, MinimumLength = 1)]
        public string Text { get; set; }
    }
}
