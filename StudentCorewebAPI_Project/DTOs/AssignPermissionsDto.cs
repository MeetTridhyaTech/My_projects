namespace StudentCorewebAPI_Project.DTOs
{
    public class AssignPermissionsDto
    {
        public Guid RoleId { get; set; }
        public Guid MenuId { get; set; }
        public List<Guid> PermissionIds { get; set; }
    }
}
