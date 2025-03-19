using Microsoft.AspNetCore.Mvc;
using StudentCorewebAPI_Project.Services;
using StudentCorewebAPI_Project.Models;
using Microsoft.AspNetCore.Authorization;
using StudentCorewebAPI_Project.Data;
using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.CommonApiResponse;
using BCrypt.Net;
using AutoMapper;
using StudentCorewebAPI_Project.Repository;
using StudentCorewebAPI_Project.Repository_Interface;

namespace StudentCorewebAPI_Project.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IUserRepository _userRepository;

        public AuthController(JwtService jwtService, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        // Register API
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var response = await _userRepository.RegisterUserAsync(registerRequest);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        // Login API

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _userRepository.LoginAsync(loginRequest);
            return response.Success ? Ok(response) : Unauthorized(response);
        }
    }
}

