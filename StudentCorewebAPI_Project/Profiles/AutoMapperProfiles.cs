using AutoMapper;
using StudentCorewebAPI_Project.DTOs;
using StudentCorewebAPI_Project.Models;
using System.Collections.Generic;
using System.Linq;

namespace StudentCorewebAPI_Project.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<AddUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            CreateMap<List<User>, List<UserDto>>()
                .ConvertUsing(src => src.Select(s => new UserDto
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    Mobile = s.Mobile
                }).ToList());

            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest=> dest.PasswordSalt,opt => opt.Ignore());
            CreateMap<LoginRequest, User>();

        }
    }
}
