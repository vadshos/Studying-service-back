using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Exceptions;
using BLL.Helpers;
using DTO;
using Hangfire;
using DAL.Commands;
using DAL.Entities;
using DAL.Queries;


namespace BLL.Services
{
    public class AccountService : IAccountService
    {
        private readonly IMailService mailService;
        private readonly IUserQuery query;
        private readonly IUserCommand command;
        private readonly IMapper mapper;

        public AccountService(IUserQuery userQuery,
            IMailService mailService,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.mailService = mailService;
            query = userQuery;
        }

        public async Task<ResponseDto> Delete(string id)
        {
            var user = await query.FindById(id);
            if (user == null)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "User with this id didn't found");
            }

            await query.Delete(user);

            return new ResponseDto {Status = "Ok", Message = "User was successful delete"};
        }

        public PaginationDto<UserDto> GetPagination(StudentParameters studentParameters)
        {
            var users = query.GetUserWithSubscribtions();
            if (!string.IsNullOrWhiteSpace(studentParameters.SeachText))
            {
                users = users.Where(u => u.UserName.ToLower().Contains(studentParameters.SeachText.ToLower()) ||
                                         u.Email.ToLower().Contains(studentParameters.SeachText.ToLower()));
            }

            users = ApplySortService.ApplySort(users, studentParameters.OrderBy).AsQueryable();

            var accounts = PagedList<ApplicationUser>.ToPagedList((users),
                studentParameters.PageNumber,
                studentParameters.PageSize);

            var metadata = new MetadataPaginationDto
            {
                TotalCount = accounts.TotalCount,
                PageSize = accounts.PageSize,
                CurrentPage = accounts.CurrentPage,
                TotalPages = accounts.TotalPages,
            };

            return new PaginationDto<UserDto> {Collection = mapper.Map<List<UserDto>>(accounts),MetadataPaginationDto = metadata};
        }

        public void Subscribe(string userName, int courseId, DateTime startStudyDate)
        {
            var userId = query.GetIdByUserName(userName);
            var daysToStart = (startStudyDate - DateTime.UtcNow).Days;
            var userCourse = query.GetUserCourseByUserIdAndCourseId(userId, courseId);

            CreateBodyForMailAboutStartStudy(userCourse, daysToStart);

            command.Subscribe(userId, courseId, startStudyDate);
        }

        public void CreateBodyForMailAboutStartStudy(UserCourse userCourse, int daysToStart)
        {
            const int thirtyDaysToStartStudy = 30;
            const int sevenDaysToStartStudy = 7;
            const int startStudyToday = 0;


            var request = new StartStudyMailDto
            {
                Name = userCourse.Student.FirstName,
                ToEmail = userCourse.Student.Email,
                CourseName = userCourse.Course.Name
            };

            int[] daysToStartStudy = {thirtyDaysToStartStudy, sevenDaysToStartStudy, startStudyToday};

            foreach (var item in daysToStartStudy)
            {
                if (item == startStudyToday)
                {
                    request.DayToStart = "today";
                }
                else
                {
                    request.DayToStart = $"{item} days";
                }

                var jobId = AddJobSendEmailAboutStartStudy(request, daysToStart - item);
                userCourse.HangfireJobs.Add(new HangfireJob {JobId = jobId, UserCourse = userCourse}); //other method
            }
        }

        public string AddJobSendEmailAboutStartStudy(StartStudyMailDto request, int days)
        {
            var jobId = BackgroundJob.Schedule(
                () => mailService.SendInfoAboutStartStudyEmailAsync(request),
                TimeSpan.FromDays(days));
            return jobId;
        }

        public void UnSubscribe(string userName, int courseId)
        {
            var userId = query.GetIdByUserName(userName);
            var userCourse = query.GetUserCourseByUserIdAndCourseId(userId, courseId);
            var jobsId = userCourse.HangfireJobs.Select(uc => uc.JobId);

            foreach (var jobId in jobsId)
            {
                BackgroundJob.Delete(jobId);
            }

            command.UnSubscribe(userId, courseId);
        }

        public async Task<UserDto> GetById(string id)
        {
            var user = await query.FindById(id);
            return mapper.Map<UserDto>(user);
        }

        public Task<UserDto> Update(string id, UpdateDto model)
        {
            throw new NotImplementedException();
        }

    }
}