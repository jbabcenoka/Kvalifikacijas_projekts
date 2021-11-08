using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProgrammingCoursesApp.Controllers;
using ProgrammingCoursesApp.Data;
using ProgrammingCoursesApp.Models;

namespace ProgrammingCoursesApp
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tasks
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
            var topicBlocks = await _context.TopicBlocks.Include(t => t.Topic)
                        .Where(t => t.Topic.Id == id).OrderByDescending(t => t.DisplayOrder)
                        .Include(t => t.Task).ToListAsync();

            ViewBag.TopicName = topic.Name;
            ViewBag.TopicId = topic.Id;
            ViewBag.CourseId = topic.CourseId;

            return View(topicBlocks);
        }

        //GET: TasksForCoursesCreator
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
            var topicBlocks = await _context.TopicBlocks.Include(t => t.Topic)
                        .Where(t => t.Topic.Id == id).OrderByDescending(t => t.DisplayOrder)
                        .Include(t => t.Task).ToListAsync();

            ViewBag.TopicName = topic.Name;
            ViewBag.TopicId = topic.Id;
            ViewBag.CourseId = topic.CourseId;

            return View("TasksForCoursesCreator", topicBlocks);
        }

        //GET: 
        [HttpGet]
        public async Task<JsonResult> GetPossibleAnswers(int? id)
        {
            var answers = await _context.PossibleAnswers.Where(t => t.ExerciseId == id).ToListAsync();
            
            return Json(answers);
        }

        // GET: Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DisplayOrder,Points")] TopicBlock topicBlock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(topicBlock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(topicBlock);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.Include(t => t.TopicBlock).Include(x => x.TopicBlock.Topic).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            if (task.GetType() == typeof(ReadTask))
            {
                var readTask = (ReadTask)task;
                return View("ReadTaskEdit", readTask);
            }

            return View(task);
        }

        
        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReadTask(int id, [Bind("Id,TopicBlockId,Name,Text")] ReadTask readTask)
        {
            if (id != readTask.Id)
            {
                return NotFound();
            }

            var topicBlock = await _context.TopicBlocks.Include(t => t.Topic).FirstOrDefaultAsync(t => t.Id == readTask.TopicBlockId);

            var topicId = topicBlock.Topic.Id;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Tasks.Update(readTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(TasksForCoursesCreator), new { id = topicId });
            }
            return View(readTask);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topicBlock = await _context.TopicBlocks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topicBlock == null)
            {
                return NotFound();
            }

            return View(topicBlock);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
