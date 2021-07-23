using System.Linq;
using DAL.Entities;

namespace DAL.Queries
{
    public class CourseQuery : ICourseQuery
    {
        private readonly ApplicationDbContext  context;

        public CourseQuery(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IQueryable<CourseModel> GetAllCourses()
        {
            return context.Courses;
        }
    }
}