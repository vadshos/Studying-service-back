﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Exceptions;
using BLL.Helpers;
using DTO;
using DAL;
using DAL.Entities;
using DAL.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BLL.Services
{
    public class AutheticateService : IAutheticateService
    {
         private readonly IMailService mailService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private readonly AppSettings appSettings;
        private readonly IMapper mapper;
        private readonly IJwtUtils jwtUtils;
        private readonly IUserQuery userQuery;
        private readonly ApplicationDbContext context;
        
        public AutheticateService(UserManager<ApplicationUser> userManager,
                                     RoleManager<IdentityRole> roleManager,
                                     IConfiguration configuration,
                                     IMailService mailService,
                                      IJwtUtils jwtUtils,
                                     IOptions<AppSettings> appSettings,
                                     ApplicationDbContext context,
                                     IMapper mapper,
                                     IUserQuery userQuery)
        {
            this.mapper = mapper;
            this.mailService = mailService;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.jwtUtils = jwtUtils;
            this.appSettings = appSettings.Value;
            this.configuration = configuration;
            this.userQuery = userQuery;
            this.context = context;
        }

        public async Task<AuthenticateResponseDto> RefreshToken(string token, string ipAddress)
        {
            var refreshToken = userQuery.GetRefreshTokenByToken(token);
            var user = refreshToken.User;

            if (refreshToken.IsRevoked)
            {
                RevokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                context.Update(user);
                await context.SaveChangesAsync();
            }

            if (!refreshToken.IsActive)
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.InternalServerError, "Invalid token");
            }

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = RotateRefreshToken(refreshToken,user,ipAddress);
   

            // remove old refresh tokens from user
            RemoveOldRefreshTokens(user);
            user.RefreshTokens.Add(newRefreshToken);
            // save changes to db
            context.Update(user);
            await context.SaveChangesAsync();
            string role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            // generate new jwt
            var jwtToken = jwtUtils.GenerateAccessToken(user,role);

            return new AuthenticateResponseDto(user,role, jwtToken, newRefreshToken.Token);
        }

        public async Task<AuthenticateResponseDto> Login(AutheticateDto model,string ipAddress)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.NotFound, "Cannot find user with this email");
            }

            if (!await userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.Unauthorized, "Invalid password");
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.Forbidden, "You don't confirm  you account please check your email");
            }

            string role = (await userManager.GetRolesAsync(user)).FirstOrDefault();

            string accessToken =  jwtUtils.GenerateAccessToken(user,role);
            var refreshToken = jwtUtils.GenerateRefreshToken(configuration, user,ipAddress);
            RemoveOldRefreshTokens(user);
            user.RefreshTokens.Add(new RefreshToken { Token = refreshToken.Token,Created = DateTime.UtcNow,Expires = DateTime.UtcNow.AddDays(7.0)});
            context.Update(user);
            await context.SaveChangesAsync();

            return new AuthenticateResponseDto(user,role, accessToken,refreshToken.Token);
        }

        public async Task<ResponseDto> Register(RegisterDto model)
        {
            var user = mapper.Map<ApplicationUser>(model);

            var validator = new UserValidator();

            var resultV = await validator.ValidateAsync(user);

            if (!resultV.IsValid)
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.Forbidden,resultV.Errors.FirstOrDefault()?.ToString());
            }

            var userExists = await userManager.FindByEmailAsync(model.Email);

            if (userExists != null)
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.Conflict, "User with this email is already exist");
            }

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.InternalServerError, "User creation failed! Please check user details and try again");
            }

            if (!await roleManager.RoleExistsAsync(UserRoles.User))
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            }

            if (await roleManager.RoleExistsAsync(UserRoles.User))
            {
                await userManager.AddToRoleAsync(user, UserRoles.User);
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            string link = $"https://localhost:3000/confirmation/{user.Id}/{token}";
            var request = new WelcomeMailDto { ToEmail = user.Email, UserName = user.UserName, Link = link };
            await SendEmail(request);

            return new ResponseDto { Status = "Success", Message = "User was successfully created" };
        }

        private async Task SendEmail(WelcomeMailDto request)
        {
            try
            {
                await mailService.SendWelcomeEmailAsync(request);
            }
            catch (Exception)
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.InternalServerError, "Can't send email");
            }
        }

        public async Task VerifyEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
             
            if (user == null)
            {
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.NotFound, "Undefined user");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            { 
                throw new HttpStatusCodeException(System.Net.HttpStatusCode.InternalServerError, "Failed verification");
            }
        }

        private RefreshToken RotateRefreshToken(RefreshToken refreshToken,ApplicationUser user,string ipAddress)
        {
            var newRefreshToken = jwtUtils.GenerateRefreshToken(configuration,user,ipAddress);
            RevokeRefreshToken(refreshToken,ipAddress,"Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, ApplicationUser user, string ipAddress, string reason)
        {
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrWhiteSpace(refreshToken.ReplacedByToken)) 
            {
                var childToken = userQuery.GetRefreshTokenFromUserByToken(user, refreshToken.ReplacedByToken);
                if (childToken.IsActive)
                {
                    RevokeRefreshToken(childToken, ipAddress, reason);
                }
                else
                {
                    RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
                }
            }
        }

        private static void RevokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }

        private void RemoveOldRefreshTokens(ApplicationUser user)
        {
            // remove old inactive refresh tokens from user based on TTL in app settings
            user.RefreshTokens.RemoveAll(x =>
                !x.IsActive &&
                x.Created.AddDays(appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        }
    }
}