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
        [Authorize(Roles="Admin,Manager")]
        [HttpGet("search")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationParams paginationParams)
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

        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult> AddUser([FromBody] AddUserDto addUser)
        {
            var response = await _userRepository.AddUserAsync(addUser);

            if (response.Success)
            {
                _logger.LogInformation(ApiMessages.UserAddedSuccessfully , "with ID: {UserId}", response.Data?.Id);
                return CreatedAtAction(nameof(GetUser), new { id = response.Data?.Id }, response);
            }

            _logger.LogError(ApiMessages.FailToAddUser);
            return BadRequest(response);
        }
        //Add and update marge Api
        [Authorize (Roles="Admin")]
        [HttpPost("AddOrUpdate")]
        public async Task<IActionResult> AddOrUpdateUser([FromQuery] Guid? id, [FromBody] AddUserDto userDto)
        {
            _logger.LogInformation("Recived Request To AddOrUpdate User. User Id: {User Id}",id ?? Guid.Empty);
            var response = await _userRepository.AddOrUpdateUserAsync(id, userDto);
            if(!response.Success)
            {
                _logger.LogWarning("Failed To AddOrUpdate User. Error: {ErrorMessage}",response.Message);
                return BadRequest(response);
            }
            _logger.LogInformation("{Message} User Id: {User Id}",response.Message,response.Data?.Id);
            return Ok(response);
        }
        [Authorize(Roles ="Admin,Manager")]
        [HttpGet("{id:Guid}")]
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
        [Authorize(Roles="Admin")]
        [HttpPut]
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
        [Authorize(Roles="Admin")]
        [HttpDelete("{id:Guid}")]
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