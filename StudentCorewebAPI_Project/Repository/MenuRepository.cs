using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.Repository_Interface;

namespace StudentCorewebAPI_Project.Repository
{
    public class MenuRepository : IMenuRepository
    {
        private readonly ApplicationDbContext _context;

        public MenuRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<MenuDto>> GetMenusByRoleIdAsync(Guid roleId)
        {
            var menuDtos = await _context.UserPermissions
                .Include(up => up.Menu)
                .Where(up => up.RoleID == roleId && up.Menu.IsActive) // active only
                .Select(up => new MenuDto
                {
                    //Id = up.Menu.Id,
                    Title = up.Menu.Title,
                    Path = up.Menu.Path,
                    Icon = up.Menu.Icon,
                    Order = up.Menu.Order,
                    ParentMenuId = up.Menu.ParentMenuId,
                    RoleName = up.Menu.RoleName
                })
                .Distinct()
                .ToListAsync();

            return menuDtos;
        }

        public async Task<bool> AssignRolesToMenuAsync(AssignRolesToMenuDto dto)
        {
            // Check if menu exists
            var menuExists = await _context.Menus.AnyAsync(m => m.Id == dto.MenuId && m.IsActive);
            if (!menuExists) return false;

            // Remove existing assignments to avoid duplicates
            var existingAssignments = _context.RoleMenuPermissions
                .Where(rmp => rmp.MenuId == dto.MenuId);

            _context.RoleMenuPermissions.RemoveRange(existingAssignments);

            // Add new assignments
            foreach (var roleId in dto.RoleIds)
            {
                var assignment = new RoleMenuPermission
                {
                    Id = Guid.NewGuid(),
                    MenuId = dto.MenuId,
                    RoleId = roleId
                };
                await _context.RoleMenuPermissions.AddAsync(assignment);
            }

            await _context.SaveChangesAsync();
            return true;
        }



        public async Task<Menu> CreateMenu(MenuDto menuDto)
        {
            var menu = new Menu
            {
                Id = Guid.NewGuid(),
                Title = menuDto.Title,
                Icon = menuDto.Icon,
                Path = menuDto.Path,
                Order = menuDto.Order,
                ParentMenuId = menuDto.ParentMenuId,
                IsActive = true,
                RoleName = menuDto.RoleName,
                //SubMenus = new List<Menu>()
            };

            //if (menuDto.SubMenus != null && menuDto.SubMenus.Any())
            //{
            //    foreach (var sub in menuDto.SubMenus)
            //    {
            //        menu.SubMenus.Add(new Menu
            //        {
            //            Id = Guid.NewGuid(),
            //            Title = sub.Title,
            //            Icon = sub.Icon,
            //            Path = sub.Path,
            //            Order = sub.Order,
            //            ParentMenuId = menu.Id
            //        });
            //    }
            //}

            await _context.Menus.AddAsync(menu);
            await _context.SaveChangesAsync();

            return menu;
        }
        public async Task<bool> SoftDeleteMenu(Guid menuId)
        {
            try
            {
                var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == menuId && m.IsActive);
                if (menu == null)
                    return false;

                menu.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SoftDeleteMenu: {ex.Message}");
                return false;
            }
        }
        public async Task<List<MenuDto>> GetAllMenusAsync()
        {
            var activeMenus = await _context.Menus
                .Where(m => m.IsActive)
                .ToListAsync();

            if (activeMenus == null || !activeMenus.Any())
            {
                Console.WriteLine("No active menus found.");
            }

            var result = activeMenus.Select(menu => new MenuDto
            {
                Id = menu.Id,
                Title = menu.Title,
                Icon = menu.Icon,
                Path = menu.Path,
                Order = menu.Order,
                ParentMenuId = menu.ParentMenuId,
                RoleName = menu.RoleName
            }).ToList();

            return result;
        }

        public async Task<bool> UpdateMenuAsync(Guid id, MenuDto menuDto)
        {
            var existingMenu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            if (existingMenu == null)
                return false;

            existingMenu.Title = menuDto.Title;
            existingMenu.Icon = menuDto.Icon;
            existingMenu.Path = menuDto.Path;
            existingMenu.Order = menuDto.Order;
            existingMenu.ParentMenuId = menuDto.ParentMenuId;
            existingMenu.RoleName = menuDto.RoleName;

            await _context.SaveChangesAsync();
            return true;
        }



    }
}
