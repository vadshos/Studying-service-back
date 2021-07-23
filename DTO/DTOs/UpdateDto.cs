using System.ComponentModel.DataAnnotations;
using DAL.Entities;

namespace DTO
{
    public class UpdateDto
    {
        private string password;
        private string role;
        private string email;

        public int Age { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }


        [EnumDataType(typeof(UserRoles))]
        public string Role
        {
            get => role;
            set => role = replaceEmptyWithNull(value);
        }

        [EmailAddress]
        public string Email
        {
            get => email;
            set => email = replaceEmptyWithNull(value);
        }

        [MinLength(6)]
        public string Password
        {
            get => password;
            set => password = replaceEmptyWithNull(value);
        }

        private string replaceEmptyWithNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}