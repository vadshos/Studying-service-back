using AutoMapper;
using DTO;
using DAL.Entities;

namespace BLL.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<UserDto, ApplicationUser>();

            CreateMap<UserCourseDto, UserCourse>();

            CreateMap<UserCourse, UserCourseDto>();

            CreateMap<ApplicationUser, UserDto>();

            CreateMap<CourseDto, CourseModel>();

            CreateMap<CourseModel, CourseDto>();

            CreateMap<UserCourse, UserCourseDto>()
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.Course.Description))
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.Id,
                           opt => opt.MapFrom(src => src.Id));


            CreateMap<RegisterDto, ApplicationUser>();
        }
    }
}