using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Models
{
    public class Topic
    {
        public int Id { get; set; }

        [Required, StringLength(256, MinimumLength = 1)]
        public string Name { get; set; }

        [Required, StringLength(256, MinimumLength = 1)]
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsOpened { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public IdentityUser User { get; set; }
        public ICollection<TopicBlock> TopicBlocks { get; set; }
    }
}
