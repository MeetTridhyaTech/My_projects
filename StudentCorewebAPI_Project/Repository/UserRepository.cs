using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.CommonApiResponse;
using StudentCorewebAPI_Project.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BCrypt.Net;
using Azure;
using StudentCorewebAPI_Project.Repository_Interface;
using StudentCorewebAPI_Project.Services;
using StudentCorewebAPI_Project.Enums;
using System.Data;

namespace StudentCorewebAPI_Project.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<User> _genericRepository;
        private readonly JwtService _jwtService;

        public UserRepository(ApplicationDbContext context, IMapper mapper, IGenericRepository<User> genericRepository, JwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _genericRepository = genericRepository;
            _jwtService = jwtService;
        }
        public async Task<PagedResponse<User>> GetAllUserAsync(PaginationParams paginationParams)
        {
            if (paginationParams.PageNumber < 1) paginationParams.PageNumber = 1;
            if (paginationParams.PageSize < 1 || paginationParams.PageSize > 100) paginationParams.PageSize = 10;

            var query = _context.Users.Where(user => !user.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(paginationParams.SearchQuery))
            {
                query = query.Where(user => user.FirstName.Contains(paginationParams.SearchQuery) ||
                                            user.LastName.Contains(paginationParams.SearchQuery) ||
                                            user.Email.Contains(paginationParams.SearchQuery));
            }

            query = paginationParams.SortBy switch
            {
                "FirstName" => paginationParams.IsDescending ? query.OrderByDescending(user => user.FirstName) : query.OrderBy(user => user.FirstName),
                "LastName" => paginationParams.IsDescending ? query.OrderByDescending(user => user.LastName) : query.OrderBy(user => user.LastName),
                "Email" => paginationParams.IsDescending ? query.OrderByDescending(user => user.Email) : query.OrderBy(user => user.Email),
                _ => query.OrderBy(user => user.FirstName)
            };

            var totalRecords = await query.CountAsync();
            var users = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResponse<User>(users, paginationParams.PageNumber, paginationParams.PageSize, totalRecords);
        }
        public async Task<ApiResponse<User>> GetByIdAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id && !user.IsDeleted);
            return user != null
                ? ApiResponse<User>.SuccessResponse(user, ApiMessages.UserRetriveSuccessfully)
                : ApiResponse<User>.FailureResponse(ApiMessages.UserNotFound);
        }
        public async Task<ApiResponse<UserResponseDto>> AddUserAsync(AddUserDto userDto)
        {
            if (string.IsNullOrEmpty(userDto.Password))
            {
                return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.PasswordRequired);
            }
            // Hash password before saving
            string passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            // Create User entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Mobile = userDto.Mobile,
                PasswordHash = hashedPassword,
                PasswordSalt = passwordSalt,
                IsDeleted = false
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            // Fetch Role based on provided RoleID (or assign a default role)
            Role role;
            if (userDto.RoleID!= null)
            {
                // If RoleID is provided, fetch that role
                role = await _context.Roles
                    .Where(r => r.RoleID == userDto.RoleID)
                    .FirstOrDefaultAsync();
                if (role == null)
                {
                    return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.RoleNotFound);
                }
            }
            else
            {
                // If RoleID is not provided, assign the default role ("Manager")
                string defaultRoleName = UserRoleEnum.Manager.ToString();

                role = await _context.Roles
                    .Where(r => r.RoleName == defaultRoleName)
                    .FirstOrDefaultAsync();

                if (role == null)
                {
                    return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.DefaultRoleNotFound);
                }
            }
            // Assign Role to User in UserRoles Table
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleID = role.RoleID
            };
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
            // Create response DTO with role details
            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Mobile = user.Mobile,
                PasswordHash=user.PasswordHash,
                PasswordSalt=user.PasswordSalt,
                Role = new RoleDto
                {
                    Id = role.RoleID,
                    RoleName = role.RoleName
                }
            };

            return ApiResponse<UserResponseDto>.SuccessResponse(userResponse, ApiMessages.UserAddedSuccessfully);
        }
        public async Task<ApiResponse<User>> UpdateAsync(Guid id, UpdateUserDto updateUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id && !user.IsDeleted);
            if (user == null) return ApiResponse<User>.FailureResponse(ApiMessages.UserNotFound);

            user.FirstName = updateUser.FirstName;
            user.LastName = updateUser.LastName;
            user.Email = updateUser.Email;
            user.Mobile = updateUser.Mobile;

            await _context.SaveChangesAsync();
            return ApiResponse<User>.SuccessResponse(user, ApiMessages.UserUpdatedSuccessfully);
        }
        public async Task<ApiResponse<UserResponseDto>> AddOrUpdateUserAsync(Guid? id, AddUserDto userDto)
        {
            var isNewUser = id == null || id == Guid.Empty;
            var existingUser = isNewUser ?
                await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email) :
                await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (isNewUser && existingUser != null)
                return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.UserAlreadyExist);

            if (!isNewUser && existingUser == null)
                return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.UserNotFound);

            var user = existingUser ?? new User { Id = Guid.NewGuid(), IsDeleted = false };

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.Mobile = userDto.Mobile;

            // Password handling only for new user
            if (isNewUser)
            {
                if (string.IsNullOrEmpty(userDto.Password))
                    return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.PasswordRequired);

                user.PasswordSalt = BCrypt.Net.BCrypt.GenerateSalt();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

                await _context.Users.AddAsync(user);
            }

            await _context.SaveChangesAsync();

            // Assign Role only for new user
            if (isNewUser)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == userDto.RoleID);
                if (role == null)
                    return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.RoleNotFound);//EnumMessages

                var userRole = new UserRole { Id = Guid.NewGuid(), UserId = user.Id, RoleID = role.RoleID };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }

            var roleName = await _context.Roles
                .Where(r => r.RoleID == adduserDto.RoleID)
                .Select(r => r.RoleName)
                .FirstOrDefaultAsync();

            return ApiResponse<UserResponseDto>.SuccessResponse(
                new UserResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    PasswordHash = user.PasswordHash,
                    PasswordSalt = user.PasswordSalt,
                    Role = roleName != null ? new RoleDto { Id = userDto.RoleID, RoleName = roleName } : null
                },
                isNewUser ? ApiMessages.UserAddedSuccessfully : ApiMessages.UserUpdatedSuccessfully 
            );
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id && !user.IsDeleted);
            if (user == null) return ApiResponse<string>.FailureResponse(ApiMessages.UserNotFound);

            user.IsDeleted = true;
            await _context.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse(null, ApiMessages.UserSoftDeleteSuccessfully);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context
                .Users
                .FirstOrDefaultAsync(user => user.Email == email && !user.IsDeleted);
        }

        public async Task<ApiResponse<string>> RegisterUserAsync(RegisterRequest registerRequest)
        {
            // Check if user already exists
            var existingUser = await _context.Users.AnyAsync(user => user.Email == registerRequest.Email);
            if (existingUser)
            {
                return ApiResponse<string>.FailureResponse(ApiMessages.UserAlreadyExist);
            }

            // Hash password before saving
            string passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
            // Default Role as "Manager"
            var defaultRole = UserRoleEnum.Manager.ToString(); 

            //  Fetch the RoleId of "Manager" from the Role table
            var managerRoleId = await _context.Roles
                .Where(role => role.RoleName == defaultRole)
                .Select(role => role.RoleID)
                .FirstOrDefaultAsync();

            if (managerRoleId == Guid.Empty)
            {
                return ApiResponse<string>.FailureResponse(ApiMessages.DefaultRoleNotFound);
            }

            //  Create a new User
            var newUser = _mapper.Map<User>(registerRequest);
            newUser.PasswordHash = hashedPassword;
            newUser.PasswordSalt = passwordSalt;
            newUser.IsDeleted = false;

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync(); //  Save to get UserId
            //  Assign Role to User in UserRoles Table
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                RoleID = managerRoleId 
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync(); 

            return ApiResponse<string>.SuccessResponse(null, ApiMessages.UserRegisterSuccessfully);
        }
        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == loginRequest.Email && !user.IsDeleted);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponseDto>.FailureResponse(ApiMessages.InvalidCredentials);
            }

            // Fetch RoleID and RoleName in a single query using a JOIN
            var userRoleInfo = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleID, r => r.RoleID, (ur, r) => new
                {
                    RoleID = ur.RoleID,
                    RoleName = r.RoleName
                })
                .FirstOrDefaultAsync();

            // If user has no role assigned, return an error
            if (userRoleInfo == null)
            {
                return ApiResponse<LoginResponseDto>.FailureResponse(ApiMessages.RoleNotFound);
            }

            var token = _jwtService.GenerateToken(user.Id, user.Email,userRoleInfo.RoleName);

            // Map user details to DTO
            var userDto = new LoginResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Mobile = user.Mobile,
                RoleID = userRoleInfo.RoleID, 
                RoleName = userRoleInfo.RoleName, 
                Token = token
            };
            return ApiResponse<LoginResponseDto>.SuccessResponse(userDto, ApiMessages.UserLoginSuccessfully);
        }
    }
}
