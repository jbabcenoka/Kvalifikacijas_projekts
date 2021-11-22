using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgrammingCoursesApp.Data;
using ProgrammingCoursesApp.Models;
using ProgrammingCoursesApp.ViewModels;
using Microsoft.AspNet.Identity;

namespace ProgrammingCoursesApp
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null)
            {
                return NotFound();
            }

            //iegūt sarakstu ar tēmas blokiem 
            var topicBlocks = await _context.TopicBlocks
                                .Include(t => t.Topic)
                                .Where(t => t.Topic.Id == id)
                                .OrderBy(t => t.DisplayOrder)
                                .Include(t => t.Task)
                                .ToListAsync();
            var blocks = new List<Block>();

            foreach (var block in topicBlocks)
            {
                var blockTask = new Block
                {
                    TopicBlock = block,
                    IsViewed = false
                };

                if (block.Task.GetType() == typeof(Exercise))
                {
                    var possibleAnswers = await _context.PossibleAnswers.Where(x => x.ExerciseId == block.Task.Id).ToListAsync();
                    blockTask.PossibleAnswers = possibleAnswers;
                }

                blocks.Add(blockTask);
            }

            var vm = new TasksVM
            {
                TopicName = topic.Name,
                TopicId = topic.Id,
                CourseId = topic.CourseId,
                Blocks = blocks
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> TasksForCoursesCreator(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null)
            {
                return NotFound();
            }

            //iegūt sarakstu ar tēmas blokiem 
            var topicBlocks = await _context.TopicBlocks
                                .Include(t => t.Topic)
                                .Where(t => t.Topic.Id == id)
                                .OrderBy(t => t.DisplayOrder)
                                .Include(t => t.Task)
                                .ToListAsync();
            var blocks = new List<Block>();

            foreach (var block in topicBlocks)
            {
                var blockTask = new Block
                {
                    TopicBlock = block
                };

                if (block.Task.GetType() == typeof(Exercise))
                {
                    var possibleAnswers = await _context.PossibleAnswers.Where(x => x.ExerciseId == block.Task.Id).ToListAsync();
                    blockTask.PossibleAnswers = possibleAnswers;
                }

                blocks.Add(blockTask);
            }

            var vm = new TasksVM
            {
                TopicName = topic.Name,
                TopicId = topic.Id,
                CourseId = topic.CourseId,
                Blocks = blocks
            };

            return View("TasksForCoursesCreator", vm);
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public IActionResult CreateExercise(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                TopicId = id.Value
            };

            return View("ExerciseTaskCreateOrEdit", vm);
        }
        
        [Authorize(Roles = "Admin, CourseCreator")]
        public IActionResult CreateVideoTask(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                TopicId = id.Value
            };

            return View("VideoTaskCreateOrEdit", vm);
        }
        
        [Authorize(Roles = "Admin, CourseCreator")]
        public IActionResult CreateReadingTask(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                TopicId = id.Value
            };

            return View("ReadTaskCreateOrEdit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateReadTask(int? id, [Bind("Name,Text,Points")] ReadTask readTask)
        {
            //ja tēma nav noradīta
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .FirstOrDefaultAsync();

            if (topic == null)
            {
                return NotFound();
            }

            var maxDisplayOrder = await _context.TopicBlocks.Where(t => t.TopicId == id).MaxAsync(d => d.DisplayOrder);

            var topicBlock = new TopicBlock();
            topicBlock.Points = readTask.Points;
            topicBlock.DisplayOrder = maxDisplayOrder + 1;
            topicBlock.Topic = topic;
            readTask.TopicBlock = topicBlock;

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.TopicBlocks.AddAsync(topicBlock);
                    await _context.Tasks.AddAsync(readTask);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(TasksForCoursesCreator), new { id = id });
            }

            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                ReadTask = readTask,
                TopicId = id.Value
            };

            return View("ReadTaskCreateOrEdit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateVideoTask(int? id, [Bind("Name,Link,Points")] VideoTask videoTask)
        {
            //ja tēma nav noradīta
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .FirstOrDefaultAsync();

            if (topic == null)
            {
                return NotFound();
            }

            var maxDisplayOrder = await _context.TopicBlocks.Where(t => t.TopicId == id).MaxAsync(d => d.DisplayOrder);

            var topicBlock = new TopicBlock();
            topicBlock.Points = videoTask.Points;
            topicBlock.DisplayOrder = maxDisplayOrder + 1;
            topicBlock.Topic = topic;
            videoTask.TopicBlock = topicBlock;

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.TopicBlocks.AddAsync(topicBlock);
                    await _context.Tasks.AddAsync(videoTask);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(TasksForCoursesCreator), new { id = id });
            }

            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                VideoTask = videoTask,
                TopicId = id.Value
            };

            return View("VideoTaskCreateOrEdit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateExerciseTask(int? id, [Bind("Name,QuestionText,PossibleAnswers,Points")] Exercise exerciseTask)
        {
            //ja tēma nav noradīta
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .FirstOrDefaultAsync();

            if (topic == null) //tēma neeksistē
            {
                return NotFound();
            }

            var maxDisplayOrder = await _context.TopicBlocks.Where(t => t.TopicId == id).MaxAsync(d => d.DisplayOrder);

            var topicBlock = new TopicBlock();  //izveidot jauno tēmas bloku
            topicBlock.Points = exerciseTask.Points;
            topicBlock.DisplayOrder = maxDisplayOrder + 1;
            topicBlock.Topic = topic;
            exerciseTask.TopicBlock = topicBlock;
            
            //atļaut izvedot uzdevumu, kurā ir vismaz 2 atbilžu varianti
            if (exerciseTask.PossibleAnswers == null || exerciseTask.PossibleAnswers.Count < 2)
            {
                return NotFound();
            }

            //atļaut izvedot uzdevumu, kurā ir tikai viena pareiza atbilde
            if (exerciseTask.PossibleAnswers.Where(x => x.IsCorrect).Count() != 1)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var answer in exerciseTask.PossibleAnswers)
                    {
                        await _context.PossibleAnswers.AddAsync(answer);
                    }

                    await _context.TopicBlocks.AddAsync(topicBlock);
                    await _context.Tasks.AddAsync(exerciseTask);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(TasksForCoursesCreator), new { id = id });
            }

            //ja nesanāca izveidot uzdevumu - atgriezties atpakaļ uz skatu
            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                Exercise = exerciseTask,
                TopicId = id.Value
            };

            return View("VideoTaskCreateOrEdit", vm);
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                        .Include(t => t.TopicBlock)
                        .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            var vm = new CreateOrEditTaskVM
            {
                TopicId = task.TopicBlock.TopicId.Value
            };

            task.Points = task.TopicBlock.Points;

            var taskType = task.GetType();
            
            if (task.GetType() == typeof(ReadTask))
            {
                var readTask = (ReadTask)task;
                vm.ReadTask = readTask;
                return View("ReadTaskCreateOrEdit", vm);
            }
            else if (task.GetType() == typeof(VideoTask))
            {
                var videoTask = (VideoTask)task;
                vm.VideoTask = videoTask;
                return View("VideoTaskCreateOrEdit", vm);
            }
            else //exercise
            {
                var exerciseTask = (Exercise)task;
                var possibleAnswers = await _context.PossibleAnswers.Where(t => t.ExerciseId == task.Id).ToListAsync();
                vm.Exercise = exerciseTask;
                vm.AnswerId = exerciseTask.AnswerId;
                vm.PossibleAnswers = possibleAnswers;
                return View("ExerciseTaskCreateOrEdit", vm);
            }
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> EditReadTask(int id, [Bind("Id,TopicBlockId,Name,Text,Points")] ReadTask readTask)
        {
            if (id != readTask.Id)
            {
                return NotFound();
            }

            //atrast tēmas bloku, kuram pieder uzdevums
            var topicBlock = await _context.TopicBlocks
                                .Where(t => t.Id == readTask.TopicBlockId)
                                .FirstOrDefaultAsync();

            if (topicBlock == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    topicBlock.Points = readTask.Points;
                    _context.TopicBlocks.Update(topicBlock);
                    _context.Tasks.Update(readTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(TasksForCoursesCreator), new { id = topicBlock.TopicId });
            }

            var vm = new CreateOrEditTaskVM
            {
                ReadTask = readTask,
                TopicId = id
            };

            return View("ReadTaskCreateOrEdit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> EditVideoTask(int id, [Bind("Id,TopicBlockId,Name,Link,Points")] VideoTask videoTask)
        {
            if (id != videoTask.Id)
            {
                return NotFound();
            }

            //atrast tēmas bloku, kuram pieder uzdevums
            var topicBlock = await _context.TopicBlocks
                                .Where(t => t.Id == videoTask.TopicBlockId)
                                .FirstOrDefaultAsync();

            if (topicBlock == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    topicBlock.Points = videoTask.Points;
                    _context.TopicBlocks.Update(topicBlock);
                    _context.Tasks.Update(videoTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(TasksForCoursesCreator), new { id = topicBlock.TopicId });
            }

            var vm = new CreateOrEditTaskVM
            {
                VideoTask = videoTask,
                TopicId = id
            };

            return View("VideoTaskCreateOrEdit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> EditExerciseTask(int id, [Bind("Id,TopicBlockId,QuestionText,Name,Points,AnswerId")] Exercise exercise, [Bind("Id,Text")] List<PossibleAnswer> possibleAnswers)
        {
            return null;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> SubmitTopicResult(List<Block> Blocks)
        {
            foreach (var block in Blocks.Where(x => x.IsViewed))
            {
                var topicBlock = await _context.TopicBlocks.Include(t => t.Task).Where(t => t.Id == block.TopicBlock.Id).FirstOrDefaultAsync();

                if (topicBlock != null) //topic block ir atrasts
                {
                    var taskResult = await _context.Results
                            .Where(t => t.TaskId == topicBlock.Task.Id && t.User.Id == User.Identity.GetUserId())
                            .FirstOrDefaultAsync(); //atrast lietotāja rezultātu

                    if (taskResult == null) //lietotājam nav rezultāta par šo uzdevumu -izveidot jauno
                    {
                        var newResult = new Result
                        {
                            TaskId = topicBlock.Task.Id
                        };

                        if (topicBlock.Task.GetType() == typeof(ReadTask) || topicBlock.Task.GetType() == typeof(VideoTask))
                        {
                            newResult.Points = topicBlock.Points;
                        }
                        else //ir uzdevums
                        {
                            if (block.SelectedAnswer != null) //ja lietotājs ir atbildējis
                            {
                                var userAnswer = await _context.PossibleAnswers.FindAsync(block.SelectedAnswer);
                                    
                                if (userAnswer != null) //ja lietotāja atbilde eksistē datubāzē
                                {
                                    newResult.Points = userAnswer.IsCorrect ? topicBlock.Points : 0;
                                }

                                newResult.UserAnswer = userAnswer;
                            }
                        }

                        newResult.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == User.Identity.GetUserId());

                        _context.Results.Add(newResult);
                    }
                    else //atrasts rezultāts - atjaunot rezultātu
                    {
                        if (topicBlock.Task.GetType() == typeof(ReadTask) || topicBlock.Task.GetType() == typeof(VideoTask))
                        {
                            taskResult.Points = topicBlock.Points;
                        }
                        else // ir uzdevums
                        {
                            var userAnswer = await _context.PossibleAnswers.FindAsync(block.SelectedAnswer);

                            if (userAnswer != null) //ja lietotāja atbilde eksistē datubāzē
                            {
                                taskResult.Points = userAnswer.IsCorrect ? topicBlock.Points : 0;
                            }

                            taskResult.UserAnswer = userAnswer;
                        }

                        _context.Results.Update(taskResult);
                    }
                    _context.SaveChanges();
                }
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topicBlock = await _context.TopicBlocks.FirstOrDefaultAsync(m => m.Id == id);
            if (topicBlock == null)
            {
                return NotFound();
            }

            return View(topicBlock);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var topicBlock = await _context.TopicBlocks.FindAsync(id);
            _context.TopicBlocks.Remove(topicBlock);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        private bool TopicBlockExists(int id)
        {
            return _context.TopicBlocks.Any(e => e.Id == id);
        }
    }
}
