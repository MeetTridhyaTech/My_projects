namespace StudentCorewebAPI_Project.DTOs
{
    public class AddUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public long? Mobile { get; set; }
        public string Password { get; set; }
        public Guid RoleID { get; set; }


    }
}
