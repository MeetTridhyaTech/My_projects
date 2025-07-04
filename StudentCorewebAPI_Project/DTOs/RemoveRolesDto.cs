namespace StudentCorewebAPI_Project.DTOs
{
    public class RemoveRolesDto
    {
        public Guid UserId { get; set; }
        public List<Guid> RoleIds { get; set; }
    }
}
