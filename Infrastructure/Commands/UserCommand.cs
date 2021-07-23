using System;
using System.Linq;
using DAL.Entities;

namespace DAL.Commands
{
    public class UserCommand : IUserCommand
    {
        ApplicationDbContext context;

        public UserCommand(ApplicationDbContext context)
        {
            this.context = context;
        }

        public void UnSubscribe(string userId, int courseId)
        {
            var course = context.UserCourses.SingleOrDefault(uc => uc.CourseId == courseId && uc.StudentId == userId);
            context.UserCourses.Remove(course);
        }

        public void Subscribe(string userId, int courseId, DateTime startStudyDate)
        {
            var userCourse = new UserCourse
            {
                StartStudyDate = startStudyDate,
                CourseId = courseId,
                StudentId = userId
            };

            context.UserCourses.Add(userCourse);
            context.SaveChanges();
        }
    }
}