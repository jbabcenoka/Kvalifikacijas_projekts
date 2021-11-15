using ProgrammingCoursesApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.ViewModels
{
    public class CreateOrEditTaskVM
    {
        public ReadTask ReadTask { get; set; }
        public VideoTask VideoTask { get; set; }
        public Exercise Exercise { get; set; }

        public int TopicId { get; set; }
        public bool IsCreation { get; set; }
    }
}
