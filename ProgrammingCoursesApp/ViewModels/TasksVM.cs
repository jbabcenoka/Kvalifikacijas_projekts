using ProgrammingCoursesApp.Models;
using System.Collections.Generic;

namespace ProgrammingCoursesApp.ViewModels
{
    public class TasksVM
    {
        public string TopicName { get; set; }
        public int TopicId { get; set; }
        public int CourseId { get; set; }
        public int? AnswerId { get; set; }
        public List<Block> Blocks { get; set; }
    }

    public class Block
    {
        public TopicBlock TopicBlock { get; set; }
        public int? UserScore { get; set; }
        public bool IsViewed { get; set; }
        public List<PossibleAnswer> PossibleAnswers { get; set; }
        public int? SelectedAnswer { get; set; }
        public PossibleAnswer Answer { get; set; }
        public int? CorrectAnswerId { get; set; }
    }
}
