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
        private readonly IEmailService _emailService;


        public UserRepository(ApplicationDbContext context, IMapper mapper, IGenericRepository<User> genericRepository, JwtService jwtService, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _genericRepository = genericRepository;
            _jwtService = jwtService;
            _emailService = emailService;
        }
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }
        public async Task<PagedResponse<UserDto>> GetAllUserAsync(PaginationParams paginationParams)
        {
            if (paginationParams.PageNumber < 1) paginationParams.PageNumber = 1;
            if (paginationParams.PageSize < 1 || paginationParams.PageSize > 100) paginationParams.PageSize = 10;

            var query = from user in _context.Users
                        where !user.IsDeleted
                        join userRole in _context.UserRoles on user.Id equals userRole.UserId
                        join role in _context.Roles on userRole.RoleID equals role.RoleID
                        select new UserDto
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Mobile = user.Mobile,
                            RoleID = role.RoleID,
                            RoleName = role.RoleName
                        };

            // Instead of manual search, filter, sort, and pagination,
            // call your reusable QueryHelper.ApplyPagination:
            return await QueryHelper.ApplyPagination(query, paginationParams);
        }
        public async Task<ApiResponse<User>> GetByIdAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id && !user.IsDeleted);
            return user != null
                ? ApiResponse<User>.SuccessResponse(user, ApiMessages.UserRetriveSuccessfully)
                : ApiResponse<User>.FailureResponse(ApiMessages.UserNotFound);
        }
        public async Task<ApiResponse<UserResponseDto>> AddUserAsync(AddUserDto userDto, IEmailService emailService)
        {
            if (string.IsNullOrEmpty(userDto.Password))
            {
                return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.PasswordRequired);
            }
            // Create User entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Mobile = userDto.Mobile,
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
        public async Task<ApiResponse<UserResponseDto>> AddOrUpdateUserAsync(Guid? id, AddUserDto adduserDto, IEmailService emailService)
        {
            var isNewUser = id == null || id == Guid.Empty;

            var existingUser = isNewUser ?
                await _context.Users.FirstOrDefaultAsync(u => u.Email == adduserDto.Email && !u.IsDeleted) :
                await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (isNewUser && existingUser != null)
                return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.UserAlreadyExist);

            if (!isNewUser && existingUser == null)
                return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.UserNotFound);

            var user = existingUser ?? new User { Id = Guid.NewGuid(), IsDeleted = false };

            user.FirstName = adduserDto.FirstName;
            user.LastName = adduserDto.LastName;
            user.Email = adduserDto.Email;
            user.Mobile = adduserDto.Mobile;

            // Password handling only for new user
            if (isNewUser)
            {
                if (string.IsNullOrEmpty(adduserDto.Password))
                    return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.PasswordRequired);

                user.PasswordSalt = BCrypt.Net.BCrypt.GenerateSalt();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adduserDto.Password);

                await _context.Users.AddAsync(user);
            }

            await _context.SaveChangesAsync();

            // Assign Role only for new user
            if (isNewUser)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == adduserDto.RoleID);
                if (role == null)
                    return ApiResponse<UserResponseDto>.FailureResponse(ApiMessages.RoleNotFound);

                var userRole = new UserRole { Id = Guid.NewGuid(), UserId = user.Id, RoleID = role.RoleID };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                var placeholders = new Dictionary<string, string>
{
                    { "{{FirstName}}", user.FirstName },
                    { "{{Email}}", user.Email },
                    { "{{RoleName}}", role.RoleName },
                    { "{{Password}}", adduserDto.Password },
                    { "{{Year}}", DateTime.Now.Year.ToString() }
                };
                string subject = "Welcome User";
                var body = EmailTemplateGenerator.GetEmailBody(EmailTemplateType.AddUser, placeholders);
                //send email
                await emailService.SendEmailAsync(user.Email, subject, body);
            }
            else
            {
                //update existing user role if changed
                var existingUserRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.Id);
                if (existingUserRole != null && existingUserRole.RoleID != adduserDto.RoleID)
                {
                    existingUserRole.RoleID = adduserDto.RoleID;
                    _context.UserRoles.Update(existingUserRole);
                    await _context.SaveChangesAsync();
                }
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
                    //PasswordHash = user.PasswordHash,
                    //PasswordSalt = user.PasswordSalt,
                    Role = roleName != null ? new RoleDto { Id = adduserDto.RoleID, RoleName = roleName } : null
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
        public async Task<ApiResponse<LoginResponseDto>> MimicUserAsync(Guid userId)
        {
            // Get user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
                return ApiResponse<LoginResponseDto>.FailureResponse("User not found.");

            // Get role and roleId
            var userRoleInfo = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleID, r => r.RoleID, (ur, r) => new
                {
                    RoleID = ur.RoleID,
                    RoleName = r.RoleName
                })
                .FirstOrDefaultAsync();

            if (userRoleInfo == null)
                return ApiResponse<LoginResponseDto>.FailureResponse("User role not found.");

            // Get permissions
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleID == userRoleInfo.RoleID)
                .Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => p.Name)
                .ToListAsync();

            // Generate new token
            var token = _jwtService.GenerateToken(user.Id, user.Email, userRoleInfo.RoleName);

            // Create DTO
            var userDto = new LoginResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Mobile = user.Mobile,
                RoleID = userRoleInfo.RoleID,
                RoleName = userRoleInfo.RoleName,
                Token = token,
                Permissions = permissions
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(userDto, "Mimic successful.");
        }
        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == loginRequest.Email && !user.IsDeleted);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponseDto>.FailureResponse(ApiMessages.InvalidCredentials);
            }

            var userRoleInfo = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleID, r => r.RoleID, (ur, r) => new
                {
                    RoleID = ur.RoleID,
                    RoleName = r.RoleName
                })
                .FirstOrDefaultAsync();

            if (userRoleInfo == null)
            {
                return ApiResponse<LoginResponseDto>.FailureResponse(ApiMessages.RoleNotFound);
            }

            var token = _jwtService.GenerateToken(user.Id, user.Email, userRoleInfo.RoleName);

            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token and expiry in user entity
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleID == userRoleInfo.RoleID)
                .Join(_context.Permissions,
                      rp => rp.PermissionId,
                      p => p.Id,
                      (rp, p) => p.Name)
                .Distinct()
                .ToListAsync();

            var userDto = new LoginResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Mobile = user.Mobile,
                RoleID = userRoleInfo.RoleID,
                RoleName = userRoleInfo.RoleName,
                Token = token,
                RefreshToken = refreshToken,   // <-- Use generated refresh token here
                Permissions = permissions
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(userDto, ApiMessages.UserLoginSuccessfully);
        }


        public async Task<UserDto> GetUserWithRoleByEmailAsync(string email)
        {
            var query = from user in _context.Users
                        where !user.IsDeleted && user.Email == email
                        join userRole in _context.UserRoles on user.Id equals userRole.UserId
                        join role in _context.Roles on userRole.RoleID equals role.RoleID
                        select new UserDto
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Mobile = user.Mobile,
                            RoleID = role.RoleID,
                            RoleName = role.RoleName
                        };

            return await query.FirstOrDefaultAsync();
        }
        // UserRepository.cs
        public async Task UpdateRefreshTokenAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GenerateAndSaveOTPAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            var otp = new Random().Next(100000, 999999).ToString();
            user.ResetOTP = otp;
            user.OTPExpiryTime = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(email, "Password Reset OTP", $"Your OTP is: {otp}");
            return true;
        }

        public async Task<bool> VerifyOtpOnlyAsync(string otp)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.ResetOTP == otp && u.OTPExpiryTime > DateTime.UtcNow);

            return user != null;
        }


        public async Task<bool> ResetPasswordAsync(string email, ResetPasswordRequestDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetOTP = null;
            user.OTPExpiryTime = null;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
