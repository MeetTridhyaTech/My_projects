using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.Repository_Interface;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleID == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

public async Task AssignPermissionsToRoleAsync(Guid roleId, Guid menuId, List<Guid> permissionIds)
{
    foreach (var permissionId in permissionIds)
    {
        bool exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleID == roleId && rp.MenuId == menuId && rp.PermissionId == permissionId);

        if (!exists)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleID = roleId,
                MenuId = menuId,
                PermissionId = permissionId
            });
        }
    }

    await _context.SaveChangesAsync();
}


    //public async Task AssignPermissionsToMenuForRoleAsync(Guid roleId, Guid menuId, List<Guid> permissionIds)
    //{
    //    // Remove existing permissions for this role & menu
    //    var existing = await _context.RoleMenuPermissions
    //        .Where(rmp => rmp.RoleId == roleId && rmp.MenuId == menuId)
    //        .ToListAsync();

    //    _context.RoleMenuPermissions.RemoveRange(existing);

    //    // Add new ones
    //    foreach (var permissionId in permissionIds)
    //    {
    //        _context.RoleMenuPermissions.Add(new RoleMenuPermission
    //        {
    //            Id = Guid.NewGuid(),
    //            RoleId = roleId,
    //            MenuId = menuId,
    //            //PermissionId = permissionId
    //        });
    //    }

    //    await _context.SaveChangesAsync();
    //}
    //public async Task<List<Guid>> GetPermissionsByRoleAndMenuAsync(Guid roleId, Guid menuId)
    //{
    //    return await _context.RoleMenuPermissions
    //        .Where(rmp => rmp.RoleId == roleId && rmp.MenuId == menuId)
    //        .Select(rmp => rmp.PermissionId)
    //        .ToListAsync();
    //}

    public async Task<IEnumerable<RoleMenuDto>> GetRolesWithPermissionsAsync()
    {
        var data = await (
            from rp in _context.RolePermissions
            join r in _context.Roles on rp.RoleID equals r.RoleID
            join p in _context.Permissions on rp.PermissionId equals p.Id
            join m in _context.Menus on rp.MenuId equals m.Id
            group new { r, m, p } by r.RoleName into roleGroup
            select new RoleMenuDto
            {
                RoleName = roleGroup.Key,
                Menus = roleGroup
                    .GroupBy(x => x.m.Title)
                    .Select(menuGroup => new MenuWithPermissionsDto
                    {
                        Title = menuGroup.Key,
                        Permissions = menuGroup
                            .Select(x => x.p.Name)
                            .Distinct()
                            .ToList()
                    })
                    .ToList()
            }
        ).ToListAsync();

        return data;
    }


    public async Task<Permission> AddPermissionAsync(Permission permission)
    {
        permission.Id = Guid.NewGuid(); // Generate a new GUID
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission;
    }


    //public async Task<bool> HasPermissionAsync(Guid userId,/* Guid menuId,*/ string permissionName)
    //{
    //    var roleIds = await _context.UserRoles
    //        .Where(ur => ur.UserId == userId)
    //        .Select(ur => ur.RoleID)
    //        .ToListAsync();

    //    return await _context.RolePermissions
    //        .AnyAsync(rp => roleIds.Contains(rp.RoleID) && /* rp.MenuId== menuId && */   rp.Permission.Name == permissionName);
    //}

 public async Task<bool> HasPermissionAsync(Guid userId, Guid menuId, string permissionName)
{
    var roleIds = await _context.UserRoles
        .Where(ur => ur.UserId == userId)
        .Select(ur => ur.RoleID)
        .ToListAsync();

    return await _context.RolePermissions
        .AnyAsync(rp => 
            roleIds.Contains(rp.RoleID) &&
            rp.MenuId == menuId &&
            rp.Permission.Name == permissionName);
}



    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions.ToListAsync();
    }

    public async Task UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds)
    {
        // Remove old role permissions
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleID == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingPermissions);

        // Add new role permissions
        var newPermissions = permissionIds.Select(pid => new RolePermission
        {
            RoleID = roleId,
            PermissionId = pid
        });

        await _context.RolePermissions.AddRangeAsync(newPermissions);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        var mapping = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleID == roleId && rp.PermissionId == permissionId);

        if (mapping == null)
            return false;

        _context.RolePermissions.Remove(mapping);
        await _context.SaveChangesAsync();

        return true;
    }
    //public async Task<bool> RemovePermissionsFromRoleBulkAsync(Guid roleId, List<Guid> permissionIds)
    //{
    //    var mappings = await _context.RolePermissions
    //        .Where(rp => rp.RoleID == roleId && permissionIds.Contains(rp.PermissionId))
    //        .ToListAsync();

    //    if (!mappings.Any())
    //        return false;

    //    _context.RolePermissions.RemoveRange(mappings);
    //    await _context.SaveChangesAsync();

    //    return true;
    //}

    public async Task<bool> RemovePermissionsFromRoleBulkAsync(Guid roleId, Guid menuId, List<Guid> permissionIds)
    {
        var mappings = await _context.RolePermissions
            .Where(rmp => rmp.RoleID == roleId &&
                          rmp.MenuId == menuId &&
                          permissionIds.Contains(rmp.PermissionId))
            .ToListAsync();

        if (!mappings.Any())
            return false;

        _context.RolePermissions.RemoveRange(mappings);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> DeletePermissionAsync(Guid permissionId)
    {
        var permission = await _context.Permissions.FindAsync(permissionId);

        if (permission == null)
            return false;

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();
        return true;
    }



    public async Task<List<Guid>> GetPermissionIdsByRoleIdAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleID == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();
    }


}
