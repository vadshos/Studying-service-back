using System.Collections.Generic;
using BLL.Helpers;
using BLL.Services;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {

        private readonly ICourseService courseService;
        public CourseController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetPagination([FromQuery] CourseParameters courseParameters)
        {
            var courses = courseService.GetPagination(courseParameters);



            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(courses.MetadataPaginationDto));

            return Ok(courses.Collection);
        }
    }
}