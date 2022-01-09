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
using ProgrammingCoursesApp.Controllers;
using System;

namespace ProgrammingCoursesApp
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Uzdevumu saraksta attēlošana - UM-01
        [Authorize]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            //atrast uzdevuma tēmu
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);

            //ja tēma neeksistē datubāzē - kļūda
            if (topic == null)
            {
                return NotFound();
            }

            //ja kursa tēma nav publicēta - kļūda
            if (!topic.IsOpened)
            {
                return NotFound();
            }

            //ja kurss nav publicēts - kļūda
            var isOpenedCourse = await _context.Courses
                .Where(x => x.Id == topic.CourseId)
                .Select(x => x.IsOpened)
                .FirstOrDefaultAsync();
            if (!isOpenedCourse)
            {
                return NotFound();
            }

            //iegūt sarakstu ar tēmas blokiem (tēmas uzdevumiem)
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

                //ja uzdevuma tips ir jautājums
                if (block.Task.GetType() == typeof(Exercise))
                {
                    //iegūt atbilžu variantus
                    var possibleAnswers = await _context.PossibleAnswers.Where(x => x.ExerciseId == block.Task.Id).ToListAsync();
                    blockTask.PossibleAnswers = possibleAnswers;

                    //iegūt pareizo atbildi
                    blockTask.CorrectAnswerId = possibleAnswers.Where(x => x.IsCorrect).Select(x => x.Id).FirstOrDefault();

                    //atrast lietotāja atbildi un rezultātu par uzdevumu
                    var userAnswer = await _context.Results
                            .Where(x => x.User.Id == User.Identity.GetUserId() && x.TaskId == block.Task.Id)
                            .Select(x => new { x.Points, x.UserAnswer })
                            .FirstOrDefaultAsync();

                    //ja lietotājam ir rezultāts par uzdevumu - attēlot lietotāja atbildi un rezultātu
                    if (userAnswer != null) 
                    {
                        blockTask.SelectedAnswer = userAnswer.UserAnswer?.Id;
                        blockTask.UserScore = userAnswer.Points;
                    }
                }
                else //uzdevuma tips ir video vai lāsāmais materiāls
                {
                    //atrast lietotāja rezultātu par uzdevumu (lietotājs ir skatījis vai nē)
                    var userResult = await _context.Results
                            .Where(x => x.User.Id == User.Identity.GetUserId() && x.TaskId == block.Task.Id)
                            .FirstOrDefaultAsync();

                    //ja lietotājam ir reltāts par uzdevumu - attēlot lietotāja rezultātu
                    if (userResult != null)
                    {
                        blockTask.UserScore = userResult.Points;
                    }
                }

                //ja ir rezultāts par uzdevumu - atzīmēt skatā, ka lietotājam ir rezultāts par uzdevumu
                blockTask.IsViewed = blockTask.UserScore != null ? true : false;

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

        //Uzdevumu attēlošana kursu veidotājam vai administratoram - UM-02
        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> TasksForCoursesCreator(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            //Atrast kursa tēmu ar noteikto identifikatoru
            var topic = await _context.Topics
                .Where(t => t.Id == id)
                .Include(t => t.Course.User)
                .FirstOrDefaultAsync();

            //Ja datubāzē neeksistē kursa tēma ar noteikto identifikatoru - kļūdas paziņojums
            if (topic == null)
            {
                return NotFound();
            }

            //Ja uzdevumu sarakstam mēģina piekļūt tas, kurš nav kursa autors - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            //iegūt sarakstu ar tēmas blokiem (uzdevumiem)
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

                //ja uzdevums ir ar tipu jautājums
                if (block.Task.GetType() == typeof(Exercise))
                {
                    //iegūt jautājuma atbilžu variantus
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

        //Skata atvēršana - jautājuma izveidošanai UM-03
        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateExercise(int? id)
        {
            //ja tēma nav noradīta
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .Include(t => t.Course.User)
                                .FirstOrDefaultAsync();

            //tēma neeksistē - kļūda
            if (topic == null)
            {
                return NotFound();
            }

            //Ja uzdevuma izveidošanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                TopicId = id.Value
            };

            return View("ExerciseTaskCreateOrEdit", vm);
        }

        //Jautājuma un tā atbilžu variantu izveidošana - UM-03
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateExerciseTask(int? id, [Bind("Name,QuestionText,Points,AnswerId")] Exercise exercise, [Bind("Text")] List<PossibleAnswer> possibleAnswers)
        {
            //ja tēma nav noradīta
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .Include(t => t.Course.User)
                                .FirstOrDefaultAsync();

            //tēma neeksistē - kļūda
            if (topic == null)
            {
                return NotFound();
            }

            //Ja uzdevuma izveidošanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            if (possibleAnswers.Count < 2)
            {
                var vm = new CreateOrEditTaskVM
                {
                    IsCreation = true,
                    Exercise = exercise,
                    PossibleAnswers = possibleAnswers.Count == 0 ? new List<PossibleAnswer>() : possibleAnswers,
                    TopicId = id.Value
                };

                return View("ExerciseTaskCreateOrEdit", vm);
            }

            var topicBlock = new TopicBlock();  //izveidot jauno tēmas bloku

            //kārtas numurs tiek piešķirts pēdējais
            var topicTasks = await _context.TopicBlocks.Where(t => t.TopicId == id).ToListAsync();
            topicBlock.DisplayOrder = topicTasks == null || !topicTasks.Any() ? 0 : topicTasks.Max(d => d.DisplayOrder) + 1;

            topicBlock.Points = exercise.Points;
            topicBlock.Topic = topic;
            exercise.TopicBlock = topicBlock;

            try
            {
                await _context.TopicBlocks.AddAsync(topicBlock);
                await _context.Tasks.AddAsync(exercise);

                //saglabājam katru atbilžu variantu
                foreach (var answer in possibleAnswers)
                {
                    //atzīmējam atbilžu varianta pareizību
                    if (possibleAnswers.IndexOf(answer) == exercise.AnswerId)
                    {
                        answer.IsCorrect = true;
                    }
                    else
                    {
                        answer.IsCorrect = false;
                    }

                    if (exercise.PossibleAnswers == null)
                    {
                        exercise.PossibleAnswers = new List<PossibleAnswer>();
                    }

                    exercise.PossibleAnswers.Add(answer);
                }

                await _context.AddAsync(exercise);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToAction(nameof(TasksForCoursesCreator), new { id = id });
        }

        //Skata atvēršana - lasāma materiāla izveidošanai UM-04
        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateReadingTask(int? id)
        {
            //ja tēma nav noradīta
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .Include(t => t.Course.User)
                                .FirstOrDefaultAsync();

            //tēma neeksistē - kļūda
            if (topic == null)
            {
                return NotFound();
            }

            //Ja uzdevuma izveidošanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                TopicId = id.Value
            };

            return View("ReadTaskCreateOrEdit", vm);
        }

        //Lasāmā materiāla izveidošana - UM-04
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateReadTask(int? id, [Bind("Name,Text,Points")] ReadTask readTask)
        {
            //ja tēma nav noradīta - kļūda
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .Include(t => t.Course.User)
                                .FirstOrDefaultAsync();

            //tēma nav atrasta - kļūda
            if (topic == null)
            {
                return NotFound();
            }

            //ja uzdevuma izveidošanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //kārtas numurs tiek piešķirts pēdējais
                    var topicBlocks = await _context.TopicBlocks.Where(t => t.TopicId == id).ToListAsync();
                    var maxDisplayOrder = topicBlocks == null || !topicBlocks.Any() ? 0 : topicBlocks.Max(x => x.DisplayOrder);

                    var topicBlock = new TopicBlock();
                    topicBlock.Points = readTask.Points;
                    topicBlock.DisplayOrder = maxDisplayOrder + 1;
                    topicBlock.Topic = topic;   
                    readTask.TopicBlock = topicBlock;

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

        //Skata atvēršana - video materiāla izveidošanai UM-05
        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateVideoTask(int? id)
        {
            //ja tēma nav noradīta
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .Include(t => t.Course.User)
                                .FirstOrDefaultAsync();

            //tēma neeksistē - kļūda
            if (topic == null)
            {
                return NotFound();
            }

            //Ja uzdevuma izveidošanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            var vm = new CreateOrEditTaskVM
            {
                IsCreation = true,
                TopicId = id.Value
            };

            return View("VideoTaskCreateOrEdit", vm);
        }

        //Video materiāla izveidošana - UM-05
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateVideoTask(int? id, [Bind("Name,Link,Points")] VideoTask videoTask)
        {
            //ja tēma nav noradīta - kļūda
            if (id == null)
            {
                return NotFound();
            }

            //atrast tēmu
            var topic = await _context.Topics
                                .Where(t => t.Id == id)
                                .Include(t => t.Course.User)
                                .FirstOrDefaultAsync();

            //tēma nav atrasta - kļūda
            if (topic == null)
            {
                return NotFound();
            }

            //ja uzdevuma izveidošanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            //kārtas numurs tiek piešķirts pēdējais
            var topicBlocks = await _context.TopicBlocks.Where(t => t.TopicId == id).ToListAsync();
            var maxDisplayOrder = topicBlocks == null || !topicBlocks.Any() ? 0 : topicBlocks.Max(x => x.DisplayOrder);

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

        //Funkcija atbildes varianta pievienošanai
        [HttpPost, Authorize(Roles = "Admin, CourseCreator")]
        public ActionResult AddPossibleAnswer(int index)
        {
            var newAnswer = new PossibleAnswer();
            // http://source.entelect.co.za/dynamically-add-and-remove-rows-for-a-table-in-mvc
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("PossibleAnswers[{0}]", index);
            
            return PartialView("PossibleAnswer", newAnswer);
        }

        //Skata atvēršana (video, lasama materiāla vai jautājuma rediģēšanai)
        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //ja uzdevums neeksistē - kļūda
            var task = await _context.Tasks
                        .Include(t => t.TopicBlock)
                        .Include(t => t.TopicBlock.Topic.Course.User)
                        .Where(t => t.Id == id)
                        .FirstOrDefaultAsync();
            if (task == null)
            {
                return NotFound();
            }

            //Ja uzdevuma izveidošanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (task.TopicBlock.Topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            var vm = new CreateOrEditTaskVM
            {
                TopicId = task.TopicBlock.TopicId.Value
            };

            task.Points = task.TopicBlock.Points;

            var taskType = task.GetType();
            
            //atvērt skatu atkarībā no uzdevuma tipa
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
            else //Uzdevums
            {
                var exerciseTask = (Exercise)task;
                var possibleAnswers = await _context.PossibleAnswers.Where(t => t.ExerciseId == task.Id).ToListAsync();
                vm.Exercise = exerciseTask;
                vm.AnswerId = exerciseTask.AnswerId;
                vm.PossibleAnswers = possibleAnswers;
                return View("ExerciseTaskCreateOrEdit", vm);
            }
        }

        //Jautājuma un tā atbilžu variantu rediģēšana - UM-06
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> EditExerciseTask(int id, [Bind("Id,TopicBlockId,QuestionText,Name,Points,AnswerId")] Exercise exercise, [Bind("Id,Text")] List<PossibleAnswer> possibleAnswers)
        {
            if (id != exercise.Id)
            {
                return NotFound();
            }

            //atrast tēmas bloku, kuram pieder uzdevums
            var topicBlock = await _context.TopicBlocks
                                .Where(t => t.Id == exercise.TopicBlockId)
                                .Include(t => t.Topic.Course.User)
                                .FirstOrDefaultAsync();

            //ja tēmas bloks nav atrasts - kļūda
            if (topicBlock == null)
            {
                return NotFound();
            }

            //ja uzdevuma rediģēšanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topicBlock.Topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    exercise.PossibleAnswers = await _context.PossibleAnswers.Where(x => x.ExerciseId == exercise.Id).ToListAsync();

                    foreach (var answer in possibleAnswers)
                    {
                        if (answer.Id == 0) //ir jaunā atbilde
                        {
                            //pievienot atbildi
                            var newAnswer = new PossibleAnswer();
                            newAnswer.IsCorrect = possibleAnswers.IndexOf(answer) == exercise.AnswerId ? true : false;
                            newAnswer.Text = answer.Text;
                            exercise.PossibleAnswers.Add(newAnswer);
                        }
                        else // ir vecā atbilde
                        {
                            var ans = exercise.PossibleAnswers.FirstOrDefault(x => x.Id == answer.Id);
                            //atzīmējam atbilžu varianta pareizību
                            ans.IsCorrect = possibleAnswers.IndexOf(answer) == exercise.AnswerId ? true : false;
                            //jauns teksts
                            ans.Text = answer.Text;
                        }
                    }
                    
                    _context.Exercises.Update(exercise);

                    var newPossibleAnswersIds = possibleAnswers.Select(x => x.Id).ToList();

                    //atrodam atbildes, kurus ir niepieciešams dzēst
                    var answersToDelete = exercise.PossibleAnswers.Where(x => !newPossibleAnswersIds.Contains(x.Id)).ToList();
                    if (answersToDelete != null)
                    {
                        foreach (var a in answersToDelete)
                        {
                            //ja par dzēsto atbildes variantu bija rezultāti - dzēst arī rezultātus.
                            var resultsForDeleted = await _context.Results.Where(x => x.TaskId == exercise.Id && x.UserAnswer.Id == a.Id).ToListAsync();
                            foreach(var resForDel in resultsForDeleted)
                            {
                                _context.Results.Remove(resForDel);
                            }
                            exercise.PossibleAnswers.Remove(a);
                        }
                    }

                    _context.Exercises.Update(exercise);

                    _context.SaveChanges();

                    //ja tiek mainīts punktu skaits
                    if (topicBlock.Points != exercise.Points)
                    {
                        //mainīt punktus
                        topicBlock.Points = exercise.Points;
                        _context.TopicBlocks.Update(topicBlock);
                    }

                    //mainām lietotāju rezultātus par šo uzdevumu
                    var results = await _context.Results.Where(x => x.TaskId == exercise.Id).ToListAsync();

                    //atrast pareizas atbildes identifikatoru
                    var correctAnswerId = exercise.PossibleAnswers.Where(x => x.IsCorrect).Select(x => x.Id).FirstOrDefault();

                    foreach(var result in results)
                    {
                        if (result.UserAnswer.Id == correctAnswerId) //lietotāja resultāts bija pareizs
                        {
                            //piešķirt maksimumu (ir pareizā atbilde)
                            result.Points = exercise.Points;
                        }
                        else //lietotāja resultāts bija nepareizs
                        {
                            //piešķirt 0 punktus (ir nepareizā atbilde)
                            result.Points = 0;
                        }
                        _context.Results.Update(result);
                    }

                    //mainīt punktus
                    topicBlock.Points = exercise.Points;
                    _context.TopicBlocks.Update(topicBlock);

                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(TasksForCoursesCreator), new { id = topicBlock.TopicId });
            }

            var vm = new CreateOrEditTaskVM
            {
                Exercise = exercise,
                PossibleAnswers = possibleAnswers,
                TopicId = id
            };

            return View("ExerciseTaskCreateOrEdit", vm);
        }

        //Lasāma materiāla rediģēšana - UM-07
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
                                .Include(x => x.Topic.Course.User)
                                .FirstOrDefaultAsync();
            
            //ja tēmas bloks nav atrasts - kļūda
            if (topicBlock == null)
            {
                return NotFound();
            }

            //ja uzdevuma rediģēšanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topicBlock.Topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //ja lietotājs maina punktus par uzdevumu
                    if (topicBlock.Points != readTask.Points)
                    {
                        //piešķirt atjaunotos punktus lietotājiem
                        var usersResults = await _context.Results.Where(x => x.TaskId == readTask.Id).ToListAsync();

                        foreach(var userResult in usersResults)
                        {
                            //par lasāmo uzdevumu vienmēr ir maksimums (lietotājs ir lasījis materiālu)
                            userResult.Points = readTask.Points;
                            _context.Results.Update(userResult);
                        }
                    }

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

        //Video materiāla rediģēšana - UM-08
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
                                .Include(x => x.Topic.Course.User)
                                .FirstOrDefaultAsync();

            if (topicBlock == null)
            {
                return NotFound();
            }

            //ja uzdevuma rediģēšanai mēģina piekļūt tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (topicBlock.Topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //ja lietotājs maina punktus par uzdevumu
                    if (topicBlock.Points != videoTask.Points)
                    {
                        //piešķirt atjaunotos punktus lietotājiem
                        var usersResults = await _context.Results.Where(x => x.TaskId == videoTask.Id).ToListAsync();

                        foreach (var userResult in usersResults)
                        {
                            //par video uzdevumu vienmēr ir maksimum (lietotājs ir skatījies video)
                            userResult.Points = videoTask.Points;
                            _context.Results.Update(userResult);
                        }
                    }

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

        //Uzdevuma dzēšana - UM-09
        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskTopicId = await _context.Tasks
                .Where(x => x.Id == id)
                .Include(x => x.TopicBlock.Topic.Course.User)
                .Select(x => x.TopicBlock.TopicId)
                .FirstOrDefaultAsync();

            var creatorId = await _context.Tasks
                .Where(x => x.Id == id)
                .Select(x => x.TopicBlock.Topic.Course.User.Id)
                .FirstOrDefaultAsync();

            //ja uzdevuma mēģina dzēst tas, kurš nav kursa autors vai administrators - kļūda
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (creatorId != currentUserId)
                {
                    return NotFound();
                }
            }

            await DeleteTask(id.Value, _context);

            return RedirectToAction(nameof(TasksForCoursesCreator), new { id = taskTopicId });
        }

        //Uzdevuma dzēšana - UM-09
        [HttpPost, Authorize(Roles = "Admin, CourseCreator")]
        public static async System.Threading.Tasks.Task DeleteTask(int id, ApplicationDbContext context)
        {
            try
            {
                //mēģina atrast uzdevumu
                var task = await context.Tasks.FindAsync(id);

                //dzest uzdevuma rezultātus
                var taskResults = await context.Results.Where(x => x.TaskId == task.Id).ToListAsync();
                context.Results.RemoveRange(taskResults);

                //dzēst uzdevuma atbildes
                var taskAnswers = await context.PossibleAnswers.Where(x => x.ExerciseId == task.Id).ToListAsync();
                context.PossibleAnswers.RemoveRange(taskAnswers);
            
                var topicBlock = await context.TopicBlocks.Where(x => x.Id == task.TopicBlockId).FirstOrDefaultAsync();

                context.Tasks.Remove(task);  //dzēst uzdevumu
                context.TopicBlocks.Remove(topicBlock);  //dzēst tēmas bloku

                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Rezultātu saglabāšana - UM-10
        [HttpPost, Authorize]
        public async Task<IActionResult> SubmitTopicResult(List<Block> Blocks, int courseId)
        {
            foreach (var block in Blocks.Where(x => x.IsViewed))
            {
                var topicBlock = await _context.TopicBlocks
                    .Include(t => t.Task)
                    .Where(t => t.Id == block.TopicBlock.Id)
                    .FirstOrDefaultAsync();

                if (topicBlock != null) //topic block ir atrasts
                {
                    var taskResult = await _context.Results
                            .Where(t => t.TaskId == topicBlock.Task.Id && t.User.Id == User.Identity.GetUserId())
                            .FirstOrDefaultAsync(); //atrast lietotāja rezultātu

                    if (taskResult == null) //lietotājam nav rezultāta par šo uzdevumu - izveidot jaunu
                    {
                        var newResult = new Result
                        {
                            TaskId = topicBlock.Task.Id
                        };

                        if (topicBlock.Task.GetType() == typeof(ReadTask) || topicBlock.Task.GetType() == typeof(VideoTask))
                        {
                            newResult.Points = topicBlock.Points;
                            newResult.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == User.Identity.GetUserId());
                            _context.Results.Add(newResult);
                        }
                        else //ir uzdevums
                        {
                            if (block.SelectedAnswer != null) //ja lietotājs ir atbildējis
                            {
                                var userAnswer = await _context.PossibleAnswers.FindAsync(block.SelectedAnswer);

                                if (userAnswer != null) //ja atbilde eksistē datubāzē
                                {
                                    newResult.Points = userAnswer.IsCorrect ? topicBlock.Points : 0;
                                    newResult.UserAnswer = userAnswer;
                                    newResult.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == User.Identity.GetUserId());
                                    _context.Results.Add(newResult);
                                }
                            }
                        }
                    }
                    else //atrasts rezultāts - atjaunot rezultātu
                    {
                        if (topicBlock.Task.GetType() == typeof(Exercise))
                        {
                            if (block.SelectedAnswer != null) //ja lietotājs ir atbildējis
                            {
                                var userAnswer = await _context.PossibleAnswers.FindAsync(block.SelectedAnswer);

                                if (userAnswer != null) //ja atbilde eksistē datubāzē
                                {
                                    taskResult.Points = userAnswer.IsCorrect ? topicBlock.Points : 0; //piešķirt jaunus punktus
                                    taskResult.UserAnswer = userAnswer;
                                    _context.Results.Update(taskResult);
                                }
                            }
                        }
                    }
                    _context.SaveChanges();
                }
            }

            return RedirectToAction(nameof(TopicsController.Index), "Topics", new { id = courseId });
        }
    }
}
