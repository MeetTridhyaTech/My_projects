using Microsoft.AspNetCore.Mvc;
using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.Repository_Interface;

namespace StudentCorewebAPI_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionsController(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }
        //[HasPermission("AddUser")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsDto dto)
        {
            await _permissionRepository.AssignPermissionsToRoleAsync(dto.RoleId, dto.MenuId, dto.PermissionIds);
            return Ok("Permissions assigned.");
        }


        [HasPermission("Add")]
        [HttpPost("Add/{menuId}")]
        public async Task<IActionResult> AddPermission([FromBody] Permission newPermission)
        {
            if (string.IsNullOrWhiteSpace(newPermission.Name))
            {
                return BadRequest("Permission name is required.");
            }

            var createdPermission = await _permissionRepository.AddPermissionAsync(newPermission);
            return Ok(createdPermission);
            //return CreatedAtAction(nameof(GetAllPermissions), new { id = createdPermission.Id }, createdPermission);
        }
        //[HasPermission("AssignMenuPermission")]
        //[HttpPost("assign-to-menu")]
        //public async Task<IActionResult> AssignPermissionsToMenu([FromBody] AssignMenuPermissionsDto dto)
        //{
        //    await _permissionRepository.AssignPermissionsToMenuForRoleAsync(dto.RoleId, dto.MenuId, dto.PermissionIds);
        //    return Ok("Permissions assigned to menu.");
        //}
        //[HttpGet("get-role-menu-permissions")]
        //public async Task<IActionResult> GetRoleMenuPermissions(Guid roleId, Guid menuId)
        //{
        //    var permissions = await _permissionRepository.GetPermissionsByRoleAndMenuAsync(roleId, menuId);
        //    return Ok(permissions);
        //}


        [HasPermission("Read")]
        [HttpGet("permission-ids-by-role/{roleId}")]
        public async Task<IActionResult> GetPermissionIdsByRoleId(Guid roleId)
        {
            var permissionIds = await _permissionRepository.GetPermissionIdsByRoleIdAsync(roleId);

            if (permissionIds == null || !permissionIds.Any())

            {
                return NotFound(new { message = "No permissions found for this role." });
            }

            return Ok(permissionIds);
        }

        //  GET all permissions
        [HasPermission("Read")]
        [HttpGet("All/{menuId}")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _permissionRepository.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        [HasPermission("Read")]
        [HttpGet("roles-with-permissions/{menuId}")]    
        public async Task<IActionResult> GetRolesWithPermissions()
        {
            var result = await _permissionRepository.GetRolesWithPermissionsAsync();

            if (result == null || !result.Any())
            {
                return NotFound("No role-menu-permission data found.");
            }

            return Ok(result);
        }
        [HasPermission("Edit")]
        [HttpPut("UpdateRolePermissions")]
        public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdateRolePermissionDto dto)
        {
            await _permissionRepository.UpdateRolePermissionsAsync(dto.RoleId, dto.PermissionIds);
            return Ok(new { message = "Permissions updated successfully." });
        }


        [HasPermission("Delete")]
        [HttpDelete("remove/{menuId}")]
        public async Task<IActionResult> RemovePermissionFromRole([FromQuery] Guid roleId, [FromQuery] Guid permissionId)
        {
            var success = await _permissionRepository.RemovePermissionFromRoleAsync(roleId, permissionId);

            if (!success)
                return NotFound(new { message = "Permission mapping not found or already removed." });

            return Ok(new { message = "Permission removed from role successfully." });
        }
        //[HasPermission("Delete")]
        //[HttpPost("remove-bulk")]
        //public async Task<IActionResult> RemovePermissionsFromRoleBulk([FromBody] RemovePermissionsDto dto)
        //{
        //    var result = await _permissionRepository.RemovePermissionsFromRoleBulkAsync(dto.RoleId, dto.PermissionIds);

        //    if (!result)
        //        return NotFound(new { message = "One or more permission mappings were not found." });

        //    return Ok(new { message = "Permissions removed from role successfully." });
        //}

        [HttpPost("remove-bulk/{menuId}")]
        [HasPermission("Delete")]
        public async Task<IActionResult> RemovePermissionsFromRoleBulk([FromBody] RemovePermissionsDto dto)
        {
            var result = await _permissionRepository.RemovePermissionsFromRoleBulkAsync(dto.RoleId, dto.MenuId, dto.PermissionIds);

            if (!result)
                return NotFound(new { message = "No matching permissions found for the specified role and menu." });

            return Ok(new { message = "Permissions removed successfully." });
        }

        [HasPermission("Delete")]
        [HttpDelete("{id}/{menuId}")]
        public async Task<IActionResult> DeletePermission(Guid id)
        {
            var deleted = await _permissionRepository.DeletePermissionAsync(id);

            if (!deleted)
                return NotFound(new { message = "Permission not found or already deleted." });

            return Ok(new { message = "Permission deleted successfully." });
        }




    }
}
