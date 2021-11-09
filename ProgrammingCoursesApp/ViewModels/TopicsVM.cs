using ProgrammingCoursesApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.ViewModels
{
    public class TopicsVM
    {
        public string CourseName { get; set; }
        public int CourseId { get; set; }
        public List<Topic> OpenedTopics { get; set; }
        public List<Topic> Topics { get; set; }
    }
}
