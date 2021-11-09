using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Models
{
    public class TopicBlock
    {
        public int Id { get; set; }
        public int DisplayOrder { get; set; }
        public int Points { get; set; }
        public int? TopicId { get; set; }
        public Topic Topic { get; set; }
        public Task Task { get; set; }
    }
}
