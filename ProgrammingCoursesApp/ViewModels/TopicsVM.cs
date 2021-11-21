using ProgrammingCoursesApp.Models;
using System.Collections.Generic;

namespace ProgrammingCoursesApp.ViewModels
{
    public class TopicsVM
    {
        public string CourseName { get; set; }
        public int CourseId { get; set; }
        public List<Topic> OpenedTopics { get; set; }
        public IDictionary<int, double> UserTopicsScores { get; set; }
        public List<Topic> Topics { get; set; }
    }
}
