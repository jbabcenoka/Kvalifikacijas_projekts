using ProgrammingCoursesApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.ViewModels
{
    public class TasksVM
    {
        public string TopicName { get; set; }
        public int TopicId { get; set; }
        public int CourseId { get; set; }
        public List<TopicBlock> TopicBlocks { get; set; }
    }
}
