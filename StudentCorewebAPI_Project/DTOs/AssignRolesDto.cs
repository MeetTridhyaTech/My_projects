namespace StudentCorewebAPI_Project.DTOs
{
    public class AssignRolesDto
    {
        public Guid UserId { get; set; }
        public List<Guid> RoleIds { get; set; }
    }
}
