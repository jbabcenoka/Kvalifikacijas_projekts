using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Models
{
    public class TopicBlock
    {
        public int Id { get; set; }
        public int DisplayOrder { get; set; }

        [Range(1, 100, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int Points { get; set; }
        public int? TopicId { get; set; }
        public Topic Topic { get; set; }
        public Task Task { get; set; }
    }
}
