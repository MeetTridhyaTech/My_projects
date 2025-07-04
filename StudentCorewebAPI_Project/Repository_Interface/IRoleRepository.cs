using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;

namespace StudentCorewebAPI_Project.Repository_Interface
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(Guid id);
        Task<Role> AddRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(Guid roleId,UpdateRoleDto updateRoleDto);
        Task<bool> DeleteRoleAsync(Guid id);
        //Task<Role> CreateRoleIfNotExistsAsync(string roleName);
        Task AssignRolesAsync(Guid userId, List<Guid> roleIds);
        Task RemoveRolesAsync(Guid userId, List<Guid> roleIds);

    }
}
