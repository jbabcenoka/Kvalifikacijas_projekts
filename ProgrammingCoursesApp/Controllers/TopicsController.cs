using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgrammingCoursesApp.Data;
using ProgrammingCoursesApp.Models;
using ProgrammingCoursesApp.ViewModels;

namespace ProgrammingCoursesApp.Controllers
{
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TopicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            //kurss ir aizvērts - kļūda
            if (!course.IsOpened)
            {
                return NotFound();
            }

            var openedTopics = await _context.Topics
                                .Where(t => t.CourseId == id && t.IsOpened)
                                .OrderBy(t => t.DisplayOrder)
                                .ToListAsync();

            Dictionary<int, double> userTopicsScores = new Dictionary<int, double>();

            if (User.Identity.IsAuthenticated)
            {
                foreach(var topic in openedTopics)
                {
                    var topicMaxPoints = (await _context.TopicBlocks.Where(t => t.TopicId == topic.Id).ToListAsync()).Sum(x => x.Points);
                
                    var tasksIds = await _context.TopicBlocks.Where(t => t.TopicId == topic.Id).Select(t => t.Task.Id).ToListAsync();

                    int userPoints = 0;
                    foreach(var taskId in tasksIds)
                    {
                        var pointsForTask = await _context.Results
                            .Where(x => x.TaskId == taskId && x.User.Id == User.Identity.GetUserId())
                            .Select(x => x.Points)
                            .FirstOrDefaultAsync();
                        userPoints = userPoints + pointsForTask;
                    }

                    double userTopicResult = userPoints == 0 ? 0 : (int)Math.Round((double)(100 * userPoints) / topicMaxPoints);

                    userTopicsScores.Add(topic.Id, userTopicResult);
                }
            }

            var vm = new TopicsVM
            {
                CourseName = course.Name,
                CourseId = course.Id,
                OpenedTopics = openedTopics,
                UserTopicsScores = userTopicsScores
            };

