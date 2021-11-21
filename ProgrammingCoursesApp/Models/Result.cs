using Microsoft.AspNetCore.Identity;

namespace ProgrammingCoursesApp.Models
{
    public class Result
    {
        public int Id { get; set; }
        public int Points { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public virtual IdentityUser User { get; set; }
        public PossibleAnswer UserAnswer { get; set; }
    }
}
