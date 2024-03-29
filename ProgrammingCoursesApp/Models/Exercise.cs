﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgrammingCoursesApp.Models
{
    public class Exercise : Task
    {
        [Required, StringLength(256, MinimumLength = 1)]
        public string QuestionText { get; set; }
        public ICollection<PossibleAnswer> PossibleAnswers { get; set; }
        
        [NotMapped, Required, DisplayName("Answer")]
        public int AnswerId { get; set; }
    }
}
