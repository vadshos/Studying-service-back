using DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<CourseModel> Courses { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }

        public DbSet<HangfireJob> HangfireJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CourseModel>().Property(u => u.Name).HasColumnType("nvarchar(70)");

            base.OnModelCreating(builder);

            builder.Entity<UserCourse>()
                .HasMany(u => u.HangfireJobs)
                .WithOne(j => j.UserCourse)
                .HasForeignKey(j => j.UserCouseId);

            builder.Entity<UserCourse>()
                .HasOne(uc => uc.Student)
                .WithMany(s => s.UserCourses)
                .HasForeignKey(uc => uc.StudentId);
            builder.Entity<UserCourse>()
                .HasOne(uc => uc.Course)
                .WithMany(c => c.UserCourses)
                .HasForeignKey(bc => bc.CourseId);
        }
    }
}