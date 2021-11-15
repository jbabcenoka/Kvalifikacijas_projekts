﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProgrammingCoursesApp.Controllers;
using ProgrammingCoursesApp.Data;
using ProgrammingCoursesApp.Models;
using ProgrammingCoursesApp.ViewModels;

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

            var vm = new TasksVM
            {
                TopicName = topic.Name,
                TopicId = topic.Id,
                CourseId = topic.CourseId,
                TopicBlocks = topicBlocks
            };

            return View(vm);
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
            var topicBlocks = await _context.TopicBlocks
                                .Include(t => t.Topic)
                                .Where(t => t.Topic.Id == id)
                                .OrderBy(t => t.DisplayOrder)
                                .Include(t => t.Task)
                                .ToListAsync();
            
            var vm = new TasksVM
            {
                TopicName = topic.Name,
                TopicId = topic.Id,
                CourseId = topic.CourseId,
                TopicBlocks = topicBlocks
            };

            return View("TasksForCoursesCreator", vm);
        }

        //GET: 
        [HttpGet]
        public async Task<JsonResult> GetPossibleAnswers(int? id)
        {
            var answers = await _context.PossibleAnswers
                            .Where(t => t.ExerciseId == id)
                            .ToListAsync();
            
            return Json(answers);
        }

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

            return View("ExerciseTaskEdit", vm);
        }
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

        [HttpPost, ValidateAntiForgeryToken]
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

        [HttpPost, ValidateAntiForgeryToken]
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

                var possibleAnswers = await _context.PossibleAnswers
                                        .Where(t => t.ExerciseId == task.Id)
                                        .ToListAsync();

                exerciseTask.PossibleAnswers = possibleAnswers;
                vm.Exercise = exerciseTask;
                return View("ExerciseTaskEdit", vm);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
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

        [HttpPost, ValidateAntiForgeryToken]
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
