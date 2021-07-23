using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.Helpers;
using BLL.Services;
using AutoMapper;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        private readonly IMapper mapper;

        public AccountController(IAccountService accountService,
                                                 IMapper mapper)
        {
            this.mapper = mapper;
            this.accountService = accountService;
        }
        

        [HttpGet]
        public  ActionResult<IEnumerable<UserDto>> GetPagination([FromQuery]  StudentParameters studentParameters)
        {
            var accounts =   accountService.GetPagination(studentParameters);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(accounts.MetadataPaginationDto));

            return Ok(accounts.Collection);
        }

        [HttpPost]
        [Route("subscribe")]
        public async Task<IActionResult> Subscribe(SubscribeDto request)
        {
            var claims = ClaimsPrincipal.Current.Identities.First().Claims.ToList();

            string Name =  claims?.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.OrdinalIgnoreCase))?.Value;

            accountService.Subscribe(Name, request.CourseId, request.StartStudyDate);
            return Ok();
        }

        [HttpPost]
        [Route("unsubscribe")]
        public async Task<IActionResult> UnSubscribe(SubscribeDto request)
        {
            var claims = ClaimsPrincipal.Current.Identities.First().Claims.ToList();

            string Name = claims?.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.OrdinalIgnoreCase))?.Value;

            accountService.UnSubscribe(Name, request.CourseId);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var account =   accountService.GetById(id);
            return Ok(account);
        }

        [HttpPut("{id}")]
        public async  Task<ActionResult<UserDto>> Update(string id, UpdateDto model)
        {
            var account = await accountService.Update(id, model);
            return Ok(account);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string  id)
        { 
             await accountService.Delete(id);
            return Ok(new { message = "Account deleted successfully" });
        }


    }
}