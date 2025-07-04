using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;

namespace StudentCorewebAPI_Project.Repository_Interface
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId);
        Task AssignPermissionsToRoleAsync(Guid roleId,Guid menuId, List<Guid> permissionIds);
        //Task AssignPermissionsToMenuForRoleAsync(Guid roleId, Guid menuId, List<Guid> permissionIds);
        //Task<List<Guid>> GetPermissionsByRoleAndMenuAsync(Guid roleId, Guid menuId);
        Task<Permission> AddPermissionAsync(Permission permission);
        //Task<List<Permission>> GetPermissionsByRoleAndMenuAsync(Guid roleId, Guid menuId);

        //Task RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds);
        //Task<bool> HasPermissionAsync(Guid userId, string permissionName);

        Task<bool> HasPermissionAsync(Guid userId, Guid menuId, string permissionName);
        Task<List<Guid>> GetPermissionIdsByRoleIdAsync(Guid roleId);
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        //Task<bool> UpdateRolePermissionAsync(Guid roleId, Guid oldPermissionId, Guid newPermissionId);
        Task UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds);
        Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
        Task<IEnumerable<RoleMenuDto>> GetRolesWithPermissionsAsync();
        //Task<bool> RemovePermissionsFromRoleBulkAsync(Guid roleId, List<Guid> permissionIds);
        Task<bool> RemovePermissionsFromRoleBulkAsync(Guid roleId,Guid menuId, List<Guid> permissionIds);


        Task<bool> DeletePermissionAsync(Guid permissionId);



    }
}
