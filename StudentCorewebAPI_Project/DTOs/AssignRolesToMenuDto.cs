namespace StudentCorewebAPI_Project.DTOs
{
    public class AssignRolesToMenuDto
    {
        public Guid MenuId { get; set; }
        public List<Guid> RoleIds { get; set; }
    }
}