            return View("Topics", vm);
        }

        [Authorize(Roles = "Admin,CourseCreator")]
        public async Task<IActionResult> TopicsForCreator(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            //iegūt kursu ar tēmām
            var topics = await _context.Topics
                            .Where(c => c.CourseId == id)
                            .OrderBy(t => t.DisplayOrder)
                            .ToListAsync();

            var vm = new TopicsVM
            {
                CourseName = course.Name,
                CourseId = course.Id,
                Topics = topics
            };

            return View("TopicsForCreator", vm);
        }

        [HttpGet, Authorize(Roles = "Admin,CourseCreator")]
        public async Task<IActionResult> ChangeTopicsOrder(int? id, List<int> topicsInOrder)
        {
            if (id == null)
            {
                return NotFound();
            }

            //ja kurss nesatur uzdevumu - kļūda
            if (topicsInOrder == null || !topicsInOrder.Any())
            {
                return NotFound();
            }

            //atrodam pirmo tēmu, lai uzzinātu kursu
            var firstTopicCourseId = await _context.Topics.Where(x => x.Id == topicsInOrder.First()).Select(x => x.CourseId).FirstOrDefaultAsync();
            
            //atrodam kursa autoru
            var courseUserId = await _context.Courses.Where(x => x.Id == firstTopicCourseId).Select(x => x.User.Id).FirstOrDefaultAsync();

            //kursa veidotājs var rediģēt tikai savu kursu
            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                if (courseUserId != currentUserId)
                {
                    return NotFound();
                }
            }

            try
            {
                //mainām katrai tēmai kārtību
                foreach (var topicId in topicsInOrder)
                {
                    var topic = await _context.Topics.FindAsync(topicId);

                    topic.DisplayOrder = topicsInOrder.IndexOf(topicId);

                    _context.Topics.Update(topic);
                }
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok();
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public IActionResult CreateTopic(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            //iegūt kursu
            var course = _context.Courses.Where(c => c.Id == id).Include(c => c.User).FirstOrDefault();

            if (course == null)
            {
                return NotFound();
            }

            if (User.IsInRole("CourseCreator"))
            {
                //ja kurss nepieder kursa veidotajam - nevar pievienot tēmu
                if (course.User.Id != User.Identity.GetUserId())
                {
                    return NotFound();
                }
            }

            var topic = new Topic()
            {
                CourseId = course.Id,
                Course = course
            };

            return View(topic);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateTopic([Bind("CourseId,Name,Description")] Topic topic)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //pēc noklusējuma izveidotā tēma nav publicēta
                    topic.IsOpened = false;

                    //kārtas numurs tiek piešķirts pēdējais
                    var topics = await _context.Topics.Where(t => t.CourseId == topic.CourseId).ToListAsync();
                    topic.DisplayOrder = topics != null && topics.Any() ? topics.Max(d => d.DisplayOrder) + 1 : 0;
                    
                    _context.Topics.Add(topic);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("TopicsForCreator", new { id = topic.CourseId });
                }
            }
            catch
            {
                throw;
            }

            return View(topic);
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> EditTopic(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics
                .Where(x => x.Id == id)
                .Include(x => x.Course)
                .Include(x => x.Course.User)
                .Include(x=> x.TopicBlocks)
                .FirstOrDefaultAsync();

            if (topic == null)
            {
                return NotFound();
            }

            if (User.IsInRole("CourseCreator"))  //kursa veidotājs var rediģēt tikai savu kursu - kursu tēmas
            {
                var currentUserId = User.Identity.GetUserId();

                if (topic.Course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            ViewBag.IsLastOpenedTopic = false;

            //ja tēma ir pēdējā atvērta tēma
            if (topic.Course.IsOpened && topic.IsOpened)
            {
                var openedTopicsCount = await _context.Topics.Where(x => x.CourseId == topic.Course.Id && x.IsOpened).CountAsync();
                
                if (openedTopicsCount == 1)  //ir vienīga
                {
                    topic.IsLastOpenedTopic = true;
                }
            }

            return View(topic);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> EditTopic(int id, [Bind("Id,CourseId,Name,Description,IsOpened,DisplayOrder,IsLastOpenedTopic")] Topic topic)
        {
            if (id != topic.Id)
            {
                return NotFound();
            }

            var course = await _context.Courses.Where(x => x.Id == topic.CourseId).Include(x => x.User).FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            if (User.IsInRole("CourseCreator"))  //kursa veidotājs var rediģēt tikai savu kursu - kursu tēmas
            {
                var currentUserId = User.Identity.GetUserId();

                if (course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (topic.IsLastOpenedTopic)
                    {
                        course.IsOpened = false;
                        _context.Update(course);
                    }

                    _context.Update(topic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TopicExists(topic.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("TopicsForCreator", new { id = topic.CourseId });
            }
            return View(topic);
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics.FindAsync(id);

            if (topic == null) //tēma neeksistē
            {
                return NotFound();
            }

            if (topic.IsOpened)
            {
                return NotFound();
            }

            var courseId = topic.CourseId;

            await DeleteTopic(id.Value, _context);

            return RedirectToAction(nameof(TopicsForCreator), new { id = courseId });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public static async System.Threading.Tasks.Task DeleteTopic(int id, ApplicationDbContext context)
        {
            try
            {
                var topic = await context.Topics.FindAsync(id);

                var topicBlocks = await context.TopicBlocks.Where(x => x.TopicId == topic.Id).ToListAsync();

                foreach (var block in topicBlocks)
                {
                    var taskId = await context.Tasks.Where(x => x.TopicBlockId == block.Id).Select(x => x.Id).FirstOrDefaultAsync();

                    await TasksController.DeleteTask(taskId, context);
                }

                context.Topics.Remove(topic);
                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool TopicExists(int id)
        {
            return _context.Topics.Any(e => e.Id == id);
        }
    }
}
