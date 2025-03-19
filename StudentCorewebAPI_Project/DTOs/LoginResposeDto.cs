public class LoginResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public long? Mobile { get; set; }
    public Guid RoleID { get; set; }
    public string RoleName { get; set; }
    public string Token { get; set; }
}
