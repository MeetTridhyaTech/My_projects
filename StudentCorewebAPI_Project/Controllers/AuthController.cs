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
using Microsoft.AspNetCore.Cors;
using StudentCorewebAPI_Project.Enums;
using StudentCorewebAPI_Project.DTOs;
using System.Security.Claims;

namespace StudentCorewebAPI_Project.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public AuthController(JwtService jwtService, IUserRepository userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // Register API
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var response = await _userRepository.RegisterUserAsync(registerRequest);

            if (!response.Success)
            {
                return BadRequest(response);
            }
            var placeholders = new Dictionary<string, string>
            {
                 { "{{Email}}", registerRequest.Email },
                 { "{{Year}}", DateTime.Now.Year.ToString() }
            };

            var body = EmailTemplateGenerator.GetEmailBody(EmailTemplateType.RegisterUser, placeholders);
            var subject = "Registration Successful!";
            await _emailService.SendEmailAsync(registerRequest.Email, subject, body);

            return Ok(response);
        }


        // Login API

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _userRepository.LoginAsync(loginRequest);
            return response.Success ? Ok(response) : Unauthorized(response);
        }

        [HttpPost("referesh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if(principal == null)
                 return BadRequest(new { message = "Invalid access token or referesh token" });

            var email = principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userRepository.GetUserByEmailAsync(email);
            var userDto = await _userRepository.GetUserWithRoleByEmailAsync(email);
            if (userDto == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid refresh token" });

            var newAccessToken = _jwtService.GenerateToken(userDto.Id, userDto.Email, userDto.RoleName);

            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateRefreshTokenAsync(user); // ✅ correct

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var result = await _userRepository.GenerateAndSaveOTPAsync(request.Email);
            if (!result)
                return BadRequest(new { message = "Invalid Email or failed to send OTP" });

            return Ok(new { message = "OTP has been sent to your email." });
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpOnlyRequestDto dto)
        {
            var isValid = await _userRepository.VerifyOtpOnlyAsync(dto.OTP);

            if (!isValid)
                return BadRequest(new { message = "Invalid or expired OTP." });

            return Ok(new { message = "OTP verified successfully." });
        }


        [HttpPost("reset-password/{email}")]
        public async Task<IActionResult> ResetPassword(string email, [FromBody] ResetPasswordRequestDto request)
        {
            var result = await _userRepository.ResetPasswordAsync(email, request);
            if (!result)
                return BadRequest(new { message = "Failed to reset password. Check email or confirm password." });

            return Ok(new { message = "Password has been reset successfully." });
        }



    }
}

