using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;

namespace StudentCorewebAPI_Project.Repository_Interface
{
    public interface IMenuRepository
    {
        //Task<bool> AssignPermissionToMenu(AssignPermissionToMenuDto dto);
        Task<Menu> CreateMenu(MenuDto menuDto);
        //Task<bool> IsPermissionAlreadyAssigned(AssignPermissionToMenuDto dto);
        Task<List<MenuDto>> GetAllMenusAsync();
        Task<bool> AssignRolesToMenuAsync(AssignRolesToMenuDto dto);

        // IMenuRepository.cs
        Task<List<MenuDto>> GetMenusByRoleIdAsync(Guid roleId);
        Task<bool> SoftDeleteMenu(Guid menuId);
        //Task<List<MenuDto>> GetMenusByRoleNameAsync(string roleName);
        Task<bool> UpdateMenuAsync(Guid id, MenuDto menuDto);



    }
}
