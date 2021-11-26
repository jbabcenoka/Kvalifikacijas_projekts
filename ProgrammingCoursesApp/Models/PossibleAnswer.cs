using System.ComponentModel.DataAnnotations;

namespace ProgrammingCoursesApp.Models
{
    public class PossibleAnswer
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public int ExerciseId { get; set; }
    }
}
