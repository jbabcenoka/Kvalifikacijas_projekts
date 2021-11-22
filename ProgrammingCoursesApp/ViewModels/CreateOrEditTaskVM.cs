using ProgrammingCoursesApp.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProgrammingCoursesApp.ViewModels
{
    public class CreateOrEditTaskVM
    {
        public ReadTask ReadTask { get; set; }
        public VideoTask VideoTask { get; set; }
        public Exercise Exercise { get; set; }

        public int TopicId { get; set; }
        public bool IsCreation { get; set; }
        public List<PossibleAnswer> PossibleAnswers { get; set; }

        [Required]
        public int? AnswerId { get; set; }
    }
}
