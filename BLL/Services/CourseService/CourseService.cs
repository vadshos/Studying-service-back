using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Helpers;
using DTO;
using DAL.Entities;
using DAL.Queries;

namespace BLL.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseQuery query;
        private readonly IMapper mapper;

        public CourseService(ICourseQuery query,IMapper mapper)
        {
            this.mapper = mapper;
            this.query = query;
        }

        public  PaginationDto<CourseDto> GetPagination(CourseParameters courseParameters)
        {
            var courses = query.GetAllCourses()
                .Where(u => u.Name.ToLower().Contains(courseParameters.SeachText.ToLower()));

            courses = ApplySortService.ApplySort(courses, courseParameters.OrderBy).AsQueryable();


            var course = PagedList<CourseModel>.ToPagedList(courses,
                courseParameters.PageNumber,
                courseParameters.PageSize);
            
            var metadata = new MetadataPaginationDto
            {
                TotalCount = course.TotalCount,
                PageSize = course.PageSize,
                CurrentPage = course.CurrentPage,
                TotalPages = course.TotalPages,
            };

            var response = mapper.Map<List<CourseDto>>(course.ToList());
            return new PaginationDto<CourseDto>{Collection = response,MetadataPaginationDto = metadata};
        }

    }
}