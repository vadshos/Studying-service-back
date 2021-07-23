using DAL.Entities;

namespace DTO
{
    public class AuthenticateResponseDto
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string JwtToken { get; set; }

        public string RefreshToken { get; set; }

        public string Role { get; set; }

        public AuthenticateResponseDto(ApplicationUser user,string role, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            UserName = user.UserName;
            Role = role;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        } 
    }
}