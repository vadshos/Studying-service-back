using System.Linq;
using DAL.Entities;

namespace DAL.Queries
{
    public interface ICourseQuery
    {
        IQueryable<CourseModel> GetAllCourses();
    }
}