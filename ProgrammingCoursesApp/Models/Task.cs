using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgrammingCoursesApp.Models
{
    public class Task
    {
        public int Id { get; set; }

        [Required, StringLength(256, MinimumLength = 1)]
        public string Name { get; set; }
        public int? TopicBlockId { get; set; }
        public TopicBlock TopicBlock { get; set; }

        [NotMapped, Required, Range(1, 100, ErrorMessage = "The field {0} must be greater than {1} and less than {2}.")]
        public int Points { get; set; }
    }
}
