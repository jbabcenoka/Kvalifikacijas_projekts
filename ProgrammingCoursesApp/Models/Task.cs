using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TopicBlockId { get; set; }
        public TopicBlock TopicBlock { get; set; }

        [NotMapped]
        public int Points { get; set; }
    }
}
