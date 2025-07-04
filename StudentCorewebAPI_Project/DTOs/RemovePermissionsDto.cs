    namespace StudentCorewebAPI_Project.DTOs
{
    public class RemovePermissionsDto
    {
        public Guid RoleId { get; set; }
        public Guid MenuId { get; set; }
        public List<Guid> PermissionIds { get; set; }
    }
}
