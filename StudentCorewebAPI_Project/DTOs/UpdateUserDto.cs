namespace StudentCorewebAPI_Project.DTOs
{
    public class UpdateUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public long? Mobile { get; set; } 
    }
}
