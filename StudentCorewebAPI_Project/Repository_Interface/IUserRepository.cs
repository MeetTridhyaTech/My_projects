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
        Task<PagedResponse<User>> GetAllUserAsync(PaginationParams paginationParams); //Pagination Dynamically
        Task<ApiResponse<User>> GetByIdAsync(Guid id);
        Task<ApiResponse<UserResponseDto>> AddUserAsync(AddUserDto userDto);
        Task<ApiResponse<User>> UpdateAsync(Guid id, UpdateUserDto updateStudent);
        // Marge Add And Update API
        Task<ApiResponse<UserResponseDto>> AddOrUpdateUserAsync(Guid? id, AddUserDto userDto);
        Task<ApiResponse<string>> DeleteAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<ApiResponse<string>> RegisterUserAsync(RegisterRequest registerRequest);

        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequest loginRequest);
    }
}
