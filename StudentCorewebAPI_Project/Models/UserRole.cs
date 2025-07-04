namespace StudentCorewebAPI_Project.Models
{
    public class UserRole
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleID { get; set; }

        public User? User { get; set; }
        public Role? Role { get; set; }

    }
}
