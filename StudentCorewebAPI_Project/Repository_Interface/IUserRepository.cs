using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.CommonApiResponse;
using StudentCorewebAPI_Project.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StudentCorewebAPI_Project.Repository_Interface
{
    public interface IUserRepository
    {
        //Task<ApiResponse<List<User>>> GetAllUserAsync(string searchTerm, string sortBy, string sortOrder, int pageNumber, int pageSize);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<PagedResponse<UserDto>> GetAllUserAsync(PaginationParams paginationParams);
        Task<ApiResponse<User>> GetByIdAsync(Guid id);
        Task<ApiResponse<UserResponseDto>> AddUserAsync(AddUserDto userDto, IEmailService emailService);
        Task<ApiResponse<User>> UpdateAsync(Guid id, UpdateUserDto updateStudent);
        // Marge Add And Update API
        Task<ApiResponse<UserResponseDto>> AddOrUpdateUserAsync(Guid? id, AddUserDto adduserDto, IEmailService emailService);
        Task<ApiResponse<string>> DeleteAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<ApiResponse<string>> RegisterUserAsync(RegisterRequest registerRequest);

        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequest loginRequest);
        Task<ApiResponse<LoginResponseDto>> MimicUserAsync(Guid userId);

        Task<UserDto> GetUserWithRoleByEmailAsync(string email);
        Task UpdateRefreshTokenAsync(User user);
        Task<bool> GenerateAndSaveOTPAsync(string email);
        Task<bool> ResetPasswordAsync(string email,ResetPasswordRequestDto dto);
        Task<bool> VerifyOtpOnlyAsync(string otp);

    }
}
