using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Models
{
    public class Topic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsOpened { get; set; }
        public Course Course { get; set; }
        public IdentityUser User { get; set; }
        public ICollection<TopicBlock> TopicBlocks { get; set; }
    }
}
