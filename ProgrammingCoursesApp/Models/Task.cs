using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgrammingCoursesApp.Models
{
    public class Task
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int? TopicBlockId { get; set; }
        public TopicBlock TopicBlock { get; set; }

        [NotMapped, Required]
        public int Points { get; set; }
    }
}
