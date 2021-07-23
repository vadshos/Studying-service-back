using System.Threading.Tasks;
using BLL.Helpers;
using DTO;

namespace BLL.Services
{
    public interface ICourseService
    {
        PaginationDto<CourseDto> GetPagination(CourseParameters courseParameters);
    }
}