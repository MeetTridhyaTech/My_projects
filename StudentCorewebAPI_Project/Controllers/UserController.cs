using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.Repository;
using StudentCorewebAPI_Project.CommonApiResponse;
using StudentCorewebAPI_Project.Repository_Interface;
using System;
using System.Threading.Tasks;
using StudentCorewebAPI_Project.Enums;
//using StudentCorewebAPI_Project.Utilities;

namespace StudentCorewebAPI_Project.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;
        //private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
            //_mapper = mapper;
        }
        //[Authorize(Roles="Admin,Manager")]
        //[HasPermission("Read")]
        //[HttpPost("search/{menuId}")]
        //public async Task<IActionResult> GetAllUsers([FromQuery] PaginationParams paginationParams)
        //{

        //    _logger.LogInformation("Fetching users with Query: {Query}, SortBy: {SortBy}, SortOrder: {SortOrder}, PageNumber: {PageNumber}, PageSize: {PageSize}",
        //        paginationParams.SearchQuery, paginationParams.SortBy, paginationParams.IsDescending, paginationParams.PageNumber, paginationParams.PageSize);

        //    var response = await _userRepository.GetAllUserAsync(paginationParams);

        //    if (response.Data.Any())
        //    {
        //        _logger.LogInformation(ApiMessages.UserRegisterSuccessfully);
        //        return Ok(response);
        //    }

        //    _logger.LogWarning("No users found for query: {Query}", paginationParams.SearchQuery);
        //    return NotFound(response);
        //}
        [HasPermission("Read")]
        [HttpPost("search/{menuId}")]
        public async Task<IActionResult> GetAllUsers([FromBody] PaginationParams paginationParams)
        {
            _logger.LogInformation("Fetching users with Query: {Query}, SortBy: {SortBy}, SortOrder: {SortOrder}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                paginationParams.SearchQuery, paginationParams.SortBy, paginationParams.IsDescending, paginationParams.PageNumber, paginationParams.PageSize);

            var response = await _userRepository.GetAllUserAsync(paginationParams);

            if (response.Data.Any())
            {
                _logger.LogInformation(ApiMessages.UserRegisterSuccessfully);
                return Ok(response);
            }

            _logger.LogWarning("No users found for query: {Query}", paginationParams.SearchQuery);
            return NotFound(response);
        }

        [HasPermission("Mimic")]
        //[Authorize(Roles = "Admin")]
        [HttpGet("mimic/{id:Guid}/{menuId}")]
        public async Task<IActionResult> MimicUser([FromRoute] Guid id)
        {
            _logger.LogInformation("Received mimic request for user ID: {UserId}", id);

            var response = await _userRepository.MimicUserAsync(id);

            if (!response.Success)
            {
                _logger.LogWarning("Failed to mimic user with ID: {UserId}. Reason: {Message}", id, response.Message);
                return NotFound(response);
            }

            _logger.LogInformation("Successfully mimicked user with ID: {UserId}", id);
            return Ok(response);
        }


        [HasPermission("Add")]
        [HttpPost("{menuId}")]
        public async Task<ActionResult> AddUser([FromBody] AddUserDto addUser, [FromServices] IEmailService emailService)
        {
            var response = await _userRepository.AddUserAsync(addUser, emailService);

            if (response.Success)
            {
                _logger.LogInformation(ApiMessages.UserAddedSuccessfully , "with ID: {UserId}", response.Data?.Id);
                return Ok(response);
            }

            _logger.LogError(ApiMessages.FailToAddUser);
            return BadRequest(response);
        }

        //Add and update marge Api
        [HasPermission("Edit")]
        [HttpPost("AddOrUpdate/{menuId}")]
        public async Task<IActionResult> AddOrUpdateUser([FromQuery] Guid? id, [FromBody] AddUserDto adduserDto, [FromServices] IEmailService emailService)
        {
            _logger.LogInformation("Received Request To AddOrUpdate User. User Id: {UserId}", id ?? Guid.Empty);

            var response = await _userRepository.AddOrUpdateUserAsync(id, adduserDto,emailService);

            if (!response.Success)
            {
                _logger.LogWarning("Failed To AddOrUpdate User. Error: {ErrorMessage}", response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("{Message} User Id: {UserId}", response.Message, response.Data?.Id);
            return Ok(response);
        }

        //[Authorize(Roles = "Manager")]
        [HasPermission("Read")]
        [HttpGet("{id:Guid}/{menuId}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser([FromRoute] Guid id)
        {
            _logger.LogInformation(ApiMessages.UserRetriveSuccessfully," ID: {UserId}", id);

            var response = await _userRepository.GetByIdAsync(id);
            if (response.Success)
            {
                _logger.LogInformation(ApiMessages.UserRetriveSuccessfully);
                return Ok(response);
            }

            _logger.LogWarning("User with ID {UserId} not found.", id);
            return NotFound(response);
        }

        //[Authorize(Roles="Admin")]
        [HasPermission("Edit")]
        [HttpPut("{menuId}")]
        public async Task<ActionResult> UpdateUser([FromBody] UpdateUserDto updateUser)
        {
            if (updateUser == null || updateUser.Id == Guid.Empty)
            {
                _logger.LogWarning("Invalid user ID provided in the request body.");
                return BadRequest(new ApiResponse<string>(false, "User ID is required in the request body.", null));
            }

            _logger.LogInformation(ApiMessages.UserUpdatedSuccessfully,"with ID: {UserId}", updateUser.Id);

            var response = await _userRepository.UpdateAsync(updateUser.Id, updateUser);
            if (response.Success)
            {
                _logger.LogInformation(ApiMessages.UserUpdatedSuccessfully);
                return Ok(response);
            }

            _logger.LogWarning("Failed to update user with ID: {UserId}", updateUser.Id);
            return NotFound(response);
        }
        // SOFT DELETE USER
        //[Authorize(Roles="Admin")]
        [HasPermission("Delete")]
        [HttpDelete("{id:Guid}/{menuId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            _logger.LogInformation(ApiMessages.UserSoftDeleteSuccessfully,"with ID: {UserId}", id);
            var response = await _userRepository.DeleteAsync(id);
            if (response.Success)
            {
                _logger.LogInformation(ApiMessages.UserSoftDeleteSuccessfully);
                return Ok(response);
            }
            _logger.LogWarning("Failed to delete user with ID: {UserId}", id);
            return NotFound(response);
        }
    }
}