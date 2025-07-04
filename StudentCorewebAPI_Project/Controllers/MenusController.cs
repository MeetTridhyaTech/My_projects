using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.Repository;
using StudentCorewebAPI_Project.Repository_Interface;

namespace StudentCorewebAPI_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class MenusController : ControllerBase
    {
        private readonly IMenuRepository _menuRepository;

        public MenusController(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }
        [HasPermission("Read")]
        [HttpGet("all/{menuId}")]
        public async Task<IActionResult> GetAllMenus()
        {
            var menus = await _menuRepository.GetAllMenusAsync();
            return Ok(menus);
        }
        [HttpGet("GetMenusByRole/{roleId}")]
        public async Task<IActionResult> GetMenusByRole(Guid roleId)
        {
            var menus = await _menuRepository.GetMenusByRoleIdAsync(roleId);
            if (menus == null || !menus.Any())
                return NotFound("No menus assigned to this role.");

            return Ok(menus);
        }

        //[HasPermission("AddUser")]
        //[HttpPost("AssignPermissionToMenu")]
        //public async Task<IActionResult> AssignPermissionToMenu([FromBody] AssignPermissionToMenuDto dto)
        //{
        //    var result = await _menuRepository.AssignPermissionToMenu(dto);
        //    if (!result)
        //    {
        //        return BadRequest(new { message = "This permission is already assigned to this menu for the selected role." });
        //    }

        //    return Ok(new { message = "Permission successfully assigned to the menu." });
        //}
        [HasPermission("Add")]
        [HttpPost("create/{menuId}")]
        public async Task<IActionResult> CreateMenu([FromBody] MenuDto menuDto)
        {
            if (menuDto == null || string.IsNullOrEmpty(menuDto.Title))
                return BadRequest("Invalid menu data.");

            var createdMenu = await _menuRepository.CreateMenu(menuDto);
            return Ok(createdMenu);
        }


        [HttpPost("assign-role-to-menu")]
        public async Task<IActionResult> AssignRolesToMenu([FromBody] AssignRolesToMenuDto dto)
        {
            var result = await _menuRepository.AssignRolesToMenuAsync(dto);
            if (!result)
                return BadRequest("Menu not found or assignment failed.");

            return Ok("Roles assigned to menu successfully.");
        }

        [HasPermission("Edit")]
        [HttpPut("update/{id}/{menuId}")]
        public async Task<IActionResult> UpdateMenu (Guid id, [FromBody] MenuDto menuDto)
        {
            if (menuDto == null || string.IsNullOrEmpty(menuDto.Title))
                return BadRequest("Invalid menu data.");

            var result = await _menuRepository.UpdateMenuAsync(id, menuDto);
            if (!result)
                return NotFound("Menu not found or already deleted.");

            return Ok(new { message = "Menu updated successfully." });
        }

        [HasPermission("Delete")]
        [HttpDelete("{id}/{menuId}")]
        public async Task<IActionResult> SoftDeleteMenu(Guid id)
        {
            var result = await _menuRepository.SoftDeleteMenu(id);
            if (!result)
                return NotFound("Menu not found or already deleted.");

            return Ok(new { message = "Menu soft deleted successfully." });
        }
    }
}
