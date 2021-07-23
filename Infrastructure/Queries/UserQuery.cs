using System.Linq;
using System.Threading.Tasks;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Queries
{
    public class UserQuery :  IUserQuery
    {
        private readonly ApplicationDbContext context ;
        public UserQuery(ApplicationDbContext context)
        {
            this.context = context;
        }
        
        public IQueryable<ApplicationUser> GetUserWithSubscribtions()
        {
            var users = context.Users.Include(u => u.UserCourses);
            return users;
        }

        public RefreshToken GetRefreshTokenByToken(string token)
        {
            return context.Users.Include(u => u.RefreshTokens)
                .Select(u => u.RefreshTokens.SingleOrDefault(t => t.Token == token))
                .Single();
        }

        public UserCourse GetUserCourseByUserIdAndCourseId(string userId, int courseId)
        {
            return context.UserCourses.Single( uc => uc.CourseId == courseId && uc.StudentId == userId );
        }

        public RefreshToken GetRefreshTokenFromUserByToken(ApplicationUser user,string token)
        {
            return user.RefreshTokens.SingleOrDefault(x => x.Token == token);
        }

        public async Task Delete(ApplicationUser user)
        {
            var res = context.Users.Remove(user);

            await context.SaveChangesAsync();
        }

        public async Task<ApplicationUser> FindById(string id)
        {
            return await context.Users.SingleOrDefaultAsync(u =>u.Id == id);
        }

        public string GetIdByUserName(string userName)
        {
            return context.Users.SingleOrDefault(u => u.UserName == userName)?.Id;
        }
    }
}