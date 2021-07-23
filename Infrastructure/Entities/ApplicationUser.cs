using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;


namespace DAL.Entities
{
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public List<UserCourse> UserCourses { get; set; }

        public DateTime RegisteredDate { get; set; }

        [JsonIgnore]
        public virtual List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
     }
}
