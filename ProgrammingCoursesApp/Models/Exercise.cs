using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Models
{
    public class Exercise : Task
    {
        public string QuestionText { get; set; }
        public ICollection<PossibleAnswer> PossibleAnswers { get; set; }
    }
}
