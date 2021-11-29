using System.ComponentModel.DataAnnotations;

namespace ProgrammingCoursesApp.Models
{
    public class VideoTask : Task
    {
        [Required, StringLength(256, MinimumLength = 1)]
        public string Link { get; set; }
    }
}
