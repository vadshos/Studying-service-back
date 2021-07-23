using System.Linq;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Queries
{
    public interface IUserQuery
    {
        UserCourse GetUserCourseByUserIdAndCourseId(string userId, int courseId);

        IQueryable<ApplicationUser> GetUserWithSubscribtions();

        RefreshToken GetRefreshTokenByToken(string token);

        RefreshToken GetRefreshTokenFromUserByToken(ApplicationUser user, string token);

        Task Delete(ApplicationUser user);

        Task<ApplicationUser> FindById(string id);
        
        string GetIdByUserName(string userName);
    }
}