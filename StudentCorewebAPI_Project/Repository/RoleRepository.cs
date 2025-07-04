using System;
using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.Repository_Interface;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role> GetRoleByIdAsync(Guid id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Role> AddRoleAsync(Role role)
    {
        role.RoleID = Guid.NewGuid();
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateRoleAsync(Guid roleId, UpdateRoleDto updateRoleDto)
    {
        var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == roleId);

        if (existingRole == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        // Update the RoleName with the value from the DTO
        existingRole.RoleName = updateRoleDto.RoleName;

        await _context.SaveChangesAsync();
        return existingRole;
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task AssignRolesAsync(Guid userId, List<Guid> roleIds)
    {
        foreach (var roleId in roleIds)
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleID == roleId);

            if (!exists)
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleID = roleId
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveRolesAsync(Guid userId, List<Guid> roleIds)
    {
        var rolesToRemove = await _context.UserRoles
            .Where(ur => ur.UserId == userId && roleIds.Contains(ur.RoleID))
            .ToListAsync();

        _context.UserRoles.RemoveRange(rolesToRemove);
        await _context.SaveChangesAsync();
    }

}
