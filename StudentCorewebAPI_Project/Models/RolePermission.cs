namespace StudentCorewebAPI_Project.Models
{
    public class RolePermission
    {
        public Guid Id { get; set; }
        public Guid RoleID { get; set; }
        public Guid MenuId { get; set; }
        public Guid PermissionId { get; set; }

        public Role Role { get; set; }
        public Permission Permission { get; set; }          

    }
}
