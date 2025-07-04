using Microsoft.AspNetCore.Mvc;
using StudentCorewebAPI_Project.Models;
using Microsoft.AspNetCore.Authorization;
using StudentCorewebAPI_Project.Repository_Interface;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using StudentCorewebAPI_Project.DTOs;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleRepository roleRepository, ILogger<RolesController> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    [HasPermission("Read")]
    [HttpGet("{menuId}")]
    public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
    {
        _logger.LogInformation("Getting all Roles:");
        var roles = await _roleRepository.GetAllRolesAsync();
        return Ok(roles);
    }

    [HasPermission("Read")]
    [HttpGet("{id}/{menuId}")]
    public async Task<ActionResult<Role>> GetRoleById(Guid id)
    {
        _logger.LogInformation("Getting Role by RoleId :{RoleId}", id);
        var role = await _roleRepository.GetRoleByIdAsync(id);
        if (role == null)
        {
            _logger.LogInformation("Role not Found in Id:{RoleId}", id);
            return NotFound();
        }
        return Ok(role);
    }
    [HasPermission("Add")]
    [HttpPost("{menuId}")]
    public async Task<ActionResult<Role>> AddRole(Role role)
    {
        _logger.LogInformation("Adding new Role: {RoleName}", role.RoleName);
        var createdRole = await _roleRepository.AddRoleAsync(role);
        return Ok(createdRole);
        //return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.RoleID }, createdRole);
    }

    [HasPermission("Edit")]
    [HttpPut("{id}/{menuId}")]
    public async Task<ActionResult<Role>> UpdateRole(Guid id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        if (id == Guid.Empty || updateRoleDto == null || string.IsNullOrEmpty(updateRoleDto.RoleName))
        {
            _logger.LogWarning("Invalid input for Role update: Url Id:{UrlId}, RoleName:{RoleName}", id, updateRoleDto?.RoleName);
            return BadRequest("Invalid Role ID or Role Name");
        }

        // Fetch the role from the repository using the provided id
        var existingRole = await _roleRepository.GetRoleByIdAsync(id);

        if (existingRole == null)
        {
            _logger.LogWarning("Role with Id {RoleId} not found for update", id);
            return NotFound();
        }

        _logger.LogInformation("Updating Role {RoleId}", id);

        var updatedRole = await _roleRepository.UpdateRoleAsync(id, updateRoleDto);

        return Ok(updatedRole);
    }

    [HasPermission("Add")]
    [HttpPost("assign/{menuId}")]
    public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto dto)
    {
        await _roleRepository.AssignRolesAsync(dto.UserId, dto.RoleIds);
        return Ok(new { message = "Roles assigned successfully." });
    }
    [HasPermission("Delete")]
    [HttpDelete("remove/{menuId}")]
    public async Task<IActionResult> RemoveRoles([FromBody] RemoveRolesDto dto)
    {
        await _roleRepository.RemoveRolesAsync(dto.UserId, dto.RoleIds);
        return Ok(new { message = "Roles removed successfully." });
    }

    [HasPermission("Delete")]
    [HttpDelete("{id}/{menuId}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        _logger.LogInformation("Deleting Role with Id{RoleId}", id);
        var success = await _roleRepository.DeleteRoleAsync(id);
        if (!success)
        {
            _logger.LogWarning("Role with Role id {RoleId} not found for deletion", id);
            return NotFound();
        }
        _logger.LogInformation("Role with Id: {RoleId} deleted successfully", id);
        return NoContent();
    }
}
