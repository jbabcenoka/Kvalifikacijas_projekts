using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Topics/1
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

            var openedTopics = await _context.Topics.Where(t => t.CourseId == id && t.IsOpened)
                                        .OrderByDescending(t => t.DisplayOrder).ToListAsync();

            var vm = new TopicsVM
            {
                CourseName = course.Name,
                CourseId = course.Id,
                OpenedTopics = openedTopics
            };

            return View("Topics", vm);
        }

        [Authorize]
        public async Task<IActionResult> TopicsForCreator(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //iegūt kursu ar tēmām
            var course = await _context.Courses.FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            var topics = await _context.Topics.Where(c => c.CourseId == id).ToListAsync();

            var vm = new TopicsVM
            {
                CourseName = course.Name,
                CourseId = course.Id,
                Topics = topics
            };

            return View("TopicsForCreator", vm);
        }

        // GET: Topics/CreateTopic/1
        [Authorize]
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

            //ja kurss nepieder kursa veidotajam - nevar pievienot tēmu
            if (course.User.Id != User.Identity.GetUserId())
            {
                return NotFound();
            }

            var topic = new Topic()
            {
                CourseId = course.Id,
                Course = course
            };

            return View(topic);
        }

        // POST: Topics/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateTopic([Bind("CourseId,Name,Description")] Topic topic)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    topic.IsOpened = false;
                    topic.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == User.Identity.GetUserId());

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

        // GET: Topics/Edit/5
        [Authorize]
        public async Task<IActionResult> EditTopic(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics.FindAsync(id);
            if (topic == null)
            {
                return NotFound();
            }
            return View(topic);
        }

        // POST: Topics/EditTopic/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTopic(int id, [Bind("Id,CourseId,Name,Description,IsOpened")] Topic topic)
        {
            if (id != topic.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                     
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

        // GET: Topics/Delete/5
        public async Task<IActionResult> DeleteTopic(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topic == null)
            {
                return NotFound();
            }

            return View(topic);
        }

        // POST: Topics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TopicExists(int id)
        {
            return _context.Topics.Any(e => e.Id == id);
        }
    }
}
