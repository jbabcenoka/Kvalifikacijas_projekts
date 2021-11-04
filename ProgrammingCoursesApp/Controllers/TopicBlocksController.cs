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
    public class TopicBlocksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TopicBlocksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TopicBlocks
        public async Task<IActionResult> Index(int? id)
        {
            var topicBlocks = await _context.TopicBlocks.Where(t => t.TopicId == id).ToListAsync();
            return View(topicBlocks);
        }

        // GET: TopicBlocks/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: TopicBlocks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TopicBlocks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: TopicBlocks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topicBlock = await _context.TopicBlocks.FindAsync(id);
            if (topicBlock == null)
            {
                return NotFound();
            }
            return View(topicBlock);
        }

        // POST: TopicBlocks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DisplayOrder,Points")] TopicBlock topicBlock)
        {
            if (id != topicBlock.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(topicBlock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TopicBlockExists(topicBlock.Id))
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
            return View(topicBlock);
        }

        // GET: TopicBlocks/Delete/5
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

        // POST: TopicBlocks/Delete/5
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
