using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgrammingCoursesApp.Data;
using ProgrammingCoursesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Courses.Where(c => c.IsOpened).ToListAsync());
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> UserCourses()
        {
            var userCourses = new List<Course>();

            if (User.IsInRole("CourseCreator"))
            {
                var currentUserId = User.Identity.GetUserId();

                userCourses = await _context.Courses.Where(c => c.User.Id == currentUserId).ToListAsync();
            }
            else //Admin
            {
                userCourses = await _context.Courses.Include(c => c.User).ToListAsync();
            }

            return View(userCourses);
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public IActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> CreateCourse([Bind("Id,Name,Description")] Course course)
        {
            try 
            {
                if (ModelState.IsValid)
                {
                    course.IsOpened = false;
                    course.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == User.Identity.GetUserId());
                    _context.Add(course);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(UserCourses));
                }
            }
            catch
            {
                throw;
            }
            
            return View(course);
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> EditCourse(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Where(c => c.Id == id)
                .Include(u => u.User)
                .Include(x => x.Topics)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }
            
            if (User.IsInRole("CourseCreator"))  //kursa veidotājs var rediģēt tikai savu kursu
            {
                var currentUserId = User.Identity.GetUserId();

                if (course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            return View(course);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> EditCourse(int id, [Bind("Id,Name,Description,IsOpened")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            //ja kursu mēģina atvērt un kurss nesatur tēmas - kļūda.
            var courseTopics = await _context.Topics.Where(x => x.CourseId == course.Id).ToListAsync();

            if (course.IsOpened && (courseTopics == null || !courseTopics.Any()))
            {
                return NotFound();
            }

            if (User.IsInRole("CourseCreator"))  //kursa veidotājs var rediģēt tikai savu kursu
            {
                var currentUserId = User.Identity.GetUserId();
                var courseCreator = await _context.Courses
                    .Where(x => x.Id == course.Id)
                    .Select(x => x.User.Id)
                    .FirstOrDefaultAsync();

                if (courseCreator != currentUserId)
                {
                    return NotFound();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(UserCourses));
            }
            return View(course);
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.Where(x => x.Id == id).Include(x => x.User).FirstOrDefaultAsync();
            if (course == null)
            {
                return NotFound();
            }

            if (User.IsInRole("CourseCreator"))  //kursa veidotājs var dzēst tikai savu kursu
            {
                var currentUserId = User.Identity.GetUserId();

                if (course.User.Id != currentUserId)
                {
                    return NotFound();
                }
            }

            if (course.IsOpened)
            {
                return NotFound();
            }

            await DeleteCourse(id.Value, _context);

            return RedirectToAction(nameof(UserCourses));
        }

        [HttpPost, Authorize(Roles = "Admin, CourseCreator")]
        [ValidateAntiForgeryToken]
        public static async System.Threading.Tasks.Task DeleteCourse(int id, ApplicationDbContext context)
        {
            try
            {
                var course = await context.Courses.FindAsync(id);

                //dzēst kursa tēmas
                var topics = await context.Topics.Where(x => x.CourseId == course.Id).ToListAsync();
                
                foreach (var topic in topics)
                {
                    await TopicsController.DeleteTopic(topic.Id, context);
                }

                context.Courses.Remove(course);
                await context.SaveChangesAsync();
            }
            catch(Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "Admin, CourseCreator")]
        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
