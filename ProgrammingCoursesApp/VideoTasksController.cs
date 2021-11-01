using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProgrammingCoursesApp.Data;
using ProgrammingCoursesApp.Models;

namespace ProgrammingCoursesApp
{
    public class VideoTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VideoTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VideoTasks
        public async Task<IActionResult> Index()
        {
            return View(await _context.VideoTasks.ToListAsync());
        }

        // GET: VideoTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var videoTask = await _context.VideoTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (videoTask == null)
            {
                return NotFound();
            }

            return View(videoTask);
        }

        // GET: VideoTasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VideoTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Link,Id,Name")] VideoTask videoTask)
        {
            if (ModelState.IsValid)
            {
                _context.Add(videoTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(videoTask);
        }

        // GET: VideoTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var videoTask = await _context.VideoTasks.FindAsync(id);
            if (videoTask == null)
            {
                return NotFound();
            }
            return View(videoTask);
        }

        // POST: VideoTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Link,Id,Name")] VideoTask videoTask)
        {
            if (id != videoTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(videoTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VideoTaskExists(videoTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(videoTask);
        }

        // GET: VideoTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var videoTask = await _context.VideoTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (videoTask == null)
            {
                return NotFound();
            }

            return View(videoTask);
        }

        // POST: VideoTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var videoTask = await _context.VideoTasks.FindAsync(id);
            _context.VideoTasks.Remove(videoTask);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VideoTaskExists(int id)
        {
            return _context.VideoTasks.Any(e => e.Id == id);
        }
    }
}
