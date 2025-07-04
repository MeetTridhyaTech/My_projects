//namespace StudentCorewebAPI_Project.DTOs
//{
//    public class UpdateRolePermissionDto
//    {
//        public Guid RoleId { get; set; }
//        public Guid OldPermissionId { get; set; }
//        public Guid NewPermissionId { get; set; }
//    }

//}


namespace StudentCorewebAPI_Project.DTOs
{
    public class UpdateRolePermissionDto
    {
        public Guid RoleId { get; set; }
        public List<Guid> PermissionIds { get; set; }  // Replaces OldPermissionId and NewPermissionId
    }
}
