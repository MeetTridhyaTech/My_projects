namespace StudentCorewebAPI_Project.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public long? Mobile { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public RoleDto Role { get; set; }

    }
    public class RoleDto
    {
        public Guid? Id { get; set; }
        public string RoleName { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();

    }
}
