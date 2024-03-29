﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProgrammingCoursesApp.Models;

namespace ProgrammingCoursesApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<TopicBlock> TopicBlocks { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<VideoTask> VideoTasks { get; set; }
        public DbSet<ReadTask> ReadTask { get; set; }
        public DbSet<PossibleAnswer> PossibleAnswers { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Task>()
                .Property("Discriminator")
                .HasMaxLength(9);
        }
    }
}
