using System;

namespace StudentCorewebAPI_Project.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }   
        public long? Mobile { get; set; }
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }

    }
}
