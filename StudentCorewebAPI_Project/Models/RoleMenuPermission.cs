namespace StudentCorewebAPI_Project.Models
{
    public class RoleMenuPermission
    {
        public Guid Id { get; set; }

        public Guid RoleId { get; set; }
        public Role Role { get; set; }

        public Guid MenuId { get; set; }
        public Menu Menu { get; set; }

        //public Guid PermissionId { get; set; }
        //public Permission Permission { get; set; }
    }
}
